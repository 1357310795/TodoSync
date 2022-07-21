using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TodoSynchronizer.Models;
using TodoSynchronizer.Services;
using TodoSynchronizer.Views;

namespace TodoSynchronizer.ViewModels
{
    public class CanvasLoginViewModel : ViewModelBase
    {
        private LoginInfoModel loginInfo = new LoginInfoModel();
        public LoginInfoModel LoginInfo
        {
            get { return loginInfo; }
            set
            {
                loginInfo = value;
                this.RaisePropertyChanged("LoginInfo");
            }
        }
        public RelayCommand LoginCommand { get; set; }
        public RelayCommand LogoutCommand { get; set; }
        public RelayCommand SwitchCommand { get; set; }

        public CanvasLoginViewModel()
        {
            LoginCommand = new RelayCommand(Login);
            LogoutCommand = new RelayCommand(Logout);
            SwitchCommand = new RelayCommand(Switch);
        }

        public void Login()
        {
            var res1 = CanvasService.TryCacheLogin();
            if (res1.success)
            {
                CanvasLoginSuccess();
                return;
            }
            var m = new CanvasLoginWindow();
            var res2 = m.ShowDialog();
            if (res2.Value == true)
            {
                CanvasLoginSuccess();
                return;
            }
            else
            {
                return;
            }
        }

        public void CanvasLoginSuccess()
        {
            var info = new LoginInfoModel();
            info.UserName = CanvasService.User.Name;
            info.UserEmail = CanvasService.User.PrimaryEmail;

            BitmapImage bitImage = new BitmapImage();
            bitImage.BeginInit();
            bitImage.UriSource = new Uri(CanvasService.User.AvatarUrl, UriKind.Absolute);
            bitImage.EndInit();
            info.UserAvatar = bitImage;
            info.IsLogin = true;
            LoginInfo = info;
        }

        public void Logout()
        {
            LoginInfo = new LoginInfoModel();
        }

        public void Switch()
        {

        }
    }
}
