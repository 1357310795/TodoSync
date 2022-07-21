using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
using TodoSynchronizer.Extensions;
using TodoSynchronizer.Helpers;
using TodoSynchronizer.Models;
using TodoSynchronizer.Services;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace TodoSynchronizer.Views
{
    /// <summary>
    /// CanvasLoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CanvasLoginWindow : Window, INotifyPropertyChanged
    {
        public CanvasLoginWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            
        }

        private string errorText;
        public string ErrorText
        {
            get { return errorText; }
            set
            {
                errorText = value;
                this.RaisePropertyChanged("ErrorText");
            }
        }
        private string token;
        public string Token
        {
            get { return token; }
            set
            {
                token = value;
                this.RaisePropertyChanged("Token");
            }
        }

        private BackgroundWorker worker;

        private void TextHelp_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            worker = new BackgroundWorker();
            worker.DoWork += Login_DoWork;
            worker.RunWorkerCompleted += Login_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void Login_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Dispatcher.Invoke(() => {
                ButtonProgressAssist.SetIsIndeterminate(CookieLoginButton, true);
                ButtonProgressAssist.SetIsIndicatorVisible(CookieLoginButton, true);
                CookieLoginButton.Content = "正在登录...";
                this.IsEnabled = false;
            });

            e.Result = CanvasService.Login(Token);
        }

        private void Login_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var res = (CommonResult)e.Result;
            this.Dispatcher.Invoke(() => {
                ButtonProgressAssist.SetIsIndeterminate(CookieLoginButton, false);
                ButtonProgressAssist.SetIsIndicatorVisible(CookieLoginButton, false);
                CookieLoginButton.Content = "登录";
                this.IsEnabled = true;
            });
            this.IsEnabled = true;
            if (res.success)
            {
                IniHelper.SetKeyValue("canvas", "token", Token);
                this.Dispatcher.Invoke(() => {
                    this.DialogResult = true;
                    this.Close();
                });
            }
            else
            {
                ErrorTextBlock.Visibility = Visibility.Visible;
                ErrorText = res.result;
            }
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
        #endregion

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = false;
        }
    }
}
