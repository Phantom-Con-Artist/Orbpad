namespace Orbpad.Models;

public class AppSettings
{
    // ============================================================
    // APPEARANCE
    // ============================================================

    public string Theme { get; set; } =
        "Dark";

    public bool ShowToolbar { get; set; } =
        true;

    public bool ShowStatusBar { get; set; } =
        true;

    public bool ShowLineNumbers { get; set; } =
        true;


    // ============================================================
    // EDITOR
    // ============================================================

    public bool WordWrap { get; set; } =
        false;

    public string FontFamily { get; set; } =
        "Inter";

    public double FontSize { get; set; } =
        16;


    // ============================================================
    // WINDOW
    // ============================================================

    public double WindowWidth { get; set; } =
        1000;

    public double WindowHeight { get; set; } =
        700;

    public int? WindowX { get; set; }

    public int? WindowY { get; set; }
}