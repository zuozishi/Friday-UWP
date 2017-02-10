using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace Friday.Views.Setting
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TermSettingPage : Page
    {
        private ObservableCollection<TermData> sourcedata;

        public TermSettingPage()
        {
            this.InitializeComponent();
            this.Transitions = new Windows.UI.Xaml.Media.Animation.TransitionCollection();
            this.Transitions.Add(new Windows.UI.Xaml.Media.Animation.PaneThemeTransition { Edge = EdgeTransitionLocation.Right });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            int nowindex = -1;
            var terms = Class.UserManager.UserData.attachmentBO.myTermList;
            sourcedata = new ObservableCollection<TermData>();
            for (int i = 0; i < terms.Count; i++)
            {
                if (terms[i].beginYear == Class.UserManager.UserData.beginYear && terms[i].term == Class.UserManager.UserData.term)
                {
                    nowindex = i;
                    sourcedata.Add(new TermData(terms[i]) { other = "(当前学期)" });
                }
                else
                {
                    sourcedata.Add(new TermData(terms[i]));
                }
            }
            termListView.ItemsSource = sourcedata;
            termListView.SelectedIndex = nowindex;
            termListView.SelectionMode = ListViewSelectionMode.Single;
            termListView.SelectionChanged += TermListView_SelectionChanged;
            for (int i = 0; i < 5; i++)
            {
                gradeListView.Items.Add(string.Format("20{0}-20{1}(大{2})", Class.UserManager.UserData.grade+i, Class.UserManager.UserData.grade + i+1,Class.Data.Int_String.NumberToChinese((i+1).ToString())));
            }
            gradeListView.SelectedIndex = 0;
            termListView1.SelectedIndex = 0;
        }

        private async void TermListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedIndex != -1)
            {
                var term_data = list.SelectedItem as TermData;
                var dialog = new Controls.DialogBox();
                dialog.Title = "提示";
                dialog.mainTextBlock.Text = "是否切换大" + term_data.grade + "第" + term_data.term_cn + "学期课程表?";
                dialog.PrimaryButtonText = "取消";
                dialog.SecondaryButtonText = "切换";
                if(await dialog.ShowAsync() == ContentDialogResult.Secondary)
                {
                    if (Class.HttpPostUntil.isInternetAvailable)
                    {
                        var postdata = Class.HttpPostUntil.GetBasicPostData();
                        postdata.Add(new KeyValuePair<string, string>("beginYear", term_data.beginYear.ToString()));
                        postdata.Add(new KeyValuePair<string, string>("term", term_data.term.ToString()));
                        var json = await Class.HttpPostUntil.HttpPost(Class.Data.Urls.Course.switchTerm, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                        if (json != null && json.Contains("1"))
                        {
                            await Class.UserManager.Login("15809628770", "www.123123");
                            Frame.GoBack();
                        }
                        else
                        {
                            Class.Tools.ShowMsgAtFrame("设置失败");
                        }
                    }
                    else
                    {
                        Class.Tools.ShowMsgAtFrame("网络异常");
                    }
                }
            }
        }

        private void GoBackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        public class TermData:Class.Model.User.Login_Result.MyTermListItem, INotifyPropertyChanged
        {
            public string term_cn
            {
                get
                {
                    return Class.Data.Int_String.NumberToChinese(term.ToString());
                }
            }
            public string grade
            {
                get
                {
                    var year = (beginYear % 2000 - Class.UserManager.UserData.grade) + 1;
                    return Class.Data.Int_String.NumberToChinese(year.ToString());
                }
            }
            public string other { get; set; }
            public TermData(Class.Model.User.Login_Result.MyTermListItem item)
            {
                this.addTime = item.addTime;
                this.beginYear = item.beginYear;
                this.id = item.id;
                this.maxCount = item.maxCount;
                this.studentId = item.studentId;
                this.term = item.term;
                other = "";
            }

            public event PropertyChangedEventHandler PropertyChanged;
            public void SetPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        private async void AddTermBtn_Clicked(object sender, RoutedEventArgs e)
        {
            var res = await addTermDialog.ShowAsync();
            if (res == ContentDialogResult.Secondary)
            {
                var year = 2000 + Class.UserManager.UserData.grade + gradeListView.SelectedIndex;
                var term = termListView1.SelectedIndex + 1;
                var terms = Class.UserManager.UserData.attachmentBO.myTermList;
                bool canadd = true;
                foreach (var item in terms)
                {
                    if (item.beginYear == year && item.term == term)
                    {
                        canadd = false;
                        break;
                    }
                }
                if (canadd)
                {
                    if (Class.HttpPostUntil.isInternetAvailable)
                    {
                        var postdata = Class.HttpPostUntil.GetBasicPostData();
                        postdata.Add(new KeyValuePair<string, string>("beginYear", year.ToString()));
                        postdata.Add(new KeyValuePair<string, string>("term", term.ToString()));
                        var json = await Class.HttpPostUntil.HttpPost(Class.Data.Urls.Course.createNewTerm, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                        if (json != null && json.Contains("nowWeek"))
                        {
                            await Class.UserManager.Login("15809628770", "www.123123");
                            Frame.GoBack();
                        }
                        else
                        {
                            Class.Tools.ShowMsgAtFrame("设置失败");
                        }
                    }
                    else
                    {
                        Class.Tools.ShowMsgAtFrame("网络异常");
                    }
                }
                else
                {
                    Class.Tools.ShowMsgAtFrame("该学期课程表已存在");
                }
            }
        }

        private async void DelTermBtn_Clicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var term_data = btn.DataContext as TermData;
            if (term_data != null)
            {
                var dialog = new Controls.DialogBox();
                dialog.Title = "提示";
                dialog.mainTextBlock.Text = "是否删除大" + term_data.grade + "第" + term_data.term_cn + "学期课程表?";
                dialog.PrimaryButtonText = "取消";
                dialog.SecondaryButtonText = "删除";
                if (await dialog.ShowAsync() == ContentDialogResult.Secondary)
                {
                    if (Class.HttpPostUntil.isInternetAvailable)
                    {
                        var postdata = Class.HttpPostUntil.GetBasicPostData();
                        postdata.Add(new KeyValuePair<string, string>("beginYear", term_data.beginYear.ToString()));
                        postdata.Add(new KeyValuePair<string, string>("term", term_data.term.ToString()));
                        var json = await Class.HttpPostUntil.HttpPost(Class.Data.Urls.Course.delTerm, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                        if (json != null && json.Contains("true"))
                        {
                            for (int i = 0; i < sourcedata.Count; i++)
                            {
                                if (sourcedata[i].id == sourcedata[i].id)
                                {
                                    sourcedata.RemoveAt(i);
                                    break;
                                }
                            }
                            await Class.UserManager.Login("15809628770", "www.123123");
                        }
                        else
                        {
                            Class.Tools.ShowMsgAtFrame("删除失败");
                        }
                    }
                    else
                    {
                        Class.Tools.ShowMsgAtFrame("网络异常");
                    }
                }
            }
        }
    }
}
