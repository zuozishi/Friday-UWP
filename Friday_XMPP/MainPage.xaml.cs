using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.XMPP;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=391641 上有介绍

namespace Friday_XMPP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: 准备此处显示的页面。

            // TODO: 如果您的应用程序包含多个页面，请确保
            // 通过注册以下事件来处理硬件“后退”按钮:
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed 事件。
            // 如果使用由某些模板提供的 NavigationHelper，
            // 则系统会为您处理该事件。
            if (e.Parameter != null)
            {
                var datadir = e.Parameter as Dictionary<string, string>;
                var XmppClient = new XMPPClient
                {
                    UserName = datadir["UserName"],
                    Password = datadir["Password"],
                    Server = "chat-ej.myfriday.cn",
                    Domain = "chat-ej.myfriday.cn",
                    Resource = "Smack",
                    Port = 5222,
                    UseTLS = true,
                    AutoReconnect = true,
                    AutoAcceptPresenceSubscribe = false
                };
                XmppClient.OnStateChanged += XmppClient_OnStateChanged;
                XmppClient.MessageReceive += XmppClient_MessageReceive;
            }
            else
            {
                textblock.Text = "没有信息";
            }
        }

        private void XmppClient_MessageReceive(object sender, Message e)
        {
            textblock.Text = e.InnerXML;
        }

        private void XmppClient_OnStateChanged(object sender, XMPPState e)
        {
            textblock.Text = e.ToString();
        }

        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            e.Handled = true;
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else
            {
                App.Current.Exit();
            }
        }
    }
}
