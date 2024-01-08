#nullable disable

// Taken from http://vbaccelerator.com/home/NET/Code/Libraries/Shell_Projects/Creating_and_Modifying_Shortcuts/article.asp

using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Tql.App.Interop;

/// <summary>
/// Utility class to aid working with shortcut (.lnk) files.
/// </summary>
internal sealed class ShellLink : IDisposable
{
    private const int PATH_BUFFER_SIZE = 260;
    private const int DESCRIPTION_BUFFER_SIZE = 1024;

    public static void NotifyShellLinksChanged()
    {
        unsafe
        {
            PInvoke.SHChangeNotify(SHCNE_ID.SHCNE_ASSOCCHANGED, SHCNF_FLAGS.SHCNF_IDLIST);
        }
    }

    // Use Unicode (W) under NT, otherwise use ANSI
    private IShellLinkW _link;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShellLink"/> class.
    /// </summary>
    public ShellLink()
    {
        _link = (IShellLinkW)new Windows.Win32.UI.Shell.ShellLink();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShellLink"/> class from a
    /// specific link file.
    /// </summary>
    /// <param name="linkFile">The Shortcut file to open.</param>
    public ShellLink(string linkFile)
        : this()
    {
        Open(linkFile);
    }

    /// <summary>
    /// Releases all resources used by the <see cref="ShellLink"/>.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            if (_link != null)
            {
                Marshal.FinalReleaseComObject(_link);
                _link = null;
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Get or sets the path of the shortcut file.
    /// </summary>
    public string ShortcutFile { get; set; } = "";

    public Icon LargeIcon => GetIcon(true);

    public Icon SmallIcon => GetIcon(false);

    private Icon GetIcon(bool large)
    {
        // Get icon index and path:
        int iconIndex;
        string iconFile;

        unsafe
        {
            fixed (char* iconFileBuffer = new char[PATH_BUFFER_SIZE])
            {
                _link.GetIconLocation(iconFileBuffer, PATH_BUFFER_SIZE, out iconIndex);
                iconFile = new string(iconFileBuffer);
            }
        }

        // If there are no details set for the icon, then we must use
        // the shell to get the icon for the target:
        if (iconFile.Length == 0)
        {
            // Use the FileIcon object to get the icon:
            var flags = SHGFI_FLAGS.SHGFI_ICON | SHGFI_FLAGS.SHGFI_ATTRIBUTES;

            if (large)
                flags |= SHGFI_FLAGS.SHGFI_LARGEICON;
            else
                flags |= SHGFI_FLAGS.SHGFI_SMALLICON;

            var fileIcon = new FileIcon(Target, flags);
            return fileIcon.ShellIcon;
        }
        else
        {
            // Use ExtractIconEx to get the icon:
            HICON hIcon;

            unsafe
            {
                fixed (char* iconFileBuffer = iconFile)
                {
                    if (large)
                        PInvoke.ExtractIconEx(iconFileBuffer, iconIndex, &hIcon, null, 1);
                    else
                        PInvoke.ExtractIconEx(iconFileBuffer, iconIndex, null, &hIcon, 1);
                }
            }

            // If success then return as a GDI+ object
            Icon icon = null;
            if (hIcon != IntPtr.Zero)
            {
                icon = Icon.FromHandle(hIcon);
                //UnManagedMethods.DestroyIcon(hIconEx[0]);
            }
            return icon;
        }
    }

    /// <summary>
    /// Get or sets the path to the file containing the icon for this shortcut.
    /// </summary>
    public string IconPath
    {
        get
        {
            unsafe
            {
                fixed (char* iconPath = new char[PATH_BUFFER_SIZE])
                {
                    _link.GetIconLocation(iconPath, PATH_BUFFER_SIZE, out _);
                    return new string(iconPath);
                }
            }
        }
        set
        {
            unsafe
            {
                fixed (char* iconPath = new char[PATH_BUFFER_SIZE])
                {
                    _link.GetIconLocation(iconPath, PATH_BUFFER_SIZE, out var iconIndex);
                    _link.SetIconLocation(value, iconIndex);
                }
            }
        }
    }

    /// <summary>
    /// Get or sets the index of this icon within the icon path's resources.
    /// </summary>
    public int IconIndex
    {
        get
        {
            unsafe
            {
                fixed (char* iconPath = new char[PATH_BUFFER_SIZE])
                {
                    _link.GetIconLocation(iconPath, PATH_BUFFER_SIZE, out var iconIndex);
                    return iconIndex;
                }
            }
        }
        set
        {
            unsafe
            {
                fixed (char* iconPath = new char[PATH_BUFFER_SIZE])
                {
                    _link.GetIconLocation(iconPath, PATH_BUFFER_SIZE, out int _);
                    _link.SetIconLocation(new string(iconPath), value);
                }
            }
        }
    }

    /// <summary>
    /// Get or sets the fully qualified path to the link's target.
    /// </summary>
    public string Target
    {
        get
        {
            unsafe
            {
                fixed (char* target = new char[PATH_BUFFER_SIZE])
                {
                    var fd = new WIN32_FIND_DATAW();
                    _link.GetPath(
                        target,
                        PATH_BUFFER_SIZE,
                        ref fd,
                        (uint)SLGP_FLAGS.SLGP_UNCPRIORITY
                    );
                    return new string(target);
                }
            }
        }
        set => _link.SetPath(value);
    }

    /// <summary>
    /// Get or sets the working directory for the Link.
    /// </summary>
    public string WorkingDirectory
    {
        get
        {
            unsafe
            {
                fixed (char* path = new char[PATH_BUFFER_SIZE])
                {
                    _link.GetWorkingDirectory(path, PATH_BUFFER_SIZE);
                    return new string(path);
                }
            }
        }
        set => _link.SetWorkingDirectory(value);
    }

    /// <summary>
    /// Get or sets the description of the link.
    /// </summary>
    public string Description
    {
        get
        {
            unsafe
            {
                fixed (char* description = new char[DESCRIPTION_BUFFER_SIZE])
                {
                    _link.GetDescription(description, DESCRIPTION_BUFFER_SIZE);
                    return new string(description);
                }
            }
        }
        set => _link.SetDescription(value);
    }

    /// <summary>
    /// Get or sets any command line arguments associated with the link.
    /// </summary>
    public string Arguments
    {
        get
        {
            unsafe
            {
                fixed (char* arguments = new char[PATH_BUFFER_SIZE])
                {
                    _link.GetArguments(arguments, PATH_BUFFER_SIZE);
                    return new string(arguments);
                }
            }
        }
        set => _link.SetArguments(value);
    }

    /// <summary>
    /// Get or sets the initial display mode when the shortcut is
    /// run.
    /// </summary>
    public SHOW_WINDOW_CMD DisplayMode
    {
        get
        {
            _link.GetShowCmd(out var cmd);
            return cmd;
        }
        set => _link.SetShowCmd(value);
    }

    public Keys HotKey
    {
        get
        {
            _link.GetHotkey(out var key);
            return (Keys)key;
        }
        set => _link.SetHotkey((ushort)value);
    }

    /// <summary>
    /// Saves the shortcut to the path stored in <see cref="ShortcutFile"/>.
    /// </summary>
    public void Save()
    {
        Save(ShortcutFile);
    }

    /// <summary>
    /// Saves the shortcut to the specified location/
    /// </summary>
    /// <param name="linkFile">The path to store the shortcut to.</param>
    public void Save(string linkFile)
    {
        // Save the object to disk
        ((IPersistFile)_link).Save(linkFile, true);
        ShortcutFile = linkFile;
    }

    /// <summary>
    /// Loads a shortcut from the specified location.
    /// </summary>
    /// <param name="linkFile">The shortcut file to load.</param>
    public void Open(string linkFile)
    {
        Open(linkFile, IntPtr.Zero, SLR_FLAGS.SLR_ANY_MATCH | SLR_FLAGS.SLR_NO_UI, 1);
    }

    /// <summary>
    /// Loads a shortcut from the specified location, and allows flags controlling
    /// the UI behavior if the shortcut's target isn't found to be set.
    /// </summary>
    /// <param name="linkFile">The path to load the shortcut from.</param>
    /// <param name="hWnd">The window handle of the application's UI, if any.</param>
    /// <param name="resolveFlags">Flags controlling resolution behavior.</param>
    public void Open(string linkFile, IntPtr hWnd, SLR_FLAGS resolveFlags)
    {
        Open(linkFile, hWnd, resolveFlags, 1);
    }

    /// <summary>
    /// Loads a shortcut from the specified location, and allows flags controlling
    /// the UI behavior if the shortcut's target isn't found to be set and a timeout.
    /// </summary>
    /// <param name="linkFile">The path to load the shortcut from.</param>
    /// <param name="hWnd">The window handle of the application's UI, if any.</param>
    /// <param name="resolveFlags">Flags controlling resolution behavior.</param>
    /// <param name="timeout">Timeout if <c>SLR_NO_UI</c> is specified, in milliseconds.</param>
    public void Open(string linkFile, IntPtr hWnd, SLR_FLAGS resolveFlags, ushort timeout)
    {
        uint flags;

        if ((resolveFlags & SLR_FLAGS.SLR_NO_UI) == SLR_FLAGS.SLR_NO_UI)
            flags = (uint)((int)resolveFlags | (timeout << 16));
        else
            flags = (uint)resolveFlags;

        ((IPersistFile)_link).Load(linkFile, 0); //STGM_DIRECT)
        _link.Resolve(new HWND(hWnd), flags);
        ShortcutFile = linkFile;
    }
}
