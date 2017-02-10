using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Friday
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static string ProtocolUri = string.Empty;
        DispatcherTimer exitTimer;
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = false;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    Edi.UWP.Helpers.UI.SetWindowsMobileStatusBarColor(Colors.White, Color.FromArgb(255, 7, 153, 252));
                    if (Class.UserManager.islogin)
                    {
                        if(!Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("coursebg"))
                        {
                            Windows.Storage.ApplicationData.Current.LocalSettings.Values["coursebg"] = "ms-appx:///Assets/images/CoursePics/1.png";
                        }
                        rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    }
                    else
                    {
                        rootFrame.Navigate(typeof(Views.UserPages.LoginPage), e.Arguments);
                    }
                }
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            if(args.Kind==ActivationKind.Protocol)
            {
                ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;
                ProtocolUri = eventArgs.Uri.Query;
            }
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                Edi.UWP.Helpers.UI.SetWindowsMobileStatusBarColor(Colors.White, Color.FromArgb(255, 7, 153, 252));
                if (Class.UserManager.islogin)
                {
                    if (!Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("coursebg"))
                    {
                        Windows.Storage.ApplicationData.Current.LocalSettings.Values["coursebg"] = "ms-appx:///Assets/images/CoursePics/1.png";
                    }
                    rootFrame.Navigate(typeof(MainPage));
                }
                else
                {
                    rootFrame.Navigate(typeof(Views.UserPages.LoginPage));
                }
            }
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
            Window.Current.Activate();
        }

        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null) return;
            if (rootFrame.CurrentSourcePageType == typeof(Views.WebViewPage)) return;
            if (rootFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }else
            {
                e.Handled = true;
                if (exitTimer == null)
                {
                    Class.Tools.ShowMsgAtFrame("再按一次退出", 1500);
                    exitTimer = new DispatcherTimer();
                    exitTimer.Interval = TimeSpan.FromMilliseconds(1500);
                    exitTimer.Tick += ExitTimer_Tick;
                    exitTimer.Start();
                }else
                {
                    Exit();
                }
            }
        }

        private void ExitTimer_Tick(object sender, object e)
        {
            exitTimer = null;
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
