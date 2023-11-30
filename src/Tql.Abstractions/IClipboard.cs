namespace Tql.Abstractions;

/// <summary>
/// Represents a service to interact with the clipboard.
/// </summary>
/// <remarks>
/// <para>
/// This service is used to create pretty clipboard links. If the user
/// pasts such a link into e.g. Word or Teams, they get a link
/// with the text and the URI you've specified. If they paste it
/// in a text editor, they should only get the URI.
/// </para>
///
/// <para>
/// The easiest method to use is <see cref="CopyUri(string, string)"/>.
/// This is generally the method you'd use. However, if you want to go the
/// extra mile, and format links like Azure DevOps does, you can use the
/// <see cref="CopyMarkdown(string, string)"/> method. The (simplified) Markdown
/// for such a link would be <c>"[{WorkItemType} {Id}]({URI}): {WorkItemTitle}</c>.
/// Use the <see cref="EscapeMarkdown(string)"/> method to escape text like a URI.
/// </para>
/// </remarks>
public interface IClipboard
{
    /// <summary>
    /// Copies the specified URI with the specified text to the clipboard.
    /// </summary>
    /// <param name="text">Text to associated with the link.</param>
    /// <param name="uri">URI the link is to.</param>
    void CopyUri(string text, string uri);

    /// <summary>
    /// Copies a rendered version of the Markdown to the clipboard with
    /// a plain text fallback for text editors.
    /// </summary>
    /// <param name="markdown">Markdown to render and put on the clipboard.</param>
    /// <param name="plainText">Plain text fallback for text editors.</param>
    void CopyMarkdown(string markdown, string plainText);

    /// <summary>
    /// Copies the specified HTML to the clipboard with a plain text
    /// fallback for text editors.
    /// </summary>
    /// <param name="html">HTML to put on the clipboard.</param>
    /// <param name="plainText">Plain text fallback for text editors.</param>
    void CopyHtml(string html, string plainText);

    /// <summary>
    /// Escapes any Markdown syntax characters.
    /// </summary>
    /// <param name="value">Text to escape.</param>
    /// <returns>Escaped text.</returns>
    string EscapeMarkdown(string value);
}
