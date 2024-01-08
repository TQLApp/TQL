#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.UI.Shell;

namespace Tql.App.Interop;

/// <summary>
/// Enables extraction of icons for any file type from
/// the Shell.
/// </summary>
internal class FileIcon
{
    /// <summary>
    /// Gets/sets the flags used to extract the icon
    /// </summary>
    public SHGFI_FLAGS Flags { get; set; }

    /// <summary>
    /// Gets/sets the filename to get the icon for
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets the icon for the chosen file
    /// </summary>
    public Icon ShellIcon { get; private set; }

    /// <summary>
    /// Gets the display name for the selected file
    /// if the DisplayName flag was set.
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Gets the type name for the selected file
    /// if the TypeName flag was set.
    /// </summary>
    public string TypeName { get; private set; }

    /// <summary>
    ///  Gets the information for the specified
    ///  file name and flags.
    /// </summary>
    public void GetInfo()
    {
        ShellIcon = null;
        TypeName = "";
        DisplayName = "";

        var shfi = new SHFILEINFOW();
        uint shfiSize = (uint)Marshal.SizeOf(shfi.GetType());

        unsafe
        {
            var ret = PInvoke.SHGetFileInfo(FileName, default, &shfi, shfiSize, Flags);
            if (ret == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        if (shfi.hIcon != IntPtr.Zero)
        {
            ShellIcon = Icon.FromHandle(shfi.hIcon);
            // Now owned by the GDI+ object
            //DestroyIcon(shfi.hIcon);
        }
        TypeName = shfi.szTypeName.ToString();
        DisplayName = shfi.szDisplayName.ToString();
    }

    /// <summary>
    /// Constructs a new, default instance of the FileIcon
    /// class.  Specify the filename and call GetInfo()
    /// to retrieve an icon.
    /// </summary>
    public FileIcon()
    {
        Flags =
            SHGFI_FLAGS.SHGFI_ICON
            | SHGFI_FLAGS.SHGFI_DISPLAYNAME
            | SHGFI_FLAGS.SHGFI_TYPENAME
            | SHGFI_FLAGS.SHGFI_ATTRIBUTES
            | SHGFI_FLAGS.SHGFI_EXETYPE;
    }

    /// <summary>
    /// Constructs a new instance of the FileIcon class
    /// and retrieves the icon, display name and type name
    /// for the specified file.
    /// </summary>
    /// <param name="fileName">The filename to get the icon,
    /// display name and type name for</param>
    public FileIcon(string fileName)
        : this()
    {
        FileName = fileName;
        GetInfo();
    }

    /// <summary>
    /// Constructs a new instance of the FileIcon class
    /// and retrieves the information specified in the
    /// flags.
    /// </summary>
    /// <param name="fileName">The filename to get information
    /// for</param>
    /// <param name="flags">The flags to use when extracting the
    /// icon and other shell information.</param>
    public FileIcon(string fileName, SHGFI_FLAGS flags)
    {
        FileName = fileName;
        Flags = flags;
        GetInfo();
    }
}
