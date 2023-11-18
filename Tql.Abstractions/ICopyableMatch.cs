namespace Tql.Abstractions;

/// <summary>
/// Represents a match that can be copied.
/// </summary>
/// <remarks>
/// <para>
/// Implement this interface on your match if you want to allow the
/// user to copy a match to the clipboard.
/// </para>
///
/// <para>
/// The most straight forward example is if you match points to a URL.
/// You can then use <see cref="IClipboard.CopyUri(string, string)"/>
/// with <see cref="IMatch.Text"/> as the text input and the URL
/// as the URI input.
/// </para>
///
/// <para>
/// Implementing this interface will add the copy button to your match.
/// If the user clicks the button, the <see cref="Copy(IServiceProvider)"/>
/// method will be called.
/// </para>
/// </remarks>
public interface ICopyableMatch : IMatch
{
    /// <summary>
    /// Copies a link to the match to the clipboard.
    /// </summary>
    /// <param name="serviceProvider">Reference to the service provider.</param>
    /// <returns>Task representing the operation.</returns>
    Task Copy(IServiceProvider serviceProvider);
}
