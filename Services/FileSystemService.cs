using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orbpad.Explorer;

namespace Orbpad.Services;

public sealed class FileSystemService
{
    // ============================================================
    // SPECIAL DIRECTORIES
    //
    // These are intentionally excluded from the first Explorer
    // implementation because they generate enormous noisy trees
    // in normal development projects.
    // ============================================================

    private static readonly HashSet<string>
        IgnoredDirectoryNames =
            new(
                StringComparer.OrdinalIgnoreCase)
            {
                ".git",
                "bin",
                "obj",
                "node_modules"
            };

    // ============================================================
    // LOAD ROOT
    // ============================================================

    public IReadOnlyList<ExplorerItem>
        GetRootItems(
            string rootPath)
    {
        if (string.IsNullOrWhiteSpace(
                rootPath))
        {
            return [];
        }

        if (!Directory.Exists(
                rootPath))
        {
            return [];
        }

        return GetDirectoryItems(
                rootPath);
    }

    // ============================================================
    // LOAD CHILDREN
    // ============================================================

    public void LoadChildren(
        ExplorerItem directory)
    {
        if (!directory.IsDirectory)
            return;

        if (directory.IsPlaceholder)
            return;

        directory.Children.Clear();

        foreach (ExplorerItem item
                 in GetDirectoryItems(
                     directory.FullPath))
        {
            directory.Children.Add(
                item);
        }

        directory.IsLoaded =
            true;
    }

    // ============================================================
    // DIRECTORY ENUMERATION
    // ============================================================

    private static IReadOnlyList<ExplorerItem>
        GetDirectoryItems(
            string directoryPath)
    {
        var directories =
            new List<ExplorerItem>();

        var files =
            new List<ExplorerItem>();


        // --------------------------------------------------------
        // DIRECTORIES
        // --------------------------------------------------------

        try
        {
            foreach (string path
                     in Directory.EnumerateDirectories(
                         directoryPath))
            {
                string name =
                    Path.GetFileName(
                        path);

                if (string.IsNullOrWhiteSpace(
                        name))
                {
                    continue;
                }

                if (IgnoredDirectoryNames.Contains(
                        name))
                {
                    continue;
                }

                bool hasChildren =
                    HasFileSystemEntries(
                        path);

                var item =
                    new ExplorerItem(
                        name,
                        path,
                        isDirectory: true)
                    {
                        HasChildren =
                            hasChildren
                    };

                // ------------------------------------------------
                // Add a placeholder so TreeView knows this
                // directory can be expanded.
                // ------------------------------------------------

                if (hasChildren)
                {
                    item.Children.Add(
                        new ExplorerItem(
                            "Loading...",
                            string.Empty,
                            isDirectory: false)
                        {
                            IsPlaceholder =
                                true
                        });
                }

                directories.Add(
                    item);
            }
        }
        catch
        {
            // Ignore directories that cannot be enumerated.
        }


        // --------------------------------------------------------
        // FILES
        // --------------------------------------------------------

        try
        {
            foreach (string path
                     in Directory.EnumerateFiles(
                         directoryPath))
            {
                string name =
                    Path.GetFileName(
                        path);

                if (string.IsNullOrWhiteSpace(
                        name))
                {
                    continue;
                }

                files.Add(
                    new ExplorerItem(
                        name,
                        path,
                        isDirectory: false));
            }
        }
        catch
        {
            // Ignore files that cannot be enumerated.
        }


        // --------------------------------------------------------
        // Folders first, files second.
        // Alphabetical within each group.
        // --------------------------------------------------------

        return
            directories
                .OrderBy(
                    item => item.Name,
                    StringComparer.OrdinalIgnoreCase)
                .Concat(
                    files.OrderBy(
                        item => item.Name,
                        StringComparer.OrdinalIgnoreCase))
                .ToList();
    }

    // ============================================================
    // CHECK WHETHER A DIRECTORY CONTAINS ANYTHING
    // ============================================================

    private static bool HasFileSystemEntries(
        string directoryPath)
    {
        try
        {
            foreach (string path
                     in Directory.EnumerateFileSystemEntries(
                         directoryPath))
            {
                string name =
                    Path.GetFileName(
                        path);

                if (Directory.Exists(path) &&
                    IgnoredDirectoryNames.Contains(
                        name))
                {
                    continue;
                }

                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }
}