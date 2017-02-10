using Friday.Class;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class UserIndexPage : Page
    {
        public ObservableCollection<Model.Playground.Topic.BOs> myobs = new ObservableCollection<Model.Playground.Topic.BOs>();
        long timestamp = 0;
        string studentId = "";
        public UserIndexPage()
        {
            this.InitializeComponent();
            this.Transitions = new TransitionCollection();
            this.Transitions.Add(new PaneThemeTransition { Edge = EdgeTransitionLocation.Right });
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Edi.UWP.Helpers.UI.SetWindowsMobileStatusBarColor(Color.FromArgb(255, 7, 153, 252),Colors.White);
            if (e.NavigationMode == NavigationMode.New)
            {
                myobs.Clear();
                studentId = e.Parameter.ToString();
                LoadUserData();
                LoadObsData();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Edi.UWP.Helpers.UI.SetWindowsMobileStatusBarColor(Colors.White, Color.FromArgb(255, 7, 153, 252));
            if (e.NavigationMode == NavigationMode.Back)
            {
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
        }

        private async void LoadUserData()
        {
            if (HttpPostUntil.isInternetAvailable)
            {
                var postdata = HttpPostUntil.GetBasicPostData();
                postdata.Add(new KeyValuePair<string, string>("studentId", studentId));
                var json = await HttpPostUntil.HttpPost(Data.Urls.user.getTopInfoById, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                try
                {
                    json = JsonObject.Parse(json).GetNamedObject("data").ToString();
                    var stu = Data.Json.DataContractJsonDeSerialize<Student>(json);
                    topGrid2.DataContext = stu;
                }
                catch (Exception)
                {
                    Tools.ShowMsgAtFrame("没有主题");
                }
            }
            else
            {
                Tools.ShowMsgAtFrame("网络异常");
            }
        }

        private async void LoadObsData()
        {
            if (HttpPostUntil.isInternetAvailable)
            {
                var postdata = HttpPostUntil.GetBasicPostData();
                postdata.Add(new KeyValuePair<string, string>("timestamp", timestamp.ToString()));
                postdata.Add(new KeyValuePair<string, string>("studentId", studentId));
                var json = await HttpPostUntil.HttpPost(Data.Urls.user.showMovement, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                try
                {
                    json = JsonObject.Parse(json).GetNamedObject("data").GetNamedArray("messageBOs").ToString();
                    var obs = Data.Json.DataContractJsonDeSerialize<List<Model.Playground.Topic.BOs>>(json);
                    foreach (var item in obs)
                    {
                        myobs.Add(item);
                    }
                    timestamp = obs[obs.Count - 1].issueTime;
                }
                catch (Exception)
                {
                    Tools.ShowMsgAtFrame("没有主题");
                }
            }
            else
            {
                Tools.ShowMsgAtFrame("网络异常");
            }
        }

        private void GoBackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void obsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedItem != null)
            {
                var ob = list.SelectedItem as Model.Playground.Topic.BOs;
                Frame.Navigate(typeof(Playground.OBDetalisPage), Data.Json.ToJsonData(ob));
                list.SelectedIndex = -1;
            }
        }

        private void obsImagesClicked(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as GridView;
            if (list.SelectedIndex >= 0)
            {
                var images = list.ItemsSource as List<Class.Model.Playground.Topic.BOs.qiniuImgBO>;
                var imglist = new List<string>();
                foreach (var item in images)
                {
                    imglist.Add(item.url);
                }
                (Window.Current.Content as Frame).Navigate(typeof(PicsViewPage), new string[] { Class.Data.Json.ToJsonData(imglist), list.SelectedIndex.ToString() });
                list.SelectedIndex = -1;
            }
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv_SP = sender as ScrollViewer;
            if (sv_SP.VerticalOffset == sv_SP.ScrollableHeight)
            {
                LoadObsData();
            }
            if (sv_SP.VerticalOffset > topGrid2.Height-50 && topGrid2.Visibility == Visibility.Visible)
            {
                topGrid1.Visibility = Visibility.Visible;
                topGrid2.Visibility = Visibility.Collapsed;
            }
            if (sv_SP.VerticalOffset < topGrid2.Height - 50 && topGrid2.Visibility == Visibility.Collapsed)
            {
                topGrid1.Visibility = Visibility.Collapsed;
                topGrid2.Visibility = Visibility.Visible;
            }
        }

        public class Student
        {
            public string avatarUrl { get; set; }
            public string bigAvatarUrl { get; set; }
            public long bornDate { get; set; }
            public string fullAvatarUrl { get; set; }
            public int gender { get; set; }
            public string nickName { get; set; }
            public int rate { get; set; }
            public string schoolName { get; set; }
            public string signature { get; set; }
            public int studentId { get; set; }
            public int studentType { get; set; }
            public int todayVisit { get; set; }
            public int totalVisit { get; set; }
            public string sex
            {
                get
                {
                    if (studentType == 0)
                    {
                        return "ms-appx:///Assets/images/ic_th_sex_boy.png";
                    }
                    else if (studentType == 2)
                    {
                        return "ms-appx:///Assets/images/ic_th_sex_girl.png";
                    }
                    else
                    {
                        return "";
                    }
                }
            }
        }

        private void Ellipse_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var list = new List<string>();
            list.Add((topGrid2.DataContext as Student).bigAvatarUrl);
            (Window.Current.Content as Frame).Navigate(typeof(PicsViewPage), new string[] { Data.Json.ToJsonData(list), "0" });
        }
    }
}
