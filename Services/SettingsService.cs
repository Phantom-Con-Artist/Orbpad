using System;
using System.IO;
using System.Text.Json;
using Orbpad.Models;

namespace Orbpad.Services;

public class SettingsService
{
    private readonly string _settingsDirectory;
    private readonly string _settingsFilePath;

    private readonly JsonSerializerOptions _jsonOptions =
        new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

    public string SettingsFilePath =>
        _settingsFilePath;

    public SettingsService()
    {
        string appDataPath =
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData);

        _settingsDirectory =
            Path.Combine(
                appDataPath,
                "Orbpad");

        _settingsFilePath =
            Path.Combine(
                _settingsDirectory,
                "settings.json");
    }

    // ============================================================
    // LOAD
    // ============================================================

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(
                    _settingsFilePath))
            {
                return new AppSettings();
            }

            string json =
                File.ReadAllText(
                    _settingsFilePath);

            var settings =
                JsonSerializer.Deserialize<AppSettings>(
                    json,
                    _jsonOptions);

            return settings
                   ?? new AppSettings();
        }
        catch
        {
            /*
             * If the settings file is missing,
             * malformed, unreadable, or otherwise
             * unusable, fall back to defaults.
             *
             * Orbpad should still launch even if
             * its settings file gets nuked.
             */

            return new AppSettings();
        }
    }

    // ============================================================
    // SAVE
    // ============================================================

    public void Save(
        AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(
                _settingsDirectory);

            string json =
                JsonSerializer.Serialize(
                    settings,
                    _jsonOptions);

            File.WriteAllText(
                _settingsFilePath,
                json);
        }
        catch
        {
            /*
             * Settings are convenience data.
             *
             * A failure here should never crash
             * the entire editor.
             */
        }
    }
}