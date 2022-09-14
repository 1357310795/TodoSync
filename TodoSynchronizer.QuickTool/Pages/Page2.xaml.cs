using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Page2.xaml 的交互逻辑
    /// </summary>
    public partial class Page2 : Page, INotifyPropertyChanged
    {
        public Page2()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private string head;

        public string Head
        {
            get { return head; }
            set
            {
                head = value;
                this.RaisePropertyChanged("Head");
            }
        }


        private string message;

        public string Message
        {
            get { return message; }
            set
            {
                message = value;
                this.RaisePropertyChanged("Message");
            }
        }


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var res = DataService.GetData<string>("uri");
            var reg = new Regex(@"code=([a-zA-Z0-9-\._]+)");
            Match match = reg.Match(res);
            if (match.Success)
            {
                Head = "用户已授权";
                Message = "正在联系服务器获取信息...";
                Task.Run(() => {
                    var forms = new List<KeyValuePair<string, string>>();
                    forms.Add(new KeyValuePair<string, string>("client_id", "49694ef2-8751-4ac9-8431-8817c27350b4"));
                    forms.Add(new KeyValuePair<string, string>("scope", "Tasks.ReadWrite User.Read offline_access"));
                    forms.Add(new KeyValuePair<string, string>("redirect_uri", "http://localhost:65399"));
                    forms.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
                    forms.Add(new KeyValuePair<string, string>("code", match.Groups[1].Value));

                    FormUrlEncodedContent form = new FormUrlEncodedContent(forms);

                    HttpClient client = new HttpClient();
                    var posttask = client.PostAsync("https://login.microsoftonline.com/consumers/oauth2/v2.0/token", form);
                    posttask.Wait();
                    var tokenres = posttask.GetAwaiter().GetResult();

                    if (!tokenres.IsSuccessStatusCode)
                    {
                        Head = "获取信息失败";
                        Message = tokenres.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    }
                    else
                    {
                        var reg1 = new Regex(@"""refresh_token"":""(.+?)""");
                        var match1 = reg1.Match(tokenres.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        if (match1.Success)
                        {
                            DataService.SetData("token", match1.Groups[1].Value);
                            this.Dispatcher.Invoke(() => { NaviService.Navigate(new Page3()); });
                        }
                        else
                        {
                            Head = "获取信息失败";
                            Message = tokenres.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        }
                    }
                });
            }
            else
                Head = "授权失败";
                Message = "错误代码：";
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
