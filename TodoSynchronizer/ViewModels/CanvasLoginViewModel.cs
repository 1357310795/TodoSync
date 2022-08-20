using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TodoSynchronizer.Core.Models;
using TodoSynchronizer.Core.Services;
using TodoSynchronizer.Helpers;
using TodoSynchronizer.Models;
using TodoSynchronizer.Views;

namespace TodoSynchronizer.ViewModels
{
    public partial class CanvasLoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private LoginInfoModel loginInfo = new LoginInfoModel();

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
            var token = IniHelper.GetKeyValue("canvas", "token", "");
            if (token != "")
            {
                var res1 = CanvasService.Login(token);
                if (res1.success)
                {
                    CanvasLoginSuccess();
                    return;
                }
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
