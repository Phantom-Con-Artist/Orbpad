using Markdig;

namespace Orbpad.Services;

public sealed class MarkdownService
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownService()
    {
        _pipeline =
            new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .DisableHtml()
                .Build();
    }

    // ============================================================
    // MARKDOWN → HTML
    // ============================================================

    public string RenderToHtml(
        string? markdown)
    {
        if (string.IsNullOrEmpty(markdown))
        {
            return string.Empty;
        }

        return Markdig.Markdown.ToHtml(
            markdown,
            _pipeline);
    }
}