using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Orbpad;

public partial class MainWindow
{
    // ============================================================
    // MARKDOWN VIEW MODE
    // ============================================================

    private enum MarkdownViewMode
    {
        Edit,
        Split,
        Preview
    }

    private MarkdownViewMode _markdownViewMode =
        MarkdownViewMode.Edit;

    private bool _markdownIntegrationReady;

    private string? _lastMarkdownFilePath;


    // ============================================================
    // INITIALIZATION
    // ============================================================

    protected override void OnOpened(
        EventArgs e)
    {
        base.OnOpened(e);

        _markdownIntegrationReady =
            true;

        Editor.TextChanged +=
            MarkdownEditor_TextChanged;

        RefreshMarkdownView();
    }


    // ============================================================
    // DOCUMENT CHECK
    // ============================================================

    private bool IsCurrentDocumentMarkdown()
    {
        var document =
            _documentManager.ActiveDocument;

        if (document is null ||
            string.IsNullOrWhiteSpace(
                document.FilePath))
        {
            return false;
        }

        string extension =
            Path.GetExtension(
                document.FilePath);

        return
            extension.Equals(
                ".md",
                StringComparison.OrdinalIgnoreCase)
            ||
            extension.Equals(
                ".markdown",
                StringComparison.OrdinalIgnoreCase);
    }


    // ============================================================
    // REFRESH MARKDOWN STATE
    // ============================================================

    private void RefreshMarkdownView()
    {
        if (!_markdownIntegrationReady)
            return;

        bool isMarkdown =
            IsCurrentDocumentMarkdown();

        MarkdownMenuItem.IsEnabled =
            isMarkdown;

        if (!isMarkdown)
        {
            _markdownViewMode =
                MarkdownViewMode.Edit;
        }

        string? currentPath =
            _documentManager
                .ActiveDocument?
                .FilePath;

        bool documentChanged =
            !string.Equals(
                _lastMarkdownFilePath,
                currentPath,
                StringComparison.OrdinalIgnoreCase);

        if (documentChanged)
        {
            _lastMarkdownFilePath =
                currentPath;
        }

        ApplyMarkdownLayout(
            isMarkdown);

        UpdateMarkdownMenu();

        if (isMarkdown)
        {
            UpdateMarkdownPreview();
        }
    }


    // ============================================================
    // EDITOR CHANGES
    // ============================================================

    private void MarkdownEditor_TextChanged(
        object? sender,
        EventArgs e)
    {
        if (!_markdownIntegrationReady)
            return;

        if (!IsCurrentDocumentMarkdown())
            return;

        UpdateMarkdownPreview();
    }


    // ============================================================
    // PREVIEW UPDATE
    // ============================================================

    private void UpdateMarkdownPreview()
    {
        if (!_markdownIntegrationReady)
            return;

        if (!IsCurrentDocumentMarkdown())
            return;

        if (_markdownViewMode ==
            MarkdownViewMode.Edit)
        {
            return;
        }

        var document =
            _documentManager.ActiveDocument;

        if (document is null)
            return;

        MarkdownPreview.SetMarkdown(
            Editor.Text ?? string.Empty,
            document.FilePath);
    }


    // ============================================================
    // VIEW MENU HANDLERS
    // ============================================================

    private void MarkdownEdit_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (!IsCurrentDocumentMarkdown())
            return;

        _markdownViewMode =
            MarkdownViewMode.Edit;

        ApplyMarkdownLayout(
            true);

        UpdateMarkdownMenu();
    }


    private void MarkdownSplit_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (!IsCurrentDocumentMarkdown())
            return;

        _markdownViewMode =
            MarkdownViewMode.Split;

        ApplyMarkdownLayout(
            true);

        UpdateMarkdownMenu();

        UpdateMarkdownPreview();
    }


    private void MarkdownPreview_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (!IsCurrentDocumentMarkdown())
            return;

        _markdownViewMode =
            MarkdownViewMode.Preview;

        ApplyMarkdownLayout(
            true);

        UpdateMarkdownMenu();

        UpdateMarkdownPreview();
    }


    // ============================================================
    // LAYOUT
    // ============================================================

    private void ApplyMarkdownLayout(
        bool isMarkdown)
    {
        if (!isMarkdown)
        {
            Editor.IsVisible =
                true;

            MarkdownPreview.IsVisible =
                false;

            MarkdownSplitter.IsVisible =
                false;

            Grid.SetColumn(
                Editor,
                0);

            Grid.SetColumnSpan(
                Editor,
                3);

            return;
        }

        switch (_markdownViewMode)
        {
            case MarkdownViewMode.Edit:

                Editor.IsVisible =
                    true;

                MarkdownPreview.IsVisible =
                    false;

                MarkdownSplitter.IsVisible =
                    false;

                Grid.SetColumn(
                    Editor,
                    0);

                Grid.SetColumnSpan(
                    Editor,
                    3);

                break;


            case MarkdownViewMode.Split:

                Editor.IsVisible =
                    true;

                MarkdownPreview.IsVisible =
                    true;

                MarkdownSplitter.IsVisible =
                    true;

                Grid.SetColumn(
                    Editor,
                    0);

                Grid.SetColumnSpan(
                    Editor,
                    1);

                Grid.SetColumn(
                    MarkdownSplitter,
                    1);

                Grid.SetColumn(
                    MarkdownPreview,
                    2);

                Grid.SetColumnSpan(
                    MarkdownPreview,
                    1);

                break;


            case MarkdownViewMode.Preview:

                Editor.IsVisible =
                    false;

                MarkdownPreview.IsVisible =
                    true;

                MarkdownSplitter.IsVisible =
                    false;

                Grid.SetColumn(
                    MarkdownPreview,
                    0);

                Grid.SetColumnSpan(
                    MarkdownPreview,
                    3);

                break;
        }
    }


    // ============================================================
    // MARKDOWN MENU STATE
    // ============================================================

    private void UpdateMarkdownMenu()
    {
        bool isMarkdown =
            IsCurrentDocumentMarkdown();

        MarkdownMenuItem.IsEnabled =
            isMarkdown;

        MarkdownEditMenuItem.Header =
            _markdownViewMode ==
            MarkdownViewMode.Edit
                ? "_Edit ✓"
                : "_Edit";

        MarkdownSplitMenuItem.Header =
            _markdownViewMode ==
            MarkdownViewMode.Split
                ? "_Split ✓"
                : "_Split";

        MarkdownPreviewMenuItem.Header =
            _markdownViewMode ==
            MarkdownViewMode.Preview
                ? "_Preview ✓"
                : "_Preview";
    }
}