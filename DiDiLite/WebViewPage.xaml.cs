using LLM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace DiDiLite
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WebViewPage : Page
    {
        public static bool isInternetAvailable
        {
            get
            {
                ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
                if (InternetConnectionProfile == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public WebViewPage()
        {
            this.InitializeComponent();
            //UrlmonLibrary.API.ChangeUserAgent("Mozilla/5.0 (Linux; Android 5.1.1; M631 Build/LMY47V) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/39.0.0.0 Mobile Safari/537.36 didi.sdk1.1.0");
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            webview.NavigationStarting += Webview_NavigationStarting;
            var localSetting = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (!localSetting.Values.ContainsKey("first_" + Edi.UWP.Helpers.Utils.GetAppVersion()))
            {
                var dialog = new Windows.UI.Popups.MessageDialog("欢迎使用滴滴出行 具体功能由地区而异\n");
                dialog.Content += "更新说明(2017/2/11)\n";
                dialog.Content += "1.主要完善功能;\n";
                dialog.Content += "2.UI微调.";
                await dialog.ShowAsync();
                localSetting.Values["first_" + Edi.UWP.Helpers.Utils.GetAppVersion()] = true;
            }
            string url = "";
            if (e.Parameter != null)
            {
                var pointdata = (double[])e.Parameter;
                url = "http://common.diditaxi.com.cn/general/webEntry?fromlat="+pointdata[0]+"&fromlng=" + pointdata[1];
            }
            else
            {
                url = "http://common.diditaxi.com.cn/general/webEntry";
            }
            if (MapPage.myPosition != null)
            {
                url = "http://common.diditaxi.com.cn/general/webEntry?fromlat=" + MapPage.myPosition.Location.Position.Latitude + "&fromlng=" + MapPage.myPosition.Location.Position.Longitude;
            }
            enterweb:
            if (isInternetAvailable)
            {
                webview.Source = new Uri(url);
            }
            else
            {
                var dialog = new Friday.Controls.DialogBox()
                {
                    Title = "提示",
                    PrimaryButtonText = "重试",
                    SecondaryButtonText = "退出",
                };
                dialog.mainTextBlock.Text = "网络异常,请检查网络设置!";
                if (await dialog.ShowAsync() == ContentDialogResult.Secondary)
                {
                    Application.Current.Exit();
                }
                else
                {
                    goto enterweb;
                }
            }
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
        }

        private void Webview_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (!args.Uri.Query.Contains("addug=1"))
            {
                args.Cancel = true;
                var hc = new Windows.Web.Http.HttpRequestMessage();
                if (args.Uri.ToString().Contains("?"))
                {
                    hc.RequestUri = new Uri(args.Uri.ToString() + "&addug=1");
                }
                else
                {
                    hc.RequestUri = new Uri(args.Uri.ToString() + "?addug=1");
                }
                hc.Headers.UserAgent.Add(new Windows.Web.Http.Headers.HttpProductInfoHeaderValue("Mozilla/5.0 (Linux; Android 5.1.1; M631 Build/LMY47V) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/39.0.0.0 Mobile Safari/537.36 didi.sdk1.1.0"));
                webview.NavigateWithHttpRequestMessage(hc);
            }
        }

        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            if (webview.CanGoBack)
            {
                webview.GoBack();
            }
            else
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
            }
        }

        private void CloseBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private void BackBtnVlicked(object sender, RoutedEventArgs e)
        {
            if (webview.CanGoBack)
            {
                webview.GoBack();
            }
        }

        private void webview_PermissionRequested(WebView sender, WebViewPermissionRequestedEventArgs args)
        {
            args.PermissionRequest.Allow();
        }

        private void webview_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (args.Uri.ToString().Contains("suggestfrom"))
            {
                ShowMsgAtFrame("地图不能移动/缩放?点击右边按钮试试");
                openMapBtn.Visibility = Visibility.Visible;
            }
            else
            {
                openMapBtn.Visibility = Visibility.Collapsed;
            }
        }

        private void openMapBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MapPage));
        }

        public static async void ShowMsgAtFrame(string msg, int time = 2000)
        {
            try
            {
                string xaml = "<Border Name=\"msgboxview\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x = \"http://schemas.microsoft.com/winfx/2006/xaml\" Margin =\"0,0,0,55\" Height=\"auto\" Visibility=\"Visible\" VerticalAlignment=\"Bottom\" CornerRadius=\"10\" HorizontalAlignment=\"Center\" Background=\"#7F000000\" ><TextBlock Foreground=\"White\" TextWrapping=\"WrapWholeWords\" VerticalAlignment=\"Center\" Margin=\"10,5\"><Run Text=\"{0}\"/></TextBlock></Border>";
                xaml = string.Format(xaml, msg);
                Border msgbox = (Border)XamlReader.Load(xaml);
                var mainFrame = Window.Current.Content as Frame;
                var page = mainFrame.Content as Page;
                var mainGrid = page.Content as Grid;
                mainGrid.Children.Add(msgbox);
                Animator.Use(AnimationType.FadeIn).PlayOn(msgbox);
                await Task.Delay(time);
                Animator.Use(AnimationType.FadeOutDown).PlayOn(msgbox);
                await Task.Delay(500);
                mainGrid.Children.Remove(msgbox);
            }
            catch (Exception)
            {

            }
        }
    }
}
