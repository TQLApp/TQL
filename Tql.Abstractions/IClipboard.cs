namespace Tql.Abstractions;

public interface IClipboard
{
    void CopyUri(string text, string uri);
    void CopyMarkdown(string markdown, string plainText);
    void CopyHtml(string html, string plainText);
    string EscapeMarkdown(string markdown);
}
