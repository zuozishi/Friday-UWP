using Friday.Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Friday.Views.UserPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RegisterPage_BandPhone : Page
    {
        DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
        int alltime = 60;
        RegisterData regdata;
        private string user;
        private string pwd;

        public RegisterPage_BandPhone()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            timer.Tick += Timer_Tick;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                regdata = Data.Json.DataContractJsonDeSerialize<RegisterData>((string)e.Parameter);
            }
            if (e.NavigationMode == NavigationMode.Back)
            {
                Window.Current.Content = new Frame();
                loodProgress.IsActive = true;
                var userdata = await Class.UserManager.Login(user, pwd);
                loodProgress.IsActive = false;
                if (userdata != null)
                {
                    (Window.Current.Content as Frame).Navigate(typeof(MainPage));
                }
                else
                {
                    Tools.ShowMsgAtFrame("呃。。。登录发生了异常,请重新登录");
                    await Task.Delay(2000);
                    (Window.Current.Content as Frame).Navigate(typeof(MainPage));
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
        }

        private async void getCaptchaBtn_Click(object sender, RoutedEventArgs e)
        {
            loodProgress.IsActive = true;
            var res = await Class.UserManager.getRegisterCaptcha(phonenum.Text,pwdtext.Password);
            loodProgress.IsActive = false;
            if (res == null) { timer.Start(); getCaptchaBtn.IsEnabled = false; }
            if (res != null) Class.Tools.ShowMsgAtFrame(res);
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
            user = phonenum.Text;
            pwd = pwdtext.Password;
            loodProgress.IsActive = true;
            var res = await Class.UserManager.register(user,pwd,captchaText.Text,regdata.academyId,regdata.grade,regdata.schoolId);
            if (res == null)
            {
                var userdata = await Class.UserManager.Login(user,pwd);
                if (userdata != null)
                {
                    Frame.Navigate(typeof(RegisterPage_AddData));
                }
                else
                {
                    Tools.ShowMsgAtFrame("呃。。。登录发生了异常,请重新登录");
                }
            }
            else
            {
                Tools.ShowMsgAtFrame(res);
            }
            loodProgress.IsActive = false;
        }

        public class RegisterData
        {
            public string id { get; set; }
            public string grade { get; set; }
            public string academyId { get; set; }
            public string schoolId { get; set; }
        }

        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Window.Current.Content = new Frame();
            (Window.Current.Content as Frame).Navigate(typeof(LoginPage));
        }
    }
}
