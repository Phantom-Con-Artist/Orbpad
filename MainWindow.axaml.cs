using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Orbpad.Managers;
using Orbpad.Models;
using Orbpad.Services;

namespace Orbpad;

public partial class MainWindow : Window
{
    private readonly DocumentManager _documentManager;
    private readonly FileService _fileService;

    private readonly Dictionary<Document, Button>
        _documentButtons = new();

    // Remembers the last real selection/caret position in the Editor.
    // Clicking a menu item moves focus away from the Editor, which
    // collapses its live selection — these fields let us restore it
    // right before running a Cut/Copy/Paste command from the menu.
    private int _lastKnownSelectionStart;

    private int _lastKnownSelectionEnd;

    private int _lastKnownCaretIndex;

    public MainWindow()
    {
        InitializeComponent();

        _documentManager =
            new DocumentManager();

        _fileService =
            new FileService();

        CreateInitialDocument();

        Editor.PropertyChanged +=
            Editor_PropertyChanged;

        UpdateWindowTitle();
        UpdateStatusBar();
        RefreshTabs();
        UpdateThemeMenu();
    }

    // ============================================================
    // KEYBOARD SHORTCUTS
    // ============================================================

    private void Window_KeyDown(
        object? sender,
        KeyEventArgs e)
    {
        if (!e.KeyModifiers.HasFlag(
                KeyModifiers.Control))
        {
            return;
        }

        if (e.Key == Key.Tab)
        {
            if (e.KeyModifiers.HasFlag(
                    KeyModifiers.Shift))
            {
                SwitchToPreviousDocument();
            }
            else
            {
                SwitchToNextDocument();
            }

            e.Handled = true;
            return;
        }

        if (e.Key == Key.W)
        {
            CloseActiveDocument();

            e.Handled = true;
            return;
        }

        if (e.Key >= Key.D1 &&
            e.Key <= Key.D9)
        {
            int index =
                e.Key - Key.D1;

            SwitchToDocumentAt(index);

            e.Handled = true;
        }
    }

    private void SwitchToNextDocument()
    {
        var document =
            _documentManager.GetNextDocument();

        if (document is null)
            return;

        SwitchToDocument(document);
    }

    private void SwitchToPreviousDocument()
    {
        var document =
            _documentManager.GetPreviousDocument();

        if (document is null)
            return;

        SwitchToDocument(document);
    }

    private void SwitchToDocumentAt(
        int index)
    {
        var document =
            _documentManager.GetDocumentAt(index);

        if (document is null)
            return;

        SwitchToDocument(document);
    }

    private void CloseActiveDocument()
    {
        var document =
            _documentManager.ActiveDocument;

        if (document is null)
            return;

        _ = CloseDocumentAsync(document);
    }

    // ============================================================
    // DOCUMENT CREATION
    // ============================================================

    private void CreateInitialDocument()
    {
        var document =
            _documentManager.CreateDocument();

        LoadDocumentIntoEditor(document);
    }

    private void NewTab_Click(
        object? sender,
        RoutedEventArgs e)
    {
        CreateNewDocument();
    }

    private void CreateNewDocument()
    {
        var document =
            _documentManager.CreateDocument();

        LoadDocumentIntoEditor(document);

        RefreshTabs();
        UpdateWindowTitle();
        UpdateStatusBar();
    }

    // ============================================================
    // DOCUMENT SWITCHING
    // ============================================================

    private void SwitchToDocument(
        Document document)
    {
        if (_documentManager.ActiveDocument ==
            document)
        {
            return;
        }

        _documentManager.SetActiveDocument(
            document);

        LoadDocumentIntoEditor(document);

        UpdateWindowTitle();
        UpdateStatusBar();
        UpdateTabAppearance();
    }

    private void LoadDocumentIntoEditor(
        Document document)
    {
        Editor.Text =
            document.Text ?? string.Empty;

        Editor.CaretIndex = 0;

        UpdateWindowTitle();
        UpdateStatusBar();
    }

    // ============================================================
    // TABS
    // ============================================================

    private void RefreshTabs()
    {
        TabPanel.Children.Clear();
        _documentButtons.Clear();

        foreach (var document
                 in _documentManager.Documents)
        {
            var tab =
                CreateTab(document);

            TabPanel.Children.Add(tab);
        }

        UpdateTabAppearance();
    }

    private Border CreateTab(
        Document document)
    {
        var container =
            new Border
            {
                Background =
                    GetThemeBrush(
                        "OrbpadSurfaceBrush"),

                CornerRadius =
                    new CornerRadius(5),

                Padding =
                    new Thickness(4, 2),

                Margin =
                    new Thickness(0)
            };

        var grid =
            new Grid
            {
                ColumnDefinitions =
                    new ColumnDefinitions(
                        "Auto,Auto")
            };

        var documentButton =
            new Button
            {
                Content =
                    GetDocumentTitle(document),

                Background =
                    Brushes.Transparent,

                BorderThickness =
                    new Thickness(0),

                Padding =
                    new Thickness(10, 6)
            };

        documentButton.Click +=
            (_, _) =>
            {
                SwitchToDocument(document);
            };

        var closeButton =
            new Button
            {
                Content = "×",

                Background =
                    Brushes.Transparent,

                BorderThickness =
                    new Thickness(0),

                Padding =
                    new Thickness(7, 4)
            };

        closeButton.Click +=
            async (_, _) =>
            {
                await CloseDocumentAsync(
                    document);
            };

        Grid.SetColumn(
            documentButton,
            0);

        Grid.SetColumn(
            closeButton,
            1);

        grid.Children.Add(
            documentButton);

        grid.Children.Add(
            closeButton);

        container.Child =
            grid;

        _documentButtons[document] =
            documentButton;

        return container;
    }

    private IBrush GetThemeBrush(
        string resourceKey)
    {
        if (Application.Current is not null &&
            Application.Current.Resources[resourceKey]
                is IBrush brush)
        {
            return brush;
        }

        return Brushes.Transparent;
    }

    private void UpdateTabAppearance()
    {
        foreach (var pair
                 in _documentButtons)
        {
            bool isActive =
                pair.Key ==
                _documentManager.ActiveDocument;

            pair.Value.Background =
                isActive
                    ? GetThemeBrush(
                        "OrbpadSurfaceHoverBrush")
                    : GetThemeBrush(
                        "OrbpadSurfaceBrush");
        }
    }

    private string GetDocumentTitle(
        Document document)
    {
        int documentNumber = 1;

        foreach (var currentDocument
                 in _documentManager.Documents)
        {
            if (currentDocument == document)
                break;

            documentNumber++;
        }

        if (document.FilePath is not null)
        {
            string fileName =
                System.IO.Path.GetFileName(
                    document.FilePath);

            return document.IsModified
                ? $"{fileName} *"
                : fileName;
        }

        return document.IsModified
            ? $"Untitled {documentNumber} *"
            : $"Untitled {documentNumber}";
    }

    // ============================================================
    // CLOSE DOCUMENT
    // ============================================================

    private async Task CloseDocumentAsync(
        Document document)
    {
        if (document.IsModified)
        {
            var result =
                await ShowCloseConfirmationAsync(
                    document);

            if (result ==
                CloseDocumentResult.Cancel)
            {
                return;
            }

            if (result ==
                CloseDocumentResult.Save)
            {
                bool saved =
                    await SaveDocumentAsync(
                        document);

                if (!saved)
                    return;
            }
        }

        bool wasActive =
            _documentManager.ActiveDocument ==
            document;

        _documentManager.RemoveDocument(
            document);

        if (_documentManager.Documents.Count == 0)
        {
            var newDocument =
                _documentManager.CreateDocument();

            LoadDocumentIntoEditor(
                newDocument);
        }
        else if (wasActive)
        {
            var activeDocument =
                _documentManager.ActiveDocument;

            if (activeDocument is not null)
            {
                LoadDocumentIntoEditor(
                    activeDocument);
            }
        }

        RefreshTabs();
        UpdateWindowTitle();
        UpdateStatusBar();
    }

    private async Task<CloseDocumentResult>
        ShowCloseConfirmationAsync(
            Document document)
    {
        var dialog =
            new ConfirmDialog();

        var result =
            await dialog.ShowDialog<bool?>(
                this);

        if (result == true)
            return CloseDocumentResult.Save;

        if (result == false)
            return CloseDocumentResult.DontSave;

        return CloseDocumentResult.Cancel;
    }

    // ============================================================
    // FILE MENU
    // ============================================================

    private async void New_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (!await ConfirmDiscardChangesAsync())
            return;

        CreateNewDocument();
    }

    private async void Open_Click(
        object? sender,
        RoutedEventArgs e)
    {
        var files =
            await StorageProvider
                .OpenFilePickerAsync(
                    new FilePickerOpenOptions
                    {
                        Title = "Open File",
                        AllowMultiple = false
                    });

        if (files.Count == 0)
            return;

        var file =
            files[0];

        if (file.TryGetLocalPath()
            is not string filePath)
        {
            return;
        }

        var document =
            _documentManager.CreateDocument();

        document.Text =
            _fileService.ReadFile(
                filePath);

        document.FilePath =
            filePath;

        document.MarkAsSaved();

        LoadDocumentIntoEditor(
            document);

        RefreshTabs();
        UpdateWindowTitle();
        UpdateStatusBar();
    }

    private async void OpenImage_Click(
        object? sender,
        RoutedEventArgs e)
    {
        var files =
            await StorageProvider
                .OpenFilePickerAsync(
                    new FilePickerOpenOptions
                    {
                        Title = "Open Image",
                        AllowMultiple = false,

                        FileTypeFilter =
                        [
                            FilePickerFileTypes.ImageAll
                        ]
                    });

        if (files.Count == 0)
            return;

        var file =
            files[0];

        try
        {
            await using var stream =
                await file.OpenReadAsync();

            var bitmap =
                new Avalonia.Media.Imaging.Bitmap(
                    stream);

            string title =
                file.Name;

            var imageWindow =
                new ImageViewerWindow(
                    bitmap,
                    title);

            await imageWindow.ShowDialog(
                this);
        }
        catch (Exception ex)
        {
            var errorDialog =
                new Window
                {
                    Title = "Unable to Open Image",
                    Width = 450,
                    Height = 220,
                    WindowStartupLocation =
                        WindowStartupLocation.CenterOwner
                };

            var panel =
                new StackPanel
                {
                    Margin =
                        new Thickness(24),
                    Spacing = 16
                };

            panel.Children.Add(
                new TextBlock
                {
                    Text =
                        "Orbpad could not open this image.",
                    FontSize = 18
                });

            panel.Children.Add(
                new TextBlock
                {
                    Text = ex.Message,
                    TextWrapping =
                        TextWrapping.Wrap
                });

            var closeButton =
                new Button
                {
                    Content = "Close",
                    HorizontalAlignment =
                        Avalonia.Layout.HorizontalAlignment.Right
                };

            closeButton.Click +=
                (_, _) =>
                {
                    errorDialog.Close();
                };

            panel.Children.Add(
                closeButton);

            errorDialog.Content =
                panel;

            await errorDialog.ShowDialog(
                this);
        }
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
        _ = ExitApplicationAsync();
    }

    // ============================================================
    // VIEW
    // ============================================================

    private void WordWrap_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (Editor.TextWrapping ==
            TextWrapping.NoWrap)
        {
            Editor.TextWrapping =
                TextWrapping.Wrap;

            WordWrapMenuItem.Header =
                "_Word Wrap ✓";
        }
        else
        {
            Editor.TextWrapping =
                TextWrapping.NoWrap;

            WordWrapMenuItem.Header =
                "_Word Wrap";
        }
    }

    private void StatusBar_Click(
        object? sender,
        RoutedEventArgs e)
    {
        StatusBar.IsVisible =
            !StatusBar.IsVisible;

        StatusBarMenuItem.Header =
            StatusBar.IsVisible
                ? "_Show Status Bar ✓"
                : "_Show Status Bar";
    }

    // ============================================================
    // THEMES
    // ============================================================

    private void DarkTheme_Click(
        object? sender,
        RoutedEventArgs e)
    {
        ThemeManager.ApplyTheme(
            ThemeManager.OrbpadTheme.Dark);

        RefreshTabs();
        UpdateThemeMenu();
    }

    private void MidnightTheme_Click(
        object? sender,
        RoutedEventArgs e)
    {
        ThemeManager.ApplyTheme(
            ThemeManager.OrbpadTheme.Midnight);

        RefreshTabs();
        UpdateThemeMenu();
    }

    private void ForestTheme_Click(
        object? sender,
        RoutedEventArgs e)
    {
        ThemeManager.ApplyTheme(
            ThemeManager.OrbpadTheme.Forest);

        RefreshTabs();
        UpdateThemeMenu();
    }

    private void LightTheme_Click(
        object? sender,
        RoutedEventArgs e)
    {
        ThemeManager.ApplyTheme(
            ThemeManager.OrbpadTheme.Light);

        RefreshTabs();
        UpdateThemeMenu();
    }

    private void UpdateThemeMenu()
    {
        DarkThemeMenuItem.Header =
            ThemeManager.CurrentTheme ==
            ThemeManager.OrbpadTheme.Dark
                ? "Orbpad _Dark ✓"
                : "Orbpad _Dark";

        MidnightThemeMenuItem.Header =
            ThemeManager.CurrentTheme ==
            ThemeManager.OrbpadTheme.Midnight
                ? "_Midnight ✓"
                : "_Midnight";

        ForestThemeMenuItem.Header =
            ThemeManager.CurrentTheme ==
            ThemeManager.OrbpadTheme.Forest
                ? "_Forest ✓"
                : "_Forest";

        LightThemeMenuItem.Header =
            ThemeManager.CurrentTheme ==
            ThemeManager.OrbpadTheme.Light
                ? "_Light ✓"
                : "_Light";
    }

    private async void About_Click(
        object? sender,
        RoutedEventArgs e)
    {
        var dialog =
            new AboutDialog();

        await dialog.ShowDialog(this);
    }

    // ============================================================
    // EXIT
    // ============================================================

    private async Task ExitApplicationAsync()
    {
        foreach (var document
                 in _documentManager.Documents)
        {
            if (!document.IsModified)
                continue;

            _documentManager.SetActiveDocument(
                document);

            LoadDocumentIntoEditor(
                document);

            bool shouldContinue =
                await ConfirmDiscardChangesAsync();

            if (!shouldContinue)
                return;
        }

        Close();
    }

    // ============================================================
    // SAVE
    // ============================================================

    private async Task<bool>
        SaveDocumentAsync(
            Document document)
    {
        if (document.FilePath is null)
        {
            var previousDocument =
                _documentManager.ActiveDocument;

            _documentManager.SetActiveDocument(
                document);

            LoadDocumentIntoEditor(
                document);

            bool saved =
                await SaveAsAsync();

            if (previousDocument is not null)
            {
                bool stillExists = false;

                foreach (var existingDocument
                         in _documentManager.Documents)
                {
                    if (existingDocument ==
                        previousDocument)
                    {
                        stillExists = true;
                        break;
                    }
                }

                if (stillExists)
                {
                    _documentManager.SetActiveDocument(
                        previousDocument);

                    LoadDocumentIntoEditor(
                        previousDocument);
                }
            }

            return saved;
        }

        _fileService.WriteFile(
            document.FilePath,
            document.Text);

        document.MarkAsSaved();

        RefreshTabs();
        UpdateWindowTitle();

        return true;
    }

    private async Task<bool>
        SaveAsAsync()
    {
        var document =
            _documentManager.ActiveDocument;

        if (document is null)
            return false;

        var file =
            await StorageProvider
                .SaveFilePickerAsync(
                    new FilePickerSaveOptions
                    {
                        Title = "Save File",

                        SuggestedFileName =
                            "Untitled.txt",

                        DefaultExtension =
                            "txt",

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
            return false;

        if (file.TryGetLocalPath()
            is not string filePath)
        {
            return false;
        }

        document.FilePath =
            filePath;

        SaveCurrentDocument();

        return true;
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

        string content =
            Editor.Text ?? string.Empty;

        document.Text =
            content;

        _fileService.WriteFile(
            document.FilePath,
            document.Text);

        document.MarkAsSaved();

        UpdateWindowTitle();
        UpdateStatusBar();
        RefreshTabs();
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
            await dialog.ShowDialog<bool?>(
                this);

        if (result == true)
        {
            if (document.FilePath is null)
            {
                return await SaveAsAsync();
            }

            SaveCurrentDocument();

            return true;
        }

        if (result == false)
            return true;

        return false;
    }

    // ============================================================
    // EDIT
    // ============================================================

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
        RestoreEditorSelection();

        Editor.Cut();
        UpdateStatusBar();
    }

    private void Copy_Click(
        object? sender,
        RoutedEventArgs e)
    {
        RestoreEditorSelection();

        Editor.Copy();
    }

    private void Paste_Click(
        object? sender,
        RoutedEventArgs e)
    {
        RestoreEditorSelection();

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

    // ============================================================
    // SEARCH
    // ============================================================

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
        SearchBar.IsVisible =
            false;

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

        if (string.IsNullOrEmpty(
                searchText))
        {
            return;
        }

        string text =
            Editor.Text ?? string.Empty;

        if (text.IndexOf(
                searchText,
                StringComparison.OrdinalIgnoreCase)
            < 0)
        {
            return;
        }

        string result =
            string.Empty;

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
                result +=
                    text[currentIndex..];

                break;
            }

            result +=
                text[currentIndex..index];

            currentIndex =
                index + searchText.Length;
        }

        Editor.Text =
            result;
    }

    private void ShowSearchBar()
    {
        SearchBar.IsVisible =
            true;

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

        if (string.IsNullOrEmpty(
                searchText))
        {
            return;
        }

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

        if (string.IsNullOrEmpty(
                searchText))
        {
            return;
        }

        string text =
            Editor.Text ?? string.Empty;

        if (text.Length == 0)
            return;

        int startIndex =
            Editor.SelectionStart - 1;

        if (startIndex < 0)
        {
            startIndex =
                text.Length - 1;
        }

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

    // ============================================================
    // EDITOR
    // ============================================================

    private void Editor_TextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        var document =
            _documentManager.ActiveDocument;

        if (document is null)
            return;

        string editorText =
            Editor.Text ?? string.Empty;

        if (editorText ==
            document.Text)
        {
            UpdateWindowTitle();
            RefreshTabs();

            return;
        }

        document.Text =
            editorText;

        UpdateWindowTitle();
        UpdateStatusBar();
        RefreshTabs();
    }

    private void Editor_PropertyChanged(
        object? sender,
        AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property ==
            TextBox.CaretIndexProperty)
        {
            _lastKnownCaretIndex =
                Editor.CaretIndex;

            UpdateStatusBar();
        }
        else if (e.Property == TextBox.SelectionStartProperty ||
                 e.Property == TextBox.SelectionEndProperty)
        {
            // Only remember a genuine, non-empty selection. Losing
            // focus collapses SelectionStart/SelectionEnd to the same
            // value, and we don't want that momentary collapse to
            // stomp on the real selection the user made.
            if (Editor.SelectionStart !=
                Editor.SelectionEnd)
            {
                _lastKnownSelectionStart =
                    Editor.SelectionStart;

                _lastKnownSelectionEnd =
                    Editor.SelectionEnd;
            }
        }
    }

    // Restores focus and the last known selection (or caret position)
    // to the Editor. Call this immediately before Cut/Copy/Paste when
    // the command may have been triggered from a menu, since opening
    // the menu moves focus off the Editor and collapses its selection.
    private void RestoreEditorSelection()
    {
        Editor.Focus();

        if (_lastKnownSelectionStart !=
            _lastKnownSelectionEnd)
        {
            Editor.SelectionStart =
                _lastKnownSelectionStart;

            Editor.SelectionEnd =
                _lastKnownSelectionEnd;
        }
        else
        {
            Editor.CaretIndex =
                _lastKnownCaretIndex;
        }
    }

    // ============================================================
    // WINDOW TITLE
    // ============================================================

    private void UpdateWindowTitle()
    {
        var document =
            _documentManager.ActiveDocument;

        if (document is null)
        {
            Title =
                "Orbpad";

            return;
        }

        string fileName;

        if (document.FilePath is null)
        {
            fileName =
                "Untitled";
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

    // ============================================================
    // STATUS BAR
    // ============================================================

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
        if (string.IsNullOrWhiteSpace(
                text))
        {
            return 0;
        }

        return text.Split(
            (char[]?)null,
            StringSplitOptions
                .RemoveEmptyEntries)
            .Length;
    }

    private enum CloseDocumentResult
    {
        Save,
        DontSave,
        Cancel
    }
}