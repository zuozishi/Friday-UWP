using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Friday.Class;
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
    public sealed partial class TopicIndexPage : Page
    {
        Grobal selectType = Grobal.all;
        Sex genderType = Sex.index;
        string timestamp = "0";
        string topicId = "";
        private Model.Playground.Main.TopicItem topicinfo;
        private Model.Playground.Topic obslist;

        public TopicIndexPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            this.Transitions = new Windows.UI.Xaml.Media.Animation.TransitionCollection();
            this.Transitions.Add(new Windows.UI.Xaml.Media.Animation.PaneThemeTransition { Edge = EdgeTransitionLocation.Right });
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            topicinfo = Class.Data.Json.DataContractJsonDeSerialize<Class.Model.Playground.Main.TopicItem>(e.Parameter.ToString());
            TopicInfoView.DataContext = topicinfo;
            if (topicId != topicinfo.topicIdInt)
            {
                topicId = topicinfo.topicIdInt;
                title.Text = topicinfo.nameStr;
                await LoadData();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                timestamp = "0";
                topicId = "";
                selectType = Grobal.all;
                genderType = Sex.index;
                obslist = null;
                Topic_gg.Visibility = Visibility.Collapsed;
                obsListView.Items.Clear();
            }
        }

        private async Task LoadData()
        {
            LoadProgress.IsActive = true;
            LoadMenu();
            try
            {
                var postdata=Class.HttpPostUntil.GetBasicPostData();
                postdata.Add(new KeyValuePair<string, string>("timestamp", timestamp));
                postdata.Add(new KeyValuePair<string, string>("selectType", ((int)selectType).ToString()));
                postdata.Add(new KeyValuePair<string, string>("genderType",((int)genderType).ToString()));
                postdata.Add(new KeyValuePair<string, string>("topicId", topicinfo.topicIdInt));
                var json = await Class.HttpPostUntil.HttpPost(Class.Data.Urls.Playground.getMessageByTopicId,new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                if (json != null)
                {
                    json = Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject().ToString();
                    obslist = Class.Data.Json.DataContractJsonDeSerialize<Class.Model.Playground.Topic>(json);
                    if (obslist.gg_BO != null)
                    {
                        Topic_gg.DataContext = obslist.gg_BO;
                        Topic_gg.Visibility = Visibility.Visible;
                    }
                    if (obslist.messageBOs != null&& obslist.messageBOs.Count>0)
                    {
                        if(obslist.messageBOs[0].studentBO == null) obslist.messageBOs.RemoveAt(0);
                    }
                    if(obslist.basicBOs!=null)
                    {
                        obsListView.Items.Clear();
                        foreach (var item in obslist.basicBOs)
                        {
                            obsListView.Items.Add(item);
                        }
                        obsListView.SelectionMode = ListViewSelectionMode.Single;
                        obsListView.SelectionChanged += ObsListView_SelectionChanged;
                        timestamp = obslist.basicBOs[obslist.basicBOs.Count - 1].issueTime.ToString();
                    }
                }
            }
            catch (Exception)
            {
                Class.Tools.ShowMsgAtFrame("网络有异常");
            }
            LoadProgress.IsActive = false;
        }

        private void ObsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedItem != null)
            {
                var data = list.SelectedItem as Class.Model.Playground.Topic.BOs;
                Frame.Navigate(typeof(OBDetalisPage), Class.Data.Json.ToJsonData(data));
                list.SelectedIndex = -1;
            }
        }

        private void LoadMenu()
        {
            var gobalmenu = new MenuFlyoutSubItem();
            var sexmenu = new MenuFlyoutSubItem();
            switch (selectType)
            {
                case Grobal.all:
                    gobalmenu.Text = "全国";
                    var gobalmenu1 = new MenuFlyoutItem();
                    gobalmenu1.Tag = "gobal";
                    gobalmenu1.Text = "本省";
                    gobalmenu1.TabIndex = 3;
                    gobalmenu1.Click += FliterMenu_Clicked;
                    gobalmenu.Items.Add(gobalmenu1);
                    break;
                case Grobal.part:
                    gobalmenu.Text = "本省";
                    var gobalmenu2 = new MenuFlyoutItem();
                    gobalmenu2.Tag = "gobal";
                    gobalmenu2.Text = "全国";
                    gobalmenu2.TabIndex = 0;
                    gobalmenu2.Click += FliterMenu_Clicked;
                    gobalmenu.Items.Add(gobalmenu2);
                    break;
                default:
                    break;
            }
            switch (genderType)
            {
                case Sex.index:
                    sexmenu.Text = "男/女";
                    var sexmenu1 = new MenuFlyoutItem();
                    sexmenu1.Tag = "sex";
                    sexmenu1.Text = "男";
                    sexmenu1.TabIndex = 1;
                    sexmenu1.Click += FliterMenu_Clicked;
                    sexmenu.Items.Add(sexmenu1);
                    var sexmenu2 = new MenuFlyoutItem();
                    sexmenu2.Tag = "sex";
                    sexmenu2.Text = "女";
                    sexmenu2.TabIndex = 0;
                    sexmenu2.Click += FliterMenu_Clicked;
                    sexmenu.Items.Add(sexmenu2);
                    break;
                case Sex.boy:
                    sexmenu.Text = "男";
                    var sexmenu3 = new MenuFlyoutItem();
                    sexmenu3.Tag = "sex";
                    sexmenu3.Text = "男/女";
                    sexmenu3.TabIndex = -1;
                    sexmenu3.Click += FliterMenu_Clicked;
                    sexmenu.Items.Add(sexmenu3);
                    var sexmenu4 = new MenuFlyoutItem();
                    sexmenu4.Tag = "sex";
                    sexmenu4.Text = "女";
                    sexmenu4.TabIndex = 0;
                    sexmenu4.Click += FliterMenu_Clicked;
                    sexmenu.Items.Add(sexmenu4);
                    break;
                case Sex.girl:
                    sexmenu.Text = "女";
                    var sexmenu5 = new MenuFlyoutItem();
                    sexmenu5.Tag = "sex";
                    sexmenu5.Text = "男/女";
                    sexmenu5.TabIndex = -1;
                    sexmenu5.Click += FliterMenu_Clicked;
                    sexmenu.Items.Add(sexmenu5);
                    var sexmenu6 = new MenuFlyoutItem();
                    sexmenu6.Tag = "sex";
                    sexmenu6.Text = "男";
                    sexmenu6.TabIndex = 1;
                    sexmenu6.Click += FliterMenu_Clicked;
                    sexmenu.Items.Add(sexmenu6);
                    break;
                default:
                    break;
            }
            filtermenu.Items.Clear();
            filtermenu.Items.Add(gobalmenu);
            filtermenu.Items.Add(sexmenu);
        }

        public enum Sex
        {
            index = -1, boy = 1, girl = 0
        }

        public enum Grobal
        {
            all = 0, part = 3
        }

        private async void FliterMenu_Clicked(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuFlyoutItem;
            if (menu.Tag.ToString() == "sex")
            {
                genderType = (Sex)menu.TabIndex;
            }
            else
            {
                selectType=(Grobal)menu.TabIndex;
            }
            
            timestamp = "0";
            await LoadData();
        }

        private void GoBackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
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
                    timestamp = "0";
                    topicId = "";
                    selectType = Grobal.all;
                    genderType = Sex.index;
                    obslist = null;
                    Topic_gg.Visibility = Visibility.Collapsed;
                    obsListView.Items.Clear();
                    await LoadData();
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
                    //await LoadTopicList();
                }
                else
                {
                    Class.Tools.ShowMsgAtFrame("操作失败");
                }
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

        private async void LoadNextPage(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv_SP = sender as ScrollViewer;
            if (sv_SP.VerticalOffset == sv_SP.ScrollableHeight)
            {
                LoadProgress.IsActive = true;
                try
                {
                    var postdata = Class.HttpPostUntil.GetBasicPostData();
                    postdata.Add(new KeyValuePair<string, string>("timestamp", timestamp));
                    postdata.Add(new KeyValuePair<string, string>("selectType", ((int)selectType).ToString()));
                    postdata.Add(new KeyValuePair<string, string>("genderType", ((int)genderType).ToString()));
                    postdata.Add(new KeyValuePair<string, string>("topicId", topicinfo.topicIdInt));
                    var json = await Class.HttpPostUntil.HttpPost(Class.Data.Urls.Playground.getMessageByTopicId, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                    if (json != null)
                    {
                        json = Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject().ToString();
                        var newobslist = Class.Data.Json.DataContractJsonDeSerialize<Class.Model.Playground.Topic>(json);
                        if (newobslist.messageBOs != null && newobslist.messageBOs.Count > 0)
                        {
                            if (newobslist.messageBOs[0].studentBO == null) newobslist.messageBOs.RemoveAt(0);
                        }
                        if (newobslist.basicBOs != null)
                        {
                            foreach (var item in newobslist.basicBOs)
                            {
                                obsListView.Items.Add(item);
                            }
                            timestamp = newobslist.basicBOs[newobslist.basicBOs.Count - 1].issueTime.ToString();
                        }
                    }
                }
                catch
                {
                    Class.Tools.ShowMsgAtFrame("网络有异常");
                }
                LoadProgress.IsActive = false;
            }
        }
    }
}
