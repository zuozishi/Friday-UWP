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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Friday.Views.Playground
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MoreCommentPage : Page
    {
        public MoreCommentPage()
        {
            this.InitializeComponent();
            this.Transitions = new Windows.UI.Xaml.Media.Animation.TransitionCollection();
            this.Transitions.Add(new Windows.UI.Xaml.Media.Animation.PaneThemeTransition { Edge = EdgeTransitionLocation.Right });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                LoadCommentData((string[])e.Parameter);
                this.NavigationCacheMode = NavigationCacheMode.Enabled;
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
        }

        private async void LoadCommentData(string[] data)
        {
            LoadProgress.IsActive = true;
            if (HttpPostUntil.isInternetAvailable)
            {
                var postdata = HttpPostUntil.GetBasicPostData();
                postdata.Add(new KeyValuePair<string, string>("timestamp", "0"));
                postdata.Add(new KeyValuePair<string, string>("plateId", data[0]));
                postdata.Add(new KeyValuePair<string, string>("commentId", data[1]));
                var json = await HttpPostUntil.HttpPost(Data.Urls.Playground.getmorecomment, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                try
                {
                    json = Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject()["commentBOs"].GetArray().ToString();
                    var comments = Data.Json.DataContractJsonDeSerialize<List<commentData>>(json);
                    for (int i = 0; i < comments.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            comments[i].isleft = false;
                        }else
                        {
                            comments[i].isleft = true;
                        }
                    }
                    CommentList.ItemsSource = comments;
                }
                catch (Exception)
                {
                    
                }
            }
            else
            {
                Tools.ShowMsgAtFrame("网络出现异常");
            }
            LoadProgress.IsActive = false;
        }

        private void GoBackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        public class commentData:Model.Playground.OBComment
        {
            public bool isleft { get; set; }
            public string position
            {
                get
                {
                    if (isleft)
                    {
                        return "Left";
                    }
                    else
                    {
                        return "Right";
                    }
                }
            }
            public string imageColumn
            {
                get
                {
                    if (isleft)
                    {
                        return "0";
                    }
                    else
                    {
                        return "1";
                    }
                }
            }
            public string dataColumn
            {
                get
                {
                    if (isleft)
                    {
                        return "1";
                    }
                    else
                    {
                        return "0";
                    }
                }
            }
            public string color
            {
                get
                {
                    if (!isleft)
                    {
                        return "#FFE8CB8C";
                    }else
                    {
                        return "#FF8CDFE8";
                    }
                }
            }
        }
    }
}
