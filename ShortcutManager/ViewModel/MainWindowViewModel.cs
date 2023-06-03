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
        if (_observableCollection.Count > 0)
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

        _context.Database.Migrate();
        // _context.Database.EnsureCreated();
        // _context.EnsureCreatingMissingTables();
        var dataList = _context.Datas.ToList();
        dataList.ForEach(data => { ShowShortcut(data.ShortcutPath, data.IsMyComputer); });
    }

    private readonly ObservableCollection<MyListBoxData> _observableCollection = new();

    private void BindSource()
    {
        _mw.ShortcutList.ItemsSource = _observableCollection;
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
        var index = GetShortcutBaseData(file, isMyComputer, out var data);

        if (SaveShortcutBaseData(data)) return;

        ShowShortcut(index, data);
    }

    private void ShowShortcut(int index, Data data)
    {
        Icon icon;
        icon = IconHelper.GetIcon(index, IconHelper.IMAGELIST_SIZE_FLAG.SHIL_EXTRALARGE);

        var hIcon = icon.ToBitmap().GetHicon();
        var sourceIcon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(hIcon,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());

        var lbData = CopyHelper.AutoCopy<Data, MyListBoxData>(data);
        lbData.Src = sourceIcon;
        _observableCollection.Add(lbData);
    }

    private void ShowShortcut(string file, bool isMyComputer)
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

        var data = new Data
        {
            Name = s.Name, RealPath = s.Path, ShortcutPath = file, Arguments = s.Arguments, Verbs = verbs,
            IsMyComputer = isMyComputer, Categories = null, UpdateTimestamp = 0, Sort = 0
        };
        ShowShortcut(index, data);
    }

    private bool SaveShortcutBaseData(Data data)
    {
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
                        _context.Datas.Remove(data);
                        MessageBox.Show($"目标{data.Name}的路径已存在，无法重复创建");
                        return true;
                    }

                    break;
                default:
                    MessageBox.Show(e.Message + "\r\n" + e.Data);
                    break;
            }
        }

        return false;
    }

    private static int GetShortcutBaseData(string file, bool isMyComputer, out Data data)
    {
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

        data = new Data
        {
            Name = s.Name, RealPath = s.Path, ShortcutPath = file, Arguments = s.Arguments, Verbs = verbs,
            IsMyComputer = isMyComputer, Categories = null, UpdateTimestamp = 0, Sort = 0
        };
        return index;
    }
}