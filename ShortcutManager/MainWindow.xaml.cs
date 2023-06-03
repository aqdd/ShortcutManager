using System;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using ShortcutManager.Core;
using ShortcutManager.ViewModel;

namespace ShortcutManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _vm;
        public MainWindow()
        {
            InitializeComponent();
            _vm = new(this);
            DataContext = _vm;
        }

        private void FileDragOver(object sender, DragEventArgs e)
        {
            _vm.FileDragOver(sender, e);
        }
        private void FileDragLeave(object sender, DragEventArgs e)
        {
            _vm.FileDragLeave(sender, e);
        }
        private void SaveShortcut(object sender, DragEventArgs e)
        {
            _vm.SaveShortcut(sender, e);
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton==MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void DarkMode(object sender, RoutedEventArgs e) => ModifyTheme(DarkModeToggleButton.IsChecked == true);

        private void ModifyTheme(bool isDark)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(isDark? Theme.Dark: Theme.Light);
            paletteHelper.SetTheme(theme);
        }
        private void CloseApp(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}