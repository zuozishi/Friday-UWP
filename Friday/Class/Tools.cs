using LLM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace Friday.Class
{
    public class Tools
    {
        public static async void ShowMsgAtTitlebar(string msg,int time = 2000)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ProgressIndicator.Text = msg;
                await statusBar.ProgressIndicator.ShowAsync();
                await Task.Delay(time);
                await statusBar.ProgressIndicator.HideAsync();
            }
        }
        public static async void ShowMsgAtFrame(string msg, int time = 2000)
        {
            try
            {
                string xaml = "<Border Name=\"msgboxview\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x = \"http://schemas.microsoft.com/winfx/2006/xaml\" Margin =\"0,0,0,55\" Height=\"auto\" Visibility=\"Visible\" VerticalAlignment=\"Bottom\" CornerRadius=\"10\" HorizontalAlignment=\"Center\" Background=\"#7F000000\" ><TextBlock Foreground=\"White\" TextWrapping=\"WrapWholeWords\" VerticalAlignment=\"Center\" Margin=\"10,5\"><Run Text=\"{0}\"/></TextBlock></Border>";
                xaml = string.Format(xaml, msg);
                Border msgbox = (Border)XamlReader.Load(xaml);
                var mainFrame= Window.Current.Content as Frame;
                var page = mainFrame.Content as Page;
                var mainGrid = page.Content as Grid;
                mainGrid.Children.Add(msgbox);
                Animator.Use(AnimationType.FadeIn).PlayOn(msgbox);
                await Task.Delay(time);
                Animator.Use(AnimationType.FadeOutDown).PlayOn(msgbox);
                await Task.Delay(500);
                mainGrid.Children.Remove(msgbox);
            }
            catch (Exception)
            {
                
            }
        }
    }
}
