using System;
using System.IO;

namespace Orbpad.Services;

public sealed class WorkspaceService
{
    // ============================================================
    // STATE
    // ============================================================

    public string? RootPath
    {
        get;
        private set;
    }

    public bool HasWorkspace =>
        !string.IsNullOrWhiteSpace(
            RootPath);


    // ============================================================
    // OPEN WORKSPACE
    // ============================================================

    public bool SetRootPath(
        string path)
    {
        if (string.IsNullOrWhiteSpace(
                path))
        {
            return false;
        }

        string fullPath;

        try
        {
            fullPath =
                Path.GetFullPath(
                    path);
        }
        catch
        {
            return false;
        }

        if (!Directory.Exists(
                fullPath))
        {
            return false;
        }

        RootPath =
            fullPath;

        return true;
    }


    // ============================================================
    // CLEAR WORKSPACE
    // ============================================================

    public void Clear()
    {
        RootPath =
            null;
    }
}