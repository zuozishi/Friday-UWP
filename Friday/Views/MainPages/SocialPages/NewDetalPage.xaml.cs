using Friday.Class;
using System;
using System.Collections.Generic;
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
    public sealed partial class NewDetalPage : Page
    {
        private Model.Social.SocialNews.New.messageBO newsdata;

        public NewDetalPage()
        {
            this.InitializeComponent();
            this.Transitions = new Windows.UI.Xaml.Media.Animation.TransitionCollection();
            this.Transitions.Add(new Windows.UI.Xaml.Media.Animation.PaneThemeTransition { Edge = EdgeTransitionLocation.Right });
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                newsdata = Data.Json.DataContractJsonDeSerialize<Model.Social.SocialNews.New.messageBO>((string)e.Parameter);
                LoadFaceData();
                LoadCommentData();
            }
        }

        private void LoadFaceData()
        {
            for (int i = 0; i < Model.FaceIcon.face_str_list.Count(); i++)
            {
                FaceGridView.Items.Add(string.Format(Model.FaceIcon.basic_face_source, (i + 1).ToString("D2")));
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


        private async void SendCommentBtnClicked(object sender, RoutedEventArgs e)
        {
            if (SendCommentTextBox.Text != "")
            {
                LoadProgress.IsActive = true;
                if (HttpPostUntil.isInternetAvailable)
                {
                    var postdata = HttpPostUntil.GetBasicPostData();
                    postdata.Add(new KeyValuePair<string, string>("anonymous","0"));
                    postdata.Add(new KeyValuePair<string, string>("plateId", newsdata.plateId));
                    postdata.Add(new KeyValuePair<string, string>("source", "Friday_android"));
                    postdata.Add(new KeyValuePair<string, string>("messageId", newsdata.messageId));
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

        private async void LoadCommentData()
        {
            LoadProgress.IsActive = true;
            if (HttpPostUntil.isInternetAvailable)
            {
                var postdata = HttpPostUntil.GetBasicPostData();
                postdata.Add(new KeyValuePair<string, string>("plateId", newsdata.plateId));
                postdata.Add(new KeyValuePair<string, string>("messageId", newsdata.messageId));
                var json = await HttpPostUntil.HttpPost(Data.Urls.Playground.getMessageDetail, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                try
                {
                    var content = Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject()["contentStr"].GetString();
                    contentView.NavigateToString(content);
                    var comments = Data.Json.DataContractJsonDeSerialize<List<Model.Playground.OBComment>>(Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject()["commentListBO"].GetObject()["commentBOs"].GetArray().ToString());
                    CommentNum.Text = comments.Count.ToString();
                    if (CommentNum.Text == "0")
                    {
                        NoCommentGrid.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        CommentList.ItemsSource = comments;
                    }
                }
                catch (Exception)
                {
                    NoCommentGrid.Visibility = Visibility.Visible;
                }
            }
            else
            {
                Tools.ShowMsgAtFrame("网络出现异常");
            }
            LoadProgress.IsActive = false;
        }

        private void CommentDialogBtnClicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            Frame.Navigate(typeof(Playground.MoreCommentPage), new string[] { newsdata.plateId, (btn.DataContext as Model.Playground.OBComment).commentId });
        }

        private void GoBackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
