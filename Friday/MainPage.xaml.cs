using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Friday
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            RegTileTask();
        }

        private static async void RegTileTask()
        {
            try
            {
                var status = await BackgroundExecutionManager.RequestAccessAsync();
                foreach (var cur in BackgroundTaskRegistration.AllTasks)
                {
                    if (cur.Value.Name == "TileUpdata")
                    {
                        //cur.Value.Unregister(true);
                        return;
                    }
                }
                var builder = new BackgroundTaskBuilder();
                builder.Name = "TileUpdata";
                builder.TaskEntryPoint = "FridayBgTask.TileUpdate";
                builder.SetTrigger(new TimeTrigger(15, false));
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                var task = builder.Register();
            }
            catch (Exception)
            {

            }
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                mainFrame.Navigate(typeof(Views.MainPages.CoursePage));
                SetFootBtnStyle(2);
                //await Class.Model.CourseManager.CopyCourseBySuperId("19882919", "2016", "1");
                //Class.Tools.ShowMsgAtFrame("登录成功");
            }
            if (e.NavigationMode == NavigationMode.Back&& mainFrame.Content == null)
            {
                this.NavigationCacheMode = NavigationCacheMode.Enabled;
                mainFrame.Navigate(typeof(Views.MainPages.CoursePage));
                SetFootBtnStyle(2);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New&&mainFrame.Content.GetType()==typeof(Views.MainPages.CoursePage))
            {
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
        }

        private async void FootBtnClicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (mainFrame.CanGoForward) mainFrame.GoForward();
            switch (btn.TabIndex)
            {
                case 0:
                    mainFrame.Navigate(typeof(Views.MainPages.SocialPage));
                    SetFootBtnStyle(btn.TabIndex);
                    //this.NavigationCacheMode = NavigationCacheMode.Enabled;
                    break;
                case 1:
                    mainFrame.Navigate(typeof(Views.MainPages.PlaygroundPage));
                    SetFootBtnStyle(btn.TabIndex);
                    //this.NavigationCacheMode = NavigationCacheMode.Enabled;
                    break;
                case 2:
                    mainFrame.Navigate(typeof(Views.MainPages.CoursePage));
                    SetFootBtnStyle(btn.TabIndex);
                    //this.NavigationCacheMode = NavigationCacheMode.Disabled;
                    break;
                case 3:
                    await new Windows.UI.Popups.MessageDialog("暂不支持XMPP").ShowAsync();
                    break;
                case 4:
                    mainFrame.Navigate(typeof(Views.MainPages.MyDataPage));
                    SetFootBtnStyle(btn.TabIndex);
                    break;
                default:
                    break;
            }
        }

        private void SetFootBtnStyle(int index)
        {
            var btn0 = FootBtnView.Children[0] as Button;
            var btn1 = FootBtnView.Children[1] as Button;
            var btn2 = FootBtnView.Children[2] as Button;
            var btn3 = FootBtnView.Children[3] as Button;
            var btn4 = FootBtnView.Children[4] as Button;
            btn0.Style = (Style)Resources["social_normal_btn"];
            btn1.Style = (Style)Resources["playground_normal_btn"];
            btn2.Style = (Style)Resources["course_normal_btn"];
            btn3.Style = (Style)Resources["papers_normal_btn"];
            btn4.Style = (Style)Resources["settings_normal_btn"];
            switch (index)
            {
                case 0:
                    btn0.Style = (Style)Resources["social_pressed_btn"];
                    break;
                case 1:
                    btn1.Style = (Style)Resources["playground_pressed_btn"];
                    break;
                case 2:
                    btn2.Style = (Style)Resources["course_pressed_btn"];
                    break;
                case 3:
                    btn3.Style = (Style)Resources["papers_pressed_btn"];
                    break;
                case 4:
                    btn4.Style = (Style)Resources["settings_pressed_btn"];
                    break;
                default:
                    break;
            }
        }
    }
}
