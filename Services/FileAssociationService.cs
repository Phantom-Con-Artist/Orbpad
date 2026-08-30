using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace Orbpad.Services;

public static class FileAssociationService
{
    // ============================================================
    // ORBPAD FILE ASSOCIATION
    // ============================================================

    private const string ProgId =
        "Orbpad.Document.1";

    private const string DisplayName =
        "Orbpad";

    private static readonly string[] SupportedExtensions =
    [
        ".txt",
        ".md",
        ".markdown",
        ".json",
        ".xml",
        ".cs",
        ".py",
        ".js",
        ".ts",
        ".html",
        ".htm",
        ".css"
    ];

    // ============================================================
    // REGISTER
    // ============================================================

    public static bool Register(
        string executablePath)
    {
        if (!OperatingSystem.IsWindows())
            return false;

        if (string.IsNullOrWhiteSpace(
                executablePath))
        {
            return false;
        }

        try
        {
            executablePath =
                Path.GetFullPath(
                    executablePath);

            if (!File.Exists(
                    executablePath))
            {
                return false;
            }

            using RegistryKey? classesRoot =
                Registry.CurrentUser.CreateSubKey(
                    @"Software\Classes");

            if (classesRoot is null)
                return false;

            // ----------------------------------------------------
            // ProgID
            // ----------------------------------------------------

            using RegistryKey? progIdKey =
                classesRoot.CreateSubKey(
                    ProgId);

            if (progIdKey is null)
                return false;

            progIdKey.SetValue(
                string.Empty,
                DisplayName);

            progIdKey.SetValue(
                "FriendlyTypeName",
                DisplayName,
                RegistryValueKind.String);

            // ----------------------------------------------------
            // Default icon
            // ----------------------------------------------------

            using RegistryKey? iconKey =
                progIdKey.CreateSubKey(
                    "DefaultIcon");

            iconKey?.SetValue(
                string.Empty,
                executablePath);

            // ----------------------------------------------------
            // Open command
            // ----------------------------------------------------

            using RegistryKey? shellKey =
                progIdKey.CreateSubKey(
                    @"shell\open\command");

            if (shellKey is null)
                return false;

            shellKey.SetValue(
                string.Empty,
                BuildCommand(
                    executablePath));

            // ----------------------------------------------------
            // Register every supported extension as an
            // OpenWithProgId.
            //
            // We deliberately DO NOT set the extension's
            // default value. That means Orbpad becomes available
            // under Open With without stealing the user's
            // existing default application.
            // ----------------------------------------------------

            foreach (string extension
                     in SupportedExtensions)
            {
                RegisterExtension(
                    classesRoot,
                    extension);
            }

            // ----------------------------------------------------
            // Tell Windows that association information changed.
            // ----------------------------------------------------

            NotifyShellAssociationChanged();

            return true;
        }
        catch
        {
            return false;
        }
    }

    // ============================================================
    // UNREGISTER
    // ============================================================

    public static bool Unregister()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        try
        {
            using RegistryKey? classesRoot =
                Registry.CurrentUser.OpenSubKey(
                    @"Software\Classes",
                    writable: true);

            if (classesRoot is null)
                return false;

            foreach (string extension
                     in SupportedExtensions)
            {
                RemoveExtensionRegistration(
                    classesRoot,
                    extension);
            }

            // Remove our ProgID.
            classesRoot.DeleteSubKeyTree(
                ProgId,
                throwOnMissingSubKey: false);

            NotifyShellAssociationChanged();

            return true;
        }
        catch
        {
            return false;
        }
    }

    // ============================================================
    // CHECK REGISTRATION
    // ============================================================

    public static bool IsRegistered()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        try
        {
            using RegistryKey? classesRoot =
                Registry.CurrentUser.OpenSubKey(
                    @"Software\Classes");

            if (classesRoot is null)
                return false;

            using RegistryKey? progIdKey =
                classesRoot.OpenSubKey(
                    ProgId);

            if (progIdKey is null)
                return false;

            using RegistryKey? commandKey =
                progIdKey.OpenSubKey(
                    @"shell\open\command");

            if (commandKey is null)
                return false;

            object? value =
                commandKey.GetValue(
                    string.Empty);

            return value is string command &&
                   !string.IsNullOrWhiteSpace(
                       command);
        }
        catch
        {
            return false;
        }
    }

    // ============================================================
    // REGISTER EXTENSION
    // ============================================================

    private static void RegisterExtension(
        RegistryKey classesRoot,
        string extension)
    {
        using RegistryKey? extensionKey =
            classesRoot.CreateSubKey(
                extension);

        if (extensionKey is null)
            return;

        using RegistryKey? openWithProgIds =
            extensionKey.CreateSubKey(
                "OpenWithProgIds");

        if (openWithProgIds is null)
            return;

        // REG_SZ with an empty string is the normal
        // OpenWithProgIds registration form.
        openWithProgIds.SetValue(
            ProgId,
            string.Empty,
            RegistryValueKind.String);
    }

    // ============================================================
    // REMOVE EXTENSION
    // ============================================================

    private static void RemoveExtensionRegistration(
        RegistryKey classesRoot,
        string extension)
    {
        using RegistryKey? extensionKey =
            classesRoot.OpenSubKey(
                extension,
                writable: true);

        if (extensionKey is null)
            return;

        using RegistryKey? openWithProgIds =
            extensionKey.OpenSubKey(
                "OpenWithProgIds",
                writable: true);

        if (openWithProgIds is null)
            return;

        openWithProgIds.DeleteValue(
            ProgId,
            throwOnMissingValue: false);

        // If our removal leaves the OpenWithProgIds
        // key empty, clean that key up.
        if (openWithProgIds.ValueCount == 0 &&
            openWithProgIds.SubKeyCount == 0)
        {
            extensionKey.DeleteSubKeyTree(
                "OpenWithProgIds",
                throwOnMissingSubKey: false);
        }
    }

    // ============================================================
    // COMMAND LINE
    // ============================================================

    private static string BuildCommand(
        string executablePath)
    {
        string quotedExecutable =
            Quote(executablePath);

        return
            $"{quotedExecutable} \"%1\"";
    }

    private static string Quote(
        string value)
    {
        return
            "\"" +
            value.Replace(
                "\"",
                "\\\"") +
            "\"";
    }

    // ============================================================
    // WINDOWS SHELL NOTIFICATION
    // ============================================================

    private static void NotifyShellAssociationChanged()
    {
        if (!OperatingSystem.IsWindows())
            return;

        SHChangeNotify(
            SHCNE_ASSOCCHANGED,
            SHCNF_IDLIST,
            IntPtr.Zero,
            IntPtr.Zero);
    }

    private const uint SHCNE_ASSOCCHANGED =
        0x08000000;

    private const uint SHCNF_IDLIST =
        0x0000;

    [DllImport(
        "shell32.dll",
        CharSet = CharSet.Unicode)]
    private static extern void SHChangeNotify(
        uint wEventId,
        uint uFlags,
        IntPtr dwItem1,
        IntPtr dwItem2);
}