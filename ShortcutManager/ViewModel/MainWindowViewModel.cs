using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ShortcutManager.Config;
using ShortcutManager.Core;
using ShortcutManager.Extension;
using ShortcutManager.Helper;
using ShortcutManager.Model;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

namespace ShortcutManager.ViewModel;

public class MainWindowViewModel : ViewModelBase, IOperator
{
    private readonly MainWindow _mw;
    private readonly MyDbContext _context = new();

    public MainWindowViewModel(MainWindow mw)
    {
        _mw = mw;
        BindSource();

        RunCommand = new RelayCommand<object>(Run);
        ContextMenuCommand = new RelayCommand<object>(ShowContextMenu);

        Init();
        if (_datas.Count > 0)
        {
            return;
        }

        var myComputer = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
        SaveShortcut(myComputer, true);
    }

    private void Init()
    {
        var dbPath = Path.Combine(Environment.CurrentDirectory, @"data.db");
        if (!File.Exists(dbPath))
        {
            try
            {
                var uri = new Uri("pack://application:,,,/Db/data.db", UriKind.RelativeOrAbsolute);
                var stream = Application.GetResourceStream(uri).Stream;
                stream.ToFile(@"data.db");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        _context.Database.EnsureCreated();
        _context.EnsureCreatingMissingTables();
        var datas = _context.Datas.ToList();
        datas.ForEach(data => { ShowShortcut(data.ShortcutPath, data.IsMyComputer); });
    }

    public byte[] StreamToBytes(Stream stream)
    {
        byte[] bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        stream.Seek(0, SeekOrigin.Begin); // 设置当前流的位置为流的开始
        return bytes;
    }

    private ObservableCollection<MyListBoxData> _datas = new();

    private void BindSource()
    {
        _mw.ShortcutList.ItemsSource = _datas;
    }

    public ICommand RunCommand { get; }
    public ICommand ContextMenuCommand { get; }

    private static void Run(object o)
    {
        var data = (MyListBoxData)o;
        if (data.RealPath == "")
        {
            Process.Start("explorer.exe", "::{20d04fe0-3aea-1069-a2d8-08002b30309d}");
            return;
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = data.RealPath,
            // Verb = "runas",
            Arguments = data.Arguments,
            WorkingDirectory = Path.GetDirectoryName(data.RealPath),
            UseShellExecute = true
        };
        if (File.Exists(data.RealPath))
        {
            var i = 0;
            foreach (var verb in processStartInfo.Verbs)
            {
                Console.WriteLine("  {0}. {1}", i.ToString(), verb);
                i++;
            }
        }

        var process = Process.Start(processStartInfo);
        // process.Start();
    }

    private static void ShowContextMenu(object o)
    {
        var data = (MyListBoxData)o;

        // File name      Purpose
        // -----------------------------------------------------------------------
        // Access.cpl     Accessibility properties
        // Appwiz.cpl     Add/Remove Programs properties
        // Desk.cpl       Display properties
        // FindFast.cpl   FindFast (included with Microsoft Office for Windows 95)
        // Inetcpl.cpl    Internet properties
        // Intl.cpl       Regional Settings properties
        // Joy.cpl        Joystick properties
        // Main.cpl       Mouse, Fonts, Keyboard, and Printers properties
        // Mlcfg32.cpl    Microsoft Exchange or Windows Messaging properties
        // Mmsys.cpl      Multimedia properties
        // Modem.cpl      Modem properties
        // Netcpl.cpl     Network properties
        // Odbccp32.cpl   Data Sources (32-bit ODBC, included w/ Microsoft Office)
        // Password.cpl   Password properties
        // Sticpl.cpl     Scanners and Cameras properties
        // Sysdm.cpl      System properties and Add New Hardware wizard
        // Themes.cpl     Desktop Themes 
        // TimeDate.cpl   Date/Time properties
        // Wgpocpl.cpl    Microsoft Mail Post Office
        if (data.ShortcutPath == "" && data.RealPath == "")
        {
            var psi = new ProcessStartInfo("sysdm.cpl")
            {
                WorkingDirectory = @"C:\Windows\System32\",
                UseShellExecute = true,
            };
            Process.Start(psi);
        }
        else
        {
            Shell.ShowFileProperties(data.ShortcutPath);
        }

        Console.WriteLine();
    }

    public void FileDragOver(object sender, DragEventArgs e)
    {
        var border = _mw.BorderLine;
        border.Stroke = Brushes.Red;
        border.StrokeThickness = 2;
    }

    public void FileDragLeave(object sender, DragEventArgs e)
    {
        var border = _mw.BorderLine;
        border.Stroke = Brushes.White;
        border.StrokeThickness = 0;
    }

    public void SaveShortcut(object sender, DragEventArgs e)
    {
        var border = _mw.BorderLine;
        border.Stroke = Brushes.White;
        border.StrokeThickness = 0;
        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        if (files is null)
        {
            MessageBox.Show("不支持的操作.");
            return;
        }

        foreach (var file in files)
        {
            SaveShortcut(file);
        }

    }

    private void SaveShortcut(string file, bool isMyComputer = false)
    {
        var data = ShowShortcut(file, isMyComputer);


        _context.Datas.Add(data);
        try
        {
            var sci = _context.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            switch (e.InnerException)
            {
                case SqliteException:
                    var se = e.InnerException as SqliteException;
                    if (se.SqliteErrorCode is 19)
                    {
                        MessageBox.Show("目标已存在，无法重复创建");
                    }

                    break;
                default:
                    MessageBox.Show(e.Message+"\r\n"+e.Data);
                    break;
            }
        }
    }

    private Data ShowShortcut(string file, bool isMyComputer)
    {
        // 1、文件夹-快捷方式 Path有具体值，值为真实路径
        // 2、真实路径文件夹 Path值为空字符串
        // 3、文件-快捷方式 Path有具体值，值为真实路径
        // 4、真实路径文件 Path值为空字符串
        // 5、其它暂未可知情况
        var s = ShFolder.ResolveShortcut(file);
        Icon icon = SystemIcons.WinLogo;
        var index = -1;
        string[] verbs = Array.Empty<string>();
        if (!isMyComputer)
        {
            index = IconHelper.GetIconIndex(file);
            var info = new FileInfo(file);
            s.Path = s.Path == "" ? file : s.Path;
            s.Name = info.Name;
            var fi = new FileInfo(s.Path);

            if ((fi.Attributes & FileAttributes.Directory) != 0)
            {
            }
            else
            {
                verbs = new ProcessStartInfo(s.Path).Verbs;
            }
        }
        else
        {
            s.Name = "我的电脑";
            index = 15;
        }

        icon = IconHelper.GetIcon(index, IconHelper.IMAGELIST_SIZE_FLAG.SHIL_EXTRALARGE);

        var hIcon = icon.ToBitmap().GetHicon();
        var sourceIcon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(hIcon,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());

        var data = new Data
        {
            Name = s.Name, RealPath = s.Path, ShortcutPath = file, Arguments = s.Arguments, Verbs = verbs,
            IsMyComputer = isMyComputer, Categories = null, UpdateTimestamp = 0, Sort = 0
        };

        var lbData = CopyHelper.AutoCopy<Data, MyListBoxData>(data);
        lbData.Src = sourceIcon;
        _datas.Add(lbData);
        return data;
    }

    private Color Invert(Color originalColor)
    {
        Color invertedColor = new()
        {
            ScR = 1.0F - originalColor.ScR,
            ScG = 1.0F - originalColor.ScG,
            ScB = 1.0F - originalColor.ScB,
            ScA = originalColor.ScA
        };
        return invertedColor;
    }

    private byte[] IconToBytes(Icon icon)
    {
        MemoryStream ms = new();
        icon.Save(ms);
        return ms.GetBuffer();
    }

    private BitmapImage LoadImage(byte[] data)
    {
        if (data == null || data.Length == 0)
        {
            return null;
        }

        var image = new BitmapImage();
        using (var mem = new MemoryStream(data.ToArray()))
        {
            mem.Position = 0;
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = null;
            image.StreamSource = mem;
            image.EndInit();
        }

        image.Freeze();
        return image;
    }

    private BitmapImage bitmapToBitmapImage(Bitmap bmp)
    {
        BitmapImage bitmapImage = new BitmapImage();
        using (var ms = new MemoryStream())
        {
            bmp.Save(ms, bmp.RawFormat);
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
        }

        return bitmapImage;
    }
}