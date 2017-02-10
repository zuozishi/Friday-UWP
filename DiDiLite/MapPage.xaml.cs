using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace DiDiLite
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MapPage : Page
    {
        public static MapIcon myPosition;
        bool cansend = false;
        public MapPage()
        {
            this.InitializeComponent();
            this.Transitions = new Windows.UI.Xaml.Media.Animation.TransitionCollection();
            this.Transitions.Add(new Windows.UI.Xaml.Media.Animation.PaneThemeTransition { Edge = EdgeTransitionLocation.Right });
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set your current location.
            var accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:

                    // Get the current location.
                    Geolocator geolocator = new Geolocator();
                    Geoposition pos = await geolocator.GetGeopositionAsync();
                    Geopoint myLocation = pos.Coordinate.Point;

                    // Set the map location.
                    mapView.Center = myLocation;
                    mapView.ZoomLevel = 16.5;
                    mapView.LandmarksVisible = true;
                    myPosition = new MapIcon();
                    myPosition.Location = myLocation;
                    myPosition.NormalizedAnchorPoint = new Point(0.5, 1.0);
                    myPosition.ZIndex = 0;
                    myPosition.Title = "我的位置";
                    mapView.MapElements.Add(myPosition);
                    break;

                case GeolocationAccessStatus.Denied:
                    // Handle the case  if access to location is denied.
                    break;

                case GeolocationAccessStatus.Unspecified:
                    // Handle the case if  an unspecified error occurs.
                    break;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if(!cansend) myPosition = null;
        }

        private void BackBtnVlicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void mapView_MapTapped(MapControl sender, MapInputEventArgs args)
        {
            if (myPosition == null)
            {
                myPosition = new MapIcon();
                myPosition.Location = args.Location;
                myPosition.NormalizedAnchorPoint = new Point(0.5, 1.0);
                myPosition.ZIndex = 0;
                myPosition.Title = "我的位置";
                mapView.MapElements.Add(myPosition);
            }
            else
            {
                myPosition.Location = args.Location;
            }
        }

        private void AcceptBtnVlicked(object sender, RoutedEventArgs e)
        {
            cansend = true;
            Frame.GoBack();
        }
    }
}
