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

    public MainWindow()
    {
        InitializeComponent();

        _document = new Document();
        _fileService = new FileService();
    }

    private void New_Click(object? sender, RoutedEventArgs e)
    {
        _document = new Document();

        Editor.Text = _document.Text;
    }

    private async void Open_Click(object? sender, RoutedEventArgs e)
    {
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

        Editor.Text = _document.Text;
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
}