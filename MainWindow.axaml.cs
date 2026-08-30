using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Orbpad.Managers;
using Orbpad.Models;
using Orbpad.Services;

namespace Orbpad;

public partial class MainWindow : Window
{
    private readonly DocumentManager _documentManager;
    private readonly FileService _fileService;

    private bool _isUpdatingEditor;

    public MainWindow()
    {
        InitializeComponent();

        _documentManager = new DocumentManager();
        _fileService = new FileService();

        CreateInitialDocument();

        Editor.PropertyChanged += Editor_PropertyChanged;

        UpdateWindowTitle();
        UpdateStatusBar();
    }

    private void CreateInitialDocument()
    {
        var document =
            _documentManager.CreateDocument();

        _isUpdatingEditor = true;

        Editor.Text =
            document.Text;

        _isUpdatingEditor = false;
    }

    private async void New_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (!await ConfirmDiscardChangesAsync())
            return;

        var document =
            _documentManager.CreateDocument();

        _isUpdatingEditor = true;

        Editor.Text =
            document.Text;

        _isUpdatingEditor = false;

        UpdateWindowTitle();
        UpdateStatusBar();
    }

    private async void Open_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (!await ConfirmDiscardChangesAsync())
            return;

        var files =
            await StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Open File",
                    AllowMultiple = false
                });

        if (files.Count == 0)
            return;

        var file = files[0];

        if (file.TryGetLocalPath()
            is not string filePath)
        {
            return;
        }

        var document =
            _documentManager.CreateDocument();

        document.Text =
            _fileService.ReadFile(filePath);

        document.FilePath =
            filePath;

        document.IsModified = false;

        _isUpdatingEditor = true;

        Editor.Text =
            document.Text;

        _isUpdatingEditor = false;

        UpdateWindowTitle();
        UpdateStatusBar();
    }

    private void Save_Click(
        object? sender,
        RoutedEventArgs e)
    {
        var document =
            _documentManager.ActiveDocument;

        if (document is null)
            return;

        if (document.FilePath is null)
        {
            _ = SaveAsAsync();
            return;
        }

        SaveCurrentDocument();
    }

    private async void SaveAs_Click(
        object? sender,
        RoutedEventArgs e)
    {
        await SaveAsAsync();
    }

    private void Exit_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (_documentManager.ActiveDocument?.IsModified
            == true)
        {
            _ = ConfirmExitAsync();
            return;
        }

        Close();
    }

    private void Undo_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Editor.Undo();
        UpdateStatusBar();
    }

    private void Redo_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Editor.Redo();
        UpdateStatusBar();
    }

    private void Cut_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Editor.Cut();
        UpdateStatusBar();
    }

    private void Copy_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Editor.Copy();
    }

    private void Paste_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Editor.Paste();
        UpdateStatusBar();
    }

    private void SelectAll_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Editor.SelectAll();
        UpdateStatusBar();
    }

    private void Find_Click(
        object? sender,
        RoutedEventArgs e)
    {
        ShowSearchBar();
    }

    private void CloseSearch_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SearchBar.IsVisible = false;
        Editor.Focus();
    }

    private void FindNext_Click(
        object? sender,
        RoutedEventArgs e)
    {
        FindNext();
    }

    private void FindPrevious_Click(
        object? sender,
        RoutedEventArgs e)
    {
        FindPrevious();
    }

    private void Replace_Click(
        object? sender,
        RoutedEventArgs e)
    {
        FindNext();
    }

    private void ReplaceAll_Click(
        object? sender,
        RoutedEventArgs e)
    {
        string searchText =
            SearchBox.Text ?? string.Empty;

        if (string.IsNullOrEmpty(searchText))
            return;

        string text =
            Editor.Text ?? string.Empty;

        int firstIndex =
            text.IndexOf(
                searchText,
                StringComparison.OrdinalIgnoreCase);

        if (firstIndex < 0)
            return;

        string result = string.Empty;
        int currentIndex = 0;

        while (true)
        {
            int index =
                text.IndexOf(
                    searchText,
                    currentIndex,
                    StringComparison.OrdinalIgnoreCase);

            if (index < 0)
            {
                result += text[currentIndex..];
                break;
            }

            result +=
                text[currentIndex..index];

            currentIndex =
                index + searchText.Length;
        }

        _isUpdatingEditor = true;

        Editor.Text =
            result;

        _isUpdatingEditor = false;

        var document =
            _documentManager.ActiveDocument;

        if (document is not null)
        {
            document.Text = result;
            document.IsModified = true;
        }

        UpdateWindowTitle();
        UpdateStatusBar();
    }

    private void ShowSearchBar()
    {
        SearchBar.IsVisible = true;

        SearchBox.Focus();

        if (!string.IsNullOrEmpty(
                Editor.SelectedText))
        {
            SearchBox.Text =
                Editor.SelectedText;
        }
    }

    private void FindNext()
    {
        string searchText =
            SearchBox.Text ?? string.Empty;

        if (string.IsNullOrEmpty(searchText))
            return;

        string text =
            Editor.Text ?? string.Empty;

        int startIndex =
            Editor.SelectionEnd;

        int index =
            text.IndexOf(
                searchText,
                startIndex,
                StringComparison.OrdinalIgnoreCase);

        if (index < 0)
        {
            index =
                text.IndexOf(
                    searchText,
                    0,
                    StringComparison.OrdinalIgnoreCase);
        }

        if (index < 0)
            return;

        Editor.SelectionStart =
            index;

        Editor.SelectionEnd =
            index + searchText.Length;

        Editor.Focus();
    }

    private void FindPrevious()
    {
        string searchText =
            SearchBox.Text ?? string.Empty;

        if (string.IsNullOrEmpty(searchText))
            return;

        string text =
            Editor.Text ?? string.Empty;

        if (text.Length == 0)
            return;

        int startIndex =
            Editor.SelectionStart - 1;

        if (startIndex < 0)
            startIndex =
                text.Length - 1;

        int index =
            text.LastIndexOf(
                searchText,
                startIndex,
                StringComparison.OrdinalIgnoreCase);

        if (index < 0)
        {
            index =
                text.LastIndexOf(
                    searchText,
                    text.Length - 1,
                    StringComparison.OrdinalIgnoreCase);
        }

        if (index < 0)
            return;

        Editor.SelectionStart =
            index;

        Editor.SelectionEnd =
            index + searchText.Length;

        Editor.Focus();
    }

    private void Editor_TextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        if (_isUpdatingEditor)
            return;

        var document =
            _documentManager.ActiveDocument;

        if (document is null)
            return;

        document.Text =
            Editor.Text ?? string.Empty;

        document.IsModified =
            true;

        UpdateWindowTitle();
        UpdateStatusBar();
    }

    private void Editor_PropertyChanged(
        object? sender,
        AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property ==
            TextBox.CaretIndexProperty)
        {
            UpdateStatusBar();
        }
    }

    private async Task SaveAsAsync()
    {
        var document =
            _documentManager.ActiveDocument;

        if (document is null)
            return;

        var file =
            await StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions
                {
                    Title = "Save File",
                    SuggestedFileName = "Untitled.txt",
                    DefaultExtension = "txt",

                    FileTypeChoices =
                    [
                        new FilePickerFileType(
                            "Text File")
                        {
                            Patterns =
                            [
                                "*.txt"
                            ]
                        },

                        new FilePickerFileType(
                            "All Files")
                        {
                            Patterns =
                            [
                                "*"
                            ]
                        }
                    ]
                });

        if (file is null)
            return;

        if (file.TryGetLocalPath()
            is not string filePath)
        {
            return;
        }

        document.FilePath =
            filePath;

        SaveCurrentDocument();
    }

    private void SaveCurrentDocument()
    {
        var document =
            _documentManager.ActiveDocument;

        if (document is null ||
            document.FilePath is null)
        {
            return;
        }

        var content =
            Editor.Text ?? string.Empty;

        _fileService.WriteFile(
            document.FilePath,
            content);

        document.Text =
            content;

        document.IsModified =
            false;

        UpdateWindowTitle();
        UpdateStatusBar();
    }

    private async Task<bool>
        ConfirmDiscardChangesAsync()
    {
        var document =
            _documentManager.ActiveDocument;

        if (document is null ||
            !document.IsModified)
        {
            return true;
        }

        var dialog =
            new ConfirmDialog();

        var result =
            await dialog.ShowDialog<bool?>(this);

        if (result == true)
        {
            if (document.FilePath is null)
            {
                await SaveAsAsync();
            }
            else
            {
                SaveCurrentDocument();
            }

            return !document.IsModified;
        }

        if (result == false)
            return true;

        return false;
    }

    private async Task ConfirmExitAsync()
    {
        var shouldContinue =
            await ConfirmDiscardChangesAsync();

        if (shouldContinue)
        {
            Close();
        }
    }

    private void UpdateWindowTitle()
    {
        var document =
            _documentManager.ActiveDocument;

        if (document is null)
        {
            Title = "Orbpad";
            return;
        }

        string fileName;

        if (document.FilePath is null)
        {
            fileName = "Untitled";
        }
        else
        {
            fileName =
                System.IO.Path.GetFileName(
                    document.FilePath);
        }

        string modifiedMarker =
            document.IsModified
                ? " *"
                : string.Empty;

        Title =
            $"Orbpad — {fileName}{modifiedMarker}";
    }

    private void UpdateStatusBar()
    {
        string text =
            Editor.Text ?? string.Empty;

        int caretIndex =
            Editor.CaretIndex;

        int line = 1;
        int column = 1;

        for (int i = 0;
             i < caretIndex &&
             i < text.Length;
             i++)
        {
            if (text[i] == '\n')
            {
                line++;
                column = 1;
            }
            else
            {
                column++;
            }
        }

        int wordCount =
            CountWords(text);

        int characterCount =
            text.Length;

        CursorPositionText.Text =
            $"Ln {line}, Col {column}";

        WordCountText.Text =
            $"{wordCount} " +
            $"{(wordCount == 1
                ? "word"
                : "words")}";

        CharacterCountText.Text =
            $"  {characterCount} " +
            $"{(characterCount == 1
                ? "character"
                : "characters")}";
    }

    private static int CountWords(
        string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text.Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries)
            .Length;
    }
}