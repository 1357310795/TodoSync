using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TodoSynchronizer.Helpers;
using TodoSynchronizer.Services;

namespace TodoSynchronizer.UnitTest
{
    /// <summary>
    /// TodoTestWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TodoTestWindow : Window, INotifyPropertyChanged
    {
        public TodoTestWindow()
        {
            InitializeComponent();
            this.DataContext = this;
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

        private string userName;
        public string UserName
        {
            get { return userName; }
            set
            {
                userName = value;
                this.RaisePropertyChanged("UserName");
            }
        }

        private string userEmail;
        public string UserEmail
        {
            get { return userEmail; }
            set
            {
                userEmail = value;
                this.RaisePropertyChanged("UserEmail");
            }
        }

        private IEnumerable items;
        public IEnumerable Items
        {
            get { return items; }
            set
            {
                items = value;
                this.RaisePropertyChanged("Items");
            }
        }

        private BitmapSource userAvatar;
        public BitmapSource UserAvatar
        {
            get { return userAvatar; }
            set
            {
                userAvatar = value;
                this.RaisePropertyChanged("UserAvatar");
            }
        }


        private string taskListId;
        public string TaskListId
        {
            get { return taskListId; }
            set
            {
                taskListId = value;
                this.RaisePropertyChanged("TaskListId");
            }
        }
        private string taskItemId;
        public string TaskItemId
        {
            get { return taskItemId; }
            set
            {
                taskItemId = value;
                this.RaisePropertyChanged("TaskItemId");
            }
        }
        private string taskCheckItemId;
        public string TaskCheckItemId
        {
            get { return taskCheckItemId; }
            set
            {
                taskCheckItemId = value;
                this.RaisePropertyChanged("TaskCheckItemId");
            }
        }
        private string taskLinkedResourceId;
        public string TaskLinkedResourceId
        {
            get { return taskLinkedResourceId; }
            set
            {
                taskLinkedResourceId = value;
                this.RaisePropertyChanged("TaskLinkedResourceId");
            }
        }

        private string taskAttachmentId;

        public string TaskAttachmentId
        {
            get { return taskAttachmentId; }
            set
            {
                taskAttachmentId = value;
                this.RaisePropertyChanged("TaskAttachmentId");
            }
        }

        private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            //var res = await MsalHelper.GetToken(this);
            //if (!res.success)
            //{
            //    MessageBox.Show(res.result);
            //    return;
            //}
            //TodoService.Token = res.result;
            //var info = TodoService.GetUserInfo();
            //UserName = info.DisplayName;
            //UserEmail = info.UserPrincipalName;
            //UserAvatar = TodoService.GetUserAvatar();
            //Message = "登录成功";
        }

        private void ButtonListLists_Click(object sender, RoutedEventArgs e)
        {
            var rawres = TodoService.ListLists();
            Items = rawres;
        }

        private void ButtonListTasks_Click(object sender, RoutedEventArgs e)
        {
            var rawres = TodoService.ListTodoTasks(TaskListId);
            Items = rawres;
        }

        private void ButtonCheckItems_Click(object sender, RoutedEventArgs e)
        {
            var rawres = TodoService.ListCheckItems(TaskListId, TaskItemId);
            Items = rawres;
        }

        private void ButtonLinkedResources_Click(object sender, RoutedEventArgs e)
        {
            var rawres = TodoService.ListLinkedResources(TaskListId ,TaskItemId);
            Items = rawres;
        }

        private void ButtonAttachments_Click(object sender, RoutedEventArgs e)
        {
            var rawres = TodoService.ListAttachments(TaskListId, TaskItemId);
            Items = rawres;
        }

        private void ButtonGetAttachment_Click(object sender, RoutedEventArgs e)
        {
            var item = TodoService.GetAttachment(TaskListId, TaskItemId, TaskAttachmentId);
            var fs = File.OpenWrite(@"D:\1.data");
            item.CopyTo(fs);
            fs.Close();
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
