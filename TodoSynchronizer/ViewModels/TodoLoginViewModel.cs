using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TodoSynchronizer.Helpers;
using TodoSynchronizer.Models;
using TodoSynchronizer.Mvvm;
using TodoSynchronizer.Services;

namespace TodoSynchronizer.ViewModels
{
    public class TodoLoginViewModel : ViewModelBase
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
        public AsyncRelayCommand LoginCommand { get; set; }
        public RelayCommand LogoutCommand { get; set; }
        public RelayCommand SwitchCommand { get; set; }

        public TodoLoginViewModel()
        {
            LoginCommand = new AsyncRelayCommand(Login);
            LogoutCommand = new RelayCommand(Logout);
            SwitchCommand = new RelayCommand(Switch);
        }

        public async Task Login()
        {
            MsalHelper helper = new MsalHelper();
            CommonResult res = await helper.GetToken(Application.Current.MainWindow);
            
            if (!res.success)
            {
                MessageBox.Show(res.result);
                return;
            }
            TodoService.Token = res.result;
            var userinfo = TodoService.GetUserInfo();
            var logininfo = new LoginInfoModel();

            logininfo.UserName = userinfo.DisplayName;
            logininfo.UserEmail = userinfo.UserPrincipalName;
            logininfo.UserAvatar = TodoService.GetUserAvatar();
            logininfo.IsLogin = true;
            LoginInfo = logininfo;
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
