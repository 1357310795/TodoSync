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
using TodoSynchronizer.QuickTool.Services;

namespace TodoSynchronizer.QuickTool.Pages
{
    /// <summary>
    /// Page1.xaml 的交互逻辑
    /// </summary>
    public partial class Page1 : Page
    {
        public Page1()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Button1.IsEnabled = false;
            DefaultOsBrowserWebUi webUi = new DefaultOsBrowserWebUi();
            var res = await webUi.AcquireAuthorizationAsync(new Uri("https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize?client_id=49694ef2-8751-4ac9-8431-8817c27350b4&response_type=code&redirect_uri=http%3A%2F%2Flocalhost%3A65399&response_mode=query&scope=Tasks.ReadWrite%20User.Read%20offline_access&state=12345"), new Uri("http://localhost:65399"), new System.Threading.CancellationToken());
            Debug.WriteLine(res);
            DataService.SetData("uri", res);
            NaviService.Navigate(new Page2());
        }
    }
}
