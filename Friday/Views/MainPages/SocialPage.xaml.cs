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
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Friday.Views.MainPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SocialPage : Page
    {
        List<Class.Model.Social.Toolbar> toolbardata { get; set; }
        Class.Model.Social.SocialNews.Club clubdata { get; set; }
        Class.Model.Social.SocialNews.New newesdata { get; set; }
        Class.Model.Social.Cave cavedata { get; set; }
        Class.Model.Social.Flea fleadata { get; set; }
        DispatcherTimer caveopentimer { get; set; }
        double cave_timestamp = 0;
        double cave_preMoodTimestap = 0;
        double flea_timestamp = 0;
        int flea_readFlag = 0;
        bool CanLoad = false;

        public SocialPage()
        {
            this.InitializeComponent();
            this.Transitions = new Windows.UI.Xaml.Media.Animation.TransitionCollection();
            this.Transitions.Add(new Windows.UI.Xaml.Media.Animation.PaneThemeTransition { Edge = EdgeTransitionLocation.Right });
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            CanLoad = true;
            if (clubdata == null || newesdata == null)
            {
                ListInit();
                LoadZiXunData();
            }
        }

        private void ListInit()
        {
            CaveListView.SelectionMode = ListViewSelectionMode.Single;
            CaveListView.SelectedIndex = -1;
            CaveListView.SelectionChanged += (s, e) =>
            {
                var cavelist = s as ListView;
                if (cavelist.SelectedIndex != -1)
                {
                    (Window.Current.Content as Frame).Navigate(typeof(Playground.OBDetalisPage), Class.Data.Json.ToJsonData(cavelist.SelectedItem));
                    CaveListView.SelectedIndex = -1;
                }
            };
            FleaListView.SelectionMode = ListViewSelectionMode.Single;
            FleaListView.SelectedIndex = -1;
            FleaListView.SelectionChanged += (s, e) =>
            {
                var flealist = s as ListView;
                if (flealist.SelectedIndex != -1)
                {
                    (Window.Current.Content as Frame).Navigate(typeof(Playground.OBDetalisPage), Class.Data.Json.ToJsonData(flealist.SelectedItem));
                    CaveListView.SelectedIndex = -1;
                }
            };
        }

        private async void LoadZiXunData()
        {
            if (Class.HttpPostUntil.isInternetAvailable)
            {
                LoadProgress.IsActive = true;
                try
                {
                    LoadTodayCourse();
                    //await LoadToolbars();
                    await LoadNews();
                    ChangeFlipViewVisibility(0, Visibility.Visible);
                }
                catch (Exception)
                {
                    Class.Tools.ShowMsgAtFrame("网络有异常");
                }
                LoadProgress.IsActive = false;
            }
        }

        private async void LoadTodayCourse()
        {
            DateText.Text = string.Format("{0}月{1}日 星期{2}",DateTime.Today.Month.ToString("D2"), DateTime.Today.Day.ToString("D2"), Class.Data.Int_String.GetWeekString(DateTime.Today.DayOfWeek));
            var year= (Class.UserManager.UserData.beginYear%2000 - Class.UserManager.UserData.grade) +1;
            TermText.Text = string.Format("大{0} 第{1}学期 第{2}周",Class.Data.Int_String.NumberToChinese(year.ToString()), Class.Data.Int_String.NumberToChinese(Class.UserManager.UserData.term.ToString()), Class.UserManager.UserData.attachmentBO.nowWeekMsg.nowWeek);
            var courselist = await Class.Model.CourseManager.GetCourse();
            var newcourselist = new ObservableCollection<Class.Model.CourseManager.CourseModel>();
            for (int i = 0; i < courselist.Count; i++)
            {
                if (courselist[i].day == 7) courselist[i].day = 0;
                if (courselist[i].day == (int)DateTime.Today.DayOfWeek) newcourselist.Add(courselist[i]);
            }
            if (newcourselist.Count == 0)
            {
                NoTodayCourseGrid.Visibility = Visibility.Visible;
            }
            else
            {
                NoTodayCourseGrid.Visibility = Visibility.Collapsed;
            }
        }

        private async void LoadCaveData()
        {
            if (Class.HttpPostUntil.isInternetAvailable)
            {
                LoadProgress.IsActive = true;
                try
                {
                    if (caveopentimer == null)
                    {
                        caveopentimer = new DispatcherTimer();
                        caveopentimer.Interval = TimeSpan.FromMilliseconds(1000);
                        caveopentimer.Tick += (s, e) =>
                        {
                            DateTime today = DateTime.Now;
                            DateTime nexttime;
                            string text = "";
                            bool iswhite = false;
                            if (today.Hour >= 6 && today.Hour < 20)
                            {
                                text = "距离黑洞开启{0}:{1}:{2}";
                                iswhite = true;
                            }
                            else
                            {
                                text = "距离白洞开启{0}:{1}:{2}";
                            }
                            if (iswhite)
                            {
                                if (CaveViewGrid.RequestedTheme != ElementTheme.Light)
                                {
                                    CaveViewGrid.RequestedTheme = ElementTheme.Light;
                                }
                                nexttime = new DateTime(today.Year, today.Month, today.Day, 20, 0, 0);
                                var time = nexttime - today;
                                CaveOpenTimeText.Text = string.Format(text, time.Hours.ToString("D2"), time.Minutes.ToString("D2"), time.Seconds.ToString("D2"));
                            }
                            else
                            {
                                if (CaveViewGrid.RequestedTheme != ElementTheme.Dark)
                                {
                                    CaveViewGrid.RequestedTheme = ElementTheme.Dark;
                                }
                                if (today.Hour < 6)
                                {
                                    nexttime = new DateTime(today.Year, today.Month, today.Day, 6, 0, 0);
                                    var time = nexttime - today;
                                    CaveOpenTimeText.Text = string.Format(text, time.Hours.ToString("D2"), time.Minutes.ToString("D2"), time.Seconds.ToString("D2"));
                                }
                                else
                                {
                                    nexttime = new DateTime(today.Year, today.Month, today.Day + 1, 6, 0, 0);
                                    var time = nexttime - today;
                                    CaveOpenTimeText.Text = string.Format(text, time.Hours.ToString("D2"), time.Minutes.ToString("D2"), time.Seconds.ToString("D2"));
                                }
                            }
                        };
                    }
                    caveopentimer.Start();
                    var postdata = Class.HttpPostUntil.GetBasicPostData();
                    cave_preMoodTimestap = (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
                    postdata.Add(new KeyValuePair<string, string>("timestamp", cave_timestamp.ToString()));
                    postdata.Add(new KeyValuePair<string, string>("preMoodTimestap", Math.Floor(cave_preMoodTimestap).ToString()));
                    var json = await Class.HttpPostUntil.HttpPost(Class.Data.Urls.Social.Cave, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                    json = Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject().ToString();
                    cavedata = Class.Data.Json.DataContractJsonDeSerialize<Class.Model.Social.Cave>(json);
                    CaveViewGrid.DataContext = cavedata;
                    foreach (var item in cavedata.basicBOs)
                    {
                        CaveListView.Items.Add(item);
                    }
                    ChangeFlipViewVisibility(1, Visibility.Visible);
                }
                catch (Exception)
                {
                    Class.Tools.ShowMsgAtFrame("网络有异常");
                }
                LoadProgress.IsActive = false;
            }
        }

        private async void LoadFleaData()
        {
            LoadProgress.IsActive = true;
            try
            {
                if (Class.HttpPostUntil.isInternetAvailable)
                {
                    var postdata = Class.HttpPostUntil.GetBasicPostData();
                    postdata.Add(new KeyValuePair<string, string>("timestamp", flea_timestamp.ToString()));
                    postdata.Add(new KeyValuePair<string, string>("readFlag", flea_readFlag.ToString()));
                    var json = await Class.HttpPostUntil.HttpPost(Class.Data.Urls.Social.Flea, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                    json = Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject().ToString();
                    fleadata = Class.Data.Json.DataContractJsonDeSerialize<Class.Model.Social.Flea>(json);
                    if (fleadata != null && fleadata.messageBOs != null)
                    {
                        FleaGridView.DataContext = fleadata;
                        foreach (var item in fleadata.basicBOs)
                        {
                            FleaListView.Items.Add(item);
                        }
                    }
                    ChangeFlipViewVisibility(2, Visibility.Visible);
                }
            }
            catch (Exception)
            {
                Class.Tools.ShowMsgAtFrame("网络有异常");
            }
            LoadProgress.IsActive = false;
        }

        private async Task LoadToolbars()
        {
            var json = await Class.HttpPostUntil.HttpPost(Class.Data.Urls.Social.findToolbars, new Windows.Web.Http.HttpFormUrlEncodedContent(Class.HttpPostUntil.GetBasicPostData()));
            if (json != null)
            {
                var obj = Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject()["moduleBOs"].GetArray();
                toolbardata = Class.Data.Json.DataContractJsonDeSerialize<List<Class.Model.Social.Toolbar>>(obj.ToString());
                if (toolbardata != null)
                {
                    ToolBarView.ItemsSource = toolbardata;
                }
            }
        }

        private async Task LoadNews()
        {
            var json = await Class.HttpPostUntil.HttpPost(Class.Data.Urls.Social.getCampusNews, new Windows.Web.Http.HttpFormUrlEncodedContent(Class.HttpPostUntil.GetBasicPostData()));
            if (json != null)
            {
                var obj = Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject();
                clubdata = new Class.Model.Social.SocialNews.Club();
                clubdata.club_data = Class.Data.Json.DataContractJsonDeSerialize<Class.Model.Social.SocialNews.Club.Club_Data>(obj["clubs"].GetObject().ToString());
                clubdata .messageBOs= Class.Data.Json.DataContractJsonDeSerialize<ObservableCollection<Class.Model.Social.SocialNews.Club.messageBO>>(obj["clubs"].GetObject()["data"].GetObject()["messageBOs"].GetArray().ToString());
                if (clubdata.club_data != null && clubdata.messageBOs != null)
                {
                    Clubs_View.DataContext = clubdata;
                }
                newesdata = new Class.Model.Social.SocialNews.New();
                newesdata.new_data = Class.Data.Json.DataContractJsonDeSerialize<Class.Model.Social.SocialNews.New.New_Data>(obj["newses"].GetObject().ToString());
                newesdata.messageBOs = Class.Data.Json.DataContractJsonDeSerialize<ObservableCollection<Class.Model.Social.SocialNews.New.messageBO>>(obj["newses"].GetObject()["data"].GetObject()["messageBOs"].GetArray().ToString());
                if (newesdata.new_data != null && newesdata.messageBOs != null)
                {
                    NewThingsView.DataContext = newesdata;
                }
            }
        }

        private void ChangeFlipViewVisibility(int index,Visibility visbility)
        {
            var item = flipview.Items[index] as FlipViewItem;
            var viewbox = item.Content as PullToRefresh.UWP.PullToRefreshBox;
            viewbox.Visibility = visbility;
        }

        private void TopNavBtn_Clicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            flipview.SelectedIndex = int.Parse(btn.Tag.ToString());
        }

        private void flipview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var view = sender as FlipView;
            switch (view.SelectedIndex)
            {
                case 0:
                    TopNavGrid0.BorderThickness = new Thickness(0, 0, 0, 2);
                    TopNavGrid1.BorderThickness = new Thickness(0);
                    TopNavGrid2.BorderThickness = new Thickness(0);
                    if (clubdata == null || newesdata == null)
                    {
                        if (CanLoad)
                        {
                            LoadZiXunData();
                        }
                    }
                    if(caveopentimer!=null) caveopentimer.Stop();
                    break;
                case 1:
                    TopNavGrid0.BorderThickness = new Thickness(0);
                    TopNavGrid1.BorderThickness = new Thickness(0, 0, 0, 2);
                    TopNavGrid2.BorderThickness = new Thickness(0);
                    if (cavedata == null)
                    {
                        if (CanLoad)
                        {
                            LoadCaveData();
                        }
                    }
                    if (caveopentimer != null) caveopentimer.Start();
                    break;
                case 2:
                    TopNavGrid0.BorderThickness = new Thickness(0);
                    TopNavGrid1.BorderThickness = new Thickness(0);
                    TopNavGrid2.BorderThickness = new Thickness(0, 0, 0, 2);
                    if (fleadata == null)
                    {
                        if (CanLoad)
                        {
                            LoadFleaData();
                        }
                    }
                    if (caveopentimer != null) caveopentimer.Stop();
                    break;
                default:
                    break;
            }
        }

        private void RefreshView(DependencyObject sender, object args)
        {
            var viewbox = sender as ScrollViewer;
            switch (viewbox.TabIndex)
            {
                case 0:
                    toolbardata = null;
                    clubdata = null;
                    newesdata = null;
                    LoadZiXunData();
                    break;
                case 1:
                    cavedata = null;
                    cave_timestamp = 0;
                    cave_preMoodTimestap = 0;
                    LoadCaveData();
                    break;
                case 2:
                    fleadata = null;
                    flea_timestamp = 0;
                    flea_readFlag = 0;
                    LoadFleaData();
                    break;
                default:
                    break;
            }
        }

        private async void LoadCaveNextPage(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv_SP = sender as ScrollViewer;
            if (sv_SP.VerticalOffset == sv_SP.ScrollableHeight)
            {
                if (Class.HttpPostUntil.isInternetAvailable)
                {
                    LoadProgress.IsActive = true;
                    cave_timestamp = (this.cavedata.basicBOs.Count > 0) ? this.cavedata.basicBOs[cavedata.basicBOs.Count - 1].issueTime : 0L;
                    var postdata = Class.HttpPostUntil.GetBasicPostData();
                    cave_preMoodTimestap = (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
                    postdata.Add(new KeyValuePair<string, string>("timestamp", cave_timestamp.ToString()));
                    postdata.Add(new KeyValuePair<string, string>("preMoodTimestap", Math.Floor(cave_preMoodTimestap).ToString()));
                    var json = await Class.HttpPostUntil.HttpPost(Class.Data.Urls.Social.Cave, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                    json = Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject().ToString();
                    var newcavedata = Class.Data.Json.DataContractJsonDeSerialize<Class.Model.Social.Cave>(json);
                    if (newcavedata != null && newcavedata.basicBOs != null)
                    {
                        foreach (var item in newcavedata.basicBOs)
                        {
                            cavedata.messageBOs.Add(item);
                            CaveListView.Items.Add(item);
                        }
                    }
                    LoadProgress.IsActive = false;
                }
            }
        }

        private async void LoadFleaNextPage(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv_SP = sender as ScrollViewer;
            if (sv_SP.VerticalOffset == sv_SP.ScrollableHeight)
            {
                LoadProgress.IsActive = true;
                var postdata = Class.HttpPostUntil.GetBasicPostData();
                flea_timestamp = (this.fleadata.basicBOs.Count > 0) ? this.fleadata.basicBOs[fleadata.basicBOs.Count - 1].issueTime : 0L;
                flea_readFlag = flea_readFlag + 1;
                postdata.Add(new KeyValuePair<string, string>("timestamp", flea_timestamp.ToString()));
                postdata.Add(new KeyValuePair<string, string>("readFlag", flea_readFlag.ToString()));
                var json = await Class.HttpPostUntil.HttpPost(Class.Data.Urls.Social.Flea, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                json = Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject().ToString();
                 var newfleadata = Class.Data.Json.DataContractJsonDeSerialize<Class.Model.Social.Flea>(json);
                if (newfleadata != null&& newfleadata.messageBOs!=null)
                {
                    foreach (var item in newfleadata.basicBOs)
                    {
                        fleadata.messageBOs.Add(item);
                        FleaListView.Items.Add(item);
                    }
                }
                LoadProgress.IsActive = false;
            }
        }

        private void FleaImagesClicked(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as GridView;
            if (list.SelectedIndex >= 0)
            {
                var images = list.ItemsSource as List<Class.Model.Social.Flea.messageBO.qiniuImgBO>;
                var imglist = new List<string>();
                foreach (var item in images)
                {
                    imglist.Add(item.url);
                }
                (Window.Current.Content as Frame).Navigate(typeof(PicsViewPage), new string[] { Class.Data.Json.ToJsonData(imglist), list.SelectedIndex.ToString()});
                list.SelectedIndex = -1;
            }
        }

        private void CaveImagesClicked(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as GridView;
            if (list.SelectedIndex >= 0)
            {
                var images = list.ItemsSource as List<Class.Model.Social.Cave.messageBO.qiniuImgBO>;
                var imglist = new List<string>();
                foreach (var item in images)
                {
                    imglist.Add(item.url);
                }
                (Window.Current.Content as Frame).Navigate(typeof(PicsViewPage), new string[] { Class.Data.Json.ToJsonData(imglist), list.SelectedIndex.ToString() });
                list.SelectedIndex = -1;
            }
        }

        private void AddCourseBtn_Clicked(object sender, RoutedEventArgs e)
        {
            (Window.Current.Content as Frame).Navigate(typeof(Course.CourseListPage));
        }

        private async void ToolBtnsClicked(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as GridView;
            switch (list.SelectedIndex)
            {
                case 0:
                    //考试查询
                    (Window.Current.Content as Frame).Navigate(typeof(WebViewPage), "http://112.124.54.19/Score/index.html?identity="+ Class.UserManager.UserData.identity);
                    list.SelectedIndex = -1;
                    break;
                case 1:
                    //OneNote
                    list.SelectedIndex = -1;
                    await Windows.System.Launcher.LaunchUriAsync(new Uri("onenote:"));
                    break;
                case 2:
                    //考试倒计时
                    var dialog1 = new Controls.DialogBox()
                    {
                        Title = "提示",
                        PrimaryButtonText = "取消",
                        SecondaryButtonText = "确定",
                    };
                    dialog1.mainTextBlock.Text = "尚未完成";
                    await dialog1.ShowAsync();
                    list.SelectedIndex = -1;
                    break;
                case 3:
                    //滴滴打车
                    LoadProgress.IsActive = true;
                    list.SelectedIndex = -1;
                    if (!Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("firstdidi_" + Edi.UWP.Helpers.Utils.GetAppVersion()))
                    {
                        await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://pdp/?productid=9p1l6rlcfr5r"));
                        Windows.Storage.ApplicationData.Current.LocalSettings.Values["firstdidi_" + Edi.UWP.Helpers.Utils.GetAppVersion()] = true;
                    }
                    Windows.Devices.Geolocation.Geolocator location = new Windows.Devices.Geolocation.Geolocator();
                    try
                    {
                        var position = await location.GetGeopositionAsync();
                        if (position != null)
                        {
                            string url = string.Format(@"http://common.diditaxi.com.cn/general/webEntry?from=sdkwebapp&hack=allbiz&openid=TWpjek1UVTVPVFF4TVRnMk5nPT0%3D&channel=71650&datatype=webapp&sign=116b6136e919ecc743704bf8c41352621473005289&openid=TWpjek1UVTVPVFF4TVRnMk5nPT0%3D&phone={0}&fromlat={1}&biz=1&fromlng={2}#", Class.UserManager.UserData.mobileNumber, position.Coordinate.Point.Position.Latitude, position.Coordinate.Point.Position.Longitude);
                            (Window.Current.Content as Frame).Navigate(typeof(WebViewPage), url);
                        }
                    }
                    catch (Exception)
                    {
                        var dialog = new Controls.DialogBox()
                        {
                            Title="提示",
                            PrimaryButtonText="取消",
                            SecondaryButtonText="转到设置",
                        };
                        LoadProgress.IsActive = false;
                        dialog.mainTextBlock.Text = "无法定位,请打开定位开关!";
                        if (await dialog.ShowAsync() == ContentDialogResult.Secondary)await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-location"));
                    }
                    LoadProgress.IsActive = false;
                    break;
                case 4:
                    //Office Lens
                    list.SelectedIndex = -1;
                    await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://pdp/?PFN=Microsoft.OfficeLens_8wekyb3d8bbwe"));
                    break;
                default:
                    break;
            }
        }

        private void FirstNewBtn_Clicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var data = btn.DataContext as Class.Model.Social.SocialNews.New.messageBO;
            try
            {
                var postdata = Class.HttpPostUntil.GetBasicPostData();
                (Window.Current.Content as Frame).Navigate(typeof(SocialPages.NewDetalPage),Class.Data.Json.ToJsonData(data));
            }
            catch (Exception)
            {
                Class.Tools.ShowMsgAtFrame("网络有异常");
            }
        }

        private void NewsListGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            (Window.Current.Content as Frame).Navigate(typeof(SocialPages.NewsListPage));
        }

        private void NewsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedItem != null)
            {
                var data = list.SelectedItem as Class.Model.Social.SocialNews.New.messageBO;
                (Window.Current.Content as Frame).Navigate(typeof(SocialPages.NewDetalPage), Class.Data.Json.ToJsonData(data));
                list.SelectedIndex = -1;
            }
        }

        private void StuImg_flea_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var obj = sender as Ellipse;
            var data = obj.DataContext as Class.Model.Social.Flea.messageBO;
            if (data != null) (Window.Current.Content as Frame).Navigate(typeof(UserPages.UserIndexPage), data.studentBO.studentId);
        }

        private void StuImg_cave_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var obj = sender as Ellipse;
            var data = obj.DataContext as Class.Model.Social.Cave.messageBO;
            if (data != null) (Window.Current.Content as Frame).Navigate(typeof(UserPages.UserIndexPage), data.studentBO.studentId);
        }
    }
}
