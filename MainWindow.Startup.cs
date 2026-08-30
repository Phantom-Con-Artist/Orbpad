using System;
using System.IO;

namespace Orbpad;

public partial class MainWindow
{
    // ============================================================
    // WINDOWS / COMMAND-LINE FILE OPENING
    // ============================================================

    public void OpenFileFromStartup(
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
                Path.GetFullPath(
                    filePath);
        }
        catch
        {
            return;
        }

        if (!File.Exists(fullPath))
        {
            return;
        }

        // ========================================================
        // REMOVE THE INITIAL EMPTY DOCUMENT
        //
        // MainWindow creates an initial Untitled document.
        // When Orbpad is launched with a file, we don't want:
        //
        // [Untitled] [README.md]
        //
        // We want:
        //
        // [README.md]
        // ========================================================

        var initialDocument =
            _documentManager.ActiveDocument;

        if (initialDocument is not null &&
            initialDocument.FilePath is null &&
            string.IsNullOrEmpty(
                initialDocument.Text) &&
            !initialDocument.IsModified)
        {
            _documentManager.RemoveDocument(
                initialDocument);
        }

        // ========================================================
        // OPEN THE FILE
        // ========================================================

        OpenTextFile(
            fullPath);

        // ========================================================
        // FINAL UI REFRESH
        // ========================================================

        RefreshTabs();
        UpdateWindowTitle();
        UpdateStatusBar();

        // Make sure the editor receives focus.
        Editor.Focus();
    }
}