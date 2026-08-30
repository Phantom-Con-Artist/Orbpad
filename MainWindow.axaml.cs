using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Orbpad.Models;
using Orbpad.Services;

namespace Orbpad;

public partial class MainWindow : Window
{
    private Document _document;
    private readonly FileService _fileService;

    private bool _isUpdatingEditor;

    public MainWindow()
    {
        InitializeComponent();

        _document = new Document();
        _fileService = new FileService();
    }

    private async void New_Click(object? sender, RoutedEventArgs e)
    {
        if (!await ConfirmDiscardChangesAsync())
            return;

        _document = new Document();

        _isUpdatingEditor = true;
        Editor.Text = _document.Text;
        _isUpdatingEditor = false;

        UpdateWindowTitle();
    }

    private async void Open_Click(object? sender, RoutedEventArgs e)
    {
        if (!await ConfirmDiscardChangesAsync())
            return;

        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Open File",
                AllowMultiple = false
            });

        if (files.Count == 0)
            return;

        var file = files[0];

        if (file.TryGetLocalPath() is not string filePath)
            return;

        _document.Text = _fileService.ReadFile(filePath);
        _document.FilePath = filePath;
        _document.IsModified = false;

        _isUpdatingEditor = true;
        Editor.Text = _document.Text;
        _isUpdatingEditor = false;

        UpdateWindowTitle();
    }

    private void Save_Click(object? sender, RoutedEventArgs e)
    {
        if (_document.FilePath is null)
        {
            _ = SaveAsAsync();
            return;
        }

        SaveCurrentDocument();
    }

    private async void SaveAs_Click(object? sender, RoutedEventArgs e)
    {
        await SaveAsAsync();
    }

    private void Exit_Click(object? sender, RoutedEventArgs e)
    {
        if (_document.IsModified)
        {
            _ = ConfirmExitAsync();
            return;
        }

        Close();
    }

    private void Undo_Click(object? sender, RoutedEventArgs e)
    {
        Editor.Undo();
    }

    private void Redo_Click(object? sender, RoutedEventArgs e)
    {
        Editor.Redo();
    }

    private void Cut_Click(object? sender, RoutedEventArgs e)
    {
        Editor.Cut();
    }

    private void Copy_Click(object? sender, RoutedEventArgs e)
    {
        Editor.Copy();
    }

    private void Paste_Click(object? sender, RoutedEventArgs e)
    {
        Editor.Paste();
    }

    private void SelectAll_Click(object? sender, RoutedEventArgs e)
    {
        Editor.SelectAll();
    }

    private void SaveCurrentDocument()
    {
        if (_document.FilePath is null)
            return;

        var content = Editor.Text ?? string.Empty;

        _fileService.WriteFile(
            _document.FilePath,
            content);

        _document.Text = content;
        _document.IsModified = false;

        UpdateWindowTitle();
    }

    private async Task SaveAsAsync()
    {
        var file = await StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "Save File",
                SuggestedFileName = "Untitled.txt",
                DefaultExtension = "txt",
                FileTypeChoices =
                [
                    new FilePickerFileType("Text File")
                    {
                        Patterns = ["*.txt"]
                    },
                    new FilePickerFileType("All Files")
                    {
                        Patterns = ["*"]
                    }
                ]
            });

        if (file is null)
            return;

        if (file.TryGetLocalPath() is not string filePath)
            return;

        _document.FilePath = filePath;

        SaveCurrentDocument();
    }

    private void Editor_TextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        if (_isUpdatingEditor)
            return;

        _document.Text = Editor.Text ?? string.Empty;
        _document.IsModified = true;

        UpdateWindowTitle();
    }

    private async Task<bool> ConfirmDiscardChangesAsync()
    {
        if (!_document.IsModified)
            return true;

        var dialog = new ConfirmDialog();

        var result = await dialog.ShowDialog<bool?>(this);

        if (result == true)
        {
            if (_document.FilePath is null)
            {
                await SaveAsAsync();
            }
            else
            {
                SaveCurrentDocument();
            }

            return !_document.IsModified;
        }

        if (result == false)
            return true;

        return false;
    }

    private async Task ConfirmExitAsync()
    {
        var shouldContinue = await ConfirmDiscardChangesAsync();

        if (shouldContinue)
        {
            Close();
        }
    }

    private void UpdateWindowTitle()
    {
        string fileName;

        if (_document.FilePath is null)
        {
            fileName = "Untitled";
        }
        else
        {
            fileName = System.IO.Path.GetFileName(
                _document.FilePath);
        }

        string modifiedMarker =
            _document.IsModified ? " *" : string.Empty;

        Title = $"Orbpad — {fileName}{modifiedMarker}";
    }
}