using Microsoft.Identity.Client.Platforms.Shared.Desktop.OsBrowser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TodoSynchronizer.QuickTool.Pages;
using TodoSynchronizer.QuickTool.Services;

namespace TodoSynchronizer.QuickTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            NaviService.SetFrame(RootFrame);
        }

        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NaviService.Navigate(new Page1());
        }
    }
}
