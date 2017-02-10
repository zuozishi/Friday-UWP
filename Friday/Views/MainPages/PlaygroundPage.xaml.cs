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
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Friday.Views.MainPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlaygroundPage : Page
    {
        private Class.Model.Playground.Main maindata { get; set; }

        public PlaygroundPage()
        {
            this.InitializeComponent();
            this.Transitions = new Windows.UI.Xaml.Media.Animation.TransitionCollection();
            this.Transitions.Add(new Windows.UI.Xaml.Media.Animation.PaneThemeTransition { Edge = EdgeTransitionLocation.Right });
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (maindata == null)
            {
                await LoadMainData();
            }
        }

        private async Task LoadMainData()
        {
            LoadProgress.IsActive = true;
            try
            {
                var json = await Class.HttpPostUntil.HttpPost(Class.Data.Urls.Playground.index);
                if (json != null)
                {
                    json = Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject().ToString();
                    maindata = Class.Data.Json.DataContractJsonDeSerialize<Class.Model.Playground.Main>(json);
                    if (maindata != null)
                    {
                        if (maindata.hotList != null)
                        {
                            var piclist = new List<string>();
                            foreach (var item in maindata.hotList)
                            {
                                piclist.Add(item.picUrlStr);
                            }
                            TopPicsView.SetItems(piclist);
                        }
                        if (maindata.recommendMessageList != null)
                        {
                            TuiJianContentView.ItemsSource = maindata.recommendMessageList;
                            ContentNum.Text = maindata.recommendMessageList.Count.ToString();
                        }
                        if (maindata.recommendTopicList != null)
                        {
                            TopicList.ItemsSource = maindata.recommendTopicList;
                            TopicList.SelectionMode = ListViewSelectionMode.Single;
                            TopicList.SelectionChanged += GoToTopicPage;
                        }
                        if (maindata.followList != null&& maindata.followList.Count>0)
                        {
                            FollowTopicList.ItemsSource = maindata.followList;
                            NoFollowView.Visibility = Visibility.Collapsed;
                            FollowTopicList.SelectionMode = ListViewSelectionMode.Single;
                            FollowTopicList.SelectionChanged += GoToTopicPage;
                        }
                        else
                        {
                            NoFollowView.Visibility = Visibility.Visible;
                        }
                        MainView.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception)
            {
                Class.Tools.ShowMsgAtFrame("网络有异常");
            }
            LoadProgress.IsActive = false;
        }

        private void GoToTopicPage(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedItem != null)
            {
                var data = list.SelectedItem as Class.Model.Playground.Main.TopicItem;
                (Window.Current.Content as Frame).Navigate(typeof(Playground.TopicIndexPage),Class.Data.Json.ToJsonData(data));
                list.SelectedIndex = -1;
            }
        }

        private async void RefreshPage(DependencyObject sender, object args)
        {
            await LoadMainData();
        }

        private void GoToTopicList(object sender, RoutedEventArgs e)
        {
            (Window.Current.Content as Frame).Navigate(typeof(Playground.TopicListPage));
        }

        private void GoToTopicList1(object sender, RoutedEventArgs e)
        {
            (Window.Current.Content as Frame).Navigate(typeof(Playground.TopicListPage));
        }

        private async void FollowTopic(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var data = btn.DataContext as Class.Model.Playground.Main.TopicItem;
            if (data.isFollow == 0)
            {
                var result = await Class.Until.Friday.Playground.FollowTopic(data.topicIdInt);
                if (result)
                {
                    Class.Tools.ShowMsgAtFrame("已关注:" + data.nameStr);
                    await LoadMainData();
                }
                else
                {
                    Class.Tools.ShowMsgAtFrame("操作失败");
                }
            }
            else
            {
                var result = await Class.Until.Friday.Playground.UnFollowTopic(data.topicIdInt);
                if (result)
                {
                    Class.Tools.ShowMsgAtFrame("已取消关注:" + data.nameStr);
                    await LoadMainData();
                }
                else
                {
                    Class.Tools.ShowMsgAtFrame("操作失败");
                }
            }
        }
    }
}
