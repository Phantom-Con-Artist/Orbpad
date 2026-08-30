using System;
using System.IO;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Orbpad.Services;

namespace Orbpad.Markdown;

public partial class MarkdownPreviewControl : UserControl
{
    private readonly MarkdownService _markdownService;

    public MarkdownPreviewControl()
    {
        InitializeComponent();

        _markdownService =
            new MarkdownService();
    }

    // ============================================================
    // MARKDOWN
    // ============================================================

    public void SetMarkdown(
        string? markdown,
        string? sourceFilePath = null)
    {
        string html =
            _markdownService.RenderToHtml(
                markdown ?? string.Empty);

        html =
            ResolveRelativeImages(
                html,
                sourceFilePath);

        string document =
            BuildHtmlDocument(html);

        PreviewWebView.NavigateToString(
            document);
    }

    // ============================================================
    // RELATIVE IMAGES
    // ============================================================

    private static string ResolveRelativeImages(
        string html,
        string? sourceFilePath)
    {
        if (string.IsNullOrWhiteSpace(
                sourceFilePath))
        {
            return html;
        }

        string? directory =
            Path.GetDirectoryName(
                Path.GetFullPath(
                    sourceFilePath));

        if (string.IsNullOrWhiteSpace(
                directory))
        {
            return html;
        }

        return Regex.Replace(
            html,
            """
            <img(\s+[^>]*?)src=["'](?!https?://|data:|file://)([^"']+)["']([^>]*)>
            """,
            match =>
            {
                string relativePath =
                    match.Groups[2].Value;

                try
                {
                    string fullPath =
                        Path.GetFullPath(
                            Path.Combine(
                                directory,
                                relativePath));

                    Uri fileUri =
                        new Uri(fullPath);

                    string normalizedUri =
                        fileUri.AbsoluteUri;

                    return
                        $"<img{match.Groups[1].Value}" +
                        $"src=\"{normalizedUri}\"" +
                        $"{match.Groups[3].Value}>";
                }
                catch
                {
                    return match.Value;
                }
            },
            RegexOptions.IgnoreCase);
    }

    // ============================================================
    // HTML DOCUMENT
    // ============================================================

    private static string BuildHtmlDocument(
        string body)
    {
        // Read the current Orbpad theme resources.
        string background =
            GetBrushHex(
                "OrbpadBackgroundBrush",
                "#121214");

        string surface =
            GetBrushHex(
                "OrbpadSurfaceBrush",
                "#19191C");

        string surfaceHover =
            GetBrushHex(
                "OrbpadSurfaceHoverBrush",
                "#25252B");

        string border =
            GetBrushHex(
                "OrbpadBorderBrush",
                "#303036");

        string text =
            GetBrushHex(
                "OrbpadTextBrush",
                "#F4F4F5");

        string mutedText =
            GetBrushHex(
                "OrbpadMutedTextBrush",
                "#A1A1AA");

        string accent =
            GetBrushHex(
                "OrbpadAccentBrush",
                "#8B5CF6");

        bool isLightTheme =
            ThemeManager.CurrentTheme ==
            ThemeManager.OrbpadTheme.Light;

        string colorScheme =
            isLightTheme
                ? "light"
                : "dark";

        string codeText =
            text;

        string alternatingSurface =
            isLightTheme
                ? surfaceHover
                : BlendColors(
                    background,
                    surface,
                    0.55);

        string selection =
            HexToRgba(
                accent,
                0.30);

        return $$"""
<!DOCTYPE html>

<html>
<head>

    <meta charset="utf-8">

    <meta name="viewport"
          content="width=device-width, initial-scale=1.0">

    <meta name="color-scheme"
          content="{{colorScheme}}">

    <title>Orbpad Markdown Preview</title>

    <style>

        :root {
            color-scheme: {{colorScheme}};
        }

        * {
            box-sizing: border-box;
        }

        html,
        body {
            margin: 0;
            padding: 0;
            min-height: 100%;
        }

        body {
            font-family:
                Inter,
                "Segoe UI",
                Arial,
                sans-serif;

            background:
                {{background}};

            color:
                {{text}};

            line-height:
                1.65;

            padding:
                32px 42px 48px 42px;

            max-width:
                1000px;

            margin:
                0 auto;
        }

        h1,
        h2,
        h3,
        h4,
        h5,
        h6 {
            color:
                {{text}};

            line-height:
                1.25;

            margin-top:
                1.5em;

            margin-bottom:
                0.6em;
        }

        h1 {
            font-size:
                2.2rem;
        }

        h2 {
            font-size:
                1.8rem;
        }

        h3 {
            font-size:
                1.45rem;
        }

        h4 {
            font-size:
                1.2rem;
        }

        p {
            margin:
                0 0 1em 0;
        }

        a {
            color:
                {{accent}};

            text-decoration:
                none;
        }

        a:hover {
            text-decoration:
                underline;
        }

        blockquote {
            margin:
                1em 0;

            padding:
                0.4em 1em;

            border-left:
                4px solid {{accent}};

            background:
                {{surface}};

            color:
                {{mutedText}};
        }

        code {
            font-family:
                Consolas,
                "Courier New",
                monospace;

            color:
                {{codeText}};

            background:
                {{surface}};

            border:
                1px solid {{border}};

            border-radius:
                5px;

            padding:
                0.15em 0.35em;
        }

        pre {
            overflow-x:
                auto;

            background:
                {{surface}};

            color:
                {{text}};

            border:
                1px solid {{border}};

            border-radius:
                8px;

            padding:
                16px;

            margin:
                1em 0;
        }

        pre code {
            background:
                transparent;

            border:
                0;

            color:
                {{text}};

            padding:
                0;
        }

        table {
            width:
                100%;

            border-collapse:
                collapse;

            margin:
                1em 0;
        }

        th,
        td {
            border:
                1px solid {{border}};

            padding:
                8px 10px;

            text-align:
                left;
        }

        th {
            background:
                {{surface}};
        }

        tr:nth-child(even) {
            background:
                {{alternatingSurface}};
        }

        hr {
            border:
                0;

            border-top:
                1px solid {{border}};

            margin:
                2em 0;
        }

        img {
            max-width:
                100%;

            height:
                auto;

            border-radius:
                8px;
        }

        ul,
        ol {
            padding-left:
                2em;
        }

        li {
            margin:
                0.25em 0;
        }

        strong {
            color:
                {{text}};
        }

        em {
            color:
                {{mutedText}};
        }

        del {
            color:
                {{mutedText}};
        }

        input[type="checkbox"] {
            margin-right:
                0.5em;
        }

        ::selection {
            background:
                {{selection}};
        }

    </style>

</head>

<body>

{{body}}

</body>

</html>
""";
    }

    // ============================================================
    // THEME RESOURCES
    // ============================================================

    private static string GetBrushHex(
        string resourceKey,
        string fallback)
    {
        if (Application.Current is null)
        {
            return fallback;
        }

        if (Application.Current.Resources[
                resourceKey]
            is not SolidColorBrush brush)
        {
            return fallback;
        }

        return ToHex(
            brush.Color);
    }

    private static string ToHex(
        Color color)
    {
        return
            $"#{color.R:X2}" +
            $"{color.G:X2}" +
            $"{color.B:X2}";
    }

    private static string HexToRgba(
        string hex,
        double opacity)
    {
        if (!TryParseHex(
                hex,
                out byte r,
                out byte g,
                out byte b))
        {
            return
                $"rgba(139, 92, 246, {opacity:0.##})";
        }

        return
            $"rgba({r}, {g}, {b}, {opacity:0.##})";
    }

    private static bool TryParseHex(
        string hex,
        out byte r,
        out byte g,
        out byte b)
    {
        r = 0;
        g = 0;
        b = 0;

        if (string.IsNullOrWhiteSpace(
                hex))
        {
            return false;
        }

        string value =
            hex.Trim();

        if (value.StartsWith("#"))
        {
            value =
                value[1..];
        }

        if (value.Length != 6)
        {
            return false;
        }

        return
            byte.TryParse(
                value.Substring(0, 2),
                System.Globalization.NumberStyles.HexNumber,
                null,
                out r)
            &&
            byte.TryParse(
                value.Substring(2, 2),
                System.Globalization.NumberStyles.HexNumber,
                null,
                out g)
            &&
            byte.TryParse(
                value.Substring(4, 2),
                System.Globalization.NumberStyles.HexNumber,
                null,
                out b);
    }

    // ============================================================
    // SIMPLE COLOR BLENDING
    // ============================================================

    private static string BlendColors(
        string firstHex,
        string secondHex,
        double amount)
    {
        if (!TryParseHex(
                firstHex,
                out byte r1,
                out byte g1,
                out byte b1))
        {
            return secondHex;
        }

        if (!TryParseHex(
                secondHex,
                out byte r2,
                out byte g2,
                out byte b2))
        {
            return firstHex;
        }

        amount =
            Math.Clamp(
                amount,
                0.0,
                1.0);

        byte r =
            (byte)Math.Round(
                r1 +
                (r2 - r1) *
                amount);

        byte g =
            (byte)Math.Round(
                g1 +
                (g2 - g1) *
                amount);

        byte b =
            (byte)Math.Round(
                b1 +
                (b2 - b1) *
                amount);

        return
            $"#{r:X2}" +
            $"{g:X2}" +
            $"{b:X2}";
    }
}