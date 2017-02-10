using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Friday.Views.Playground
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OBDetalisPage : Page
    {
        private Model.Playground.Topic.BOs obsdata;

        public OBDetalisPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            this.Transitions = new Windows.UI.Xaml.Media.Animation.TransitionCollection();
            this.Transitions.Add(new Windows.UI.Xaml.Media.Animation.PaneThemeTransition { Edge = EdgeTransitionLocation.Right });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                obsdata = Class.Data.Json.DataContractJsonDeSerialize<Class.Model.Playground.Topic.BOs>((string)e.Parameter);
                obsInfoView.SelectionMode = ListViewSelectionMode.None;
                obsInfoView.Items.Add(obsdata);
                LoadCommentData();
                LoadFaceData();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
        }

        private void LoadFaceData()
        {
            for (int i = 0; i < Model.FaceIcon.face_str_list.Count(); i++)
            {
                FaceGridView.Items.Add(string.Format(Model.FaceIcon.basic_face_source,(i+1).ToString("D2")));
            }
            FaceGridView.SelectionMode = ListViewSelectionMode.Single;
            FaceGridView.SelectionChanged += (s, e) =>
            {
                var list = s as GridView;
                if (list.SelectedItem != null)
                {
                    SendCommentTextBox.Text = SendCommentTextBox.Text + Model.FaceIcon.GetFaceText(list.SelectedItem as string);
                    list.SelectedIndex = -1;
                }
            };
        }

        private async void LoadCommentData()
        {
            LoadProgress.IsActive = true;
            if (HttpPostUntil.isInternetAvailable)
            {
                var postdata = HttpPostUntil.GetBasicPostData();
                postdata.Add(new KeyValuePair<string, string>("plateId", "0"));
                postdata.Add(new KeyValuePair<string, string>("messageId", obsdata.messageId));
                var json = await HttpPostUntil.HttpPost(Data.Urls.Playground.getMessageDetail, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                try
                {
                    json = Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject()["commentListBO"].GetObject()["commentBOs"].GetArray().ToString();
                    var comments = Data.Json.DataContractJsonDeSerialize<List<Model.Playground.OBComment>>(json);
                    CommentNum.Text = comments.Count.ToString();
                    if (CommentNum.Text == "0")
                    {
                        NoCommentGrid.Visibility = Visibility.Visible;
                    }else
                    {
                        CommentList.ItemsSource = comments;
                    }
                }
                catch (Exception)
                {
                    NoCommentGrid.Visibility = Visibility.Visible;
                }
            }else
            {
                Tools.ShowMsgAtFrame("网络出现异常");
            }
            LoadProgress.IsActive = false;
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

        private async void SendCommentBtnClicked(object sender, RoutedEventArgs e)
        {
            if (SendCommentTextBox.Text != "")
            {
                LoadProgress.IsActive = true;
                if (HttpPostUntil.isInternetAvailable)
                {
                    var postdata = HttpPostUntil.GetBasicPostData();
                    postdata.Add(new KeyValuePair<string, string>("anonymous", isanonymous.IsChecked.ToString()));
                    postdata.Add(new KeyValuePair<string, string>("plateId", "0"));
                    postdata.Add(new KeyValuePair<string, string>("source", "Friday_android"));
                    postdata.Add(new KeyValuePair<string, string>("messageId", obsdata.messageId));
                    postdata.Add(new KeyValuePair<string, string>("content", SendCommentTextBox.Text));
                    var json = await HttpPostUntil.HttpPost(Data.Urls.Playground.sendcomment, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                    if (json.Contains("commentId"))
                    {
                        Tools.ShowMsgAtFrame("评论成功");
                        SendCommentTextBox.Text = "";
                        LoadCommentData();
                    }
                    else
                    {
                        Tools.ShowMsgAtFrame("评论失败");
                    }
                }
                else
                {
                    Tools.ShowMsgAtFrame("网络出现异常");
                }
                LoadProgress.IsActive = false;
            }
            else
            {
                Tools.ShowMsgAtFrame("请输入评论内容");
            }
        }

        private async void LikeBtnClicked(object sender, RoutedEventArgs e)
        {
            if (HttpPostUntil.isInternetAvailable)
            {
                var postdata = HttpPostUntil.GetBasicPostData();
                postdata.Add(new KeyValuePair<string, string>("plateId", "0"));
                postdata.Add(new KeyValuePair<string, string>("num", "1"));
                postdata.Add(new KeyValuePair<string, string>("messageId", obsdata.messageId));
                var json = await HttpPostUntil.HttpPost(Data.Urls.Playground.sendlike, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                if (json.Contains("true"))
                {
                    obsdata.likeCount = obsdata.likeCount + 1;
                    obsdata.RaisePropertyChanged("likeCount");
                }
                else
                {
                    Tools.ShowMsgAtFrame("点赞失败");
                }
            }
            else
            {
                Tools.ShowMsgAtFrame("网络出现异常");
            }
            
        }

        private void CommentDialogBtnClicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            Frame.Navigate(typeof(MoreCommentPage), new string[] { "2", (btn.DataContext as Model.Playground.OBComment).commentId });
        }

        private void Ellipse_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var obj = sender as Ellipse;
            var data = obj.DataContext as Class.Model.Playground.Topic.BOs;
            if (data != null&&data.studentBO.studentId>0) Frame.Navigate(typeof(UserPages.UserIndexPage), data.studentBO.studentId);
        }
    }
}
