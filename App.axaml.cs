using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Orbpad;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow =
                new MainWindow();

            desktop.MainWindow =
                mainWindow;

            // ====================================================
            // COMMAND-LINE FILE OPENING
            //
            // Example:
            //
            // Orbpad.exe "C:\Users\subhr\Downloads\test.md"
            // ====================================================

            string[] args =
                desktop.Args ??
                Array.Empty<string>();

            if (args.Length > 0)
            {
                string filePath =
                    args[0];

                if (!string.IsNullOrWhiteSpace(
                        filePath))
                {
                    try
                    {
                        string fullPath =
                            Path.GetFullPath(
                                filePath);

                        if (File.Exists(
                                fullPath))
                        {
                            mainWindow.OpenFileFromStartup(
                                fullPath);
                        }
                    }
                    catch
                    {
                        // Ignore invalid startup paths.
                        // Orbpad continues normally.
                    }
                }
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}