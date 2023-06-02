using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ShortcutManager.Helper;

public class IconHelper
{
    #region 方法1

    /// <summary>
    /// 从文件获取ICON图标[一般是Size(32,32)]
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns></returns>
    public static System.Drawing.Icon GetIconFromFile(string fileName)
    {
        if (System.IO.File.Exists(fileName) == false)
            return null;
        return System.Drawing.Icon.ExtractAssociatedIcon(fileName);
    }

    #endregion

    #region 方法2

    [DllImport("shell32.dll")]
    public static extern int ExtractIconEx(string lpszFile, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall,
        int nIcons);

    /// <summary>
    /// 方法2：从应用程序文件中获取Icon图标
    /// </summary>
    /// <param name="appPath">应用程序文件名</param>
    /// <returns>返回获取到的Icon图标集，顺序为图标A Size(32,32)、图标A Size(16,16)、图标B Size(32,32)、图标B Size(16,16)....</returns>
    public static System.Drawing.Icon[] GetIconFromAPP(string appPath)
    {
        int count = ExtractIconEx(appPath, -1, null, null, 0);

        IntPtr[] largeIcons = new IntPtr[count];

        IntPtr[] smallIcons = new IntPtr[count];

        ExtractIconEx(appPath, 0, largeIcons, smallIcons, count);
        System.Drawing.Icon[] icons = new System.Drawing.Icon[count * 2];
        for (int i = 0; i < count; i++)
        {
            icons[i * 2] = System.Drawing.Icon.FromHandle(largeIcons[i]);
            icons[i * 2 + 1] = System.Drawing.Icon.FromHandle(smallIcons[i]);
        }

        return icons;
    }

    #endregion

    #region 方法3 获取数据全-[推荐]

    [DllImport("shell32.dll", EntryPoint = "#727")]
    public extern static int SHGetImageList(IMAGELIST_SIZE_FLAG iImageList, ref Guid riid, ref IImageList ppv);

    [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
    public static extern int DestroyIcon(IntPtr hIcon);

    [DllImport("shell32.dll")]
    public static extern uint SHGetIDListFromObject([MarshalAs(UnmanagedType.IUnknown)] object iUnknown,
        out IntPtr ppidl);

    [DllImport("Shell32.dll")]
    public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi,
        uint cbFileInfo, uint uFlags);

    /// <summary>
    /// 系统图标大小标识
    /// </summary>
    public enum IMAGELIST_SIZE_FLAG : int
    {
        /// <summary>
        /// Size(32,32)
        /// </summary>
        SHIL_LARGE = 0x0,

        /// <summary>
        /// Size(16,16)
        /// </summary>
        SHIL_SMALL = 0x1,

        /// <summary>
        /// Size(48,48)
        /// </summary>
        SHIL_EXTRALARGE = 0x2,

        /// <summary>
        /// Size(16,16)
        /// </summary>
        SHIL_SYSSMALL = 0x3,

        /// <summary>
        /// Size(256,256)
        /// </summary>
        SHIL_JUMBO = 0x4,

        /// <summary>
        /// 保留使用：目前测试效果为Size(256,256)
        /// </summary>
        SHIL_LAST = 0x4,
    }

    public const int ILD_TRANSPARENT = 0x00000001;
    public const int ILD_IMAGE = 0x00000020;

    public const string IID_IImageList2 = "192B9D83-50FC-457B-90A0-2B82A8B5DAE1"; //GUID的两个com标识中的一个，底层固定
    public const string IID_IImageList = "46EB5926-582E-4017-9FDF-E8998DAA0950"; //GUID的两个com标识中的一个，底层固定

    [Flags]
    public enum SHGFI : uint
    {
        /// <summary>get icon</summary>
        Icon = 0x000000100,

        /// <summary>get display name</summary>
        DisplayName = 0x000000200,

        /// <summary>get type name</summary>
        TypeName = 0x000000400,

        /// <summary>get attributes</summary>
        Attributes = 0x000000800,

        /// <summary>get icon location</summary>
        IconLocation = 0x000001000,

        /// <summary>return exe type</summary>
        ExeType = 0x000002000,

        /// <summary>get system icon index</summary>
        SysIconIndex = 0x000004000,

        /// <summary>put a link overlay on icon</summary>
        LinkOverlay = 0x000008000,

        /// <summary>show icon in selected state</summary>
        Selected = 0x000010000,

        /// <summary>get only specified attributes</summary>
        Attr_Specified = 0x000020000,

        /// <summary>get large icon</summary>
        LargeIcon = 0x000000000,

        /// <summary>get small icon</summary>
        SmallIcon = 0x000000001,

        /// <summary>get open icon</summary>
        OpenIcon = 0x000000002,

        /// <summary>get shell size icon</summary>
        ShellIconSize = 0x000000004,

        /// <summary>pszPath is a pidl</summary>
        PIDL = 0x000000008,

        /// <summary>use passed dwFileAttribute</summary>
        UseFileAttributes = 0x000000010,

        /// <summary>apply the appropriate overlays</summary>
        AddOverlays = 0x000000020,

        /// <summary>Get the index of the overlay in the upper 8 bits of the iIcon</summary>
        OverlayIndex = 0x000000040,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO
    {
        public const int NAMESIZE = 80;
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left, top, right, bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        int x;
        int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGELISTDRAWPARAMS
    {
        public int cbSize;
        public IntPtr himl;
        public int i;
        public IntPtr hdcDst;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public int xBitmap; // x offest from the upperleft of bitmap
        public int yBitmap; // y offset from the upperleft of bitmap
        public int rgbBk;
        public int rgbFg;
        public int fStyle;
        public int dwRop;
        public int fState;
        public int Frame;
        public int crEffect;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGEINFO
    {
        public IntPtr hbmImage;
        public IntPtr hbmMask;
        public int Unused1;
        public int Unused2;
        public RECT rcImage;
    }

    [ComImportAttribute()]
    [GuidAttribute("192B9D83-50FC-457B-90A0-2B82A8B5DAE1")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IImageList
    {
        [PreserveSig]
        int Add(IntPtr hbmImage, IntPtr hbmMask, ref int pi);

        [PreserveSig]
        int ReplaceIcon(int i, IntPtr hicon, ref int pi);

        [PreserveSig]
        int SetOverlayImage(int iImage, int iOverlay);

        [PreserveSig]
        int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);

        [PreserveSig]
        int AddMasked(IntPtr hbmImage, int crMask, ref int pi);

        [PreserveSig]
        int Draw(ref IMAGELISTDRAWPARAMS pimldp);

        [PreserveSig]
        int Remove(int i);

        [PreserveSig]
        int GetIcon(int i, int flags, ref IntPtr picon);

        [PreserveSig]
        int GetImageInfo(int i, ref IMAGEINFO pImageInfo);

        [PreserveSig]
        int Copy(int iDst, IImageList punkSrc, int iSrc, int uFlags);

        [PreserveSig]
        int Merge(int i1, IImageList punk2, int i2, int dx, int dy, ref Guid riid, ref IntPtr ppv);

        [PreserveSig]
        int Clone(ref Guid riid, ref IntPtr ppv);

        [PreserveSig]
        int GetImageRect(int i, ref RECT prc);

        [PreserveSig]
        int GetIconSize(ref int cx, ref int cy);

        [PreserveSig]
        int SetIconSize(int cx, int cy);

        [PreserveSig]
        int GetImageCount(ref int pi);

        [PreserveSig]
        int SetImageCount(int uNewCount);

        [PreserveSig]
        int SetBkColor(int clrBk, ref int pclr);

        [PreserveSig]
        int GetBkColor(ref int pclr);

        [PreserveSig]
        int BeginDrag(int iTrack, int dxHotspot, int dyHotspot);

        [PreserveSig]
        int EndDrag();

        [PreserveSig]
        int DragEnter(IntPtr hwndLock, int x, int y);

        [PreserveSig]
        int DragLeave(IntPtr hwndLock);

        [PreserveSig]
        int DragMove(int x, int y);

        [PreserveSig]
        int SetDragCursorImage(ref IImageList punk, int iDrag, int dxHotspot, int dyHotspot);

        [PreserveSig]
        int DragShowNolock(int fShow);

        [PreserveSig]
        int GetDragImage(ref POINT ppt, ref POINT pptHotspot, ref Guid riid, ref IntPtr ppv);

        [PreserveSig]
        int GetItemFlags(int i, ref int dwFlags);

        [PreserveSig]
        int GetOverlayImage(int iOverlay, ref int piIndex);
    };

    /// <summary>
    /// 获取文件的图标索引号
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns>图标索引号</returns>
    public static int GetIconIndex(string fileName)
    {
        SHFILEINFO info = new SHFILEINFO();
        IntPtr iconIntPtr = SHGetFileInfo(fileName, 0, ref info, (uint)Marshal.SizeOf(info),
            (uint)(SHGFI.SysIconIndex | SHGFI.OpenIcon));
        if (iconIntPtr == IntPtr.Zero)
            return -1;
        return info.iIcon;
    }

    /// <summary>
    /// 根据图标索引号获取图标
    /// </summary>
    /// <param name="iIcon">图标索引号</param>
    /// <param name="flag">图标尺寸标识</param>
    /// <returns></returns>
    public static Icon GetIcon(int iIcon, IMAGELIST_SIZE_FLAG flag)
    {
        IImageList list = null;
        Guid theGuid = new Guid(IID_IImageList); //目前所知用IID_IImageList2也是一样的
        SHGetImageList(flag, ref theGuid, ref list); //获取系统图标列表
        IntPtr hIcon = IntPtr.Zero;
        int r = list.GetIcon(iIcon, ILD_TRANSPARENT | ILD_IMAGE, ref hIcon); //获取指定索引号的图标句柄
        return Icon.FromHandle(hIcon);
    }

    /// <summary>
    ///  方法3：从文件获取Icon图标
    /// </summary>
    /// <param name="fileName">文件名称</param>
    /// <param name="flag">图标尺寸标识</param>
    /// <returns></returns>
    public static Icon GetIconFromFile(string fileName, IMAGELIST_SIZE_FLAG flag)
    {
        return GetIcon(GetIconIndex(fileName), flag);
    }

    #endregion
}