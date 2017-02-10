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
    public sealed partial class MyCollectPage : Page
    {
        public ObservableCollection<Model.Playground.Topic.BOs> myobs = new ObservableCollection<Model.Playground.Topic.BOs>();
        long timestamp = 0;
        public MyCollectPage()
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
                LoadObsData();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
        }

        private async void LoadObsData()
        {
            if (HttpPostUntil.isInternetAvailable)
            {
                var postdata = HttpPostUntil.GetBasicPostData();
                postdata.Add(new KeyValuePair<string, string>("timestamp", timestamp.ToString()));
                var json = await HttpPostUntil.HttpPost(Data.Urls.user.mycollect, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
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

        private void PullToRefreshBox_RefreshInvoked(DependencyObject sender, object args)
        {
            myobs.Clear();
            timestamp = 0;
            LoadObsData();
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv_SP = sender as ScrollViewer;
            if (sv_SP.VerticalOffset == sv_SP.ScrollableHeight)
            {
                LoadObsData();
            }
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
    }
}
