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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Friday.Views.Course
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CourseListPage : Page
    {
        int page = 0;
        public string FilderText { get; set; }
        public string[] ParameterData { get; private set; }

        public CourseListPage()
        {
            this.InitializeComponent();
            this.Transitions = new Windows.UI.Xaml.Media.Animation.TransitionCollection();
            this.Transitions.Add(new Windows.UI.Xaml.Media.Animation.PaneThemeTransition { Edge = EdgeTransitionLocation.Right });
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                SchoolText.Text = Class.UserManager.UserData.schoolName;
                var year = Class.UserManager.UserData.beginYear;
                TermText.Text = year+"-"+(year+1) +"学年 第"+ Class.Data.Int_String.NumberToChinese(Class.UserManager.UserData.term.ToString()) + "学期";
                if (e.Parameter == null)
                {
                    SearchTextBox.Style = (Style)Resources["TextBoxStyle1"];
                    CourseList.Style = (Style)Resources["LessionListStyle"];
                    FilderBtn.Visibility = Visibility.Collapsed;
                    await LoadData();
                }else
                {
                    ParameterData = e.Parameter as string[];
                    var section = ParameterData[0];
                    var day = ParameterData[1];
                    SearchTextBox.Style = (Style)Resources["TextBoxStyle2"];
                    CourseList.Style = (Style)Resources["CourseListStyle"];
                    FilderBtn.Visibility = Visibility.Visible;
                    FilderText = string.Format("周{0} 第{1}节",Class.Data.Int_String.GetWeekString(day),section);
                    await LoadData(section,day);
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if(e.NavigationMode!=NavigationMode.New) this.NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        private async Task LoadData(string section, string day)
        {
            LoadProgress.IsActive = true;
            if (Class.HttpPostUntil.isInternetAvailable)
            {
                var postdata = Class.HttpPostUntil.GetBasicPostData();
                postdata.Add(new KeyValuePair<string, string>("page", page.ToString()));
                postdata.Add(new KeyValuePair<string, string>("sectionStart", section));
                postdata.Add(new KeyValuePair<string, string>("sectionEnd", section));
                postdata.Add(new KeyValuePair<string, string>("day", day));
                postdata.Add(new KeyValuePair<string, string>("term", Class.UserManager.UserData.term.ToString()));
                postdata.Add(new KeyValuePair<string, string>("startYear", Class.UserManager.UserData.beginYear.ToString()));
                var json = await Class.HttpPostUntil.HttpPost(Class.Data.Urls.Course.getCourseListBySchoolTime, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                try
                {
                    var service = new FridayCloudService.ServiceClient(FridayCloudService.ServiceClient.EndpointConfiguration.BasicHttpBinding_IService);
                    json = Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject()["courseList"].GetArray().ToString();
                    json = await service.EncryptCourseListAsync(json, "cn.friday@" + Class.UserManager.UserData.studentId);
                    await service.CloseAsync();
                    var courselist = Class.Data.Json.DataContractJsonDeSerialize<ObservableCollection<Class.Model.CourseManager.CourseModel>>(json);
                    var courselistjson = Class.Data.Json.ToJsonData(await Class.Model.CourseManager.GetCourse());
                    if (courselist != null)
                    {
                        foreach (var item in courselist)
                        {
                            if (item.time != "")
                            {
                                if (courselistjson.Contains(item.id))
                                {
                                    item.isadd = true;
                                }
                                else
                                {
                                    item.isadd = false;
                                }
                                CourseList.Items.Add(item);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    if (page != 0) page = page - 1;
                    Class.Tools.ShowMsgAtFrame("网路异常");
                }
            }
            else
            {
                Class.Tools.ShowMsgAtFrame("网路异常");
            }
            LoadProgress.IsActive = false;
        }

        private async Task LoadData()
        {
            LoadProgress.IsActive = true;
            if (Class.HttpPostUntil.isInternetAvailable)
            {
                try
                {
                    var postdata = Class.HttpPostUntil.GetBasicPostData();
                    postdata.Add(new KeyValuePair<string, string>("page", page.ToString()));
                    postdata.Add(new KeyValuePair<string, string>("term", Class.UserManager.UserData.term.ToString()));
                    postdata.Add(new KeyValuePair<string, string>("startYear", Class.UserManager.UserData.beginYear.ToString()));
                    var json = await Class.HttpPostUntil.HttpPost(Class.Data.Urls.Course.getPopCourseGroups, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                    var array = Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject()["datas"].GetArray();
                    foreach (var item in array)
                    {
                        CourseList.Items.Add(item.GetObject()["name"].GetString());
                    }
                }
                catch (Exception)
                {
                    if (page != 0) page = page - 1;
                }
            }
            else
            {
                Class.Tools.ShowMsgAtFrame("网路异常");
            }
            LoadProgress.IsActive = false;
        }

        private void GoBackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void AddCourseBtn_Clicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var course = btn.DataContext as Class.Model.CourseManager.CourseModel;
            if (course.isadd)
            {
                await Class.Model.CourseManager.Remove(course);
                course.isadd = !course.isadd;
            }
            else
            {
                course.isadd= await Class.Model.CourseManager.Add(course);
            }
            course.RaisePropertyChanged("btntext");
            course.RaisePropertyChanged("btncolor");
        }

        private async void FilderBtn_Clicked(object sender, RoutedEventArgs e)
        {
            var dialog = new Controls.DialogBox();
            dialog.mainTextBlock.Text = "确定去掉时间筛选条件?";
            dialog.Title = "提示";
            dialog.PrimaryButtonText = "取消";
            dialog.SecondaryButtonText = "确定";
            var asdf= await dialog.ShowAsync();
            if (asdf == ContentDialogResult.Secondary)
            {
                SearchTextBox.Style = (Style)Resources["TextBoxStyle1"];
                CourseList.Items.Clear();
                page = 0;
                CourseList.Style = (Style)Resources["LessionListStyle"];
                FilderBtn.Visibility = Visibility.Collapsed;
                await LoadData();
            }
        }

        private async void LoadNextPage(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv_SP = sender as ScrollViewer;
            if (sv_SP.VerticalOffset == sv_SP.ScrollableHeight)
            {
                page = page + 1;
                if (CourseList.Style == (Style)Resources["CourseListStyle"])
                {
                    await LoadData(ParameterData[0], ParameterData[1]);
                }else
                {
                    await LoadData();
                }
            }
        }

        private void CourseList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedIndex != -1)
            {
                if (list.Style == (Style)Resources["CourseListStyle"])
                {
                    Frame.Navigate(typeof(EditCoursePage), Class.Data.Json.ToJsonData(list.SelectedItem as Class.Model.CourseManager.CourseModel));
                }else
                {
                    Frame.Navigate(typeof(CourseListPage2), (string)list.SelectedItem);
                }
                list.SelectedIndex = -1;
            }
        }

        private void NewCourseBtn_Clicked(object sender, RoutedEventArgs e)
        {
            if (ParameterData == null)
            {
                Frame.Navigate(typeof(EditCoursePage));
            }else
            {
                var section = ParameterData[0];
                var day = ParameterData[1];
                Frame.Navigate(typeof(EditCoursePage),new int[] { int.Parse(day),int.Parse(section) });
            }
        }
    }
}
