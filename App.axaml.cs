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
            // Windows can launch Orbpad like:
            //
            // Orbpad.exe "C:\Notes\README.md"
            //
            // Avalonia exposes those arguments through
            // desktop.Args.
            // ====================================================

            if (desktop.Args.Length > 0)
            {
                string? filePath =
                    desktop.Args[0];

                if (!string.IsNullOrWhiteSpace(
                        filePath))
                {
                    string fullPath;

                    try
                    {
                        fullPath =
                            Path.GetFullPath(
                                filePath);
                    }
                    catch
                    {
                        fullPath =
                            string.Empty;
                    }

                    if (!string.IsNullOrWhiteSpace(
                            fullPath) &&
                        File.Exists(fullPath))
                    {
                        mainWindow.OpenFileFromStartup(
                            fullPath);
                    }
                }
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}