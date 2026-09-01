using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Orb.Engine.Graph;
using Orb.Engine.Serialization;
using Orbpad.Models;
using Orbpad.Orbis.Services;
using Orbpad.Orbis.ViewModels;

namespace Orbpad;

public partial class MainWindow
{
    private readonly OrbEngineService _orbEngineService =
        new();

    private OrbisDocumentService? _orbisDocumentService;

    private string? _currentEntityFilePath;

    private bool _entityEditorIsDirty;


    // ============================================================
    // ORBIS INITIALIZATION
    // ============================================================

    private void InitializeOrbis()
    {
        _orbisDocumentService =
            new OrbisDocumentService(
                _orbEngineService);

        _currentEntityFilePath = null;
        _entityEditorIsDirty = false;

        // --------------------------------------------------------
        // ENTITY EDITOR EVENTS
        // --------------------------------------------------------
        //
        // Remove first so repeated initialization never creates
        // duplicate event subscriptions.
        // --------------------------------------------------------

        OrbisEntityEditor.SaveRequested -=
            EntityEditor_SaveRequested;

        OrbisEntityEditor.SaveRequested +=
            EntityEditor_SaveRequested;

        OrbisEntityEditor.OpenRequested -=
            EntityEditor_OpenRequested;

        OrbisEntityEditor.OpenRequested +=
            EntityEditor_OpenRequested;

        OrbisEntityEditor.CloseRequested -=
            EntityEditor_CloseRequested;

        OrbisEntityEditor.CloseRequested +=
            EntityEditor_CloseRequested;
    }


    // ============================================================
    // NEW ENTITY
    // ============================================================

    private void NewEntity_Click(
        object? sender,
        RoutedEventArgs e)
    {
        CreateNewEntity();
    }


    private void CreateNewEntity()
    {
        var entity =
            new OrbEntity
            {
                Name = "New Entity",
                Type = "Entity"
            };

        var graph =
            new OrbGraph();

        graph.AddEntity(entity);

        // --------------------------------------------------------
        // Create the corresponding document/tab immediately.
        // The file path is assigned when the entity is saved.
        // --------------------------------------------------------

        var document =
            _documentManager.CreateDocument();

        document.Text =
            string.Empty;

        document.MarkAsSaved();

        _documentManager.SetActiveDocument(
            document);

        ShowEntityEditor(
            entity,
            graph,
            null);

        _entityEditorIsDirty = true;

        RefreshTabs();
        UpdateWindowTitle();
        UpdateStatusBar();
    }


    // ============================================================
    // SHOW ENTITY EDITOR
    // ============================================================

    private void ShowEntityEditor(
        OrbEntity entity,
        OrbGraph graph,
        string? filePath)
    {
        if (_orbisDocumentService is null)
        {
            InitializeOrbis();
        }

        _orbisEntityEditorViewModel =
            new EntityEditorViewModel(
                entity,
                graph,
                _orbisDocumentService);

        OrbisEntityEditor.DataContext =
            _orbisEntityEditorViewModel;

        _currentEntityFilePath =
            filePath;

        _entityEditorIsDirty =
            false;

        Editor.IsVisible =
            false;

        OrbisEntityEditor.IsVisible =
            true;

        MarkdownSplitter.IsVisible =
            false;

        MarkdownPreview.IsVisible =
            false;
    }


    // ============================================================
    // OPEN ENTITY
    // ============================================================

    private async Task OpenEntityAsync()
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
                            "Open Orbis Entity",

                        AllowMultiple =
                            false,

                        FileTypeFilter =
                        [
                            new FilePickerFileType(
                                "Orbis Entity")
                            {
                                Patterns =
                                [
                                    "*.entity"
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
                "Orbpad could not access the selected entity file.");

            return;
        }

        try
        {
            // ----------------------------------------------------
            // Reuse an already-open entity document.
            // ----------------------------------------------------

            foreach (var existingDocument
                     in _documentManager.Documents)
            {
                if (string.Equals(
                        existingDocument.FilePath,
                        path,
                        StringComparison.OrdinalIgnoreCase))
                {
                    _documentManager.SetActiveDocument(
                        existingDocument);

                    LoadDocumentIntoEditor(
                        existingDocument);

                    RefreshTabs();
                    UpdateWindowTitle();
                    UpdateStatusBar();

                    return;
                }
            }

            // ----------------------------------------------------
            // Load the EXISTING entity from disk.
            // This preserves its original ID.
            // ----------------------------------------------------

            OrbEntity entity =
                await _orbisDocumentService!
                    .LoadEntityAsync(path);

            var graph =
                new OrbGraph();

            graph.AddEntity(entity);

            // ----------------------------------------------------
            // Register the entity in the main document/tab system.
            // Entity data is owned by OrbEntity, not Document.Text.
            // ----------------------------------------------------

            var document =
                _documentManager.CreateDocument();

            document.FilePath =
                path;

            document.Text =
                string.Empty;

            document.MarkAsSaved();

            _documentManager.SetActiveDocument(
                document);

            ShowEntityEditor(
                entity,
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
                "Orbpad could not open the entity.",
                ex.Message);
        }
    }


    // ============================================================
    // SAVE ENTITY AS
    // ============================================================

    private async Task<bool> SaveEntityAsAsync(
        Document document)
    {
        if (_orbisEntityEditorViewModel is null)
        {
            return false;
        }

        if (_orbisDocumentService is null)
        {
            InitializeOrbis();
        }

        OrbEntity sourceEntity =
            _orbisEntityEditorViewModel.GetEntity();

        string? oldPath =
            _currentEntityFilePath;

        string? targetPath =
            await ChooseEntitySavePathAsync(
                sourceEntity.Name);

        if (string.IsNullOrWhiteSpace(targetPath))
        {
            return false;
        }

        targetPath =
            EnsureEntityExtension(targetPath);

        if (!string.IsNullOrWhiteSpace(oldPath)
            && string.Equals(
                oldPath,
                targetPath,
                StringComparison.OrdinalIgnoreCase))
        {
            await ShowOrbisErrorAsync(
                "Save As requires a different file.",
                "Choose a different file name so the original entity remains unchanged.");

            return false;
        }

        if (File.Exists(targetPath))
        {
            await ShowOrbisErrorAsync(
                "Orbpad could not save the entity copy.",
                $"An entity named '{Path.GetFileName(targetPath)}' already exists.");

            return false;
        }

        try
        {
            var copiedEntity =
                CloneEntityForSaveAs(
                    sourceEntity,
                    Path.GetFileNameWithoutExtension(targetPath));

            var copiedGraph =
                new OrbGraph();

            copiedGraph.AddEntity(copiedEntity);

            await _orbisDocumentService!
                .SaveEntityAsync(
                    targetPath,
                    copiedEntity);

            document.FilePath =
                targetPath;

            document.Text =
                string.Empty;

            document.MarkAsSaved();

            _documentManager.SetActiveDocument(
                document);

            ShowEntityEditor(
                copiedEntity,
                copiedGraph,
                targetPath);

            _entityEditorIsDirty =
                false;

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
                "Orbpad could not save the entity copy.",
                ex.Message);

            return false;
        }
    }


    private static OrbEntity CloneEntityForSaveAs(
        OrbEntity source,
        string targetName)
    {
        // --------------------------------------------------------
        // Clone through the Orbis serializer so nested property
        // values are copied rather than sharing the original
        // property/value objects in memory.
        // --------------------------------------------------------

        string serialized =
            EntitySerializer.Serialize(source);

        var clone =
            EntitySerializer.Deserialize(serialized);

        // --------------------------------------------------------
        // Save As creates a NEW entity identity.
        // The chosen filename becomes the new entity name.
        // --------------------------------------------------------

        clone.Id =
            Guid.NewGuid();

        clone.Name =
            string.IsNullOrWhiteSpace(targetName)
                ? source.Name
                : targetName;

        return clone;
    }


    // ============================================================
    // SAVE ENTITY
    // ============================================================

    private async Task SaveEntityAsync()
    {
        if (_orbisEntityEditorViewModel is null)
        {
            return;
        }

        if (_orbisDocumentService is null)
        {
            InitializeOrbis();
        }

        OrbEntity entity =
            _orbisEntityEditorViewModel
                .GetEntity();

        string? oldPath =
            _currentEntityFilePath;

        string? targetPath;

        // ========================================================
        // NEW ENTITY
        // ========================================================

        if (string.IsNullOrWhiteSpace(oldPath))
        {
            targetPath =
                await ChooseEntitySavePathAsync(
                    entity.Name);

            if (string.IsNullOrWhiteSpace(targetPath))
            {
                return;
            }
        }
        else
        {
            // ====================================================
            // EXISTING ENTITY
            // ====================================================
            //
            // The entity keeps the same ID.
            //
            // Only the file name follows the entity name.
            //
            // Example:
            //
            // Test.entity
            // Name = Amon
            //
            // becomes:
            //
            // Amon.entity
            // ====================================================

            string directory =
                Path.GetDirectoryName(oldPath)
                ?? Environment.CurrentDirectory;

            string safeName =
                SanitizeFileName(
                    string.IsNullOrWhiteSpace(entity.Name)
                        ? "New Entity"
                        : entity.Name);

            targetPath =
                Path.Combine(
                    directory,
                    safeName + ".entity");
        }

        targetPath =
            EnsureEntityExtension(
                targetPath);

        try
        {
            // ====================================================
            // SAVE THE CURRENT ENTITY
            // ====================================================
            //
            // This is the SAME OrbEntity instance that was loaded
            // or originally created.
            //
            // Therefore its ID remains unchanged.
            // ====================================================

            if (!string.Equals(
                    oldPath,
                    targetPath,
                    StringComparison.OrdinalIgnoreCase)
                && File.Exists(targetPath))
            {
                await ShowOrbisErrorAsync(
                    "Orbpad could not rename the entity.",
                    $"An entity named '{Path.GetFileName(targetPath)}' already exists.");

                return;
            }

            await _orbisDocumentService!
                .SaveEntityAsync(
                    targetPath,
                    entity);

            // ====================================================
            // REMOVE OLD FILE AFTER SUCCESSFUL SAVE
            // ====================================================
            //
            // Only do this when:
            //
            // 1. There was an old file.
            // 2. The new path is different.
            // 3. The new file successfully saved.
            //
            // This prevents losing Test.entity if saving Amon.entity
            // fails.
            // ====================================================

            if (!string.IsNullOrWhiteSpace(oldPath)
                && !string.Equals(
                    oldPath,
                    targetPath,
                    StringComparison.OrdinalIgnoreCase)
                && File.Exists(oldPath))
            {
                File.Delete(oldPath);
            }

            // ====================================================
            // UPDATE CURRENT FILE PATH
            // ====================================================

            _currentEntityFilePath =
                targetPath;

            _entityEditorIsDirty =
                false;

            // ----------------------------------------------------
            // Keep the corresponding Orbpad document/tab in sync.
            // ----------------------------------------------------

            Document? entityDocument =
                null;

            foreach (var document
                     in _documentManager.Documents)
            {
                if (string.Equals(
                        document.FilePath,
                        oldPath,
                        StringComparison.OrdinalIgnoreCase)
                    || (string.IsNullOrWhiteSpace(oldPath)
                        && document ==
                           _documentManager.ActiveDocument))
                {
                    entityDocument =
                        document;

                    break;
                }
            }

            if (entityDocument is null)
            {
                entityDocument =
                    _documentManager.CreateDocument();
            }

            entityDocument.FilePath =
                targetPath;

            entityDocument.Text =
                string.Empty;

            entityDocument.MarkAsSaved();

            _documentManager.SetActiveDocument(
                entityDocument);

            UpdateWindowTitle();
            UpdateStatusBar();
            RefreshTabs();
        }
        catch (Exception ex)
        {
            await ShowOrbisErrorAsync(
                "Orbpad could not save the entity.",
                ex.Message);
        }
    }


    // ============================================================
    // ENTITY SAVE PATH
    // ============================================================

    private async Task<string?> ChooseEntitySavePathAsync(
        string? entityName)
    {
        string safeName =
            SanitizeFileName(
                string.IsNullOrWhiteSpace(entityName)
                    ? "New Entity"
                    : entityName);

        var file =
            await StorageProvider
                .SaveFilePickerAsync(
                    new FilePickerSaveOptions
                    {
                        Title =
                            "Save Orbis Entity",

                        SuggestedFileName =
                            safeName + ".entity",

                        DefaultExtension =
                            "entity",

                        FileTypeChoices =
                        [
                            new FilePickerFileType(
                                "Orbis Entity")
                            {
                                Patterns =
                                [
                                    "*.entity"
                                ]
                            }
                        ]
                    });

        return file?.TryGetLocalPath();
    }


    // ============================================================
    // ENTITY FILE EXTENSION
    // ============================================================

    private static string EnsureEntityExtension(
        string path)
    {
        if (path.EndsWith(
                ".entity",
                StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        return path + ".entity";
    }


    // ============================================================
    // SAFE FILE NAME
    // ============================================================

    private static string SanitizeFileName(
        string value)
    {
        foreach (char invalidCharacter
                 in Path.GetInvalidFileNameChars())
        {
            value =
                value.Replace(
                    invalidCharacter,
                    '_');
        }

        return value.Trim();
    }


    // ============================================================
    // ENTITY EDITOR SAVE BUTTON
    // ============================================================

    private async void EntityEditor_SaveRequested(
        object? sender,
        EventArgs e)
    {
        await SaveEntityAsync();
    }


    // ============================================================
    // ENTITY EDITOR OPEN BUTTON
    // ============================================================

    private async void EntityEditor_OpenRequested(
        object? sender,
        EventArgs e)
    {
        await OpenEntityAsync();
    }


    // ============================================================
    // ENTITY EDITOR CLOSE BUTTON
    // ============================================================

    private async void EntityEditor_CloseRequested(
        object? sender,
        EventArgs e)
    {
        await CloseEntityEditorAsync();
    }


    // ============================================================
    // CLOSE ENTITY EDITOR
    // ============================================================

private async Task CloseEntityEditorAsync()
{
    if (_entityEditorIsDirty)
    {
        bool save =
            await ConfirmEntitySaveAsync();

        if (save)
        {
            await SaveEntityAsync();

            if (_entityEditorIsDirty)
            {
                return;
            }
        }
    }

    // ============================================================
    // FIND THE DOCUMENT REPRESENTING THIS ENTITY
    // ============================================================

    Document? entityDocument = null;

    if (!string.IsNullOrWhiteSpace(
            _currentEntityFilePath))
    {
        foreach (var document
                 in _documentManager.Documents)
        {
            if (string.Equals(
                    document.FilePath,
                    _currentEntityFilePath,
                    StringComparison.OrdinalIgnoreCase))
            {
                entityDocument =
                    document;

                break;
            }
        }
    }
    else
    {
        // A new unsaved entity has no path yet, but it still owns
        // the active document created by CreateNewEntity().
        entityDocument =
            _documentManager.ActiveDocument;
    }

    // ============================================================
    // REMOVE ENTITY DOCUMENT FROM TAB SYSTEM
    // ============================================================

    if (entityDocument is not null)
    {
        bool wasActive =
            _documentManager.ActiveDocument ==
            entityDocument;

        _documentManager.RemoveDocument(
            entityDocument);

        // --------------------------------------------------------
        // If that was the active document, activate another one.
        // --------------------------------------------------------

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
                var newDocument =
                    _documentManager.CreateDocument();

                LoadDocumentIntoEditor(
                    newDocument);
            }
        }
    }
    else
    {
        // --------------------------------------------------------
        // Entity wasn't opened through the document/tab system.
        // Just return to the normal editor.
        // --------------------------------------------------------

        ShowNormalEditor();
    }

    // ============================================================
    // CLEAR ORBIS EDITOR STATE
    // ============================================================

    _orbisEntityEditorViewModel =
        null;

    _currentEntityFilePath =
        null;

    _entityEditorIsDirty =
        false;

    // ============================================================
    // REFRESH UI
    // ============================================================

    RefreshTabs();
    UpdateWindowTitle();
    UpdateStatusBar();
}


    // ============================================================
    // CONFIRM SAVE
    // ============================================================

    private async Task<bool> ConfirmEntitySaveAsync()
    {
        var dialog =
            new Window
            {
                Title =
                    "Unsaved Entity",

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
                    new Avalonia.Thickness(24),

                Spacing =
                    16
            };

        panel.Children.Add(
            new TextBlock
            {
                Text =
                    "This entity has unsaved changes.",

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
                result = true;
                dialog.Close();
            };

        discardButton.Click +=
            (_, _) =>
            {
                result = false;
                dialog.Close();
            };

        cancelButton.Click +=
            (_, _) =>
            {
                result = null;
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

        await dialog.ShowDialog(this);

        return result == true;
    }


    // ============================================================
    // ERROR DIALOG
    // ============================================================

    private async Task ShowOrbisErrorAsync(
        string title,
        string? details = null)
    {
        var dialog =
            new Window
            {
                Title =
                    title,

                Width =
                    500,

                Height =
                    260,

                WindowStartupLocation =
                    WindowStartupLocation.CenterOwner
            };

        var panel =
            new StackPanel
            {
                Margin =
                    new Avalonia.Thickness(24),

                Spacing =
                    16
            };

        panel.Children.Add(
            new TextBlock
            {
                Text =
                    title,

                FontSize =
                    18
            });

        if (!string.IsNullOrWhiteSpace(details))
        {
            panel.Children.Add(
                new TextBlock
                {
                    Text =
                        details,

                    TextWrapping =
                        Avalonia.Media.TextWrapping.Wrap
                });
        }

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
                dialog.Close();
            };

        panel.Children.Add(
            closeButton);

        dialog.Content =
            panel;

        await dialog.ShowDialog(this);
    }
}