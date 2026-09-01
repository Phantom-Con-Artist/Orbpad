using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Orb.Engine.Graph;
using Orbpad.Models;
using Orbpad.Orbis.ViewModels;
using Orbpad.Orbis.Views;

namespace Orbpad;

public partial class MainWindow
{
    // ============================================================
    // LORE EDITOR STATE
    // ============================================================

    private LoreEditorView? _orbisLoreEditor;

    private LoreEditorViewModel? _orbisLoreEditorViewModel;

    private string? _currentLoreFilePath;

    private bool _loreEditorIsDirty;

    private readonly HashSet<Document> _loreDocuments = new();


    // ============================================================
    // LORE DOCUMENT DETECTION
    // ============================================================

    private bool IsLoreDocument(
        Document document)
    {
        if (_loreDocuments.Contains(document))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(document.FilePath)
            && string.Equals(
                Path.GetExtension(document.FilePath),
                ".lore",
                StringComparison.OrdinalIgnoreCase);
    }


    // ============================================================
    // NEW LORE
    // ============================================================

    private async void NewLore_Click(
        object? sender,
        RoutedEventArgs e)
    {
        await CreateNewLoreAsync();
    }


    private async Task CreateNewLoreAsync()
    {
        if (_orbisDocumentService is null)
        {
            InitializeOrbis();
        }

        // --------------------------------------------------------
        // Create a completely new graph.
        // Nothing is written to disk yet.
        // --------------------------------------------------------

        var graph =
            new OrbGraph();

        var document =
            _documentManager.CreateDocument();

        _loreDocuments.Add(document);

        document.Text =
            string.Empty;

        document.MarkAsSaved();

        _documentManager.SetActiveDocument(
            document);

        ShowLoreEditor(
            graph,
            null);

        _loreEditorIsDirty =
            false;

        RefreshTabs();
        UpdateWindowTitle();
        UpdateStatusBar();
    }


    // ============================================================
    // SHOW LORE EDITOR
    // ============================================================

    private void ShowLoreEditor(
        OrbGraph graph,
        string? filePath)
    {
        if (_orbisDocumentService is null)
        {
            InitializeOrbis();
        }

        // --------------------------------------------------------
        // If this is the Lore already being edited, simply reveal the
        // existing editor. This preserves unsaved in-memory changes
        // while the user switches to another tab.
        // --------------------------------------------------------

        if (_orbisLoreEditor is not null
            && _orbisLoreEditorViewModel is not null
            && string.Equals(
                _currentLoreFilePath,
                filePath,
                StringComparison.OrdinalIgnoreCase))
        {
            _orbisLoreEditor.IsVisible = true;

            Editor.IsVisible = false;
            OrbisEntityEditor.IsVisible = false;
            MarkdownSplitter.IsVisible = false;
            MarkdownPreview.IsVisible = false;

            return;
        }

        // --------------------------------------------------------
        // Remove an existing Lore Editor instance when opening a
        // different Lore document.
        // --------------------------------------------------------

        if (_orbisLoreEditor is not null)
        {
            EditorWorkspace.Children.Remove(
                _orbisLoreEditor);
        }

        // --------------------------------------------------------
        // Create the ViewModel around the REAL OrbGraph.
        // --------------------------------------------------------

        _orbisLoreEditorViewModel =
            new LoreEditorViewModel(
                graph);

        SetLoreTitleFromPath(
            _orbisLoreEditorViewModel,
            filePath);

        _orbisLoreEditor =
            new LoreEditorView
            {
                DataContext =
                    _orbisLoreEditorViewModel,

                IsVisible =
                    true,

                HorizontalAlignment =
                    Avalonia.Layout.HorizontalAlignment.Stretch,

                VerticalAlignment =
                    Avalonia.Layout.VerticalAlignment.Stretch
            };

        Grid.SetColumn(
            _orbisLoreEditor,
            0);

        Grid.SetColumnSpan(
            _orbisLoreEditor,
            3);


        // --------------------------------------------------------
        // Subscribe to Lore Editor events.
        // --------------------------------------------------------

        _orbisLoreEditorViewModel.SaveRequested +=
            LoreEditor_SaveRequested;

        _orbisLoreEditorViewModel.SaveAsRequested +=
            LoreEditor_SaveAsRequested;

        _orbisLoreEditorViewModel.CloseRequested +=
            LoreEditor_CloseRequested;


        // --------------------------------------------------------
        // Add the Lore Editor to the existing workspace.
        // --------------------------------------------------------

        EditorWorkspace.Children.Add(
            _orbisLoreEditor);


        // --------------------------------------------------------
        // Hide normal editor views.
        // --------------------------------------------------------

        Editor.IsVisible =
            false;

        OrbisEntityEditor.IsVisible =
            false;

        MarkdownSplitter.IsVisible =
            false;

        MarkdownPreview.IsVisible =
            false;


        // --------------------------------------------------------
        // Store Lore state.
        // --------------------------------------------------------

        _currentLoreFilePath =
            filePath;

        _loreEditorIsDirty =
            false;
    }


    // ============================================================
    // HIDE LORE EDITOR
    // ============================================================

    private void HideLoreEditor()
    {
        if (_orbisLoreEditor is not null)
        {
            _orbisLoreEditor.IsVisible = false;
        }
    }


    // ============================================================
    // OPEN LORE
    // ============================================================

    private async void OpenLore_Click(
        object? sender,
        RoutedEventArgs e)
    {
        await OpenLoreAsync();
    }


    private async Task OpenLoreAsync()
    {
        if (_orbisDocumentService is null)
        {
            InitializeOrbis();
        }


        var files =
            await StorageProvider
                .OpenFilePickerAsync(
                    new FilePickerOpenOptions
                    {
                        Title =
                            "Open Orbis Lore",

                        AllowMultiple =
                            false,

                        FileTypeFilter =
                        [
                            new FilePickerFileType(
                                "Orbis Lore")
                            {
                                Patterns =
                                [
                                    "*.lore"
                                ]
                            }
                        ]
                    });


        if (files.Count == 0)
        {
            return;
        }


        var file =
            files[0];


        string? path =
            file.TryGetLocalPath();


        if (string.IsNullOrWhiteSpace(path))
        {
            await ShowOrbisErrorAsync(
                "Orbpad could not access the selected Lore file.");

            return;
        }


        try
        {
            // ----------------------------------------------------
            // Reuse an already-open Lore document.
            // ----------------------------------------------------

            foreach (
                var existingDocument
                in _documentManager.Documents)
            {
                if (string.Equals(
                        existingDocument.FilePath,
                        path,
                        StringComparison.OrdinalIgnoreCase))
                {
                    _documentManager.SetActiveDocument(
                        existingDocument);

                    await LoadLoreDocumentAsync(
                        existingDocument);

                    RefreshTabs();
                    UpdateWindowTitle();
                    UpdateStatusBar();

                    return;
                }
            }


            // ----------------------------------------------------
            // Load the actual OrbGraph from disk.
            // ----------------------------------------------------

            OrbGraph graph =
                await _orbisDocumentService!
                    .LoadLoreAsync(
                        path);


            // ----------------------------------------------------
            // Register the Lore in the document manager.
            // ----------------------------------------------------

            var document =
                _documentManager.CreateDocument();

            _loreDocuments.Add(document);

            document.FilePath =
                path;

            document.Text =
                string.Empty;

            document.MarkAsSaved();

            _documentManager.SetActiveDocument(
                document);


            ShowLoreEditor(
                graph,
                path);


            RefreshTabs();
            UpdateWindowTitle();
            UpdateStatusBar();

            AddRecentFile(
                path);
        }
        catch (Exception ex)
        {
            await ShowOrbisErrorAsync(
                "Orbpad could not open the Lore.",
                ex.Message);
        }
    }


    // ============================================================
    // LOAD LORE DOCUMENT
    // ============================================================

    private async Task LoadLoreDocumentAsync(
        Document document)
    {
        if (string.IsNullOrWhiteSpace(
                document.FilePath))
        {
            return;
        }

        // When returning to the Lore tab, keep the live graph in memory.
        // This preserves unsaved edits made before switching to another tab.
        if (_orbisLoreEditorViewModel is not null
            && string.Equals(
                _currentLoreFilePath,
                document.FilePath,
                StringComparison.OrdinalIgnoreCase))
        {
            ShowLoreEditor(
                _orbisLoreEditorViewModel.Graph,
                document.FilePath);

            return;
        }


        try
        {
            if (_orbisDocumentService is null)
            {
                InitializeOrbis();
            }


            OrbGraph graph =
                await _orbisDocumentService!
                    .LoadLoreAsync(
                        document.FilePath);


            ShowLoreEditor(
                graph,
                document.FilePath);
        }
        catch (Exception ex)
        {
            await ShowOrbisErrorAsync(
                "Orbpad could not load the Lore.",
                ex.Message);
        }
    }


    // ============================================================
    // LORE SAVE
    // ============================================================

    private async void LoreEditor_SaveRequested(
        object? sender,
        EventArgs e)
    {
        await SaveLoreAsync();
    }


    private async Task<bool> SaveLoreAsync()
    {
        if (_orbisLoreEditorViewModel is null)
        {
            return false;
        }


        if (_orbisDocumentService is null)
        {
            InitializeOrbis();
        }


        OrbGraph graph =
            _orbisLoreEditorViewModel.Graph;


        // --------------------------------------------------------
        // Prevent completely empty Lore documents.
        // --------------------------------------------------------

        if (graph.Entities.Count == 0
            && graph.Relationships.Count == 0)
        {
            await ShowOrbisErrorAsync(
                "Empty Lore",
                "A Lore document must contain at least one entity or relationship.");

            return false;
        }


        string? oldPath =
            _currentLoreFilePath;


        // --------------------------------------------------------
        // New Lore has no path yet.
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(oldPath))
        {
            string? targetPath =
                await ChooseLoreSavePathAsync();

            if (string.IsNullOrWhiteSpace(
                    targetPath))
            {
                return false;
            }


            targetPath =
                EnsureLoreExtension(
                    targetPath);


            if (File.Exists(targetPath))
            {
                await ShowOrbisErrorAsync(
                    "Orbpad could not save the Lore.",
                    $"A Lore file named '{Path.GetFileName(targetPath)}' already exists.");

                return false;
            }


            try
            {
                await _orbisDocumentService!
                    .SaveLoreAsync(
                        targetPath,
                        graph);


                _currentLoreFilePath =
                    targetPath;


                _orbisLoreEditorViewModel
                    .MarkAsSaved();


                SyncLoreDocument(
                    oldPath,
                    targetPath);


                AddRecentFile(
                    targetPath);


                RefreshTabs();
                UpdateWindowTitle();
                UpdateStatusBar();


                return true;
            }
            catch (Exception ex)
            {
                await ShowOrbisErrorAsync(
                    "Orbpad could not save the Lore.",
                    ex.Message);

                return false;
            }
        }


        // --------------------------------------------------------
        // Existing Lore.
        // --------------------------------------------------------

        try
        {
            await _orbisDocumentService!
                .SaveLoreAsync(
                    oldPath,
                    graph);


            _orbisLoreEditorViewModel
                .MarkAsSaved();


            SyncLoreDocument(
                oldPath,
                oldPath);


            RefreshTabs();
            UpdateWindowTitle();
            UpdateStatusBar();


            return true;
        }
        catch (Exception ex)
        {
            await ShowOrbisErrorAsync(
                "Orbpad could not save the Lore.",
                ex.Message);

            return false;
        }
    }


    // ============================================================
    // LORE SAVE AS
    // ============================================================

    private async void LoreEditor_SaveAsRequested(
        object? sender,
        EventArgs e)
    {
        await SaveLoreAsAsync();
    }


    private async Task<bool> SaveLoreAsAsync()
    {
        if (_orbisLoreEditorViewModel is null)
        {
            return false;
        }


        if (_orbisDocumentService is null)
        {
            InitializeOrbis();
        }


        OrbGraph graph =
            _orbisLoreEditorViewModel.Graph;


        if (graph.Entities.Count == 0
            && graph.Relationships.Count == 0)
        {
            await ShowOrbisErrorAsync(
                "Empty Lore",
                "A Lore document must contain at least one entity or relationship.");

            return false;
        }


        string? targetPath =
            await ChooseLoreSavePathAsync();


        if (string.IsNullOrWhiteSpace(
                targetPath))
        {
            return false;
        }


        targetPath =
            EnsureLoreExtension(
                targetPath);


        string? oldPath =
            _currentLoreFilePath;


        if (!string.IsNullOrWhiteSpace(oldPath)
            && string.Equals(
                oldPath,
                targetPath,
                StringComparison.OrdinalIgnoreCase))
        {
            return await SaveLoreAsync();
        }


        if (File.Exists(targetPath))
        {
            await ShowOrbisErrorAsync(
                "Orbpad could not save the Lore copy.",
                $"A Lore file named '{Path.GetFileName(targetPath)}' already exists.");

            return false;
        }


        try
        {
            await _orbisDocumentService!
                .SaveLoreAsync(
                    targetPath,
                    graph);


            _currentLoreFilePath =
                targetPath;


            _orbisLoreEditorViewModel
                .MarkAsSaved();


            SyncLoreDocument(
                oldPath,
                targetPath);


            AddRecentFile(
                targetPath);


            RefreshTabs();
            UpdateWindowTitle();
            UpdateStatusBar();


            return true;
        }
        catch (Exception ex)
        {
            await ShowOrbisErrorAsync(
                "Orbpad could not save the Lore copy.",
                ex.Message);

            return false;
        }
    }


    // ============================================================
    // LORE TITLE
    // ============================================================

    private static void SetLoreTitleFromPath(
        LoreEditorViewModel viewModel,
        string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            viewModel.Title = "Untitled Lore";
            return;
        }

        string name =
            Path.GetFileNameWithoutExtension(filePath);

        viewModel.Title =
            string.IsNullOrWhiteSpace(name)
                ? "Untitled Lore"
                : name;
    }


    private string GetSuggestedLoreFileName()
    {
        if (_orbisLoreEditorViewModel is null)
            return "Untitled.lore";

        string title =
            _orbisLoreEditorViewModel.Title;

        if (string.IsNullOrWhiteSpace(title)
            || title.Equals(
                "Untitled Lore",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Untitled.lore";
        }

        foreach (char invalid in Path.GetInvalidFileNameChars())
        {
            title = title.Replace(invalid, '_');
        }

        return title + ".lore";
    }


    // ============================================================
    // LORE SAVE PATH
    // ============================================================

    private async Task<string?>
        ChooseLoreSavePathAsync()
    {
        var file =
            await StorageProvider
                .SaveFilePickerAsync(
                    new FilePickerSaveOptions
                    {
                        Title =
                            "Save Orbis Lore",

                        SuggestedFileName =
                            GetSuggestedLoreFileName(),

                        DefaultExtension =
                            "lore",

                        FileTypeChoices =
                        [
                            new FilePickerFileType(
                                "Orbis Lore")
                            {
                                Patterns =
                                [
                                    "*.lore"
                                ]
                            }
                        ]
                    });


        return file?.TryGetLocalPath();
    }


    // ============================================================
    // LORE FILE EXTENSION
    // ============================================================

    private static string EnsureLoreExtension(
        string path)
    {
        if (path.EndsWith(
                ".lore",
                StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }


        return path + ".lore";
    }


    // ============================================================
    // DOCUMENT/TAB SYNCHRONIZATION
    // ============================================================

    private void SyncLoreDocument(
        string? oldPath,
        string targetPath)
    {
        Document? loreDocument =
            null;


        foreach (
            var document
            in _documentManager.Documents)
        {
            if ((!string.IsNullOrWhiteSpace(oldPath)
                    && string.Equals(
                        document.FilePath,
                        oldPath,
                        StringComparison.OrdinalIgnoreCase))
                || (document ==
                    _documentManager.ActiveDocument))
            {
                loreDocument =
                    document;

                break;
            }
        }


        if (loreDocument is null)
        {
            loreDocument =
                _documentManager.CreateDocument();
        }

        _loreDocuments.Add(loreDocument);


        loreDocument.FilePath =
            targetPath;


        // Keep raw text empty because the Lore Editor owns the
        // actual graph. This prevents the normal text editor from
        // accidentally becoming the source of truth.
        loreDocument.Text =
            string.Empty;


        loreDocument.MarkAsSaved();


        _documentManager.SetActiveDocument(
            loreDocument);
    }


    // ============================================================
    // CLOSE LORE EDITOR
    // ============================================================

    private async void LoreEditor_CloseRequested(
        object? sender,
        EventArgs e)
    {
        await CloseLoreEditorAsync();
    }


    private async Task CloseLoreEditorAsync()
    {
        if (_orbisLoreEditorViewModel is not null
            && _orbisLoreEditorViewModel.IsDirty)
        {
            bool save =
                await ConfirmLoreSaveAsync();


            if (save)
            {
                bool saved =
                    await SaveLoreAsync();


                if (!saved)
                {
                    return;
                }
            }
        }


        // --------------------------------------------------------
        // Find the document belonging to this Lore.
        // --------------------------------------------------------

        Document? loreDocument =
            null;


        foreach (
            var document
            in _documentManager.Documents)
        {
            if (!string.IsNullOrWhiteSpace(
                    _currentLoreFilePath)
                && string.Equals(
                    document.FilePath,
                    _currentLoreFilePath,
                    StringComparison.OrdinalIgnoreCase))
            {
                loreDocument =
                    document;

                break;
            }


            if (string.IsNullOrWhiteSpace(
                    _currentLoreFilePath)
                && document ==
                   _documentManager.ActiveDocument)
            {
                loreDocument =
                    document;

                break;
            }
        }


        // --------------------------------------------------------
        // Remove the Lore editor.
        // --------------------------------------------------------

        if (_orbisLoreEditor is not null)
        {
            _orbisLoreEditorViewModel!
                .SaveRequested -=
                LoreEditor_SaveRequested;

            _orbisLoreEditorViewModel!
                .SaveAsRequested -=
                LoreEditor_SaveAsRequested;

            _orbisLoreEditorViewModel!
                .CloseRequested -=
                LoreEditor_CloseRequested;


            EditorWorkspace.Children.Remove(
                _orbisLoreEditor);
        }


        _orbisLoreEditor =
            null;


        _orbisLoreEditorViewModel =
            null;


        _currentLoreFilePath =
            null;


        _loreEditorIsDirty =
            false;


        // --------------------------------------------------------
        // Remove Lore document/tab.
        // --------------------------------------------------------

        if (loreDocument is not null)
        {
            bool wasActive =
                _documentManager.ActiveDocument ==
                loreDocument;


            _documentManager.RemoveDocument(
                loreDocument);

            _loreDocuments.Remove(loreDocument);


            if (wasActive)
            {
                var activeDocument =
                    _documentManager.ActiveDocument;


                if (activeDocument is not null)
                {
                    LoadDocumentIntoEditor(
                        activeDocument);
                }
                else
                {
                    ShowNormalEditor();
                }
            }
        }
        else
        {
            ShowNormalEditor();
        }


        RefreshTabs();
        UpdateWindowTitle();
        UpdateStatusBar();
    }


    // ============================================================
    // LORE SAVE CONFIRMATION
    // ============================================================

    private async Task<bool>
        ConfirmLoreSaveAsync()
    {
        var dialog =
            new Window
            {
                Title =
                    "Unsaved Lore",

                Width =
                    420,

                Height =
                    220,

                WindowStartupLocation =
                    WindowStartupLocation.CenterOwner
            };


        var panel =
            new StackPanel
            {
                Margin =
                    new Avalonia.Thickness(
                        24),

                Spacing =
                    16
            };


        panel.Children.Add(
            new TextBlock
            {
                Text =
                    "This Lore has unsaved changes.",

                FontSize =
                    18
            });


        panel.Children.Add(
            new TextBlock
            {
                Text =
                    "Would you like to save them before closing?",

                TextWrapping =
                    Avalonia.Media.TextWrapping.Wrap
            });


        var buttons =
            new StackPanel
            {
                Orientation =
                    Avalonia.Layout.Orientation.Horizontal,

                HorizontalAlignment =
                    Avalonia.Layout.HorizontalAlignment.Right,

                Spacing =
                    8
            };


        var saveButton =
            new Button
            {
                Content =
                    "Save",

                Padding =
                    new Avalonia.Thickness(
                        16,
                        8)
            };


        var discardButton =
            new Button
            {
                Content =
                    "Don't Save",

                Padding =
                    new Avalonia.Thickness(
                        16,
                        8)
            };


        var cancelButton =
            new Button
            {
                Content =
                    "Cancel",

                Padding =
                    new Avalonia.Thickness(
                        16,
                        8)
            };


        bool? result =
            null;


        saveButton.Click +=
            (_, _) =>
            {
                result =
                    true;

                dialog.Close();
            };


        discardButton.Click +=
            (_, _) =>
            {
                result =
                    false;

                dialog.Close();
            };


        cancelButton.Click +=
            (_, _) =>
            {
                result =
                    null;

                dialog.Close();
            };


        buttons.Children.Add(
            saveButton);

        buttons.Children.Add(
            discardButton);

        buttons.Children.Add(
            cancelButton);


        panel.Children.Add(
            buttons);


        dialog.Content =
            panel;


        await dialog.ShowDialog(
            this);


        return result == true;
    }
}
