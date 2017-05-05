using QRCoder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Friday.Views.MainPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CoursePage : Page
    {
        public Class.Model.User.Login_Result userdata = Class.UserManager.UserData;
        public string CourseGridZoom
        {
            get
            {
                if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("CourseGridZoom"))
                {
                    if (((string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["CourseGridZoom"]) == "True")
                    {
                        CourseScrollViewer.ZoomMode = ZoomMode.Enabled;
                    }
                    else
                    {
                        CourseScrollViewer.ZoomMode = ZoomMode.Disabled;
                    }
                    return (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["CourseGridZoom"];
                }
                else
                {
                    CourseScrollViewer.ZoomMode = ZoomMode.Enabled;
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values["CourseGridZoom"] = "True";
                    return "True";
                }
            }
            set
            {
                if (value=="True")
                {
                    CourseScrollViewer.ZoomMode = ZoomMode.Enabled;
                }
                else
                {
                    CourseScrollViewer.ZoomMode = ZoomMode.Disabled;
                }
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["CourseGridZoom"] = value;
            }
        }
        public string DynamicTile
        {
            get
            {
                if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("DynamicTile"))
                {
                    return (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["DynamicTile"];
                }
                else
                {
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values["DynamicTile"] = "True";
                    return "True";
                }
            }
            set
            {
                if (value == "True")
                {
                    RegTileTask();
                }
                else
                {
                    foreach (var cur in BackgroundTaskRegistration.AllTasks)
                    {
                        if (cur.Value.Name == "TileUpdata")
                        {
                            cur.Value.Unregister(true);
                            break;
                        }
                    }
                }
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["CourseGridZoom"] = value;
            }
        }
        ApplicationDataContainer localSetting = ApplicationData.Current.LocalSettings;
        public Windows.UI.Xaml.Media.Imaging.BitmapImage course_bg = new Windows.UI.Xaml.Media.Imaging.BitmapImage() { UriSource= new Uri("ms-appx:///Assets/images/CoursePics/1.png") };
        private byte[] qrdata;

        public CoursePage()
        {
            this.InitializeComponent();
            this.Transitions = new TransitionCollection();
            this.Transitions.Add(new PaneThemeTransition { Edge = EdgeTransitionLocation.Right });
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var shareurl = "http://www.super.cn/d/index.php?t=1&i={0}&p=1&v=7.8.4&y={1}&tm={2}";
            shareurl = string.Format(shareurl, Class.UserManager.UserData.id, Class.UserManager.UserData.beginYear, Class.UserManager.UserData.term);
            args.Request.Data.SetText(shareurl+"\n打开我分享的链接，即可复制我的课表。(来自超级课程表UWP)");
            args.Request.Data.Properties.Title = "分享你的课表";
            args.Request.Data.Properties.Description = "让你的同学打开你分享的链接，即可复制你的课表。";
            args.Request.GetDeferral().Complete();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.ProtocolUri != string.Empty)
            {
                var paramsdata=App.ProtocolUri.Replace("?","").Split('&');
                foreach (var item in paramsdata)
                {
                    if (item.Contains("i="))
                    {
                        string id = item.Replace("i=", "");
                        var dialog2 = new Controls.DialogBox()
                        {
                            Title = "提示",
                            PrimaryButtonText = "取消",
                            SecondaryButtonText = "确定"
                        };
                        if (id == userdata.id.ToString())
                        {
                            dialog2.mainTextBlock.Text = "无法导入自己的课表!";
                            await dialog2.ShowAsync();
                            break;
                        }
                        dialog2.mainTextBlock.Text = "是否导入课表";
                        if (await dialog2.ShowAsync() == ContentDialogResult.Secondary)
                        {
                            await Class.Model.CourseManager.CopyCourseBySuperId(id, Class.UserManager.UserData.beginYear.ToString(), Class.UserManager.UserData.term.ToString());
                            BuildCourseGrid();
                        }
                        break;
                    }
                }
                App.ProtocolUri = string.Empty;
            }
            mainGridImage.ImageSource = course_bg;
            LoadCoursebg();
            if (Class.UserManager.UserData.attachmentBO.myTermList.Count != 0)
            {
                title1.Visibility = Visibility.Visible;
                title2.Visibility = Visibility.Collapsed;
                LoadDateData();
            }
            else
            {
                title1.Visibility = Visibility.Collapsed;
                title2.Visibility = Visibility.Visible;
                nowtermtext.Foreground = new SolidColorBrush(Colors.OrangeRed);
                nowtermtext.Text = "未设置";
                weektext.Foreground = new SolidColorBrush(Colors.OrangeRed);
                weektext.Text = "未设置";
            }
        }

        private async void LoadCoursebg()
        {
            var picpath = (string)localSetting.Values["coursebg"];
            if (picpath == null) picpath = "ms-appx:///Assets/images/CoursePics/1.png";
            if (picpath.Contains("ms-appx"))
            {
                var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(picpath));
                course_bg.SetSource(await file.OpenReadAsync());
            }
            else
            {
                var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(picpath);
                course_bg.SetSource(await file.OpenReadAsync());
            }
        }

        private static async void RegTileTask()
        {
            try
            {
                var status = await BackgroundExecutionManager.RequestAccessAsync();
                foreach (var cur in BackgroundTaskRegistration.AllTasks)
                {
                    if (cur.Value.Name == "TileUpdata")
                    {
                        return;
                    }
                }
                var builder = new BackgroundTaskBuilder();
                builder.Name = "TileUpdata";
                builder.TaskEntryPoint = "FridayBgTask.TileUpdate";
                builder.SetTrigger(new TimeTrigger(15, false));
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                var task = builder.Register();
            }
            catch (Exception)
            {

            }
        }

        private void LoadDateData()
        {
            TermText.Text = Class.Data.Int_String.NumberToChinese(userdata.term.ToString());
            var year = (userdata.beginYear%2000 - userdata.grade) +1;
            YearText.Text= Class.Data.Int_String.NumberToChinese(year.ToString());
            var nowweek = userdata.attachmentBO.nowWeekMsg.nowWeek;
            WeekList.Items.Clear();
            for (int i = 1; i <= 25; i++)
            {
                if (nowweek == i)
                {
                    WeekList.Items.Add("第" + i + "周(本周)");
                }
                else
                {
                    WeekList.Items.Add("第" + i + "周");
                }
            }
            WeekList.SelectedIndex = nowweek - 1;
            BuildCourseGrid();
        }

        private async void BuildCourseGrid()
        {
            try
            {
                CourseGrid.Children.Clear();
                CourseGrid.RowDefinitions.Clear();
                CourseGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(35) });
                var coursemaxcount = userdata.maxCount;
                for (int i = 0; i < coursemaxcount; i++)
                {
                    CourseGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(75) });
                }
                for (int i = 0; i < CourseGrid.RowDefinitions.Count; i++)
                {
                    string gridxaml = "<Grid xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" BorderThickness=\"0.25\" BorderBrush=\"#FFBFBFBF\" Grid.Row=\"{0}\" Grid.Column=\"0\"><TextBlock FontSize=\"13\" TextWrapping=\"WrapWholeWords\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Foreground=\"{ThemeResource Friday-Foreground}\">{1}</TextBlock></Grid>";
                    if (i == 0)
                    {
                        CourseGrid.Children.Add((Grid)XamlReader.Load(gridxaml.Replace("{0}", i.ToString()).Replace("{1}", DateTime.Today.Month + "月")));
                    }
                    else
                    {
                        CourseGrid.Children.Add((Grid)XamlReader.Load(gridxaml.Replace("{0}", i.ToString()).Replace("{1}", i.ToString())));
                    }
                }
                for (int i = 1; i < 8; i++)
                {
                    int week = (int)DateTime.Today.DayOfWeek;
                    if (week == 0) week = 7;
                    string gridxaml = "<Grid  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" Grid.Row=\"0\" Grid.Column=\"{0}\" BorderBrush=\"{ThemeResource Friday-BorderBrush}\" BorderThickness=\"0.25\"><Grid.RowDefinitions><RowDefinition/><RowDefinition/></Grid.RowDefinitions><TextBlock Grid.Row=\"0\" FontSize=\"13\" Foreground=\"{ThemeResource Friday-Foreground}\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\">{1}</TextBlock><TextBlock Grid.Row=\"1\" FontSize=\"13\" Foreground=\"{ThemeResource Friday-Foreground}\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\">周{2}</TextBlock></Grid>";
                    DateTime newday;
                    if (week - i > 0)
                    {
                        newday = DateTime.Today - TimeSpan.FromDays(week - i);
                    }
                    else
                    {
                        newday = DateTime.Today + TimeSpan.FromDays(i - week);
                    }
                    var grid = (Grid)XamlReader.Load(gridxaml.Replace("{0}", i.ToString()).Replace("{1}", newday.Day.ToString()).Replace("{2}", Class.Data.Int_String.NumberToChinese(i.ToString())));
                    if (week == i) grid.Background = new SolidColorBrush(Color.FromArgb(100, 7, 153, 252));
                    CourseGrid.Children.Add(grid);
                }
                for (int i = 1; i < CourseGrid.ColumnDefinitions.Count; i++)
                {
                    for (int j = 1; j < CourseGrid.RowDefinitions.Count; j++)
                    {
                        /*
                        var text = new TextBlock();
                        text.FontSize = 10;
                        text.Text = "+";
                        Grid.SetColumn(text, i);
                        Grid.SetRow(text, j);
                        text.HorizontalAlignment = HorizontalAlignment.Right;
                        text.VerticalAlignment = VerticalAlignment.Bottom;
                        CourseGrid.Children.Add(text);
                        */
                        var btn = new Button()
                        {
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            Background = new SolidColorBrush(Colors.Transparent)
                        };
                        btn.Click += CourseBtn_Clicked;
                        Grid.SetColumn(btn, i);
                        Grid.SetRow(btn, j);

                        var currentWeek = userdata.attachmentBO.nowWeekMsg.nowWeek.ToString();
                        var course = await Class.Model.CourseManager.GetCourse(i, j, null, null, currentWeek);
                        if (course != null)
                        {
                            btn.Content = course.BtnContent;
                            btn.DataContext = course;
                            
                            btn.Style = (Style)Resources["FullButtonStyle"];
                            btn.Background = new SolidColorBrush(Colors.Gray);
                            btn.Background.Opacity = 0.4;
                            var weeks = course.period.Split(' ', ',');
                            foreach (var item in weeks)
                            {
                                if(item== userdata.attachmentBO.nowWeekMsg.nowWeek.ToString())
                                {
                                    btn.Background = new SolidColorBrush(course.CourseButton.BackgroundColor);
                                    btn.Background.Opacity = 1;
                                }
                            }
                        }
                        CourseGrid.Children.Add(btn);
                        LLM.Animator.Use(LLM.AnimationType.FadeIn).PlayOn(btn);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void CourseBtn_Clicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.Content != null)
            {
                (Window.Current.Content as Frame).Navigate(typeof(Course.LookCoursePage), Class.Data.Json.ToJsonData(btn.DataContext as Class.Model.CourseManager.CourseModel));
            }
            else
            {
                (Window.Current.Content as Frame).Navigate(typeof(Course.CourseListPage),new string[] { Grid.GetRow(btn).ToString(), Grid.GetColumn(btn).ToString() });
            }
        }

        private void WeekList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedIndex != -1)
            {
                var nowweek = userdata.attachmentBO.nowWeekMsg.nowWeek;
                if (list.SelectedIndex==(nowweek-1))
                {
                    WeekText.Text = "第" + (list.SelectedIndex + 1) + "周";
                    WeekText.Foreground = new SolidColorBrush(Color.FromArgb(255, 7, 153, 252));
                }
                else
                {
                    WeekText.Text = "第"+ (list.SelectedIndex + 1) + "周(非本周)";
                    WeekText.Foreground = new SolidColorBrush(Color.FromArgb(255, 252, 107, 7));
                }
            }
        }

        private async void SettingBtnClicked(object sender, RoutedEventArgs e)
        {
            if (SettingGrid.Visibility==Visibility.Visible)
            {
                CloseSettingSb.Begin();
                LLM.Animator.Use(LLM.AnimationType.FadeOutUp).SetDuration(TimeSpan.FromMilliseconds(300)).PlayOn(SettingGrid);
                await Task.Delay(300);
                SettingGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                OpenSettingSb.Begin();
                SettingGrid.Visibility = Visibility.Visible;
                LLM.Animator.Use(LLM.AnimationType.FadeInDown).SetDuration(TimeSpan.FromMilliseconds(300)).PlayOn(SettingGrid);
            }
        }

        private async void SetWeekBtn_Clicked(object sender, RoutedEventArgs e)
        {
            if (WeekList.SelectedIndex != -1)
            {
                var week = WeekList.Items[WeekList.SelectedIndex] as string;
                week = week.Replace("第", "").Replace("周", "");
                if (await Class.Model.CourseManager.Setting.SetWeek(int.Parse(week))) Frame.Navigate(typeof(CoursePage));
            }
        }

        private async void TopBtnClicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            switch (btn.TabIndex)
            {
                case 0:
                    (Window.Current.Content as Frame).Navigate(typeof(Course.CourseListPage));
                    break;
                case 1:
                    var dialog = new Controls.DialogBox()
                    {
                        Title = "提示",
                        PrimaryButtonText = "取消",
                        SecondaryButtonText = "确定"
                    };
                    dialog.mainTextBlock.Text = "将从服务器获得当前学期课表数据,是否继续?";
                    if(await dialog.ShowAsync() == ContentDialogResult.Secondary)
                    {
                        loadProgress.IsActive = true;
                        await Class.Model.CourseManager.Async.GetCourseTableFromServer();
                        loadProgress.IsActive = false;
                        BuildCourseGrid();
                        dialog = new Controls.DialogBox()
                        {
                            Title = "提示",
                            PrimaryButtonText = "取消",
                            SecondaryButtonText = "确定"
                        };
                        dialog.mainTextBlock.Text = "更新完成";
                        await dialog.ShowAsync();
                    }
                    break;
                case 2:
                    var scanner = new ZXing.Mobile.MobileBarcodeScanner();
                    var result = await scanner.Scan();
                    if (result != null)
                        //http://www.super.cn/d?t=1&i=19882919&p=1&v=7.8.0&y=2016&tm=1
                        if(result.Text.Contains("http://"))
                        {
                            if(result.Text.Contains("www.super.cn/d"))
                            {
                                var id = result.Text.Split('=')[2].Split('&')[0];
                                var dialog2 = new Controls.DialogBox()
                                {
                                    Title = "提示",
                                    PrimaryButtonText = "取消",
                                    SecondaryButtonText = "确定"
                                };
                                dialog2.mainTextBlock.Text = "是否导入课表";
                                if (await dialog2.ShowAsync() == ContentDialogResult.Secondary)
                                {
                                    await Class.Model.CourseManager.CopyCourseBySuperId(id,Class.UserManager.UserData.beginYear.ToString(), Class.UserManager.UserData.term.ToString());
                                    OpenSettingSb.Begin();
                                    SettingGrid.Visibility = Visibility.Visible;
                                    LLM.Animator.Use(LLM.AnimationType.FadeInDown).SetDuration(TimeSpan.FromMilliseconds(300)).PlayOn(SettingGrid);
                                    CourseGrid.Children.Clear();
                                    BuildCourseGrid();
                                }
                            }
                            else
                            {
                                var dialog1 = new Controls.DialogBox()
                                {
                                    Title = "是否打开网站",
                                    PrimaryButtonText = "取消",
                                    SecondaryButtonText = "确定"
                                };
                                dialog1.mainTextBlock.Text = result.Text;
                                if(await dialog1.ShowAsync() == ContentDialogResult.Secondary)
                                {
                                    await Windows.System.Launcher.LaunchUriAsync(new Uri(result.Text));
                                }
                            }
                        }
                        else
                        {
                            var dialog1 = new Controls.DialogBox()
                            {
                                Title = result.Text,
                                PrimaryButtonText = "取消",
                                SecondaryButtonText = "确定"
                            };
                            dialog1.mainTextBlock.Text = result.Text;
                            if (await dialog1.ShowAsync() == ContentDialogResult.Secondary)
                            {
                                await Windows.System.Launcher.LaunchUriAsync(new Uri(result.Text));
                            }
                        }
                        break;
                default:
                    break;
            }
        }

        private async void SetListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedIndex != -1)
            {
                switch (list.SelectedIndex)
                {
                    case 0:
                        (Window.Current.Content as Frame).Navigate(typeof(Views.Setting.TermSettingPage));
                        break;
                    case 1:
                        var item = list.Items[list.SelectedIndex] as ListViewItem;
                        WeekFlyout.ShowAt(item);
                        break;
                    case 2:
                        var dialog = new Controls.DialogBox() {
                            Title="设置课表最大节数",
                            PrimaryButtonText="取消",
                            SecondaryButtonText="确定",
                            Height=500
                        };
                        /*
                        var listbox = new ListView() { Height=300};
                        for (int i = 5; i < 25; i++)
                        {
                            listbox.Items.Add(i);
                        }
                        listbox.SelectedIndex = 0;
                        dialog.mainDialogGrid.Children.Add(listbox);
                        */
                        var slider = new Slider()
                        {
                            Name= "slider",
                            Minimum = 5,
                            Maximum = 24,
                            Margin = new Thickness(0, 10, 50, 0),
                            TickFrequency=1
                        };
                        dialog.mainTextBlock.HorizontalAlignment = HorizontalAlignment.Right;
                        dialog.mainTextBlock.VerticalAlignment = VerticalAlignment.Center;
                        slider.ValueChanged += (s,arg) =>
                        {
                            dialog.mainTextBlock.Text = arg.NewValue.ToString();
                        };
                        slider.Value = Class.UserManager.UserData.maxCount;
                        dialog.mainDialogGrid.Children.Add(slider);
                        var result =await dialog.ShowAsync();
                        if (result == ContentDialogResult.Secondary)
                        {
                            if(await Class.Model.CourseManager.Setting.SetCourseMaxCount(int.Parse(slider.Value.ToString()))) Frame.Navigate(typeof(CoursePage));
                        }
                        break;
                    case 3:
                        (Window.Current.Content as Frame).Navigate(typeof(Views.Course.BackgroundSetPage));
                        break;
                    default:
                        break;
                }
                list.SelectedIndex = -1;
            }
        }

        private async void ShareBtn_Clicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as MenuFlyoutItem;
            var shareurl = "http://www.super.cn/d/index.php?t=1&i={0}&p=1&v=7.8.4&y={1}&tm={2}";
            shareurl = string.Format(shareurl, Class.UserManager.UserData.id, Class.UserManager.UserData.beginYear, Class.UserManager.UserData.term);
            switch (btn.TabIndex)
            {
                case 2:
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(new PayloadGenerator.Url(shareurl).ToString(), QRCodeGenerator.ECCLevel.H);
                    BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
                    qrdata = qrCode.GetGraphic(50);
                    using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                    {
                        using (DataWriter writer = new DataWriter(stream.GetOutputStreamAt(0)))
                        {
                            writer.WriteBytes(qrdata);
                            await writer.StoreAsync();
                        }
                        var image = new BitmapImage();
                        await image.SetSourceAsync(stream);
                        qrimage.Source = image;
                    }
                    var res=await qrcode_dialig.ShowAsync();
                    if (res == ContentDialogResult.Primary)
                    {
                        DataTransferManager.ShowShareUI();
                    }
                    else if (res == ContentDialogResult.Secondary)
                    {
                        var qrfile =await Windows.Storage.KnownFolders.PicturesLibrary.CreateFileAsync("friday-" + Class.HttpPostUntil.NowTime + ".png", CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteBytesAsync(qrfile, qrdata);
                        Class.Tools.ShowMsgAtFrame("二维码已保存到图片库");
                    }
                    break;
                case 3:
                    DataTransferManager.ShowShareUI();
                    break;
                default:
                    break;
            }
        }

        private void CloseQrDialogBtn_Clicked(object sender, RoutedEventArgs e)
        {
            qrcode_dialig.Hide();
        }
    }
}