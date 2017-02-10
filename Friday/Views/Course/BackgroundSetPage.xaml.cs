using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Friday.Views.Course
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BackgroundSetPage : Page
    {
        StorageFolder pics_flder;
        StorageFolder appdata_floder = ApplicationData.Current.LocalFolder;
        ApplicationDataContainer localSetting = ApplicationData.Current.LocalSettings;
        public BackgroundSetPage()
        {
            this.InitializeComponent();
            this.Transitions = new Windows.UI.Xaml.Media.Animation.TransitionCollection();
            this.Transitions.Add(new Windows.UI.Xaml.Media.Animation.PaneThemeTransition { Edge = EdgeTransitionLocation.Right });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                LoadOnlinePics();
            }
        }

        private async void LoadOnlinePics()
        {
            picsList.Items.Clear();
            if (Class.HttpPostUntil.isInternetAvailable)
            {
                var postdata = Class.HttpPostUntil.GetBasicPostData();
                postdata.Add(new KeyValuePair<string, string>("lastModifiedTime", Class.HttpPostUntil.NowTime));
                postdata.Add(new KeyValuePair<string, string>("height", Math.Floor(Window.Current.Bounds.Height).ToString()));
                postdata.Add(new KeyValuePair<string, string>("width", Math.Floor(Window.Current.Bounds.Width).ToString()));
                var json = await Class.HttpPostUntil.HttpPost(Class.Data.Urls.Course.getSyllabusTheme, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                try
                {
                    json = JsonObject.Parse(json).GetNamedObject("data").GetNamedArray("themes").ToString();
                    var pics = Class.Data.Json.DataContractJsonDeSerialize<List<OnlineBg>>(json);
                    pics_flder =await appdata_floder.CreateFolderAsync("course_background", CreationCollisionOption.OpenIfExists);
                    foreach (var item in pics)
                    {
                        var picfile = await pics_flder.TryGetItemAsync(item.themeIdInt + ".png");
                        if (picfile == null)
                        {
                            item.isdown = Visibility.Visible;
                            item.isusing = Visibility.Collapsed;
                        }
                        else
                        {
                            item.isdown = Visibility.Collapsed;
                            item.localpath = picfile.Path;
                            if ((string)localSetting.Values["coursebg"] == item.localpath)
                            {
                                item.isusing = Visibility.Visible;
                            }
                            else
                            {
                                item.isusing = Visibility.Collapsed;
                            }
                        }
                        if (item.themeIdInt == "21")
                        {
                            item.localpath = "ms-appx:///Assets/images/CoursePics/1.png";
                            item.isdown = Visibility.Collapsed;
                            if ((string)localSetting.Values["coursebg"] == item.localpath)
                            {
                                item.isusing = Visibility.Visible;
                            }
                            else
                            {
                                item.isusing = Visibility.Collapsed;
                            }
                        }
                        picsList.Items.Add(item);
                    }
                }
                catch (Exception)
                {
                    Class.Tools.ShowMsgAtFrame("网路异常");
                }
            }
         }

        private void GoBackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        public class OnlineBg
        {
            public string themeIdInt { get; set; }
            public string bgImgUrlStr { get; set; }
            public string themePreUrlStr { get; set; }
            public Visibility isdown { get; set; }
            public string localpath { get; set; }
            public Visibility isusing { get; set; }
        }

        private async void picsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as GridView;
            if (list.SelectedItem != null)
            {
                var pic = list.SelectedItem as OnlineBg;
                if (pic.isdown == Visibility.Visible)
                {
                    try
                    {
                        loadPrrogress.IsActive = true;
                        var hc = new HttpClient();
                        var data = await hc.GetByteArrayAsync(pic.bgImgUrlStr);
                        var file=await pics_flder.CreateFileAsync(pic.themeIdInt + ".png", CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteBytesAsync(file, data);
                        pic.localpath = file.Path;
                        localSetting.Values["coursebg"] = pic.localpath;
                        Frame.GoBack();
                        loadPrrogress.IsActive = false;
                    }
                    catch (Exception)
                    {
                        Class.Tools.ShowMsgAtFrame("网路异常");
                    }
                }
                else
                {
                    localSetting.Values["coursebg"] = pic.localpath;
                    Frame.GoBack();
                }
                list.SelectedIndex = -1;
            }
        }

        private async void LocalPicBtn_Clicked(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".bmp");
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                localSetting.Values["coursebg"] = file.Path;
                Frame.GoBack();
            }
        }
    }
}
