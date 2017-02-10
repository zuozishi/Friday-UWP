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
    public sealed partial class RegisterPage_Xueyuan : Page
    {
        public ObservableCollection<XueYuan> allxueyuan = new ObservableCollection<XueYuan>();
        public ObservableCollection<XueYuan> searchxueyuan = new ObservableCollection<XueYuan>();
        private string schoolId;

        public RegisterPage_Xueyuan()
        {
            this.InitializeComponent();
            this.Transitions = new TransitionCollection();
            this.Transitions.Add(new PaneThemeTransition { Edge = EdgeTransitionLocation.Right });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                schoolId = (string)e.Parameter;
                if (allxueyuan.Count == 0) GetAllXueYuan();
            }
        }

        private async void GetAllXueYuan()
        {
            progressBar.Visibility = Visibility.Visible;
            if (HttpPostUntil.isInternetAvailable)
            {
                try
                {
                    var postdata = HttpPostUntil.GetBasicPostData();
                    postdata.Add(new KeyValuePair<string, string>("schoolId", schoolId));
                    var json = await HttpPostUntil.HttpPost(Data.Urls.user.Register.findAcademysBySchoolId, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata), false);
                    json = JsonObject.Parse(json).GetNamedObject("data").GetNamedArray("academyList").ToString();
                    var xueyuan = Data.Json.DataContractJsonDeSerialize<List<XueYuan>>(json);
                    foreach (var item in xueyuan)
                    {
                        allxueyuan.Add(item);
                    }
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
            progressBar.Visibility = Visibility.Collapsed;
        }

        public class XueYuan
        {
            public string id { get; set; }
            public string name { get; set; }
            public string academyId { get; set; }
            public string schoolId { get; set; }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = (sender as TextBox).Text;
            if (text.Length == 0)
            {
                allGrid.Visibility = Visibility.Visible;
                searchGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                searchGrid.Visibility = Visibility.Visible;
                allGrid.Visibility = Visibility.Collapsed;
                progressBar.Visibility = Visibility.Visible;
                searchxueyuan.Clear();
                foreach (var item in allxueyuan)
                {
                    if (text == GetStrByLen(item.name, text.Length))
                        searchxueyuan.Add(item);
                }
                progressBar.Visibility = Visibility.Collapsed;
            }
        }

        private string GetStrByLen(string str, int len)
        {
            string res = "";
            if (str == null || str.Length < len) return res;
            for (int i = 0; i < len; i++)
            {
                res = res + str[i];
            }
            return res;
        }

        private void XueyuanSelected(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedItem != null)
            {
                var data = list.SelectedItem as XueYuan;
                data.schoolId = schoolId;
                data.academyId = data.id;
                Frame.Navigate(typeof(RegisterPage_BeginYear), Data.Json.ToJsonData(list.SelectedItem));
                list.SelectedIndex = -1;
            }
        }

        private void GoBackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
