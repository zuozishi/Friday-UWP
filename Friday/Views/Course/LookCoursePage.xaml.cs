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

namespace Friday.Views.Course
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LookCoursePage : Page
    {
        public CourseModel course { get; set; }
        public LookCoursePage()
        {
            this.InitializeComponent();
            this.Transitions = new Windows.UI.Xaml.Media.Animation.TransitionCollection();
            this.Transitions.Add(new Windows.UI.Xaml.Media.Animation.PaneThemeTransition { Edge = EdgeTransitionLocation.Right });
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            course = Class.Data.Json.DataContractJsonDeSerialize<CourseModel>(e.Parameter.ToString());
            var courselist = await Class.Model.CourseManager.GetCourse();
            foreach (var item in courselist)
            {
                if (course.id == item.id)
                {
                    course = Class.Data.Json.DataContractJsonDeSerialize<CourseModel>(Class.Data.Json.ToJsonData(item));
                    break;
                }
            }
            CourseInfoList.DataContext = course;
            Title.Text = course.name;
            LoadClassmate();
        }

        private async void LoadClassmate()
        {
            if (Class.HttpPostUntil.isInternetAvailable)
            {
                try
                {
                    var postdata = Class.HttpPostUntil.GetBasicPostData();
                    postdata.Add(new KeyValuePair<string, string>("courseId", course.id));
                    postdata.Add(new KeyValuePair<string, string>("page", "0"));
                    var json =await Class.HttpPostUntil.HttpPost(Class.Data.Urls.Course.getStudentsByCourseId, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                    var obj = Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject();
                    ClassmateGrid.DataContext = obj["students"].GetArray().Count;
                    foreach (var item in obj["students"].GetArray())
                    {
                        ClassmateList.Items.Add(Class.Data.Json.DataContractJsonDeSerialize<Class.Model.CourseManager.StudentModel>(item.GetObject().ToString()));
                    }
                    ClassmateGrid.Visibility = Visibility;
                }
                catch (Exception)
                {
                    
                }
            }
        }

        private void GoBackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void DelCourseBtn_Clicked(object sender, RoutedEventArgs e)
        {
            var dialog = new Controls.DialogBox()
            {
                Title="提示",
                PrimaryButtonText="取消",
                SecondaryButtonText="确定"
            };
            dialog.mainTextBlock.Text = "确定要删除该课程?";
            if(await dialog.ShowAsync()==ContentDialogResult.Secondary)
            await Class.Model.CourseManager.Remove(course);
            Frame.GoBack();
        }

        private void EditCourseBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(EditCoursePage),Class.Data.Json.ToJsonData(course));
        }

        public class CourseModel : Class.Model.CourseManager.CourseModel
        {
            public string weektext
            {
                get
                {
                    string[] weeks;
                    if (smartPeriod != null) weeks = smartPeriod.Split(' '); else weeks = period.Split(' ', ',');

                    var firstWeekValid = int.TryParse(weeks[0], out int firstWeek);
                    var lastWeekValid = int.TryParse(weeks[weeks.Count() - 1], out int lastWeek);

                    if (firstWeekValid && lastWeekValid && (lastWeek - firstWeek == weeks.Count() - 1))
                    {
                        return weeks[0] + "-" + weeks[weeks.Count() - 1] + "周";
                    }
                    else
                    {
                        return smartPeriod + "周";
                    }
                }
            }
            public string sectext
            {
                get
                {
                    string result = "";
                    result = " 周" + Class.Data.Int_String.NumberToChinese(day.ToString());
                    result = result + " ";
                    if (sectionStart == sectionEnd)
                    {
                        result = result + "第" + sectionStart + "节";
                    }
                    else
                    {
                        result = result + sectionStart + "-" + sectionEnd + "节";
                    }
                    return result;
                }
            }
        }
    }
}
