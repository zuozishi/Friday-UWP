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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DiDiLite
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(5) };
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            timer.Tick += (s, arg) =>
            {
                gotoBtn.Visibility = Visibility.Visible;
                timer.Stop();
            };
            timer.Start();
            Windows.Devices.Geolocation.Geolocator location = new Windows.Devices.Geolocation.Geolocator();
            try
            {
                var position = await location.GetGeopositionAsync();
                if (position != null)
                {
                    double[] data = { position.Coordinate.Point.Position.Latitude,position.Coordinate.Point.Position.Longitude};
                    Frame.Navigate(typeof(WebViewPage),data);
                }
            }
            catch (Exception)
            {
                var dialog = new Windows.UI.Popups.MessageDialog("无法定位,请打开定位开关以获取您的出发地点")
                {
                    Title = "提示"
                };
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("转到设置") { Id = 0 });
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("忽略") { Id = 1 });
                dialog.DefaultCommandIndex = 0;
                var res = await dialog.ShowAsync();
                if ((int)res.Id == 1) Frame.Navigate(typeof(WebViewPage));
                if ((int)res.Id == 0)
                {
                    await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-location"));
                    Application.Current.Exit();
                }
            }
        }

        private void gotoBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(WebViewPage));
        }
    }
}
