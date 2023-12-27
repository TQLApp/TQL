using System.Windows.Forms;

namespace Tql.Abstractions;

/// <summary>
/// Represents an interactive authentication operation.
/// </summary>
/// <remarks>
/// <para>
/// If you can, you should use interactive authentication to
/// authenticate the user with online resources.
/// </para>
///
/// <para>
/// Interactive authentication is initiated by calling
/// <see cref="IUI.PerformInteractiveAuthentication(IInteractiveAuthentication)"/>.
/// This will show a dialog to the user telling him that he needs
/// to enter credentials. The <see cref="ResourceName"/> is used
/// to tell the user what resource is being authenticated.
/// You should include the name of a connection in this to ensure
/// the user knows what credentials he needs to enter. If the
/// user proceeds to authenticate, the <see cref="Authenticate(IWin32Window)"/>
/// method is called to allow you to show UI to the user.
/// </para>
/// </remarks>
public interface IInteractiveAuthentication
{
    /// <summary>
    /// Gets the name of the resource that requires authentication.
    /// </summary>
    string ResourceName { get; }

    /// <summary>
    /// Show UI to the user to allow him to authenticate with the resource.
    /// </summary>
    /// <remarks>
    /// If you're using a library to talk to the online resource,
    /// there's a good chance the interactive authentication functionality
    /// takes a <see cref="IWin32Window"/> as the owner. However, if you're
    /// creating your own WPF UI, you should use <see cref="System.Windows.Interop.WindowInteropHelper.Owner"/>
    /// to set the owner of your window to the provided handle.
    /// </remarks>
    /// <param name="owner">Window to use as the owner for the authentication window.</param>
    /// <returns>Task representing the operation.</returns>
    Task Authenticate(IWin32Window owner);
}
