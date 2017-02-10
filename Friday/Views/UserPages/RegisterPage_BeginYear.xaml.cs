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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Friday.Views.UserPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RegisterPage_BeginYear : Page
    {
        RegisterData regdata;
        public RegisterPage_BeginYear()
        {
            this.InitializeComponent();
            this.Transitions = new TransitionCollection();
            this.Transitions.Add(new PaneThemeTransition { Edge = EdgeTransitionLocation.Right });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            regdata = Data.Json.DataContractJsonDeSerialize<RegisterData>((string)e.Parameter);
            for (int i = 0; i < 16; i++)
            {
                yearListView.Items.Add(DateTime.Today.Year - i);
            }
        }

        private void GoBackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void yearSelected(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedItem != null)
            {
                var year = (int)list.SelectedItem;
                regdata.grade = (year % 2000).ToString();
                Frame.Navigate(typeof(RegisterPage_BandPhone), Data.Json.ToJsonData(regdata));
                list.SelectedIndex = -1;
            }
        }

        public class RegisterData
        {
            public string id { get; set; }
            public string grade { get; set; }
            public string academyId { get; set; }
            public string schoolId { get; set; }
        }
    }
}
