using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Friday.Views.Playground
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TopicListPage : Page
    {
        ObservableCollection<Class.Model.Playground.Main.TopicItem> topiclistdata { get; set; }
        bool firstload = true;

        public TopicListPage()
        {
            this.InitializeComponent();
            this.Transitions = new Windows.UI.Xaml.Media.Animation.TransitionCollection();
            this.Transitions.Add(new Windows.UI.Xaml.Media.Animation.PaneThemeTransition { Edge = EdgeTransitionLocation.Right });
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (topiclistdata == null)
            {
                await LoadTopicList();
            }
        }

        private async Task LoadTopicList()
        {
            LoadProgress.IsActive = true;
            try
            {
                var json = await Class.HttpPostUntil.HttpPost(Class.Data.Urls.Playground.GetAllTopicList);
                if (json != null)
                {
                    json = Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject()["commonList"].GetArray().ToString();
                    topiclistdata = Class.Data.Json.DataContractJsonDeSerialize<ObservableCollection<Class.Model.Playground.Main.TopicItem>>(json);
                    if (topiclistdata != null)
                    {
                        TopicList.ItemsSource = topiclistdata;
                    }
                    if (firstload)
                    {
                        TopicList.SelectionMode = ListViewSelectionMode.Single;
                        TopicList.SelectionChanged += TopicList_SelectionChanged;
                        firstload = false;
                    }
                    mainGrid.Visibility = Visibility.Visible;
                }
            }
            catch (Exception)
            {
                Class.Tools.ShowMsgAtFrame("网络有异常");
            }
            LoadProgress.IsActive = false;
        }

        private void TopicList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedItem != null)
            {
                var data = list.SelectedItem as Class.Model.Playground.Main.TopicItem;
                Frame.Navigate(typeof(TopicIndexPage), Class.Data.Json.ToJsonData(data));
                list.SelectedIndex = -1;
            }
        }

        private void GoBackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void RefreshList(DependencyObject sender, object args)
        {
            await LoadTopicList();
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
                    await LoadTopicList();
                }else
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
                    await LoadTopicList();
                }
                else
                {
                    Class.Tools.ShowMsgAtFrame("操作失败");
                }
            }
        }
    }
}
