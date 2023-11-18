using Tql.Abstractions;
using Tql.PluginTestSupport.Support;

namespace Tql.PluginTestSupport.Services;

public class TestClipboard : IClipboard
{
    // From https://spec.commonmark.org/0.15/#backslash-escapes.
    private static readonly string EscapeChars = @"!""#$%&'()*+,-./:;<=>?@[\]^_`{|}~";

    public CopyUriData? CopyUriData { get; private set; }
    public CopyMarkdownData? CopyMarkdownData { get; private set; }
    public CopyHtmlData? CopyHtmlData { get; private set; }

    public void CopyUri(string text, string uri) => CopyUriData = new CopyUriData(text, uri);

    public void CopyMarkdown(string markdown, string plainText) =>
        CopyMarkdownData = new CopyMarkdownData(markdown, plainText);

    public void CopyHtml(string html, string plainText) =>
        CopyHtmlData = new CopyHtmlData(html, plainText);

    public string EscapeMarkdown(string value)
    {
        var sb = StringBuilderCache.Acquire();

        foreach (var c in value)
        {
            if (EscapeChars.IndexOf(c) != -1)
                sb.Append('\\');
            sb.Append(c);
        }

        return StringBuilderCache.GetStringAndRelease(sb);
    }
}

public record CopyUriData(string Text, string Uri);

public record CopyMarkdownData(string Markdown, string PlainText);

public record CopyHtmlData(string Html, string PlainText);
