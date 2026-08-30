using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Orbpad.Services;

namespace Orbpad.Explorer;

public partial class ExplorerView : UserControl
{
    private readonly FileSystemService
        _fileSystemService;

    private readonly WorkspaceService
        _workspaceService;

    private readonly ObservableCollection<ExplorerItem>
        _rootItems =
            new();

    // ============================================================
    // EVENTS
    // ============================================================

    public event EventHandler<string>?
        FileOpenRequested;


    // ============================================================
    // CONSTRUCTORS
    // ============================================================

    public ExplorerView()
        : this(new WorkspaceService())
    {
    }


    public ExplorerView(
        WorkspaceService workspaceService)
    {
        InitializeComponent();

        _workspaceService =
            workspaceService;

        _fileSystemService =
            new FileSystemService();

        FileTree.ItemsSource =
            _rootItems;
    }


    // ============================================================
    // OPEN FOLDER
    // ============================================================

    private async void OpenFolder_Click(
        object? sender,
        RoutedEventArgs e)
    {
        var topLevel =
            TopLevel.GetTopLevel(this);

        if (topLevel is null)
            return;

        if (!topLevel.StorageProvider.CanPickFolder)
            return;

        var folders =
            await topLevel.StorageProvider
                .OpenFolderPickerAsync(
                    new FolderPickerOpenOptions
                    {
                        Title =
                            "Open Folder",

                        AllowMultiple =
                            false
                    });

        if (folders.Count == 0)
            return;

        string path =
            folders[0]
                .Path
                .LocalPath;

        if (string.IsNullOrWhiteSpace(
                path))
        {
            return;
        }

        SetWorkspace(
            path);
    }


    // ============================================================
    // COLLAPSE
    // ============================================================

    private void Collapse_Click(
        object? sender,
        RoutedEventArgs e)
    {
        var topLevel =
            TopLevel.GetTopLevel(this);

        if (topLevel
            is MainWindow mainWindow)
        {
            mainWindow.CollapseExplorer();
        }
    }


    // ============================================================
    // REFRESH
    // ============================================================

    private void Refresh_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Refresh();
    }


    public void Refresh()
    {
        string? rootPath =
            _workspaceService.RootPath;

        if (string.IsNullOrWhiteSpace(
                rootPath))
        {
            return;
        }

        SetWorkspace(
            rootPath);
    }


    // ============================================================
    // SET WORKSPACE
    // ============================================================

    public void SetWorkspace(
        string rootPath)
    {
        if (!_workspaceService.SetRootPath(
                rootPath))
        {
            return;
        }

        _rootItems.Clear();

        foreach (ExplorerItem item
                 in _fileSystemService
                     .GetRootItems(
                         rootPath))
        {
            AttachItem(
                item);

            _rootItems.Add(
                item);
        }

        WorkspacePathText.Text =
            rootPath;

        EmptyState.IsVisible =
            false;

        FileTree.IsVisible =
            true;
    }


    // ============================================================
    // ITEM EVENT BINDING
    // ============================================================

    private void AttachItem(
        ExplorerItem item)
    {
        item.PropertyChanged +=
            ExplorerItem_PropertyChanged;
    }


    private void ExplorerItem_PropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (e.PropertyName !=
            nameof(
                ExplorerItem.IsExpanded))
        {
            return;
        }

        if (sender is not ExplorerItem item)
            return;

        if (!item.IsExpanded)
            return;

        if (!item.IsDirectory)
            return;

        if (item.IsPlaceholder)
            return;

        if (item.IsLoaded)
            return;

        LoadChildren(
            item);
    }


    // ============================================================
    // LOAD CHILDREN
    // ============================================================

    private void LoadChildren(
        ExplorerItem item)
    {
        _fileSystemService.LoadChildren(
            item);

        foreach (ExplorerItem child
                 in item.Children)
        {
            AttachItem(
                child);
        }
    }


    // ============================================================
    // OPEN FILE
    // ============================================================

    private void FileTree_DoubleTapped(
        object? sender,
        TappedEventArgs e)
    {
        OpenSelectedItem();
    }


    private void FileTree_KeyDown(
        object? sender,
        KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        OpenSelectedItem();

        e.Handled =
            true;
    }


    private void OpenSelectedItem()
    {
        if (FileTree.SelectedItem
            is not ExplorerItem item)
        {
            return;
        }

        if (item.IsPlaceholder)
            return;

        if (item.IsDirectory)
            return;

        if (string.IsNullOrWhiteSpace(
                item.FullPath))
        {
            return;
        }

        if (!File.Exists(
                item.FullPath))
        {
            Refresh();

            return;
        }

        FileOpenRequested?.Invoke(
            this,
            item.FullPath);
    }


    // ============================================================
    // WORKSPACE
    // ============================================================

    public string? GetWorkspacePath()
    {
        return
            _workspaceService.RootPath;
    }
}