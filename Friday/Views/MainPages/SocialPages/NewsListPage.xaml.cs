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
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Friday.Views.MainPages.SocialPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NewsListPage : Page
    {
        double timestamp = 0;
        public ObservableCollection<Class.Model.Social.SocialNews.New.messageBO> obsdata =new ObservableCollection<Class.Model.Social.SocialNews.New.messageBO>();
        public NewsListPage()
        {
            this.InitializeComponent();
            this.Transitions = new Windows.UI.Xaml.Media.Animation.TransitionCollection();
            this.Transitions.Add(new Windows.UI.Xaml.Media.Animation.PaneThemeTransition { Edge = EdgeTransitionLocation.Right });
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadData();
        }

        private async void LoadData()
        {
            prgs.IsActive = true;
            if (Class.HttpPostUntil.isInternetAvailable)
            {
                var postdata = Class.HttpPostUntil.GetBasicPostData();
                postdata.Add(new KeyValuePair<string, string>("timestamp", timestamp.ToString()));
                var json = await Class.HttpPostUntil.HttpPost(Class.Data.Urls.Social.getNewsList, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                if (json != null)
                {
                    var obj = Windows.Data.Json.JsonObject.Parse(json);
                    timestamp = obj.GetNamedObject("data").GetNamedNumber("timestampLong");
                    var obs = Class.Data.Json.DataContractJsonDeSerialize<List<Class.Model.Social.SocialNews.New.messageBO>>(obj.GetNamedObject("data").GetNamedArray("messageBOs").ToString());
                    if (obs != null)
                    {
                        foreach (var item in obs)
                        {
                            obsdata.Add(item);
                        }
                    }
                }
            }
            else
            {
                Class.Tools.ShowMsgAtFrame("网络异常");
            }
            prgs.IsActive = false;
        }

        private void GoBackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void PullToRefreshBox_RefreshInvoked(DependencyObject sender, object args)
        {
            obsdata.Clear();
            timestamp = 0;
            LoadData();
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv_SP = sender as ScrollViewer;
            if (sv_SP.VerticalOffset == sv_SP.ScrollableHeight)
            {
                LoadData();
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedItem != null)
            {
                var data = list.SelectedItem as Class.Model.Social.SocialNews.New.messageBO;
                Frame.Navigate(typeof(NewDetalPage),Class.Data.Json.ToJsonData(data));
                list.SelectedIndex = -1;
            }
        }
    }
}
