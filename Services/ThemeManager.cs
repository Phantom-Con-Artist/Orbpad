using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace Orbpad.Services;

public static class ThemeManager
{
    public enum OrbpadTheme
    {
        Dark,
        Midnight,
        Forest,
        Light
    }

    public static OrbpadTheme CurrentTheme { get; private set; }
        = OrbpadTheme.Dark;

    public static void ApplyTheme(
        OrbpadTheme theme)
    {
        var application =
            Application.Current;

        if (application is null)
            return;

        var palette =
            GetPalette(theme);

        SetBrush(
            application,
            "OrbpadBackgroundBrush",
            palette.Background);

        SetBrush(
            application,
            "OrbpadSurfaceBrush",
            palette.Surface);

        SetBrush(
            application,
            "OrbpadSurfaceHoverBrush",
            palette.SurfaceHover);

        SetBrush(
            application,
            "OrbpadBorderBrush",
            palette.Border);

        SetBrush(
            application,
            "OrbpadTextBrush",
            palette.Text);

        SetBrush(
            application,
            "OrbpadMutedTextBrush",
            palette.MutedText);

        SetBrush(
            application,
            "OrbpadAccentBrush",
            palette.Accent);

        SetBrush(
            application,
            "OrbpadAccentMutedBrush",
            palette.Accent,
            opacity: 0.30);

        application.RequestedThemeVariant =
            theme == OrbpadTheme.Light
                ? ThemeVariant.Light
                : ThemeVariant.Dark;

        CurrentTheme =
            theme;
    }

    private static void SetBrush(
        Application application,
        string key,
        string color,
        double opacity = 1.0)
    {
        application.Resources[key] =
            new SolidColorBrush(
                Color.Parse(color),
                opacity);
    }

    private static ThemePalette GetPalette(
        OrbpadTheme theme)
    {
        return theme switch
        {
            OrbpadTheme.Midnight =>
                new ThemePalette
                {
                    Background = "#0B1020",
                    Surface = "#11182A",
                    SurfaceHover = "#1B2740",
                    Border = "#293756",

                    Text = "#E8EEF9",
                    MutedText = "#91A0B8",

                    Accent = "#60A5FA"
                },

            OrbpadTheme.Forest =>
                new ThemePalette
                {
                    Background = "#0E1712",
                    Surface = "#14221A",
                    SurfaceHover = "#1D3024",
                    Border = "#2D4936",

                    Text = "#ECF6EF",
                    MutedText = "#9CB2A3",

                    Accent = "#4ADE80"
                },

            OrbpadTheme.Light =>
                new ThemePalette
                {
                    Background = "#F7F7F4",
                    Surface = "#FFFFFF",
                    SurfaceHover = "#F0F0EA",
                    Border = "#D8D8D0",

                    Text = "#202124",
                    MutedText = "#6B7280",

                    Accent = "#7C3AED"
                },

            _ =>
                new ThemePalette
                {
                    Background = "#121214",
                    Surface = "#19191C",
                    SurfaceHover = "#25252B",
                    Border = "#303036",

                    Text = "#F4F4F5",
                    MutedText = "#A1A1AA",

                    Accent = "#8B5CF6"
                }
        };
    }

    private sealed class ThemePalette
    {
        public string Background { get; init; } =
            string.Empty;

        public string Surface { get; init; } =
            string.Empty;

        public string SurfaceHover { get; init; } =
            string.Empty;

        public string Border { get; init; } =
            string.Empty;

        public string Text { get; init; } =
            string.Empty;

        public string MutedText { get; init; } =
            string.Empty;

        public string Accent { get; init; } =
            string.Empty;
    }
}