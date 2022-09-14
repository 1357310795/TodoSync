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
    /// Page4.xaml 的交互逻辑
    /// </summary>
    public partial class Page4 : Page, INotifyPropertyChanged
    {
        public Page4()
        {
            InitializeComponent();
            this.DataContext = this;
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


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Token = DataService.GetData<string>("tokenenc");
            Clipboard.SetText(Token);
        }
    }
}
