using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Orbpad.Explorer;
using Orbpad.Services;

namespace Orbpad;

public partial class MainWindow
{
    // ============================================================
    // EXPLORER
    // ============================================================

    private readonly WorkspaceService
        _workspaceService =
            new();

    private ExplorerView?
        _explorerView;

    private Grid?
        _workspaceGrid;

    private GridSplitter?
        _explorerSplitter;

    private Border?
        _editorHost;


    // ============================================================
    // EXPLORER DIMENSIONS
    // ============================================================

    private const double DefaultExplorerWidth = 260;

    private const double MinExplorerWidth = 180;

    private const double MaxExplorerWidth = 450;

    private const double ExplorerSplitterWidth = 4;


    // ============================================================
    // INITIALIZE EXPLORER
    // ============================================================

    private void InitializeExplorer()
    {
        if (_explorerView is not null)
            return;

        if (Content is not DockPanel rootDock)
            return;

        if (rootDock.Children.Count == 0)
            return;


        // ========================================================
        // THE LAST CHILD OF THE ROOT DOCKPANEL IS CURRENTLY
        // THE EDITOR HOST.
        // ========================================================

        if (rootDock.Children[^1]
            is not Border editorHost)
        {
            return;
        }

        _editorHost =
            editorHost;


        // ========================================================
        // REMOVE EDITOR FROM ROOT DOCKPANEL
        // ========================================================

        rootDock.Children.Remove(
            _editorHost);


        // ========================================================
        // WORKSPACE GRID
        //
        // Column 0 = Explorer
        // Column 1 = Splitter
        // Column 2 = Editor
        // ========================================================

        _workspaceGrid =
            new Grid
            {
                HorizontalAlignment =
                    HorizontalAlignment.Stretch,

                VerticalAlignment =
                    VerticalAlignment.Stretch,

                ColumnDefinitions =
                    new ColumnDefinitions(
                        $"{DefaultExplorerWidth}," +
                        $"{ExplorerSplitterWidth},*")
            };


        // ========================================================
        // EXPLORER VIEW
        // ========================================================

        _explorerView =
            new ExplorerView(
                _workspaceService);

        _explorerView.FileOpenRequested +=
            ExplorerView_FileOpenRequested;


        Grid.SetColumn(
            _explorerView,
            0);


        _workspaceGrid.Children.Add(
            _explorerView);


        // ========================================================
        // SPLITTER
        // ========================================================

        _explorerSplitter =
            new GridSplitter
            {
                Width =
                    ExplorerSplitterWidth,

                HorizontalAlignment =
                    HorizontalAlignment.Stretch,

                VerticalAlignment =
                    VerticalAlignment.Stretch,

                ResizeDirection =
                    GridResizeDirection.Columns,

                ResizeBehavior =
                    GridResizeBehavior.PreviousAndNext,

                Background =
                    GetThemeBrush(
                        "OrbpadBorderBrush")
            };


        Grid.SetColumn(
            _explorerSplitter,
            1);


        _workspaceGrid.Children.Add(
            _explorerSplitter);


        // ========================================================
        // EDITOR
        // ========================================================

        Grid.SetColumn(
            _editorHost,
            2);


        _workspaceGrid.Children.Add(
            _editorHost);


        // ========================================================
        // PUT THE ENTIRE WORKSPACE BACK INTO THE ROOT DOCKPANEL
        // ========================================================

        rootDock.Children.Add(
            _workspaceGrid);
    }


    // ============================================================
    // FILE OPEN REQUEST
    // ============================================================

    private void ExplorerView_FileOpenRequested(
        object? sender,
        string filePath)
    {
        if (string.IsNullOrWhiteSpace(
                filePath))
        {
            return;
        }


        if (!File.Exists(
                filePath))
        {
            return;
        }


        // ========================================================
        // DON'T OPEN THE SAME DOCUMENT TWICE
        // ========================================================

        foreach (var document
                 in _documentManager.Documents)
        {
            if (document.FilePath is null)
                continue;


            if (!string.Equals(
                    document.FilePath,
                    filePath,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }


            SwitchToDocument(
                document);

            return;
        }


        // ========================================================
        // IMAGE
        // ========================================================

        if (IsImageFile(
                filePath))
        {
            OpenDroppedImage(
                filePath);

            return;
        }


        // ========================================================
        // TEXT / SOURCE / MARKDOWN
        // ========================================================

        OpenTextFile(
            filePath);

        RefreshTabs();
        UpdateWindowTitle();
        UpdateStatusBar();
    }


    // ============================================================
    // MAIN WINDOW EXPLORER BUTTON
    // ============================================================

    private void ExplorerButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        ToggleExplorer();
    }


    // ============================================================
    // EXPLORER VISIBILITY
    // ============================================================

    public void ToggleExplorer()
    {
        if (_workspaceGrid is null)
            return;

        if (_workspaceGrid.ColumnDefinitions.Count < 3)
            return;


        bool currentlyVisible =
            _workspaceGrid
                .ColumnDefinitions[0]
                .Width.Value > 0;


        if (currentlyVisible)
        {
            CollapseExplorer();
        }
        else
        {
            ShowExplorer();
        }
    }


    public void CollapseExplorer()
    {
        if (_workspaceGrid is null)
            return;

        if (_workspaceGrid.ColumnDefinitions.Count < 3)
            return;


        _workspaceGrid
            .ColumnDefinitions[0]
            .Width =
                new GridLength(
                    0,
                    GridUnitType.Pixel);


        _workspaceGrid
            .ColumnDefinitions[1]
            .Width =
                new GridLength(
                    0,
                    GridUnitType.Pixel);


        if (_explorerSplitter is not null)
        {
            _explorerSplitter.IsHitTestVisible =
                false;
        }


        if (_explorerView is not null)
        {
            _explorerView.IsHitTestVisible =
                false;
        }
    }


    public void ShowExplorer()
    {
        if (_workspaceGrid is null)
            return;

        if (_workspaceGrid.ColumnDefinitions.Count < 3)
            return;


        SetExplorerWidth(
            DefaultExplorerWidth);


        _workspaceGrid
            .ColumnDefinitions[1]
            .Width =
                new GridLength(
                    ExplorerSplitterWidth,
                    GridUnitType.Pixel);


        if (_explorerSplitter is not null)
        {
            _explorerSplitter.IsHitTestVisible =
                true;
        }


        if (_explorerView is not null)
        {
            _explorerView.IsHitTestVisible =
                true;
        }
    }


    public bool IsExplorerVisible()
    {
        if (_workspaceGrid is null)
            return false;

        if (_workspaceGrid.ColumnDefinitions.Count < 3)
            return false;


        return
            _workspaceGrid
                .ColumnDefinitions[0]
                .ActualWidth > 0;
    }


    // ============================================================
    // EXPLORER WIDTH
    // ============================================================

    public void SetExplorerWidth(
        double width)
    {
        if (_workspaceGrid is null)
            return;

        if (_workspaceGrid.ColumnDefinitions.Count < 3)
            return;


        double clampedWidth =
            Math.Clamp(
                width,
                MinExplorerWidth,
                MaxExplorerWidth);


        _workspaceGrid
            .ColumnDefinitions[0]
            .Width =
                new GridLength(
                    clampedWidth,
                    GridUnitType.Pixel);


        _workspaceGrid
            .ColumnDefinitions[1]
            .Width =
                new GridLength(
                    ExplorerSplitterWidth,
                    GridUnitType.Pixel);
    }


    public double GetExplorerWidth()
    {
        if (_workspaceGrid is null)
            return DefaultExplorerWidth;

        if (_workspaceGrid.ColumnDefinitions.Count < 3)
            return DefaultExplorerWidth;


        double width =
            _workspaceGrid
                .ColumnDefinitions[0]
                .ActualWidth;


        if (width <= 0)
            return DefaultExplorerWidth;


        return Math.Clamp(
            width,
            MinExplorerWidth,
            MaxExplorerWidth);
    }
}
