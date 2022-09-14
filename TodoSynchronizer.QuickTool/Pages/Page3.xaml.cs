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
            }
            var token = DataService.GetData<string>("token");
            var enc = AesHelper.Encrypt(Password, token);

            DataService.SetData("tokenenc", enc);
            NaviService.Navigate(new Page4());
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
    }
}
