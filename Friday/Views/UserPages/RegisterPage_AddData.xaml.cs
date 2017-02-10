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
    public sealed partial class RegisterPage_AddData : Page
    {
        public RegisterPage_AddData()
        {
            this.InitializeComponent();
            this.Transitions.Add(new PaneThemeTransition { Edge = EdgeTransitionLocation.Right });
        }

        private async void subBtn_Clicked(object sender, RoutedEventArgs e)
        {
            if (nickname_text.Text.Length > 0)
            {
                if ((await Class.UserManager.editNecessaryInfo(nickname_text.Text, zhuanye_text.Text, class_text.Text))==null){
                    Class.Tools.ShowMsgAtFrame("网络异常");
                    await Task.Delay(2000);
                    Frame.GoBack();
                }
            }
            else
            {
                Class.Tools.ShowMsgAtFrame("请填写你的真实姓名");
            }
        }
    }
}
