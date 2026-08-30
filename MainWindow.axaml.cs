using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using AvaloniaEdit;
using Orbpad.Managers;
using Orbpad.Models;
using Orbpad.Services;

namespace Orbpad;

public partial class MainWindow : Window
{
    private readonly DocumentManager _documentManager;
    private readonly FileService _fileService;

    private readonly SettingsService _settingsService;
    private readonly AppSettings _settings;

    private readonly SyntaxHighlightingService
        _syntaxHighlightingService;

    private readonly Dictionary<Document, Button>
        _documentButtons = new();

    private const int MaxRecentFiles = 10;

    private int _lastKnownSelectionStart;
    private int _lastKnownSelectionLength;
    private int _lastKnownCaretOffset;


    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public MainWindow()
    {
        InitializeComponent();

        // ========================================================
        // FILE EXPLORER
        // ========================================================

        InitializeExplorer();


        // ========================================================
        // SETTINGS
        // ========================================================

        _settingsService =
            new SettingsService();

        _settings =
            _settingsService.Load();


        // ========================================================
        // SYNTAX HIGHLIGHTING
        // ========================================================

        _syntaxHighlightingService =
            new SyntaxHighlightingService(
                Editor);


        // ========================================================
        // APPLY SAVED SETTINGS
        // ========================================================

        ApplySavedSettings();


        // ========================================================
        // CORE SERVICES
        // ========================================================

        _documentManager =
            new DocumentManager();

        _fileService =
            new FileService();


        // ========================================================
        // INITIAL DOCUMENT
        // ========================================================

        CreateInitialDocument();


        // ========================================================
        // EDITOR EVENTS
        // ========================================================

        Editor.TextChanged +=
            Editor_TextChanged;

        Editor.TextArea.Caret.PositionChanged +=
            Editor_CaretPositionChanged;

        Editor.TextArea.SelectionChanged +=
            Editor_SelectionChanged;


        // ========================================================
        // INITIAL UI
        // ========================================================

        UpdateWindowTitle();
        UpdateStatusBar();
        RefreshTabs();
        UpdateThemeMenu();
        UpdateViewMenu();
        UpdateRecentFilesMenu();

        SaveEditorSelection();
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

        LoadDocumentIntoEditor(
            document);
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

        LoadDocumentIntoEditor(
            document);

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

        LoadDocumentIntoEditor(
            document);

        UpdateWindowTitle();
        UpdateStatusBar();
        UpdateTabAppearance();
    }


    private void LoadDocumentIntoEditor(
        Document document)
    {
        Editor.Text =
            document.Text ?? string.Empty;


        // ========================================================
        // SYNTAX HIGHLIGHTING
        // ========================================================

        _syntaxHighlightingService.ApplyForFile(
            document.FilePath);


        // ========================================================
        // MARKDOWN VIEW
        // ========================================================

        RefreshMarkdownView();


        Editor.CaretOffset = 0;

        _lastKnownSelectionStart = 0;
        _lastKnownSelectionLength = 0;
        _lastKnownCaretOffset = 0;

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
                    GetDocumentTitle(
                        document),

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
                SwitchToDocument(
                    document);
            };


        var closeButton =
            new Button
            {
                Content =
                    "×",

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
            Application.Current.Resources[
                resourceKey] is IBrush brush)
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
                        Title =
                            "Open File",

                        AllowMultiple =
                            false
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


        OpenTextFile(filePath);
    }


    private void OpenTextFile(
        string filePath)
    {
        try
        {
            string fullPath =
                System.IO.Path.GetFullPath(
                    filePath);


            if (!System.IO.File.Exists(
                    fullPath))
            {
                RemoveRecentFile(
                    fullPath);

                return;
            }


            var document =
                _documentManager.CreateDocument();


            document.Text =
                _fileService.ReadFile(
                    fullPath);


            document.FilePath =
                fullPath;


            document.MarkAsSaved();


            LoadDocumentIntoEditor(
                document);


            RefreshTabs();
            UpdateWindowTitle();
            UpdateStatusBar();


            AddRecentFile(
                fullPath);
        }
        catch
        {
            RemoveRecentFile(
                filePath);
        }
    }


    // ============================================================
    // RECENT FILES
    // ============================================================

    private void AddRecentFile(
        string filePath)
    {
        if (string.IsNullOrWhiteSpace(
                filePath))
        {
            return;
        }


        string fullPath;


        try
        {
            fullPath =
                System.IO.Path.GetFullPath(
                    filePath);
        }
        catch
        {
            return;
        }


        if (_settings.RecentFiles is null)
        {
            _settings.RecentFiles =
                new List<string>();
        }


        for (int i =
                 _settings.RecentFiles.Count - 1;
             i >= 0;
             i--)
        {
            if (string.Equals(
                    _settings.RecentFiles[i],
                    fullPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                _settings.RecentFiles.RemoveAt(i);
            }
        }


        _settings.RecentFiles.Insert(
            0,
            fullPath);


        while (_settings.RecentFiles.Count >
               MaxRecentFiles)
        {
            _settings.RecentFiles.RemoveAt(
                _settings.RecentFiles.Count - 1);
        }


        UpdateRecentFilesMenu();

        SaveCurrentSettings();
    }


    private void RemoveRecentFile(
        string filePath)
    {
        if (_settings.RecentFiles is null)
            return;


        for (int i =
                 _settings.RecentFiles.Count - 1;
             i >= 0;
             i--)
        {
            if (string.Equals(
                    _settings.RecentFiles[i],
                    filePath,
                    StringComparison.OrdinalIgnoreCase))
            {
                _settings.RecentFiles.RemoveAt(i);
            }
        }


        UpdateRecentFilesMenu();

        SaveCurrentSettings();
    }


    private void UpdateRecentFilesMenu()
    {
        RecentFilesMenuItem.Items.Clear();


        if (_settings.RecentFiles is null)
        {
            _settings.RecentFiles =
                new List<string>();
        }


        var validFiles =
            new List<string>();


        foreach (var filePath
                 in _settings.RecentFiles)
        {
            if (string.IsNullOrWhiteSpace(
                    filePath))
            {
                continue;
            }


            if (!System.IO.File.Exists(
                    filePath))
            {
                continue;
            }


            bool duplicate =
                false;


            foreach (var existingPath
                     in validFiles)
            {
                if (string.Equals(
                        existingPath,
                        filePath,
                        StringComparison.OrdinalIgnoreCase))
                {
                    duplicate =
                        true;

                    break;
                }
            }


            if (!duplicate)
            {
                validFiles.Add(
                    filePath);
            }


            if (validFiles.Count >=
                MaxRecentFiles)
            {
                break;
            }
        }


        _settings.RecentFiles.Clear();


        foreach (var filePath
                 in validFiles)
        {
            _settings.RecentFiles.Add(
                filePath);
        }


        if (_settings.RecentFiles.Count == 0)
        {
            RecentFilesMenuItem.Items.Add(
                new MenuItem
                {
                    Header =
                        "No recent files",

                    IsEnabled =
                        false
                });

            return;
        }


        foreach (var filePath
                 in _settings.RecentFiles)
        {
            var menuItem =
                new MenuItem
                {
                    Header =
                        System.IO.Path.GetFileName(
                            filePath),

                    Tag =
                        filePath
                };


            menuItem.Click +=
                RecentFile_Click;


            RecentFilesMenuItem.Items.Add(
                menuItem);
        }


        RecentFilesMenuItem.Items.Add(
            new Separator());


        var clearItem =
            new MenuItem
            {
                Header =
                    "Clear Recent Files"
            };


        clearItem.Click +=
            ClearRecentFiles_Click;


        RecentFilesMenuItem.Items.Add(
            clearItem);
    }


    private void RecentFile_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem)
            return;


        if (menuItem.Tag is not string filePath)
            return;


        OpenTextFile(filePath);
    }


    private void ClearRecentFiles_Click(
        object? sender,
        RoutedEventArgs e)
    {
        _settings.RecentFiles.Clear();

        UpdateRecentFilesMenu();

        SaveCurrentSettings();
    }


    // ============================================================
    // IMAGE OPENING
    // ============================================================

    private async void OpenImage_Click(
        object? sender,
        RoutedEventArgs e)
    {
        var files =
            await StorageProvider
                .OpenFilePickerAsync(
                    new FilePickerOpenOptions
                    {
                        Title =
                            "Open Image",

                        AllowMultiple =
                            false,

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
                    Title =
                        "Unable to Open Image",

                    Width =
                        450,

                    Height =
                        220,

                    WindowStartupLocation =
                        WindowStartupLocation.CenterOwner
                };


            var panel =
                new StackPanel
                {
                    Margin =
                        new Thickness(24),

                    Spacing =
                        16
                };


            panel.Children.Add(
                new TextBlock
                {
                    Text =
                        "Orbpad could not open this image.",

                    FontSize =
                        18
                });


            panel.Children.Add(
                new TextBlock
                {
                    Text =
                        ex.Message,

                    TextWrapping =
                        TextWrapping.Wrap
                });


            var closeButton =
                new Button
                {
                    Content =
                        "Close",

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
        Editor.WordWrap =
            !Editor.WordWrap;


        WordWrapMenuItem.Header =
            Editor.WordWrap
                ? "_Word Wrap ✓"
                : "_Word Wrap";


        SaveCurrentSettings();
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


        SaveCurrentSettings();
    }


    private void LineNumbers_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Editor.ShowLineNumbers =
            !Editor.ShowLineNumbers;


        LineNumbersMenuItem.Header =
            Editor.ShowLineNumbers
                ? "_Show Line Numbers ✓"
                : "_Show Line Numbers";


        SaveCurrentSettings();
    }


    private void Toolbar_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Toolbar.IsVisible =
            !Toolbar.IsVisible;


        ToolbarMenuItem.Header =
            Toolbar.IsVisible
                ? "_Show Toolbar ✓"
                : "_Show Toolbar";


        SaveCurrentSettings();
    }


    // ============================================================
    // FONT CONTROLS
    // ============================================================

    private void InterFont_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SetEditorFont("Inter");
    }


    private void SegoeUIFont_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SetEditorFont("Segoe UI");
    }


    private void ConsolasFont_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SetEditorFont("Consolas");
    }


    private void CourierNewFont_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SetEditorFont("Courier New");
    }


    private void SetEditorFont(
        string fontName)
    {
        Editor.FontFamily =
            new FontFamily(
                fontName);


        UpdateFontMenu();

        SaveCurrentSettings();
    }


    private void FontSize10_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SetEditorFontSize(10);
    }


    private void FontSize12_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SetEditorFontSize(12);
    }


    private void FontSize14_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SetEditorFontSize(14);
    }


    private void FontSize16_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SetEditorFontSize(16);
    }


    private void FontSize18_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SetEditorFontSize(18);
    }


    private void FontSize20_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SetEditorFontSize(20);
    }


    private void FontSize24_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SetEditorFontSize(24);
    }


    private void FontSize28_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SetEditorFontSize(28);
    }


    private void SetEditorFontSize(
        double fontSize)
    {
        Editor.FontSize =
            fontSize;


        UpdateFontMenu();

        SaveCurrentSettings();
    }


    private void UpdateFontMenu()
    {
        string currentFont =
            Editor.FontFamily?.Name
            ?? "Inter";


        InterFontMenuItem.Header =
            string.Equals(
                currentFont,
                "Inter",
                StringComparison.OrdinalIgnoreCase)
                ? "Inter ✓"
                : "Inter";


        SegoeUIFontMenuItem.Header =
            string.Equals(
                currentFont,
                "Segoe UI",
                StringComparison.OrdinalIgnoreCase)
                ? "Segoe UI ✓"
                : "Segoe UI";


        ConsolasFontMenuItem.Header =
            string.Equals(
                currentFont,
                "Consolas",
                StringComparison.OrdinalIgnoreCase)
                ? "Consolas ✓"
                : "Consolas";


        CourierNewFontMenuItem.Header =
            string.Equals(
                currentFont,
                "Courier New",
                StringComparison.OrdinalIgnoreCase)
                ? "Courier New ✓"
                : "Courier New";


        SetFontSizeMenuHeader(
            FontSize10MenuItem,
            10);


        SetFontSizeMenuHeader(
            FontSize12MenuItem,
            12);


        SetFontSizeMenuHeader(
            FontSize14MenuItem,
            14);


        SetFontSizeMenuHeader(
            FontSize16MenuItem,
            16);


        SetFontSizeMenuHeader(
            FontSize18MenuItem,
            18);


        SetFontSizeMenuHeader(
            FontSize20MenuItem,
            20);


        SetFontSizeMenuHeader(
            FontSize24MenuItem,
            24);


        SetFontSizeMenuHeader(
            FontSize28MenuItem,
            28);
    }


    // ============================================================
    // VIEW MENU STATE
    // ============================================================

    private void UpdateViewMenu()
    {
        WordWrapMenuItem.Header =
            Editor.WordWrap
                ? "_Word Wrap ✓"
                : "_Word Wrap";


        StatusBarMenuItem.Header =
            StatusBar.IsVisible
                ? "_Show Status Bar ✓"
                : "_Show Status Bar";


        LineNumbersMenuItem.Header =
            Editor.ShowLineNumbers
                ? "_Show Line Numbers ✓"
                : "_Show Line Numbers";


        ToolbarMenuItem.Header =
            Toolbar.IsVisible
                ? "_Show Toolbar ✓"
                : "_Show Toolbar";


        UpdateFontMenu();
    }


    private void SetFontSizeMenuHeader(
        MenuItem menuItem,
        double size)
    {
        menuItem.Header =
            Math.Abs(
                Editor.FontSize - size) < 0.1
                ? $"{size:0} ✓"
                : $"{size:0}";
    }


    // ============================================================
    // THEMES
    // ============================================================

    private void PurpleTheme_Click(
        object? sender,
        RoutedEventArgs e)
    {
        ThemeManager.ApplyTheme(
            ThemeManager.OrbpadTheme.Purple);

        RefreshTabs();
        UpdateThemeMenu();

        // ========================================================
        // MARKDOWN THEME REFRESH
        // ========================================================

        UpdateMarkdownPreview();

        SaveCurrentSettings();
    }


    private void DarkTheme_Click(
        object? sender,
        RoutedEventArgs e)
    {
        ThemeManager.ApplyTheme(
            ThemeManager.OrbpadTheme.Dark);


        RefreshTabs();
        UpdateThemeMenu();

        // ========================================================
        // MARKDOWN THEME REFRESH
        // ========================================================

        UpdateMarkdownPreview();


        SaveCurrentSettings();
    }


    private void MidnightTheme_Click(
        object? sender,
        RoutedEventArgs e)
    {
        ThemeManager.ApplyTheme(
            ThemeManager.OrbpadTheme.Midnight);


        RefreshTabs();
        UpdateThemeMenu();

        // ========================================================
        // MARKDOWN THEME REFRESH
        // ========================================================

        UpdateMarkdownPreview();


        SaveCurrentSettings();
    }


    private void ForestTheme_Click(
        object? sender,
        RoutedEventArgs e)
    {
        ThemeManager.ApplyTheme(
            ThemeManager.OrbpadTheme.Forest);


        RefreshTabs();
        UpdateThemeMenu();

        // ========================================================
        // MARKDOWN THEME REFRESH
        // ========================================================

        UpdateMarkdownPreview();


        SaveCurrentSettings();
    }


    private void LightTheme_Click(
        object? sender,
        RoutedEventArgs e)
    {
        ThemeManager.ApplyTheme(
            ThemeManager.OrbpadTheme.Light);


        RefreshTabs();
        UpdateThemeMenu();

        // ========================================================
        // MARKDOWN THEME REFRESH
        // ========================================================

        UpdateMarkdownPreview();


        SaveCurrentSettings();
    }


    // ============================================================
    // SETTINGS
    // ============================================================

    private void ApplySavedSettings()
    {
        if (Enum.TryParse<
                ThemeManager.OrbpadTheme>(
                _settings.Theme,
                true,
                out var theme))
        {
            ThemeManager.ApplyTheme(
                theme);
        }
        else
        {
            ThemeManager.ApplyTheme(
                ThemeManager.OrbpadTheme.Purple);
        }


        Toolbar.IsVisible =
            _settings.ShowToolbar;


        StatusBar.IsVisible =
            _settings.ShowStatusBar;


        Editor.ShowLineNumbers =
            _settings.ShowLineNumbers;


        Editor.WordWrap =
            _settings.WordWrap;


        if (!string.IsNullOrWhiteSpace(
                _settings.FontFamily))
        {
            Editor.FontFamily =
                new FontFamily(
                    _settings.FontFamily);
        }


        if (_settings.FontSize > 0)
        {
            Editor.FontSize =
                _settings.FontSize;
        }


        Width =
            Math.Max(
                MinWidth,
                _settings.WindowWidth);


        Height =
            Math.Max(
                MinHeight,
                _settings.WindowHeight);


        if (_settings.WindowX.HasValue &&
            _settings.WindowY.HasValue)
        {
            Position =
                new PixelPoint(
                    _settings.WindowX.Value,
                    _settings.WindowY.Value);
        }
    }


    private void SaveCurrentSettings()
    {
        _settings.Theme =
            ThemeManager.CurrentTheme.ToString();


        _settings.ShowToolbar =
            Toolbar.IsVisible;


        _settings.ShowStatusBar =
            StatusBar.IsVisible;


        _settings.ShowLineNumbers =
            Editor.ShowLineNumbers;


        _settings.WordWrap =
            Editor.WordWrap;


        _settings.FontFamily =
            Editor.FontFamily?.Name ??
            "Inter";


        _settings.FontSize =
            Editor.FontSize;


        _settings.WindowWidth =
            Width;


        _settings.WindowHeight =
            Height;


        _settings.WindowX =
            Position.X;


        _settings.WindowY =
            Position.Y;


        _settingsService.Save(
            _settings);
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


    // ============================================================
    // HELP
    // ============================================================

    private async void About_Click(
        object? sender,
        RoutedEventArgs e)
    {
        var dialog =
            new AboutDialog();


        await dialog.ShowDialog(
            this);
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
                bool stillExists =
                    false;


                foreach (var existingDocument
                         in _documentManager.Documents)
                {
                    if (existingDocument ==
                        previousDocument)
                    {
                        stillExists =
                            true;

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
                        Title =
                            "Save File",


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


        _syntaxHighlightingService.ApplyForFile(
            filePath);


        RefreshMarkdownView();


        AddRecentFile(
            filePath);


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

        SaveEditorSelection();

        UpdateStatusBar();
    }


    // ============================================================
    // EDITOR SELECTION
    // ============================================================

    private void Editor_CaretPositionChanged(
        object? sender,
        EventArgs e)
    {
        if (Editor.TextArea.IsFocused)
        {
            _lastKnownCaretOffset =
                Editor.CaretOffset;


            if (Editor.SelectionLength > 0)
            {
                SaveEditorSelection();
            }
        }


        UpdateStatusBar();
    }


    private void Editor_SelectionChanged(
        object? sender,
        EventArgs e)
    {
        if (!Editor.TextArea.IsFocused)
            return;


        SaveEditorSelection();
    }


    private void SaveEditorSelection()
    {
        _lastKnownSelectionStart =
            Editor.SelectionStart;


        _lastKnownSelectionLength =
            Editor.SelectionLength;


        _lastKnownCaretOffset =
            Editor.CaretOffset;
    }


    private void RestoreEditorSelection()
    {
        Editor.TextArea.Focus();


        int textLength =
            (Editor.Text ?? string.Empty).Length;


        if (_lastKnownSelectionLength > 0)
        {
            int start =
                Math.Clamp(
                    _lastKnownSelectionStart,
                    0,
                    textLength);


            int length =
                Math.Clamp(
                    _lastKnownSelectionLength,
                    0,
                    textLength - start);


            Editor.Select(
                start,
                length);


            return;
        }


        Editor.CaretOffset =
            Math.Clamp(
                _lastKnownCaretOffset,
                0,
                textLength);
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


        Editor.TextArea.Focus();
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
        string searchText =
            SearchBox.Text ?? string.Empty;


        if (string.IsNullOrEmpty(
                searchText))
        {
            return;
        }


        string replacementText =
            ReplaceBox.Text ?? string.Empty;


        string selectedText =
            Editor.SelectedText ?? string.Empty;


        bool selectionMatchesSearch =
            Editor.SelectionLength > 0 &&
            string.Equals(
                selectedText,
                searchText,
                StringComparison.OrdinalIgnoreCase);


        if (selectionMatchesSearch)
        {
            int start =
                Editor.SelectionStart;


            Editor.Document.Replace(
                start,
                Editor.SelectionLength,
                replacementText);


            Editor.CaretOffset =
                start + replacementText.Length;


            SaveEditorSelection();
        }


        FindNext();


        UpdateStatusBar();
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


        string replacementText =
            ReplaceBox.Text ?? string.Empty;


        string text =
            Editor.Text ?? string.Empty;


        var matchOffsets =
            new List<int>();


        int searchIndex = 0;


        while (true)
        {
            int index =
                text.IndexOf(
                    searchText,
                    searchIndex,
                    StringComparison.OrdinalIgnoreCase);


            if (index < 0)
                break;


            matchOffsets.Add(
                index);


            searchIndex =
                index + searchText.Length;
        }


        if (matchOffsets.Count == 0)
            return;


        var document =
            Editor.Document;


        document.UndoStack.StartUndoGroup();


        try
        {
            for (int i =
                     matchOffsets.Count - 1;
                 i >= 0;
                 i--)
            {
                document.Replace(
                    matchOffsets[i],
                    searchText.Length,
                    replacementText);
            }
        }
        finally
        {
            document.UndoStack.EndUndoGroup();
        }


        Editor.CaretOffset =
            Math.Min(
                matchOffsets[0] +
                replacementText.Length,
                Editor.Text?.Length ?? 0);


        SaveEditorSelection();


        UpdateStatusBar();
    }


    private void SearchBox_KeyDown(
        object? sender,
        KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;


        if (e.KeyModifiers.HasFlag(
                KeyModifiers.Shift))
        {
            FindPrevious();
        }
        else
        {
            FindNext();
        }


        e.Handled = true;
    }


    private void ReplaceBox_KeyDown(
        object? sender,
        KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;


        Replace_Click(
            sender,
            new RoutedEventArgs());


        e.Handled = true;
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
            Editor.SelectionStart +
            Editor.SelectionLength;


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


        Editor.Select(
            index,
            searchText.Length);


        SaveEditorSelection();


        Editor.TextArea.Focus();
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


        Editor.Select(
            index,
            searchText.Length);


        SaveEditorSelection();


        Editor.TextArea.Focus();
    }


    // ============================================================
    // EDITOR TEXT
    // ============================================================

    private void Editor_TextChanged(
        object? sender,
        EventArgs e)
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


        var caret =
            Editor.TextArea.Caret;


        int line =
            caret.Line;


        int column =
            caret.Column;


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


    // ============================================================
    // WINDOW LIFETIME
    // ============================================================

    protected override void OnClosed(
        EventArgs e)
    {
        SaveCurrentSettings();


        _syntaxHighlightingService.Dispose();


        base.OnClosed(e);
    }


    private enum CloseDocumentResult
    {
        Save,
        DontSave,
        Cancel
    }
}