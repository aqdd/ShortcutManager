using System;
using System.Runtime.InteropServices;
using System.Text;
using ShortcutManager.Model;

namespace ShortcutManager.Core;

public class ShFolder
{
    [DllImport("shfolder.dll", CharSet = CharSet.Auto)]
    internal static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags,
        StringBuilder lpszPath);


    [Flags()]
    enum SLGP_FLAGS
    {
        /// <summary>Retrieves the standard short (8.3 format) file name</summary>
        SLGP_SHORTPATH = 0x1,

        /// <summary>Retrieves the Universal Naming Convention (UNC) path name of the file</summary>
        SLGP_UNCPRIORITY = 0x2,

        /// <summary>Retrieves the raw path name. A raw path is something that might not exist and may include environment variables that need to be expanded</summary>
        SLGP_RAWPATH = 0x4
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct WIN32_FIND_DATAW
    {
        public uint dwFileAttributes;
        public long ftCreationTime;
        public long ftLastAccessTime;
        public long ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;
    }

    [Flags()]
    enum SLR_FLAGS
    {
        /// <summary>
        /// Do not display a dialog box if the link cannot be resolved. When SLR_NO_UI is set,
        /// the high-order word of fFlags can be set to a time-out value that specifies the
        /// maximum amount of time to be spent resolving the link. The function returns if the
        /// link cannot be resolved within the time-out duration. If the high-order word is set
        /// to zero, the time-out duration will be set to the default value of 3,000 milliseconds
        /// (3 seconds). To specify a value, set the high word of fFlags to the desired time-out
        /// duration, in milliseconds.
        /// </summary>
        SLR_NO_UI = 0x1,

        /// <summary>Obsolete and no longer used</summary>
        SLR_ANY_MATCH = 0x2,

        /// <summary>If the link object has changed, update its path and list of identifiers.
        /// If SLR_UPDATE is set, you do not need to call IPersistFile::IsDirty to determine
        /// whether or not the link object has changed.</summary>
        SLR_UPDATE = 0x4,

        /// <summary>Do not update the link information</summary>
        SLR_NOUPDATE = 0x8,

        /// <summary>Do not execute the search heuristics</summary>
        SLR_NOSEARCH = 0x10,

        /// <summary>Do not use distributed link tracking</summary>
        SLR_NOTRACK = 0x20,

        /// <summary>Disable distributed link tracking. By default, distributed link tracking tracks
        /// removable media across multiple devices based on the volume name. It also uses the
        /// Universal Naming Convention (UNC) path to track remote file systems whose drive letter
        /// has changed. Setting SLR_NOLINKINFO disables both types of tracking.</summary>
        SLR_NOLINKINFO = 0x40,

        /// <summary>Call the Microsoft Windows Installer</summary>
        SLR_INVOKE_MSI = 0x80
    }

    /// <summary>The IShellLink interface allows Shell links to be created, modified, and resolved</summary>
    [ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214F9-0000-0000-C000-000000000046")]
    interface IShellLinkW
    {
        /// <summary>Retrieves the path and file name of a Shell link object</summary>
        void GetPath([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath,
            out WIN32_FIND_DATAW pfd, SLGP_FLAGS fFlags);

        /// <summary>Retrieves the list of item identifiers for a Shell link object</summary>
        void GetIDList(out IntPtr ppidl);

        /// <summary>Sets the pointer to an item identifier list (PIDL) for a Shell link object.</summary>
        void SetIDList(IntPtr pidl);

        /// <summary>Retrieves the description string for a Shell link object</summary>
        void GetDescription([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);

        /// <summary>Sets the description for a Shell link object. The description can be any application-defined string</summary>
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

        /// <summary>Retrieves the name of the working directory for a Shell link object</summary>
        void GetWorkingDirectory([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);

        /// <summary>Sets the name of the working directory for a Shell link object</summary>
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

        /// <summary>Retrieves the command-line arguments associated with a Shell link object</summary>
        void GetArguments([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);

        /// <summary>Sets the command-line arguments for a Shell link object</summary>
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

        /// <summary>Retrieves the hot key for a Shell link object</summary>
        void GetHotkey(out short pwHotkey);

        /// <summary>Sets a hot key for a Shell link object</summary>
        void SetHotkey(short wHotkey);

        /// <summary>Retrieves the show command for a Shell link object</summary>
        void GetShowCmd(out int piShowCmd);

        /// <summary>Sets the show command for a Shell link object. The show command sets the initial show state of the window.</summary>
        void SetShowCmd(int iShowCmd);

        /// <summary>Retrieves the location (path and index) of the icon for a Shell link object</summary>
        void GetIconLocation([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
            int cchIconPath, out int piIcon);

        /// <summary>Sets the location (path and index) of the icon for a Shell link object</summary>
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

        /// <summary>Sets the relative path to the Shell link object</summary>
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);

        /// <summary>Attempts to find the target of a Shell link, even if it has been moved or renamed</summary>
        void Resolve(IntPtr hwnd, SLR_FLAGS fFlags);

        /// <summary>Sets the path and file name of a Shell link object</summary>
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [ComImport()]
    [Guid("000214fa-0000-0000-c000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    //http://msdn.microsoft.com/en-us/library/windows/desktop/bb761852(v=vs.85).aspx
    interface IExtractIcon
    {
        /// <summary>
        /// Gets the location and index of an icon.
        /// </summary>
        /// <param name="uFlags">One or more of the following values. This parameter can also be NULL.use GIL_ Consts</param>
        /// <param name="szIconFile">A pointer to a buffer that receives the icon location. The icon location is a null-terminated string that identifies the file that contains the icon.</param>
        /// <param name="cchMax">The size of the buffer, in characters, pointed to by pszIconFile.</param>
        /// <param name="piIndex">A pointer to an int that receives the index of the icon in the file pointed to by pszIconFile.</param>
        /// <param name="pwFlags">A pointer to a UINT value that receives zero or a combination of the following value</param>
        /// <returns></returns>
        ///
        [PreserveSig]
        int GetIconLocation(IExtractIconuFlags uFlags,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 2)] StringBuilder szIconFile, int cchMax,
            out int piIndex, out IExtractIconpwFlags pwFlags);
        /// <summary>
        /// Extracts an icon image from the specified location.
        /// </summary>
        /// <param name="pszFile">A pointer to a null-terminated string that specifies the icon location.</param>
        /// <param name="nIconIndex">The index of the icon in the file pointed to by pszFile.</param>
        /// <param name="phiconLarge">A pointer to an HICON value that receives the handle to the large icon. This parameter may be NULL.</param>
        /// <param name="phiconSmall">A pointer to an HICON value that receives the handle to the small icon. This parameter may be NULL.</param>
        /// <param name="nIconSize">The desired size of the icon, in pixels. The low word contains the size of the large icon, and the high word contains the size of the small icon. The size specified can be the width or height. The width of an icon always equals its height.</param>
        /// <returns>
        /// Returns S_OK if the function extracted the icon, or S_FALSE if the calling application should extract the icon.
        /// </returns>
        [PreserveSig]
        int Extract([MarshalAs(UnmanagedType.LPWStr)] string pszFile,uint nIconIndex,out IntPtr phiconLarge,out IntPtr phiconSmall,uint nIconSize);
    }
    [Flags()]
    public enum IExtractIconuFlags:uint
    {
        GIL_ASYNC=0x0020,
        GIL_DEFAULTICON =0x0040,
        GIL_FORSHELL =0x0002,
        GIL_FORSHORTCUT =0x0080,
        GIL_OPENICON = 0x0001,
        GIL_CHECKSHIELD = 0x0200
    }

    [Flags()]
    public enum IExtractIconpwFlags : uint
    {
        GIL_DONTCACHE = 0x0010,
        GIL_NOTFILENAME = 0x0008,
        GIL_PERCLASS = 0x0004,
        GIL_PERINSTANCE = 0x0002,
        GIL_SIMULATEDOC = 0x0001,
        GIL_SHIELD = 0x0200,//Windows Vista only
        GIL_FORCENOSHIELD = 0x0400//Windows Vista only
    }


    [ComImport, Guid("0000010c-0000-0000-c000-000000000046"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPersist
    {
        [PreserveSig]
        void GetClassID(out Guid pClassID);
    }


    [ComImport, Guid("0000010b-0000-0000-C000-000000000046"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPersistFile : IPersist
    {
        new void GetClassID(out Guid pClassID);

        [PreserveSig]
        int IsDirty();

        [PreserveSig]
        void Load([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);

        [PreserveSig]
        void Save([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
            [In, MarshalAs(UnmanagedType.Bool)] bool fRemember);

        [PreserveSig]
        void SaveCompleted([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

        [PreserveSig]
        void GetCurFile([In, MarshalAs(UnmanagedType.LPWStr)] string ppszFileName);
    }

    const uint STGM_READ = 0;
    const int MAX_PATH = 260;
    
    

// CLSID_ShellLink from ShlGuid.h 
    [
        ComImport(),
        Guid("00021401-0000-0000-C000-000000000046")
    ]
    public class ShellLink
    {
    }
    
    public static Shortcut ResolveShortcut(string filename)
    {
        ShellLink link = new ShellLink();
        ((IPersistFile)link).Load(filename, STGM_READ);
        // TODO: if I can get hold of the hwnd call resolve first. This handles moved and renamed files.  
        // ((IShellLinkW)link).Resolve(hwnd, 0) 
        StringBuilder path = new StringBuilder(MAX_PATH);
        StringBuilder args = new StringBuilder(MAX_PATH);
        StringBuilder wd = new StringBuilder(MAX_PATH);
        StringBuilder il = new StringBuilder(MAX_PATH);
        StringBuilder desc = new StringBuilder(MAX_PATH);
        WIN32_FIND_DATAW data = new WIN32_FIND_DATAW();
        ((IShellLinkW)link).GetPath(path, path.Capacity, out data, 0);
        ((IShellLinkW)link).GetArguments(args, args.Capacity);
        ((IShellLinkW)link).GetWorkingDirectory(wd, wd.Capacity);
        ((IShellLinkW)link).GetDescription(desc, desc.Capacity);
        int piIcon = 0;
        ((IShellLinkW)link).GetIconLocation(il, il.Capacity, out piIcon);
        return new Shortcut { Path = path.ToString(), Arguments = args.ToString(), WorkingDirectory = wd.ToString(), IconLocation = il.ToString(), Desc = desc.ToString()};
    }

    public static string ResolveIcon(string path)
    {
        ShellLink link = new ShellLink();
        ((IPersistFile)link).Load(path, STGM_READ);
        StringBuilder sb = new StringBuilder(MAX_PATH);
        int data = 0;
        IExtractIconpwFlags flags;
        // ((IExtractIcon)link).GetIconLocation(IExtractIconuFlags.GIL_DEFAULTICON, sb, MAX_PATH, out data, out flags);
        ((IShellLinkW)link).GetIconLocation(sb, sb.Capacity, out data);
        return sb.ToString();
    }
}