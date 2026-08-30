using System;
using System.IO;
using Avalonia;
using Avalonia.Input;

namespace Orbpad;

public partial class MainWindow
{
    // ============================================================
    // DRAG & DROP
    // ============================================================

    private void MainWindow_DragEnter(
        object? sender,
        DragEventArgs e)
    {
        if (ContainsFiles(e))
        {
            ShowDropFeedback();

            e.DragEffects =
                DragDropEffects.Copy;
        }
        else
        {
            e.DragEffects =
                DragDropEffects.None;
        }
    }


    private void MainWindow_DragLeave(
        object? sender,
        DragEventArgs e)
    {
        HideDropFeedback();

        e.DragEffects =
            DragDropEffects.None;
    }


    private void MainWindow_DragOver(
        object? sender,
        DragEventArgs e)
    {
        if (ContainsFiles(e))
        {
            e.DragEffects =
                DragDropEffects.Copy;

            ShowDropFeedback();
        }
        else
        {
            e.DragEffects =
                DragDropEffects.None;

            HideDropFeedback();
        }
    }


    private void MainWindow_Drop(
        object? sender,
        DragEventArgs e)
    {
        HideDropFeedback();


        var files =
            e.DataTransfer.TryGetFiles();


        if (files is null ||
            files.Length == 0)
        {
            e.DragEffects =
                DragDropEffects.None;

            return;
        }


        int openedCount = 0;


        foreach (var file
                 in files)
        {
            if (file is null)
                continue;


            string? localPath;


            try
            {
                localPath =
                    file.Path.LocalPath;
            }
            catch
            {
                continue;
            }


            if (string.IsNullOrWhiteSpace(
                    localPath))
            {
                continue;
            }


            // ----------------------------------------------------
            // Ignore directories for now.
            // ----------------------------------------------------

            if (!File.Exists(localPath))
                continue;


            string fullPath;


            try
            {
                fullPath =
                    Path.GetFullPath(
                        localPath);
            }
            catch
            {
                continue;
            }


            // ----------------------------------------------------
            // Images
            // ----------------------------------------------------

            if (IsImageFile(fullPath))
            {
                OpenDroppedImage(
                    fullPath);

                openedCount++;

                continue;
            }


            // ----------------------------------------------------
            // Text / source / Markdown / other files
            //
            // Reuse the existing Orbpad file-opening pipeline.
            // This means syntax highlighting and Markdown
            // detection continue to work automatically.
            // ----------------------------------------------------

            OpenTextFile(
                fullPath);

            openedCount++;
        }


        e.DragEffects =
            openedCount > 0
                ? DragDropEffects.Copy
                : DragDropEffects.None;


        if (openedCount > 0)
        {
            RefreshTabs();
            UpdateWindowTitle();
            UpdateStatusBar();
        }


        Editor.Focus();
    }


    // ============================================================
    // FILE DETECTION
    // ============================================================

    private static bool ContainsFiles(
        DragEventArgs e)
    {
        // Avalonia 12 provides dropped filesystem items
        // through TryGetFiles().
        var files =
            e.DataTransfer.TryGetFiles();

        return
            files is not null &&
            files.Length > 0;
    }


    private static bool IsImageFile(
        string filePath)
    {
        string extension =
            Path.GetExtension(
                filePath);


        return
            extension.Equals(
                ".png",
                StringComparison.OrdinalIgnoreCase)
            ||
            extension.Equals(
                ".jpg",
                StringComparison.OrdinalIgnoreCase)
            ||
            extension.Equals(
                ".jpeg",
                StringComparison.OrdinalIgnoreCase)
            ||
            extension.Equals(
                ".gif",
                StringComparison.OrdinalIgnoreCase)
            ||
            extension.Equals(
                ".bmp",
                StringComparison.OrdinalIgnoreCase)
            ||
            extension.Equals(
                ".webp",
                StringComparison.OrdinalIgnoreCase)
            ||
            extension.Equals(
                ".tif",
                StringComparison.OrdinalIgnoreCase)
            ||
            extension.Equals(
                ".tiff",
                StringComparison.OrdinalIgnoreCase);
    }


    // ============================================================
    // DROP FEEDBACK
    // ============================================================

    private void ShowDropFeedback()
    {
        EditorWorkspaceBorder.BorderThickness =
            new Thickness(2);
    }


    private void HideDropFeedback()
    {
        EditorWorkspaceBorder.BorderThickness =
            new Thickness(0);
    }


    // ============================================================
    // IMAGE DROP
    // ============================================================

    private void OpenDroppedImage(
        string filePath)
    {
        try
        {
            var bitmap =
                new Avalonia.Media.Imaging.Bitmap(
                    filePath);


            string title =
                Path.GetFileName(
                    filePath);


            var imageWindow =
                new ImageViewerWindow(
                    bitmap,
                    title);


            imageWindow.Show(
                this);
        }
        catch
        {
            // Ignore invalid or unsupported image drops.
        }
    }
}