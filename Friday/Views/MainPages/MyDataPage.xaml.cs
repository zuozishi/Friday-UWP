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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Friday.Views.MainPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MyDataPage : Page
    {
        public Class.Model.User.Login_Result UserData
        {
            get
            {
                return Class.UserManager.UserData;
            }
        }
        public MyDataPage()
        {
            this.InitializeComponent();
            this.Transitions = new TransitionCollection();
            this.Transitions.Add(new PaneThemeTransition { Edge = EdgeTransitionLocation.Right });
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            switch (list.SelectedIndex)
            {
                case 0:
                    (Window.Current.Content as Frame).Navigate(typeof(UserPages.UserIndexPage), Class.UserManager.UserData.studentId);
                    list.SelectedIndex = -1;
                    break;
                case 1:
                    (Window.Current.Content as Frame).Navigate(typeof(UserPages.MyObsPage));
                    list.SelectedIndex = -1;
                    break;
                case 2:
                    (Window.Current.Content as Frame).Navigate(typeof(UserPages.MyCollectPage));
                    list.SelectedIndex = -1;
                    break;
                case 3:
                    (Window.Current.Content as Frame).Navigate(typeof(UserPages.ReSetPwdPage));
                    list.SelectedIndex = -1;
                    break;
                case 4:
                    (Window.Current.Content as Frame).Navigate(typeof(AboutPage));
                    list.SelectedIndex = -1;
                    break;
                default:
                    break;
            }
        }

        private async void UnloginBtn_Clicked(object sender, RoutedEventArgs e)
        {
            var dialog = new Controls.DialogBox()
            {
                Title = "提示",
                PrimaryButtonText = "取消",
                SecondaryButtonText = "确定",
            };
            dialog.mainTextBlock.Text = "是否注销当前账户?";
            if(await dialog.ShowAsync() == ContentDialogResult.Secondary)
            {
                Class.UserManager.Unlogin();
                Window.Current.Content = new Frame();
                (Window.Current.Content as Frame).Navigate(typeof(UserPages.LoginPage));
            }
        }
    }
}
