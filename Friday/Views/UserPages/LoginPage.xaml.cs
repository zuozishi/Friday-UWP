using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Friday.Views.UserPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        string userid
        {
            get
            {
                return user_text.Text;
            }
            set
            {
                user_text.Text = value;
            }
        }
        string password
        {
            get
            {
                return pwd_text.Password;
            }
            set
            {
                pwd_text.Password = value;
            }
        }
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private async void LoginBtn_Clicked(object sender, RoutedEventArgs e)
        {
            if (userid.Length == 0)
            {
                Class.Tools.ShowMsgAtFrame("请输入手机或邮箱账号");
                return;
            }
            if (password.Length == 0)
            {
                Class.Tools.ShowMsgAtFrame("请输入密码");
                return;
            }
            if (Class.HttpPostUntil.isInternetAvailable)
            {
                Class.UserManager.Unlogin();
                pgsBar.Visibility =Visibility.Visible;
                var user = await Class.UserManager.Login(userid, password,true);
                pgsBar.Visibility = Visibility.Collapsed;
                if (user != null)
                {
                    Window.Current.Content = new Frame();
                    (Window.Current.Content as Frame).Navigate(typeof(MainPage));
                }
                else
                {
                    Class.Tools.ShowMsgAtFrame("用户名或密码错误");
                    Class.UserManager.Unlogin();
                }
            }
            else
            {
                Class.Tools.ShowMsgAtFrame("网络异常");
                Class.UserManager.Unlogin();
            }
        }

        private void ReSetPwdBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(UserPages.ReSetPwdPage));
        }

        private void RegBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(UserPages.RegisterPage_School));
        }
    }
}
