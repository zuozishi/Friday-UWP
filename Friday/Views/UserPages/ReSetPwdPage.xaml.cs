using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class ReSetPwdPage : Page
    {
        DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
        int alltime = 60;
        public ReSetPwdPage()
        {
            this.InitializeComponent();
            timer.Tick += Timer_Tick;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                if (Class.UserManager.islogin)
                {
                    phonenum.Text = Class.UserManager.UserData.mobileNumber;
                }
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            timetext.Text = "(" + alltime + ")";
            alltime--;
            if (alltime == -1)
            {
                timetext.Text = "";
                alltime = 60;
                timer.Stop();
                getCaptchaBtn.IsEnabled = true;
            }
        }

        private void GoBackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void SetPwdByPhone_Clicked(object sender, RoutedEventArgs e)
        {
            loodProgress.IsActive = true;
            var res = await Class.UserManager.ResetPassword(phonenum.Text,captchaText.Text,pwdBox.Password);
            loodProgress.IsActive = false;
            if (res != null)
            {
                Class.Tools.ShowMsgAtFrame(res);
            }
            else
            {
                Class.Tools.ShowMsgAtFrame("密码修改成功,请使用新密码登录账户");
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = (sender as TextBox).Text;
            if (text.Length == 0)
            {
                getCaptchaBtn.IsEnabled = false;
            }
            else
            {
                getCaptchaBtn.IsEnabled = true;
            }
        }

        private async void getCaptchaBtn_Click(object sender, RoutedEventArgs e)
        {
            loodProgress.IsActive = true;
            var res = await Class.UserManager.GetResetPasswordCaptcha(phonenum.Text);
            loodProgress.IsActive = false;
            if (res == null) { timer.Start(); getCaptchaBtn.IsEnabled = false; }
            if (res != null) Class.Tools.ShowMsgAtFrame(res);
        }
    }
}
