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
    public sealed partial class CourseListPage2 : Page
    {
        public CourseListPage2()
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
                title_text.Text = (string)e.Parameter;
                await LoadData((string)e.Parameter);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
        }

        private async Task LoadData(string name)
        {
            if (Class.HttpPostUntil.isInternetAvailable)
            {
                try
                {
                    var postdata = Class.HttpPostUntil.GetBasicPostData();
                    postdata.Add(new KeyValuePair<string, string>("type", "0"));
                    postdata.Add(new KeyValuePair<string, string>("sectionEnd", "0"));
                    postdata.Add(new KeyValuePair<string, string>("sectionStart", "0"));
                    postdata.Add(new KeyValuePair<string, string>("term", Class.UserManager.UserData.term.ToString()));
                    postdata.Add(new KeyValuePair<string, string>("startYear", Class.UserManager.UserData.beginYear.ToString()));
                    postdata.Add(new KeyValuePair<string, string>("page", "0"));
                    postdata.Add(new KeyValuePair<string, string>("name",name));
                    postdata.Add(new KeyValuePair<string, string>("day","0"));
                    var json = await Class.HttpPostUntil.HttpPost(Class.Data.Urls.Course.getCourseListByPreciseName, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                    var service = new FridayCloudService.ServiceClient(FridayCloudService.ServiceClient.EndpointConfiguration.BasicHttpBinding_IService);
                    json = await service.EncryptCourseListAsync(Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject()["courseList"].GetArray().ToString(), "cn.friday@" + Class.UserManager.UserData.studentId);
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
                    Class.Tools.ShowMsgAtFrame("网路异常");
                }
            }else
            {
                Class.Tools.ShowMsgAtFrame("网路异常");
            }
        }

        private void GoBackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void CourseList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedIndex != -1)
            {
                Frame.Navigate(typeof(EditCoursePage), Class.Data.Json.ToJsonData(list.SelectedItem as Class.Model.CourseManager.CourseModel));
                list.SelectedIndex = -1;
            }
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
                course.isadd = await Class.Model.CourseManager.Add(course);
            }
            course.RaisePropertyChanged("btntext");
            course.RaisePropertyChanged("btncolor");
        }
    }
}
