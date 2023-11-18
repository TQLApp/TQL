namespace Tql.Abstractions;

/// <summary>
/// Represents a runnable match.
/// </summary>
/// <remarks>
/// <para>
/// Implement this interface on your match class if the match represents
/// anything that can be run. A running man icon will be shown next
/// to the search result to indicate that the match can be activated.
/// </para>
///
/// <para>
/// Most of the time this is the case if the
/// matched resource can be turned into a URL. Use <see cref="IUI.OpenUrl(string)"/>
/// to open the URL in that case.
/// </para>
///
/// <para>
/// However, it's also possible for a match
/// to show UI itself. If e.g. your plugin provides the capability to create
/// tickets, and you want to ask the user for input to create the ticket,
/// you can do that also. In this case though, make sure you correctly
/// set the owner of the screen. If you're creating a WPF screen,
/// use <see cref="System.Windows.Interop.WindowInteropHelper.Owner"/>
/// to set the owner of your window to the provided handle.
/// </para>
/// </remarks>
public interface IRunnableMatch : IMatch
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    Task Run(IServiceProvider serviceProvider, IWin32Window owner);
}
