using Markdig;
using Markdig.Renderers;
using Tql.Abstractions;
using Tql.App.Support;

namespace Tql.App.Services;

internal class ClipboardImpl : IClipboard
{
    // From https://spec.commonmark.org/0.15/#backslash-escapes.
    private static readonly string EscapeChars = @"!""#$%&'()*+,-./:;<=>?@[\]^_`{|}~";

    private static readonly MarkdownPipeline Pipeline = BuildMarkdownPipeline();

    private static MarkdownPipeline BuildMarkdownPipeline()
    {
        var builder = new MarkdownPipelineBuilder();

        // The standard requires that the outputted HTML is wrapped in
        // a paragraph. This messes up formatting of the links in e.g.
        // Word, and other apps don't do this. Setting ImplicitParagraph
        // on the HtmlRenderer disables this behavior.
        builder.Extensions.Add(new SetImplicitParagraphExtension());

        return builder.Build();
    }

    public void CopyUri(string text, string uri)
    {
        CopyMarkdown($"[{EscapeMarkdown(text)}]({EscapeMarkdown(uri)})", uri);
    }

    public void CopyMarkdown(string markdown, string plainText)
    {
        CopyHtml(Markdown.ToHtml(markdown, Pipeline), plainText);
    }

    public void CopyHtml(string html, string plainText)
    {
        ClipboardHelper.CopyToClipboard(html, plainText);
    }

    public string EscapeMarkdown(string markdown)
    {
        var sb = StringBuilderCache.Acquire();

        foreach (var c in markdown)
        {
            if (EscapeChars.IndexOf(c) != -1)
                sb.Append('\\');
            sb.Append(c);
        }

        return StringBuilderCache.GetStringAndRelease(sb);
    }

    private class SetImplicitParagraphExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline) { }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer is HtmlRenderer htmlRenderer)
                htmlRenderer.ImplicitParagraph = true;
        }
    }
}
