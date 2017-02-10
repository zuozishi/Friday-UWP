using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Friday.Class;
using Windows.Data.Json;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Friday.Views.UserPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RegisterPage_School : Page
    {
        public ObservableCollection<School> nearschools=new ObservableCollection<School>();
        public List<School> allschools;
        public ObservableCollection<School> searchschools=new ObservableCollection<School>();
        public RegisterPage_School()
        {
            this.InitializeComponent();
            this.Transitions = new TransitionCollection();
            this.Transitions.Add(new PaneThemeTransition { Edge = EdgeTransitionLocation.Right });
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                if (allschools == null) GetAllSchools();
                if (nearschools.Count==0) GetNearSchools();
            }
        }

        private async void GetAllSchools()
        {
            if (HttpPostUntil.isInternetAvailable)
            {
                try
                {
                    var postdata = HttpPostUntil.GetBasicPostData();
                    postdata.Add(new KeyValuePair<string, string>("addTime", "0"));
                    var json = await HttpPostUntil.HttpPost(Data.Urls.user.Register.getUpdateSchoolList, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata), false);
                    json = JsonObject.Parse(json).GetNamedObject("data").GetNamedArray("schoolBOs").ToString();
                    allschools = Data.Json.DataContractJsonDeSerialize<List<School>>(json);
                    searchBox.IsEnabled = true;
                }
                catch (Exception)
                {
                    Tools.ShowMsgAtFrame("网络异常");
                }
            }
            else
            {
                Tools.ShowMsgAtFrame("网络异常");
            }
        }

        private async void GetNearSchools()
        {
            progressBar.Visibility = Visibility.Visible;
            if (HttpPostUntil.isInternetAvailable)
            {
                try
                {
                    label1:
                    Windows.Devices.Geolocation.Geolocator location = new Windows.Devices.Geolocation.Geolocator();
                    try
                    {
                        var position = await location.GetGeopositionAsync();
                        if (position != null)
                        {
                            var postdata = HttpPostUntil.GetBasicPostData();
                            postdata.Add(new KeyValuePair<string, string>("lng",position.Coordinate.Point.Position.Longitude.ToString()));
                            postdata.Add(new KeyValuePair<string, string>("lat", position.Coordinate.Point.Position.Latitude.ToString()));
                            var json = await HttpPostUntil.HttpPost(Data.Urls.user.Register.getRecommendSchoolList, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata), false);
                            json = JsonObject.Parse(json).GetNamedObject("data").GetNamedArray("schoolBOs").ToString();
                            var schools = Data.Json.DataContractJsonDeSerialize<List<School>>(json);
                            if (schools != null)
                            {
                                foreach (var item in schools)
                                {
                                    nearschools.Add(item);
                                }
                            }
                            else
                            {
                                Tools.ShowMsgAtFrame("附近没有学校");
                            }
                        }
                    }
                    catch (Exception)
                    {
                        var dialog = new Windows.UI.Popups.MessageDialog("无法定位,请打开定位开关")
                        {
                            Title = "提示"
                        };
                        dialog.Commands.Add(new Windows.UI.Popups.UICommand("转到设置") { Id = 0 });
                        dialog.Commands.Add(new Windows.UI.Popups.UICommand("重试") { Id = 1 });
                        dialog.DefaultCommandIndex = 0;
                        var res = await dialog.ShowAsync();
                        if ((int)res.Id == 1) goto label1;
                        if ((int)res.Id == 0)
                        {
                            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-location"));
                        }
                    }
                }
                catch (Exception)
                {
                    Tools.ShowMsgAtFrame("网络异常");
                }
            }
            else
            {
                Tools.ShowMsgAtFrame("网络异常");
            }
            progressBar.Visibility = Visibility.Collapsed;
        }

        private void GoBackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        public class School
        {
            public string id { get; set; }
            public string name { get; set; }
            public string initials { get; set; }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = (sender as TextBox).Text;
            if (text.Length == 0)
            {
                nearGrid.Visibility = Visibility.Visible;
                searchGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                searchGrid.Visibility = Visibility.Visible;
                nearGrid.Visibility = Visibility.Collapsed;
                progressBar.Visibility = Visibility.Visible;
                searchschools.Clear();
                foreach (var item in allschools)
                {
                    if (text == GetStrByLen(item.name, text.Length) || text == GetStrByLen(item.initials, text.Length))
                        searchschools.Add(item);
                }
                progressBar.Visibility = Visibility.Collapsed;
            }
        }
        
        private string GetStrByLen(string str,int len)
        {
            string res = "";
            if (str==null||str.Length < len) return res;
            for (int i = 0; i < len; i++)
            {
                res = res + str[i];
            }
            return res;
        }

        private void SchoolSelected(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedItem != null)
            {
                Frame.Navigate(typeof(RegisterPage_Xueyuan),(list.SelectedItem as School).id);
                list.SelectedIndex = -1;
            }
        }
    }
}
