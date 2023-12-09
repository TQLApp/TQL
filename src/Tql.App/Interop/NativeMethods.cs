#nullable disable

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Tql.App.Interop;

internal static class NativeMethods
{
    /// <summary>
    /// Allow any match during resolution.  Has no effect
    /// on ME/2000 or above, use the other flags instead.
    /// </summary>
    public const uint SLR_ANY_MATCH = 0x2;

    /// <summary>
    /// Call the Microsoft Windows Installer.
    /// </summary>
    public const uint SLR_INVOKE_MSI = 0x80;

    /// <summary>
    /// Disable distributed link tracking. By default,
    /// distributed link tracking tracks removable media
    /// across multiple devices based on the volume name.
    /// It also uses the UNC path to track remote file
    /// systems whose drive letter has changed. Setting
    /// SLR_NOLINKINFO disables both types of tracking.
    /// </summary>
    public const uint SLR_NOLINKINFO = 0x40;

    /// <summary>
    /// Do not display a dialog box if the link cannot be resolved.
    /// When SLR_NO_UI is set, a time-out value that specifies the
    /// maximum amount of time to be spent resolving the link can
    /// be specified in milliseconds. The function returns if the
    /// link cannot be resolved within the time-out duration.
    /// If the timeout is not set, the time-out duration will be
    /// set to the default value of 3,000 milliseconds (3 seconds).
    /// </summary>
    public const uint SLR_NO_UI = 0x1;

    /// <summary>
    /// Not documented in SDK.  Assume same as SLR_NO_UI but
    /// intended for applications without a hWnd.
    /// </summary>
    public const uint SLR_NO_UI_WITH_MSG_PUMP = 0x101;

    /// <summary>
    /// Do not update the link information.
    /// </summary>
    public const uint SLR_NOUPDATE = 0x8;

    /// <summary>
    /// Do not execute the search heuristics.
    /// </summary>
    public const uint SLR_NOSEARCH = 0x10;

    /// <summary>
    /// Do not use distributed link tracking.
    /// </summary>
    public const uint SLR_NOTRACK = 0x20;

    public const int SHCNE_ASSOCCHANGED = 0x8000000;

    public const uint SHCNF_IDLIST = 0;

    public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
    public const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
    public const int FORMAT_MESSAGE_FROM_HMODULE = 0x800;
    public const int FORMAT_MESSAGE_FROM_STRING = 0x400;
    public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
    public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
    public const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0xFF;

    /// <summary>
    /// If the link object has changed, update its path and list
    /// of identifiers. If SLR_UPDATE is set, you do not need to
    /// call IPersistFile::IsDirty to determine whether or not
    /// the link object has changed.
    /// </summary>
    public const uint SLR_UPDATE = 0x4;

    [DllImport("user32", CharSet = CharSet.Auto)]
    public static extern int ExtractIconEx(
        [MarshalAs(UnmanagedType.LPTStr)] string lpszFile,
        int nIconIndex,
        IntPtr[] phIconLarge,
        IntPtr[] phIconSmall,
        int nIcons
    );

    [DllImport("shell32.dll")]
    public static extern void SHChangeNotify(
        int wEventId,
        uint uFlags,
        IntPtr dwItem1,
        IntPtr dwItem2
    );

    [DllImport("shell32")]
    public static extern IntPtr SHGetFileInfo(
        string pszPath,
        uint dwFileAttributes,
        ref SHFILEINFO psfi,
        uint cbSizeFileInfo,
        uint uFlags
    );

    [DllImport("kernel32")]
    public static extern int GetLastError();

    [DllImport("kernel32")]
    public static extern int FormatMessage(
        int dwFlags,
        IntPtr lpSource,
        int dwMessageId,
        int dwLanguageId,
        string lpBuffer,
        uint nSize,
        int argumentsLong
    );

    [ComImport]
    [Guid("0000010B-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPersistFile
    {
        // can't get this to go if I extend IPersist, so put it here:
        [PreserveSig]
        void GetClassID(out Guid pClassID);

        //[helpstring("Checks for changes since last file write")]
        void IsDirty();

        //[helpstring("Opens the specified file and initializes the object from its contents")]
        void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);

        //[helpstring("Saves the object into the specified file")]
        void Save(
            [MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
            [MarshalAs(UnmanagedType.Bool)] bool fRemember
        );

        //[helpstring("Notifies the object that save is completed")]
        void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

        //[helpstring("Gets the current name of the file associated with the object")]
        void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
    }

    [ComImport]
    [Guid("000214EE-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IShellLinkA
    {
        //[helpstring("Retrieves the path and filename of a shell link object")]
        void GetPath(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile,
            int cchMaxPath,
            ref _WIN32_FIND_DATAA pfd,
            uint fFlags
        );

        //[helpstring("Retrieves the list of shell link item identifiers")]
        void GetIDList(out IntPtr ppidl);

        //[helpstring("Sets the list of shell link item identifiers")]
        void SetIDList(IntPtr pidl);

        //[helpstring("Retrieves the shell link description string")]
        void GetDescription(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile,
            int cchMaxName
        );

        //[helpstring("Sets the shell link description string")]
        void SetDescription([MarshalAs(UnmanagedType.LPStr)] string pszName);

        //[helpstring("Retrieves the name of the shell link working directory")]
        void GetWorkingDirectory(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszDir,
            int cchMaxPath
        );

        //[helpstring("Sets the name of the shell link working directory")]
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPStr)] string pszDir);

        //[helpstring("Retrieves the shell link command-line arguments")]
        void GetArguments(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszArgs,
            int cchMaxPath
        );

        //[helpstring("Sets the shell link command-line arguments")]
        void SetArguments([MarshalAs(UnmanagedType.LPStr)] string pszArgs);

        //[propget, helpstring("Retrieves or sets the shell link hot key")]
        void GetHotkey(out short pwHotkey);

        //[propput, helpstring("Retrieves or sets the shell link hot key")]
        void SetHotkey(short pwHotkey);

        //[propget, helpstring("Retrieves or sets the shell link show command")]
        void GetShowCmd(out uint piShowCmd);

        //[propput, helpstring("Retrieves or sets the shell link show command")]
        void SetShowCmd(uint piShowCmd);

        //[helpstring("Retrieves the location (path and index) of the shell link icon")]
        void GetIconLocation(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszIconPath,
            int cchIconPath,
            out int piIcon
        );

        //[helpstring("Sets the location (path and index) of the shell link icon")]
        void SetIconLocation([MarshalAs(UnmanagedType.LPStr)] string pszIconPath, int iIcon);

        //[helpstring("Sets the shell link relative path")]
        void SetRelativePath([MarshalAs(UnmanagedType.LPStr)] string pszPathRel, uint dwReserved);

        //[helpstring("Resolves a shell link. The system searches for the shell link object and updates the shell link path and its list of identifiers (if necessary)")]
        void Resolve(IntPtr hWnd, uint fFlags);

        //[helpstring("Sets the shell link path and filename")]
        void SetPath([MarshalAs(UnmanagedType.LPStr)] string pszFile);
    }

    [ComImport]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IShellLinkW
    {
        //[helpstring("Retrieves the path and filename of a shell link object")]
        void GetPath(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
            int cchMaxPath,
            ref _WIN32_FIND_DATAW pfd,
            uint fFlags
        );

        //[helpstring("Retrieves the list of shell link item identifiers")]
        void GetIDList(out IntPtr ppidl);

        //[helpstring("Sets the list of shell link item identifiers")]
        void SetIDList(IntPtr pidl);

        //[helpstring("Retrieves the shell link description string")]
        void GetDescription(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
            int cchMaxName
        );

        //[helpstring("Sets the shell link description string")]
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

        //[helpstring("Retrieves the name of the shell link working directory")]
        void GetWorkingDirectory(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir,
            int cchMaxPath
        );

        //[helpstring("Sets the name of the shell link working directory")]
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

        //[helpstring("Retrieves the shell link command-line arguments")]
        void GetArguments(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs,
            int cchMaxPath
        );

        //[helpstring("Sets the shell link command-line arguments")]
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

        //[propget, helpstring("Retrieves or sets the shell link hot key")]
        void GetHotkey(out short pwHotkey);

        //[propput, helpstring("Retrieves or sets the shell link hot key")]
        void SetHotkey(short pwHotkey);

        //[propget, helpstring("Retrieves or sets the shell link show command")]
        void GetShowCmd(out uint piShowCmd);

        //[propput, helpstring("Retrieves or sets the shell link show command")]
        void SetShowCmd(uint piShowCmd);

        //[helpstring("Retrieves the location (path and index) of the shell link icon")]
        void GetIconLocation(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
            int cchIconPath,
            out int piIcon
        );

        //[helpstring("Sets the location (path and index) of the shell link icon")]
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

        //[helpstring("Sets the shell link relative path")]
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);

        //[helpstring("Resolves a shell link. The system searches for the shell link object and updates the shell link path and its list of identifiers (if necessary)")]
        void Resolve(IntPtr hWnd, uint fFlags);

        //[helpstring("Sets the shell link path and filename")]
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [Guid("00021401-0000-0000-C000-000000000046")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComImport]
    public class CShellLink;

    public enum EShellLinkGP : uint
    {
        SLGP_SHORTPATH = 1,
        SLGP_UNCPRIORITY = 2
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("0000000B-0000-0000-C000-000000000046")
    ]
    public interface IStorage
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        IStream CreateStream(
            [In, MarshalAs(UnmanagedType.BStr)] string pwcsName,
            [In, MarshalAs(UnmanagedType.U4)] int grfMode,
            [In, MarshalAs(UnmanagedType.U4)] int reserved1,
            [In, MarshalAs(UnmanagedType.U4)] int reserved2
        );

        [return: MarshalAs(UnmanagedType.Interface)]
        IStream OpenStream(
            [In, MarshalAs(UnmanagedType.BStr)] string pwcsName,
            IntPtr reserved1,
            [In, MarshalAs(UnmanagedType.U4)] int grfMode,
            [In, MarshalAs(UnmanagedType.U4)] int reserved2
        );

        [return: MarshalAs(UnmanagedType.Interface)]
        IStorage CreateStorage(
            [In, MarshalAs(UnmanagedType.BStr)] string pwcsName,
            [In, MarshalAs(UnmanagedType.U4)] int grfMode,
            [In, MarshalAs(UnmanagedType.U4)] int reserved1,
            [In, MarshalAs(UnmanagedType.U4)] int reserved2
        );

        [return: MarshalAs(UnmanagedType.Interface)]
        IStorage OpenStorage(
            [In, MarshalAs(UnmanagedType.BStr)] string pwcsName,
            IntPtr pstgPriority,
            [In, MarshalAs(UnmanagedType.U4)] int grfMode,
            IntPtr snbExclude,
            [In, MarshalAs(UnmanagedType.U4)] int reserved
        );
        void CopyTo(
            int ciidExclude,
            [In, MarshalAs(UnmanagedType.LPArray)] Guid[] pIIDExclude,
            IntPtr snbExclude,
            [In, MarshalAs(UnmanagedType.Interface)] IStorage stgDest
        );
        void MoveElementTo(
            [In, MarshalAs(UnmanagedType.BStr)] string pwcsName,
            [In, MarshalAs(UnmanagedType.Interface)] IStorage stgDest,
            [In, MarshalAs(UnmanagedType.BStr)] string pwcsNewName,
            [In, MarshalAs(UnmanagedType.U4)] int grfFlags
        );
        void Commit(int grfCommitFlags);
        void Revert();
        void EnumElements(
            [In, MarshalAs(UnmanagedType.U4)] int reserved1,
            IntPtr reserved2,
            [In, MarshalAs(UnmanagedType.U4)] int reserved3,
            [MarshalAs(UnmanagedType.Interface)] out object ppVal
        );
        void DestroyElement([In, MarshalAs(UnmanagedType.BStr)] string pwcsName);
        void RenameElement(
            [In, MarshalAs(UnmanagedType.BStr)] string pwcsOldName,
            [In, MarshalAs(UnmanagedType.BStr)] string pwcsNewName
        );
        void SetElementTimes(
            [In, MarshalAs(UnmanagedType.BStr)] string pwcsName,
            [In] FILETIME pctime,
            [In] FILETIME patime,
            [In] FILETIME pmtime
        );
        void SetClass([In] ref Guid clsid);
        void SetStateBits(int grfStateBits, int grfMask);
        void Stat([Out] out STATSTG pStatStg, int grfStatFlag);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0, CharSet = CharSet.Unicode)]
    public struct _WIN32_FIND_DATAW
    {
        public uint dwFileAttributes;
        public _FILETIME ftCreationTime;
        public _FILETIME ftLastAccessTime;
        public _FILETIME ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] // MAX_PATH
        public string cFileName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0, CharSet = CharSet.Ansi)]
    public struct _WIN32_FIND_DATAA
    {
        public uint dwFileAttributes;
        public _FILETIME ftCreationTime;
        public _FILETIME ftLastAccessTime;
        public _FILETIME ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] // MAX_PATH
        public string cFileName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0)]
    public struct _FILETIME
    {
        public uint dwLowDateTime;
        public uint dwHighDateTime;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }
}
