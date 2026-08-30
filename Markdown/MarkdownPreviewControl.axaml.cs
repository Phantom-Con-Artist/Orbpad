using System;
using System.IO;
using System.Text.RegularExpressions;
using Avalonia.Controls;
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
                        new Uri(
                            fullPath);

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
        return $$"""
<!DOCTYPE html>

<html>
<head>

    <meta charset="utf-8">

    <meta name="viewport"
          content="width=device-width, initial-scale=1.0">

    <title>Orbpad Markdown Preview</title>

    <style>

        :root {
            color-scheme: dark;
        }

        * {
            box-sizing: border-box;
        }

        html,
        body {
            margin: 0;
            padding: 0;
        }

        body {
            font-family:
                Inter,
                "Segoe UI",
                Arial,
                sans-serif;

            background: #121214;
            color: #F4F4F5;

            line-height: 1.65;

            padding:
                32px 42px 48px 42px;

            max-width: 1000px;

            margin: 0 auto;
        }

        h1,
        h2,
        h3,
        h4,
        h5,
        h6 {
            color: #F4F4F5;

            line-height: 1.25;

            margin-top: 1.5em;
            margin-bottom: 0.6em;
        }

        h1 {
            font-size: 2.2rem;
        }

        h2 {
            font-size: 1.8rem;
        }

        h3 {
            font-size: 1.45rem;
        }

        h4 {
            font-size: 1.2rem;
        }

        p {
            margin:
                0 0 1em 0;
        }

        a {
            color: #8B5CF6;
            text-decoration: none;
        }

        a:hover {
            text-decoration: underline;
        }

        blockquote {
            margin: 1em 0;

            padding:
                0.4em 1em;

            border-left:
                4px solid #8B5CF6;

            background:
                #19191C;

            color:
                #A1A1AA;
        }

        code {
            font-family:
                Consolas,
                "Courier New",
                monospace;

            background:
                #19191C;

            border-radius:
                5px;

            padding:
                0.15em 0.35em;
        }

        pre {
            overflow-x:
                auto;

            background:
                #19191C;

            border:
                1px solid #303036;

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

            padding:
                0;
        }

        table {
            width: 100%;

            border-collapse:
                collapse;

            margin:
                1em 0;
        }

        th,
        td {
            border:
                1px solid #303036;

            padding:
                8px 10px;

            text-align:
                left;
        }

        th {
            background:
                #19191C;
        }

        tr:nth-child(even) {
            background:
                #17171A;
        }

        hr {
            border:
                0;

            border-top:
                1px solid #303036;

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
                #FFFFFF;
        }

        em {
            color:
                #E4E4E7;
        }

        del {
            color:
                #A1A1AA;
        }

        input[type="checkbox"] {
            margin-right:
                0.5em;
        }

        ::selection {
            background:
                rgba(139, 92, 246, 0.35);
        }

    </style>

</head>

<body>

{{body}}

</body>

</html>
""";
    }
}