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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TodoSynchronizer.QuickTool.Helpers;
using TodoSynchronizer.QuickTool.Services;

namespace TodoSynchronizer.QuickTool.Pages
{
    /// <summary>
    /// Page3.xaml 的交互逻辑
    /// </summary>
    public partial class Page3 : Page, INotifyPropertyChanged
    {
        public Page3()
        {
            InitializeComponent();
            this.DataContext = this;
            Password = WordHelper.GetRandomChinese(10);
        }

        private string password;

        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                this.RaisePropertyChanged("Password");
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Password == string.Empty || Password == "")
            {
                MessageBox.Show("密钥不能为空！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Password.Any(check))
            {
                MessageBox.Show("为确保安全，密钥必须包含中文字符！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var token = DataService.GetData<string>("token");
            var enc = AesHelper.Encrypt(Password, token);

            DataService.SetData("tokenenc", enc);
            DataService.SetData("password", Password);
            NaviService.Navigate(new Page4());
        }

        public bool check(char c)
        {
            return ((int)c) > 127;
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

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var token = DataService.GetData<string>("token");
            Clipboard.SetText(token);
            MessageBox.Show("复制成功！");
        }
    }
}
