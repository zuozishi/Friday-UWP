using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Friday.Class
{
    public class Model
    {
        public static Random random = new Random();
        public class Social
        {
            public class Toolbar
            {
                public int cmd { get; set; }
                public string colour { get; set; }
                public string logoUrl { get; set; }
                public string name { get; set; }
                public string targetUrl { get; set; }
                public int moduleId { get; set; }
            }
            public class SocialNews
            {
                public class Club
                {
                    public Club_Data club_data { get; set; }
                    public ObservableCollection<messageBO> messageBOs { get; set; }
                    public class Club_Data
                    {
                        public string icon { get; set; }
                        public string name { get; set; }
                        public string show_s
                        {
                            get
                            {
                                if (show == "true")
                                {
                                    return "Visible";
                                }
                                else
                                {
                                    return "Collapsed";
                                }
                            }
                        }
                        public string show { get; set; }
                    }
                    public class messageBO
                    {
                        public string title { get; set; }
                        public string likeCount { get; set; }
                        public string activityTime { get; set; }
                        public string activityTime_s
                        {
                            get
                            {
                                DateTime dtStart = new DateTime(1970, 1, 1);
                                long lTime = long.Parse(activityTime + "0000");
                                TimeSpan time = new TimeSpan(lTime);
                                dtStart=dtStart.Add(time);
                                return dtStart.ToString();
                            }
                        }
                        public string unit { get; set; }
                        public string activitySite { get; set; }
                        public string titleFontColor { get; set; }
                        public string imgurl
                        {
                            get
                            {
                                if (qiniuImgBOs != null && qiniuImgBOs.Count > 0)
                                {
                                    return qiniuImgBOs[0].url;
                                }
                                else
                                {
                                    return "";
                                }
                            }
                        }
                        public List<ImgBO> qiniuImgBOs { get; set; }
                        public class ImgBO
                        {
                            public string url { get; set; }
                        }
                    }
                }
                public class New
                {
                    public New_Data new_data { get; set; }
                    public messageBO firstitem
                    {
                        get
                        {
                            if (messageBOs != null && messageBOs.Count > 0)
                            {
                                return messageBOs[0];
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                    public ObservableCollection<messageBO> messageBOs { get; set; }
                    public ObservableCollection<messageBO> messageBOs1 {
                        get
                        {
                            if (messageBOs != null && messageBOs.Count > 1)
                            {
                                var newbos = messageBOs;
                                newbos.RemoveAt(0);
                                return newbos;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                    public class New_Data
                    {
                        public string icon { get; set; }
                        public string name { get; set; }
                        public string readTip { get; set; }
                        public string show { get; set; }
                    }
                    public class messageBO
                    {
                        public string title { get; set; }
                        public string plateId { get; set; }
                        public string messageId { get; set; }
                        public string issueTime { get; set; }
                        public int likeCount { get; set; }
                        public int comments { get; set; }
                        public string issueTime_s
                        {
                            get
                            {
                                DateTime dtStart = new DateTime(1970, 1, 1);
                                long lTime = long.Parse(issueTime + "0000");
                                TimeSpan time = new TimeSpan(lTime);
                                dtStart = dtStart.Add(time);
                                TimeSpan tonow = new TimeSpan();
                                tonow = DateTime.Today - dtStart;
                                if (tonow.TotalHours < DateTime.Today.Hour)
                                {
                                    return "今天" + dtStart.Hour + ":" + dtStart.Minute;
                                }
                                else if(tonow.TotalHours >= DateTime.Today.Hour&&tonow.TotalHours-24<DateTime.Today.Hour)
                                {
                                    return "昨天" + dtStart.Hour + ":" + dtStart.Minute;
                                }
                                return dtStart.ToString();
                            }
                        }
                        public string imgurl
                        {
                            get
                            {
                                if (qiniuImgBOs != null && qiniuImgBOs.Count > 0)
                                {
                                    return qiniuImgBOs[0].url;
                                }
                                else
                                {
                                    return "";
                                }
                            }
                        }
                        public List<ImgBO> qiniuImgBOs { get; set; }
                        public class ImgBO
                        {
                            public string url { get; set; }
                        }
                    }
                }
            }
            public class Cave
            {
                public ObservableCollection<messageBO> messageBOs { get; set; }
                public messageBO ggmessageBO
                {
                    get
                    {
                        if (messageBOs != null && messageBOs.Count > 0 && !messageBOs[0].anonymous)
                        {
                            return messageBOs[0];
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                public string hasgg
                {
                    get
                    {
                        if (ggmessageBO == null || ggmessageBO.content == null || ggmessageBO.content == "")
                        {
                            return "Collapsed";
                        }
                        else
                        {
                            return "Visible";
                        }
                    }
                }
                public ObservableCollection<messageBO> basicBOs
                {
                    get
                    {
                        if (messageBOs != null && messageBOs.Count > 0)
                        {
                            var newbos = new ObservableCollection<messageBO>();
                            foreach (var item in messageBOs)
                            {
                                if (item.anonymous)
                                {
                                    newbos.Add(item);
                                }
                            }
                            return newbos;
                        }
                        else
                        {
                            return new ObservableCollection<messageBO>();
                        }
                    }
                }
                public class messageBO
                {
                    public string content { get; set; }
                    public string icon { get; set; }
                    public string publisher { get; set; }
                    public bool delete { get; set; }
                    public string messageId { get; set; }
                    public string plateId { get; set; }
                    public string schoolName { get; set; }
                    public string titleFontColor { get; set; }
                    public int likeCount { get; set; }
                    public int comments { get; set; }
                    public long issueTime { get; set; }
                    public bool anonymous { get; set; }
                    public RichTextBlock ContentUI
                    {
                        get
                        {
                            var graph = new Windows.UI.Xaml.Documents.Paragraph();
                            if (content.Contains("$"))
                            {
                                var strs = content.Split('$');
                                foreach (var item in strs)
                                {
                                    var image = Class.Model.FaceIcon.GetFaceImage(item);
                                    if (image != null)
                                    {
                                        var uiconter = new Windows.UI.Xaml.Documents.InlineUIContainer();
                                        uiconter.Child = image;
                                        graph.Inlines.Add(uiconter);
                                    }
                                    else
                                    {
                                        graph.Inlines.Add(new Windows.UI.Xaml.Documents.Run() { Text = item });
                                    }
                                }
                            }
                            else
                            {
                                graph.Inlines.Add(new Windows.UI.Xaml.Documents.Run() { Text = content });
                            }
                            var result = new RichTextBlock();
                            result.Margin = new Thickness(0, 5, 0, 5);
                            result.TextWrapping = TextWrapping.WrapWholeWords;
                            if (DateTime.Now.Hour > 6 && DateTime.Now.Hour < 20)
                            {
                                result.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Black);
                            }
                            else
                            {
                                result.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(162,197,201,255));
                            }
                            result.Blocks.Add(graph);
                            result.IsTextSelectionEnabled = false;
                            if((Window.Current.Content as Frame).CurrentSourcePageType != typeof(Friday.Views.Playground.OBDetalisPage))
                            {
                                result.Tapped += (s, e) =>
                                {
                                    (Window.Current.Content as Frame).Navigate(typeof(Friday.Views.Playground.OBDetalisPage), Class.Data.Json.ToJsonData(this));
                                };
                            }
                            return result;
                        }
                    }
                    public string hasTag
                    {
                        get
                        {
                            if (moodTagBO == null || moodTagBO.moodTagName == null || moodTagBO.moodTagName == "")
                            {
                                return "Collapsed";
                            }
                            else
                            {
                                return "Visible";
                            }
                        }
                    }
                    public string publishtime
                    {
                        get
                        {
                            if (issueTime > 0)
                            {
                                DateTime dtStart = new DateTime(1970, 1, 1);
                                long lTime = long.Parse(issueTime + "0000");
                                TimeSpan time = new TimeSpan(lTime);
                                dtStart = dtStart.Add(time);
                                return dtStart.ToString("yyyy-MM-dd hh:mm");
                            }
                            else
                            {
                                return "";
                            }
                        }
                    }
                    public student studentBO { get; set; }
                    public List<qiniuImgBO> qiniuImgBOs { get; set; }
                    public moodTag moodTagBO { get; set; }
                    public class student
                    {
                        public int studentId { get; set; }
                        public int studentType { get; set; }
                        public string sex
                        {
                            get
                            {
                                if (studentType == 0)
                                {
                                    return "ms-appx:///Assets/images/ic_th_sex_boy.png";
                                }else if (studentType == 2)
                                {
                                    return "ms-appx:///Assets/images/ic_th_sex_girl.png";
                                }
                                else
                                {
                                    return "";
                                }
                            }
                        }
                        public string nickName { get; set; }
                        public string fullAvatarUrl { get; set; }
                    }
                    public class qiniuImgBO
                    {
                        public string url { get; set; }
                    }
                    public class moodTag
                    {
                        public string moodTagName { get; set; }
                        public string moodTagIcon { get; set; }
                        public int plateId { get; set; }
                        public int moodTagId { get; set; }
                    }
                }
            }
            public class Flea
            {
                public ObservableCollection<messageBO> messageBOs { get; set; }
                public messageBO ggmessageBO
                {
                    get
                    {
                        messageBO result=new messageBO();
                        if (messageBOs != null && messageBOs.Count > 0)
                        {
                            foreach (var item in messageBOs)
                            {
                                if (item.studentBO == null && item.content != null)
                                {
                                    result = item;
                                    break;
                                }
                            }
                            return result;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                public ObservableCollection<messageBO> basicBOs
                {
                    get
                    {
                        var result = new ObservableCollection<messageBO>();
                        if (messageBOs != null&&messageBOs.Count>0)
                        {
                            foreach (var item in messageBOs)
                            {
                                if (item.studentBO != null)
                                {
                                    result.Add(item);
                                }
                            }
                            if (result.Count > 0)
                            {
                                return result;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                public class messageBO
                {
                    public bool anonymous { get; set; }
                    public string content { get; set; }
                    public string icon { get; set; }
                    public string messageId { get; set; }
                    public string publisher { get; set; }
                    public string plateId { get; set; }
                    public bool delete { get; set; }
                    public bool freePost { get; set; }
                    public int likeCount { get; set; }
                    public string schoolName { get; set; }
                    public string contactNum { get; set; }
                    public string costText { get; set; }
                    public int comments { get; set; }
                    public string sellPriceText { get; set; }
                    public long issueTime { get; set; }
                    public string like
                    {
                        get
                        {
                            if (likeCount > 0)
                            {
                                return likeCount.ToString();
                            }
                            else
                            {
                                return "喜欢";
                            }
                        }
                    }
                    public string tag
                    {
                        get
                        {
                            if (contactNum == null || contactNum == "")
                            {
                                return "小纸条";
                            }
                            else
                            {
                                var text = "";
                                for (int i = 0; i < contactNum.Length; i++)
                                {
                                    if (i < 3)
                                    {
                                        text = text + contactNum[i];
                                    }
                                    else
                                    {
                                        text = text + "*";
                                    }
                                }
                                return text;
                            }
                        }
                    }
                    public string comment
                    {
                        get
                        {
                            if (comments > 0)
                            {
                                return comments.ToString();
                            }
                            else
                            {
                                return "评论";
                            }
                        }
                    }
                    public string publishtime
                    {
                        get
                        {
                            if (issueTime > 0)
                            {
                                DateTime dtStart = new DateTime(1970, 1, 1);
                                long lTime = long.Parse(issueTime + "0000");
                                TimeSpan time = new TimeSpan(lTime);
                                dtStart = dtStart.Add(time);
                                return dtStart.ToString("yyyy-MM-dd hh:mm");
                            }
                            else
                            {
                                return "";
                            }
                        }
                    }
                    public Windows.UI.Xaml.Controls.RichTextBlock ContentUI
                    {
                        get
                        {
                            var graph = new Windows.UI.Xaml.Documents.Paragraph();
                            if (content.Contains("$"))
                            {
                                var strs = content.Split('$');
                                foreach (var item in strs)
                                {
                                    var image = Class.Model.FaceIcon.GetFaceImage(item);
                                    if (image != null)
                                    {
                                        var uiconter = new Windows.UI.Xaml.Documents.InlineUIContainer();
                                        uiconter.Child = image;
                                        graph.Inlines.Add(uiconter);
                                    }
                                    else
                                    {
                                        graph.Inlines.Add(new Windows.UI.Xaml.Documents.Run() { Text = item });
                                    }
                                }
                            }
                            else
                            {
                                graph.Inlines.Add(new Windows.UI.Xaml.Documents.Run() { Text = content });
                            }
                            var result = new Windows.UI.Xaml.Controls.RichTextBlock();
                            result.Margin = new Windows.UI.Xaml.Thickness(0, 5, 0, 5);
                            result.TextWrapping = Windows.UI.Xaml.TextWrapping.WrapWholeWords;
                            result.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Black);
                            result.Blocks.Add(graph);
                            result.IsTextSelectionEnabled = false;
                            if ((Window.Current.Content as Frame).CurrentSourcePageType!=typeof(Friday.Views.Playground.OBDetalisPage))
                            {
                                result.Tapped += (s, e) =>
                                {
                                    (Window.Current.Content as Frame).Navigate(typeof(Friday.Views.Playground.OBDetalisPage), Class.Data.Json.ToJsonData(this));
                                };
                            }
                            return result;
                        }
                    }
                    public student studentBO { get; set; }
                    public List<qiniuImgBO> qiniuImgBOs { get; set; }
                    public class student
                    {
                        public int studentId { get; set; }
                        public int studentType { get; set; }
                        public string sex
                        {
                            get
                            {
                                if (studentType == 0)
                                {
                                    return "ms-appx:///Assets/images/ic_th_sex_boy.png";
                                }
                                else if (studentType == 2)
                                {
                                    return "ms-appx:///Assets/images/ic_th_sex_girl.png";
                                }
                                else
                                {
                                    return "";
                                }
                            }
                        }
                        public string nickName { get; set; }
                        public string fullAvatarUrl { get; set; }
                    }
                    public class qiniuImgBO
                    {
                        public string url { get; set; }
                    }
                }
            }
        }
        public class Playground
        {
            public class Main
            {
                public List<TopicItem> followList { get; set; }
                public List<HotTopicItem> hotList { get; set; }
                public List<TopicItem> recommendTopicList { get; set; }
                public List<ContentItem> recommendMessageList { get; set; }
                public class TopicItem
                {
                    public string typeInt { get; set; }
                    public string topicIdInt { get; set; }
                    public string nameStr { get; set; }
                    public string detailStr { get; set; }
                    public string picUrlStr { get; set; }
                    public string iconUrlStr { get; set; }
                    public string todayNum { get; set; }
                    public int isFollow { get; set; }
                    public string followtext
                    {
                        get
                        {
                            if (isFollow == 0)
                            {
                                return "关注";
                            }
                            else
                            {
                                return "取消关注";
                            }
                        }
                    }
                    public string followbtncolor
                    {
                        get
                        {
                            if (isFollow == 0)
                            {
                                return "#FFEEC847";
                            }
                            else
                            {
                                return "#FF8B8B8B";
                            }
                        }
                    }
                }
                public class HotTopicItem
                {
                    public string typeInt { get; set; }
                    public string topicIdInt { get; set; }
                    public string detailStr { get; set; }
                    public string picUrlStr { get; set; }
                }
                public class ContentItem
                {
                    public string category { get; set; }
                    public string messageId { get; set; }
                    public string content { get; set; }
                    public string plateName { get; set; }
                    public string hasvoice
                    {
                        get
                        {
                            if (voiceInfoBO != null && voiceInfoBO.lengthInt != 0)
                            {
                                return "Visible";
                            }
                            else
                            {
                                return "Collapsed";
                            }
                        }
                    }
                    public string voicetime
                    {
                        get
                        {
                            if (voiceInfoBO != null && voiceInfoBO.lengthInt != 0)
                            {
                                var time = TimeSpan.FromSeconds(voiceInfoBO.lengthInt);
                                return time.Minutes.ToString() + "'" + time.Seconds.ToString("D2") + "''";
                            }
                            else
                            {
                                return "";
                            }
                        }
                    }
                    public StudentItem studentBO { get; set; }
                    public VoiceItem voiceInfoBO { get; set; }
                }
                public class VoiceItem
                {
                    public int lengthInt { get; set; }
                    public string urlStr { get; set; }
                }
                public class StudentItem
                {
                    public string fullAvatarUrl { get; set; }
                }
            }
            public class Topic
            {
                public BOs announcementBOs { get; set; }
                public BOs gg_BO
                {
                    get
                    {
                        if (basicBOs != null && basicBOs.Count > 0)
                        {
                            if(basicBOs[0].studentBO==null)
                            {
                                return basicBOs[0];
                            }
                            else
                            {
                                return null;
                            }
                        }else
                        {
                            return null;
                        }
                    }
                }
                public ObservableCollection<BOs> basicBOs
                {
                    get
                    {
                        if(messageBOs != null && messageBOs.Count > 0)
                        {
                            var result = new ObservableCollection<BOs>();
                            foreach (var item in messageBOs)
                            {
                                if (item.content != null && item.content != "")
                                {
                                    result.Add(item);
                                }
                            }
                            return result;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                public ObservableCollection<BOs> messageBOs { get; set; }
                public class BOs: INotifyPropertyChanged
                {
                    public int myLike { get; set; }
                    public int likeCount { get; set; }
                    public long issueTime { get; set; }
                    public string messageId { get; set; }
                    public string icon { get; set; }
                    public string content { get; set; }
                    public int comments { get; set; }
                    public string publisher { get; set; }
                    public Windows.UI.Xaml.Controls.RichTextBlock ContentUI
                    {
                        get
                        {
                            var graph = new Windows.UI.Xaml.Documents.Paragraph();
                            if (content.Contains("$"))
                            {
                                var strs = content.Split('$');
                                foreach (var item in strs)
                                {
                                    var image = Class.Model.FaceIcon.GetFaceImage(item);
                                    if (image != null)
                                    {
                                        var uiconter = new Windows.UI.Xaml.Documents.InlineUIContainer();
                                        uiconter.Child = image;
                                        graph.Inlines.Add(uiconter);
                                    }
                                    else
                                    {
                                        graph.Inlines.Add(new Windows.UI.Xaml.Documents.Run() { Text = item });
                                    }
                                }
                            }
                            else
                            {
                                graph.Inlines.Add(new Windows.UI.Xaml.Documents.Run() { Text = content });
                            }
                            var result = new Windows.UI.Xaml.Controls.RichTextBlock();
                            result.IsTextSelectionEnabled = false;
                            result.Margin = new Windows.UI.Xaml.Thickness(0,5,0,5);
                            result.TextWrapping = Windows.UI.Xaml.TextWrapping.WrapWholeWords;
                            result.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Black);
                            result.Blocks.Add(graph);
                            return result;
                        }
                    }

                    public string nearSchoolName { get; set; }
                    public List<qiniuImgBO> qiniuImgBOs { get; set; }
                    public student studentBO { get; set; }
                    public class student
                    {
                        public int studentId { get; set; }
                        public int gender { get; set; }
                        public string rate { get; set; }
                        public string sex
                        {
                            get
                            {
                                if (gender == 1)
                                {
                                    return "ms-appx:///Assets/images/ic_th_sex_boy.png";
                                }
                                else
                                {
                                    return "ms-appx:///Assets/images/ic_th_sex_girl.png";
                                }
                            }
                        }
                        public string nickName { get; set; }
                        public string fullAvatarUrl { get; set; }
                    }
                    public class qiniuImgBO
                    {
                        public string url { get; set; }
                    }
                    public string publishtime
                    {
                        get
                        {
                            if (issueTime > 0)
                            {
                                DateTime dtStart = new DateTime(1970, 1, 1);
                                long lTime = long.Parse(issueTime + "0000");
                                TimeSpan time = new TimeSpan(lTime);
                                dtStart = dtStart.Add(time);
                                return dtStart.ToString("yyyy-MM-dd hh:mm");
                            }
                            else
                            {
                                return "";
                            }
                        }
                    }

                    public event PropertyChangedEventHandler PropertyChanged;
                    public void RaisePropertyChanged(string name)
                    {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                    }
                }
            }
            public class OBComment
            {
                public string commentId { get; set; }
                public string rootCommentId { get; set; }
                public string messageId { get; set; }
                public long commentTime { get; set; }
                public string content { get; set; }
                public string schoolName { get; set; }
                public int floor { get; set; }
                public string reContent { get; set; }
                public string publishtime
                {
                    get
                    {
                        if (commentTime > 0)
                        {
                            DateTime dtStart = new DateTime(1970, 1, 1);
                            long lTime = long.Parse(commentTime + "0000");
                            TimeSpan time = new TimeSpan(lTime);
                            dtStart = dtStart.Add(time);
                            return dtStart.ToString("yyyy-MM-dd hh:mm");
                        }
                        else
                        {
                            return "";
                        }
                    }
                }
                public string hasmore
                {
                    get
                    {
                        if (reContent == null || reContent == "")
                        {
                            return "Collapsed";
                        }
                        else
                        {
                            return "Visible";
                        }
                    }
                }
                public Windows.UI.Xaml.Controls.RichTextBlock ContentUI
                {
                    get
                    {
                        var graph = new Windows.UI.Xaml.Documents.Paragraph();
                        if (content.Contains("$"))
                        {
                            var strs = content.Split('$');
                            foreach (var item in strs)
                            {
                                var image = Class.Model.FaceIcon.GetFaceImage(item);
                                if (image != null)
                                {
                                    var uiconter = new Windows.UI.Xaml.Documents.InlineUIContainer();
                                    uiconter.Child = image;
                                    graph.Inlines.Add(uiconter);
                                }
                                else
                                {
                                    graph.Inlines.Add(new Windows.UI.Xaml.Documents.Run() { Text = item });
                                }
                            }
                        }
                        else
                        {
                            graph.Inlines.Add(new Windows.UI.Xaml.Documents.Run() { Text = content });
                        }
                        var result = new Windows.UI.Xaml.Controls.RichTextBlock();
                        result.Margin = new Windows.UI.Xaml.Thickness(0, 5, 0, 5);
                        result.TextWrapping = Windows.UI.Xaml.TextWrapping.WrapWholeWords;
                        result.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Black);
                        result.Blocks.Add(graph);
                        result.IsTextSelectionEnabled = false;
                        if ((Window.Current.Content as Frame).CurrentSourcePageType != typeof(Friday.Views.Playground.OBDetalisPage))
                        {
                            result.Tapped += (s, e) =>
                            {
                                (Window.Current.Content as Frame).Navigate(typeof(Friday.Views.Playground.OBDetalisPage), Class.Data.Json.ToJsonData(this));
                            };
                        }
                        return result;
                    }
                }
                public Windows.UI.Xaml.Controls.RichTextBlock MoreContentUI
                {
                    get
                    {
                        if (reContent != null)
                        {
                            var graph = new Windows.UI.Xaml.Documents.Paragraph();
                            if (reContent.Contains("$"))
                            {
                                var strs = reContent.Split('$');
                                foreach (var item in strs)
                                {
                                    var image = Class.Model.FaceIcon.GetFaceImage(item);
                                    if (image != null)
                                    {
                                        var uiconter = new Windows.UI.Xaml.Documents.InlineUIContainer();
                                        uiconter.Child = image;
                                        graph.Inlines.Add(uiconter);
                                    }
                                    else
                                    {
                                        graph.Inlines.Add(new Windows.UI.Xaml.Documents.Run() { Text = item });
                                    }
                                }
                            }
                            else
                            {
                                graph.Inlines.Add(new Windows.UI.Xaml.Documents.Run() { Text = reContent });
                            }
                            var result = new Windows.UI.Xaml.Controls.RichTextBlock();
                            result.Margin = new Windows.UI.Xaml.Thickness(0, 5, 0, 5);
                            result.TextWrapping = Windows.UI.Xaml.TextWrapping.WrapWholeWords;
                            result.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Black);
                            result.Blocks.Add(graph);
                            return result;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                public Student student { get; set; }
                public class Student
                {
                    public int studentId { get; set; }
                    public int gender { get; set; }
                    public string rate { get; set; }
                    public string sex
                    {
                        get
                        {
                            if (gender == 1)
                            {
                                return "ms-appx:///Assets/images/ic_th_sex_boy.png";
                            }
                            else
                            {
                                return "ms-appx:///Assets/images/ic_th_sex_girl.png";
                            }
                        }
                    }
                    public string nickName { get; set; }
                    public string fullAvatarUrl { get; set; }
                }
            }
        }
        public class CourseManager
        {
            public class CourseModel:INotifyPropertyChanged
            {
                public bool autoEntry { get; set; }
                public int beginYear { get; set; }
                public string classroom { get; set; }
                public long courseId { get; set; }
                public int courseMark { get; set; }
                public int courseType { get; set; }
                public int day { get; set; }
                public int endYear { get; set; }
                public string id { get; set; }
                public string name { get; set; }
                public int nonsupportAmount { get; set; }
                public string period { get; set; }
                public int provinceId { get; set; }
                public string schoolId { get; set; }
                public long schooltime { get; set; }
                public int sectionEnd { get; set; }
                public int sectionStart { get; set; }
                public string smartPeriod { get; set; }
                public int studentCount { get; set; }
                public int supportAmount { get; set; }
                public string teacher { get; set; }
                public int term { get; set; }
                public int verifyStatus { get; set; }
                public string time
                {
                    get
                    {
                        string result = "";
                        if (period!="")
                        {
                            var weeks = period.Split(' ');
                            if ((int.Parse(weeks[weeks.Count() - 1]) - int.Parse(weeks[0])) == (weeks.Count() - int.Parse(weeks[0])))
                            {
                                result = weeks[0] + "-" + weeks[weeks.Count() - 1] + "周";
                            }
                            else
                            {
                                result = period + "周";
                            }
                            result = result + " 周" + Data.Int_String.NumberToChinese(day.ToString());
                            result = result + " ";
                            if (sectionStart == sectionEnd)
                            {
                                result = result + "第" + sectionStart + "节";
                            }
                            else
                            {
                                result = result + sectionStart + "-" + sectionEnd + "节";
                            }
                        }
                        return result;
                    }
                }
                public bool isadd { get; set; }
                public string btntext
                {
                    get
                    {
                        if (isadd)
                        {
                            return "从课表移除";
                        }
                        else
                        {
                            return "添加到课表";
                        }
                    }
                }
                public string btncolor
                {
                    get
                    {
                        if (isadd)
                        {
                            return "#FFC7C7C7";
                        }
                        else
                        {
                            return "#FF65E271";
                        }
                    }
                }
                public Windows.UI.Xaml.Controls.TextBlock BtnContent {
                    get
                    {
                        var textblock = new Windows.UI.Xaml.Controls.TextBlock();
                        textblock.Text = name + "@" + classroom;
                        textblock.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(CourseButton.ForegroundColor);
                        textblock.TextWrapping = Windows.UI.Xaml.TextWrapping.WrapWholeWords;
                        textblock.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                        textblock.Margin = new Windows.UI.Xaml.Thickness(5);
                        textblock.FontSize = 12;
                        textblock.DataContext = id;
                        return textblock;
                    }
                }
                public event PropertyChangedEventHandler PropertyChanged;
                public void RaisePropertyChanged(string name)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                }
                public CourseBtnStyle CourseButton { get; set; }
                public class CourseBtnStyle
                {
                    public Windows.UI.Color ForegroundColor { get; set; }
                    public Windows.UI.Color BackgroundColor { get; set; }
                    public CourseBtnStyle()
                    {
                        var ColorList = new List<Windows.UI.Color>();
                        ColorList.Add(Windows.UI.Color.FromArgb(255,238,88,88));
                        ColorList.Add(Windows.UI.Color.FromArgb(255, 231, 238, 88));
                        ColorList.Add(Windows.UI.Color.FromArgb(255, 163, 238, 88));
                        ColorList.Add(Windows.UI.Color.FromArgb(255, 108, 238, 88));
                        ColorList.Add(Windows.UI.Color.FromArgb(255, 88, 238, 108));
                        ColorList.Add(Windows.UI.Color.FromArgb(255, 88, 238, 170));
                        ColorList.Add(Windows.UI.Color.FromArgb(255, 88, 238, 101));
                        ColorList.Add(Windows.UI.Color.FromArgb(255, 88, 238, 0));
                        ColorList.Add(Windows.UI.Color.FromArgb(255, 226, 23, 60));
                        ColorList.Add(Windows.UI.Color.FromArgb(255, 79, 168, 147));
                        int num = random.Next(0, ColorList.Count);
                        ForegroundColor = Windows.UI.Colors.White;
                        BackgroundColor = ColorList[num];
                    }
                }
                public CourseModel()
                {
                    CourseButton = new CourseBtnStyle();
                }
            }
            public class StudentModel
            {
                public string academyName { get; set; }
                public string fullAvatarUrl { get; set; }
                public int grade { get; set; }
                public int gender { get; set; }
                public string nickName { get; set; }
                public string signature { get; set; }
                public string studentId { get; set; }
                public string sex
                {
                    get
                    {
                        if (gender == 1)
                        {
                            return "ms-appx:///Assets/images/ic_th_sex_boy.png";
                        }
                        else
                        {
                            return "ms-appx:///Assets/images/ic_th_sex_girl.png";
                        }
                    }
                }
                public string hassignature
                {
                    get
                    {
                        if (signature== null||signature=="")
                        {
                            return "Collapsed";
                        }
                        else
                        {
                            return "Visible";
                        }
                    }
                }
            }
            public static async Task<bool> Add(CourseModel course,string yaer=null,string term=null)
            {
                var courselist =await GetCourse(yaer, term);
                if (CanAdd(course, courselist))
                {
                    var id = await Async.Add(course);
                    if (id != null && id.Length > 0)
                    {
                        course.id = id;
                        course.courseId = long.Parse(id);
                    }
                    courselist.Add(course);
                    await SaveCourse(courselist, yaer, term);
                    return true;
                }
                else
                {
                    Tools.ShowMsgAtFrame("有课程冲突");
                    return false;
                }
            }
            public static async Task Remove(CourseModel course, string yaer = null, string term = null)
            {
                var courselist = await GetCourse(yaer, term);
                for (int i = 0; i < courselist.Count; i++)
                {
                    if (courselist[i].id == course.id)
                    {
                        Async.Del(courselist[i].id);
                        courselist.RemoveAt(i);
                        await SaveCourse(courselist, yaer, term);
                    }
                }
            }
            public static async Task Remove(string id, string yaer = null, string term = null)
            {
                var courselist = await GetCourse(yaer, term);
                for (int i = 0; i < courselist.Count; i++)
                {
                    if (courselist[i].id == id)
                    {
                        Async.Del(courselist[i].id);
                        courselist.RemoveAt(i);
                        await SaveCourse(courselist, yaer, term);
                    }
                }
            }
            public static bool CanAdd(CourseModel Course, ObservableCollection<CourseModel> CourseList)
            {
                var CourseDir = new Dictionary<int, List<int>>();
                bool can = true;
                for (int i = 1; i < 8; i++)
                {
                    CourseDir.Add(i, new List<int>());
                }
                foreach (var item in CourseList)
                {
                    if (item.sectionStart == item.sectionEnd)
                    {
                        CourseDir[item.day].Add(item.sectionStart);
                    }else
                    {
                        for (int i = 0; i < item.sectionEnd- item.sectionStart; i++)
                        {
                            CourseDir[item.day].Add(item.sectionStart+i);
                        }
                    }
                }
                if (Course.sectionStart == Course.sectionEnd)
                {
                    return !CourseDir[Course.day].Contains(Course.sectionStart);
                }
                else
                {
                    for (int i = 0; i < Course.sectionEnd - Course.sectionStart; i++)
                    {
                        if (CourseDir[Course.day].Contains(Course.sectionStart + i))
                        {
                            can = false;
                        }
                    }
                    return can;
                }

            }
            public static async Task<ObservableCollection<CourseModel>> GetCourse(string yaer = null, string term = null)
            {
                string filename = "course_";
                if (yaer == null)
                {
                    filename = filename + MD5.GetMd5String(UserManager.UserData.beginYear.ToString() + UserManager.UserData.term.ToString());
                }
                else
                {
                    filename = filename + MD5.GetMd5String(yaer + term);
                }
                var CourseFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Course",CreationCollisionOption.OpenIfExists);
                var CourseFile = await CourseFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
                var json = await FileIO.ReadTextAsync(CourseFile);
                ObservableCollection<CourseModel> CourseList;
                if (json == "")
                {
                    CourseList = null;
                }
                else
                {
                    CourseList = Data.Json.DataContractJsonDeSerialize<ObservableCollection<CourseModel>>(json);
                }
                if (CourseList == null)
                {
                    CourseList = new ObservableCollection<CourseModel>();
                }
                return CourseList;
            }
            public static async Task<CourseModel> GetCourse(int day, int section, string yaer = null, string term = null, string week = null)
            {
                string filename = "course_";
                if (yaer == null)
                {
                    filename = filename + MD5.GetMd5String(UserManager.UserData.beginYear.ToString() + UserManager.UserData.term.ToString());
                }
                else
                {
                    filename = filename + MD5.GetMd5String(yaer + term);
                }

                
                var CourseFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Course", CreationCollisionOption.OpenIfExists);
                var CourseFile = await CourseFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
                var json = await FileIO.ReadTextAsync(CourseFile);
                
                ObservableCollection<CourseModel> CourseList;
                if (json == "")
                {
                    CourseList = null;
                }
                else
                {
                    CourseList = Data.Json.DataContractJsonDeSerialize<ObservableCollection<CourseModel>>(json);
                }
                if (CourseList == null)
                {
                    return null;
                }else
                {
                    CourseModel result = null;

                    var resultList = new List<CourseModel>();
                    foreach (var item in CourseList)
                    {
                        if(item.day==day&&(item.sectionStart<=section&& section <= item.sectionEnd))
                        {
                            resultList.Add(item);
                            result = item;
                        }
                    }
                    
                    var isWeekValid = int.TryParse(week, out int _week);


                    if (resultList.Count > 1 && isWeekValid)
                    {
                        foreach (var item in resultList)
                        {
                            var weeks = item.smartPeriod.Split(' ');

                            if (Array.IndexOf(weeks, week) >= 0)
                            {
                                result = item;
                                break;
                            }
                        }
                        
                    }
                    
                    

                    return result;
                }
            }
            public static async Task SaveCourse(ObservableCollection<CourseModel> CourseList, string yaer = null, string term = null)
            {
                string filename = "course_";
                if (yaer == null)
                {
                    filename = filename + MD5.GetMd5String(UserManager.UserData.beginYear.ToString() + UserManager.UserData.term.ToString());
                }
                else
                {
                    filename = filename + MD5.GetMd5String(yaer + term);
                }
                var CourseFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Course", CreationCollisionOption.OpenIfExists);
                var CourseFile = await CourseFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(CourseFile, Data.Json.ToJsonData(CourseList));
            }
            public static async Task<bool> CopyCourseBySuperId(string superId,string year,string term)
            {
                try
                {
                    if (HttpPostUntil.isInternetAvailable)
                    {
                        var postdata = HttpPostUntil.GetBasicPostData();
                        postdata.Add(new KeyValuePair<string, string>("superId",superId));
                        postdata.Add(new KeyValuePair<string, string>("beginYear", year));
                        postdata.Add(new KeyValuePair<string, string>("term", term));
                        var json = await HttpPostUntil.HttpPost(Data.Urls.Course.getCourseTableBySuperId, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                        var obj = Windows.Data.Json.JsonObject.Parse(json);
                        await Setting.SetCourseMaxCount((int)obj["data"].GetObject()["maxCountInt"].GetNumber());
                        var courselist = Data.Json.DataContractJsonDeSerialize<ObservableCollection<CourseModel>>(obj["data"].GetObject()["courseList"].GetArray().ToString());
                        await SaveCourse(courselist, year,term);
                        foreach (var item in courselist)
                        {
                            await Async.Add(item);
                        }
                        return true;
                    }else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            public class Setting
            {
                public static async Task<bool> SetWeek(int week)
                {
                    if (HttpPostUntil.isInternetAvailable)
                    {
                        var localSetting = ApplicationData.Current.LocalSettings;
                        var userdata1 = Data.Json.DataContractJsonDeSerialize<User.Login_Result>((string)localSetting.Values["userdata"]);
                        userdata1.attachmentBO.nowWeekMsg.nowWeek = week;
                        var postdata = HttpPostUntil.GetBasicPostData();
                        postdata.Add(new KeyValuePair<string, string>("setTime", HttpPostUntil.NowTime));
                        postdata.Add(new KeyValuePair<string, string>("nowWeek", week.ToString()));
                        var json = await HttpPostUntil.HttpPost(Data.Urls.Course.setNowWeek, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                        localSetting.Values["userdata"] = Data.Json.ToJsonData(userdata1);
                        return true;
                    }
                    else
                    {
                        Tools.ShowMsgAtFrame("网路异常");
                        return false;
                    }
                }
                public static async Task<bool> SetCourseMaxCount(int count)
                {
                    if (HttpPostUntil.isInternetAvailable)
                    {
                        var localSetting = ApplicationData.Current.LocalSettings;
                        var userdata1 = Data.Json.DataContractJsonDeSerialize<User.Login_Result>((string)localSetting.Values["userdata"]);
                        userdata1.maxCount = count;
                        var postdata = HttpPostUntil.GetBasicPostData();
                        postdata.Add(new KeyValuePair<string, string>("startYear", userdata1.beginYear.ToString()));
                        postdata.Add(new KeyValuePair<string, string>("term", userdata1.term.ToString()));
                        postdata.Add(new KeyValuePair<string, string>("maxCount", count.ToString()));
                        var json = await HttpPostUntil.HttpPost(Data.Urls.Course.setMaxCount, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                        localSetting.Values["userdata"] = Data.Json.ToJsonData(userdata1);
                        return true;
                    }
                    else
                    {
                        Tools.ShowMsgAtFrame("网路异常");
                        return false;
                    }
                }
            }
            public class Async
            {
                public static async void Del(string id)
                {
                    try
                    {
                        if (HttpPostUntil.isInternetAvailable)
                        {
                            var postdata = HttpPostUntil.GetBasicPostData();
                            postdata.Add(new KeyValuePair<string, string>("id",id));
                            postdata.Add(new KeyValuePair<string, string>("term", UserManager.UserData.term.ToString()));
                            postdata.Add(new KeyValuePair<string, string>("beginYear", UserManager.UserData.beginYear.ToString()));
                            await HttpPostUntil.HttpPost(Data.Urls.Course.Async.deleteCourse, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message);
                    }
                }

                public static async Task<string> Add(CourseModel course)
                {
                    try
                    {
                        if (HttpPostUntil.isInternetAvailable)
                        {
                            var postdata = HttpPostUntil.GetBasicPostData();
                            postdata.Add(new KeyValuePair<string, string>("endYear", course.endYear.ToString()));
                            postdata.Add(new KeyValuePair<string, string>("period",course.period));
                            postdata.Add(new KeyValuePair<string, string>("sectionEnd",course.sectionEnd.ToString()));
                            postdata.Add(new KeyValuePair<string, string>("period", course.period));
                            postdata.Add(new KeyValuePair<string, string>("sourseId","-1"));
                            postdata.Add(new KeyValuePair<string, string>("classroom", course.classroom));
                            postdata.Add(new KeyValuePair<string, string>("sectionStart", course.sectionStart.ToString()));
                            postdata.Add(new KeyValuePair<string, string>("term", course.term.ToString()));
                            postdata.Add(new KeyValuePair<string, string>("beginYear", course.beginYear.ToString()));
                            postdata.Add(new KeyValuePair<string, string>("name", course.name));
                            postdata.Add(new KeyValuePair<string, string>("courseMark", "0"));
                            postdata.Add(new KeyValuePair<string, string>("day", course.day.ToString()));
                            postdata.Add(new KeyValuePair<string, string>("teacher", course.teacher));
                            var json= await HttpPostUntil.HttpPost(Data.Urls.Course.Async.addCourse, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                            return JsonObject.Parse(json).GetNamedObject("data").GetNamedNumber("courseId").ToString();
                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message);
                        return null;
                    }
                }

                public static async Task GetCourseTableFromServer()
                {
                    try
                    {
                        if (HttpPostUntil.isInternetAvailable)
                        {
                            var postdata = HttpPostUntil.GetBasicPostData();
                            postdata.Add(new KeyValuePair<string, string>("term", UserManager.UserData.term.ToString()));
                            postdata.Add(new KeyValuePair<string, string>("beginYear", UserManager.UserData.beginYear.ToString()));
                            var json = await HttpPostUntil.HttpPost(Data.Urls.Course.Async.getCourseTableFromServer, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                            json = JsonObject.Parse(json).GetNamedObject("data").GetNamedArray("lessonList").ToString().Replace("locale","classroom").Replace("sectionstart", "sectionStart").Replace("sectionend", "sectionEnd").Replace("startSchoolYear", "beginYear").Replace("endSchoolYear", "endYear");
                            var courselist = Data.Json.DataContractJsonDeSerialize<ObservableCollection<CourseModel>>(json);
                            await SaveCourse(courselist);
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message);
                    }
                }
            }
        }
        public class User
        {
            public class Login_Result
            {
                /// <summary>
                /// 
                /// </summary>
                public long academyId { get; set; }
                /// <summary>
                /// 计算机技术与工程学院
                /// </summary>
                public string academyName { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long addTime { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public AttachmentBO attachmentBO { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string avatarUrl { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public int beginYear { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string bigAvatarUrl { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long bornCityId { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long bornDate { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long bornProvinceId { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string fullAvatarUrl { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long gender { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long grade { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long highSchoolId { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long id { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string identity { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long lastLoglongime { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long loveState { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public int maxCount { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string mobileNumber { get; set; }
                /// <summary>
                /// 王文斌
                /// </summary>
                public string nickName { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public List<PhotoBOItem> photoBO { get; set; }
                /// <summary>
                /// 王文斌
                /// </summary>
                public string realName { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long schoolId { get; set; }
                /// <summary>
                /// 长春工程学院
                /// </summary>
                public string schoolName { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long schoolRoll { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long studentId { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string superId { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string supportAuto { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public int term { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long versionId { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long weiboAccount { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long weiboExpiresIn { get; set; }
                public class GopushBO
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    public string aliasName { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long mid { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long pmid { get; set; }
                }
                public class MyTermListItem
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    public long addTime { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long beginYear { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long id { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long maxCount { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long studentId { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long term { get; set; }
                }
                public class NowWeekMsg
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    public int nowWeek { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long setTime { get; set; }
                }
                public class AttachmentBO
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    public long contactStatus { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long courseRemind { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long courseRemindTime { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long dayOfWeek { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long defaultOpen { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public GopushBO gopushBO { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string hasTermList { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string hasVerCode { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string identity { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public List<MyTermListItem> myTermList { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long needSASL { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public NowWeekMsg nowWeekMsg { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string openGopush { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string openJpush { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long openRubLessonlong { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long purviewValue { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long rate { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long realNameMsgNum { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string rubLessonTips { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string showRate { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string supportAuto { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string type { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long vipLevel { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string xmppDomain { get; set; }
                }
                public class PhotoBOItem
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    public string avatar { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long id { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long photoId { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string photoUrl { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long studentId { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string thumUrl { get; set; }
                }
            }
        }
        public class FaceIcon
        {
            public static string face_str= "微笑$呲牙$doge$挖鼻$闭嘴$生气$流汗$泪$偷笑$打瞌睡$惊讶$可怜$害羞$冷汗$撇嘴$哈哈$生病$太开心$可爱$挤眼$爱你$白眼$委屈$巴掌$衰$鄙视$悲伤$思考$怒$抓狂$馋嘴$晕$酷$亲亲$发财$鼓掌$愤怒$笑哭$色$打哈欠";
            public static string[] face_str_list
            {
                get
                {
                    return face_str.Split('$');
                }
            }
            public static string basic_face_source = "ms-appx:///Assets/images/FaceIcon/bb_{0}.png";
            public static string GetFaceText(string source)
            {
                var indexstr = source.Replace("ms-appx:///Assets/images/FaceIcon/bb_", "").Replace(".png", "");
                if (indexstr[0] == '0')
                {
                    return "$"+face_str_list[int.Parse(indexstr[1].ToString())-1]+ "$";
                }
                else
                {
                    return "$"+face_str_list[int.Parse(indexstr)-1]+ "$";
                }
            }
            public static string GetFaceSource(int index)
            {
                return string.Format(basic_face_source, (index+1).ToString("D2"));
            }
            public static string GetFaceSource(string facestr)
            {
                facestr = facestr.Replace("$", "");
                for (int i = 0; i < face_str_list.Count(); i++)
                {
                    if (facestr == face_str_list[i]) return GetFaceSource(i);
                }
                return null;
            }
            public static Windows.UI.Xaml.Controls.Image GetFaceImage(int index,int width=25,int height=25)
            {
                var source= string.Format(basic_face_source, index.ToString("D2"));
                var image = new Windows.UI.Xaml.Controls.Image();
                image.Width = width;
                image.Height = height;
                image.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(source));
                return image;
            }
            public static Windows.UI.Xaml.Controls.Image GetFaceImage(string facestr, int width= 25, int height = 25)
            {
                facestr = facestr.Replace("$", "");
                for (int i = 0; i < face_str_list.Count(); i++)
                {
                    if (facestr == face_str_list[i])
                    {
                        var source= GetFaceSource(i);
                        var image = new Windows.UI.Xaml.Controls.Image();
                        image.Width = width;
                        image.Height = height;
                        image.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(source));
                        return image;
                    }
                }
                return null;
            }
        }
    }
}
