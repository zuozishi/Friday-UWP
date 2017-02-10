using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Friday.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PicsViewPage : Page
    {
        public PicsViewPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //LLM.Animator.Use(LLM.AnimationType.ZoomIn).PlayOn(this);
            var data = e.Parameter as string[];
            var pics = Class.Data.Json.DataContractJsonDeSerialize<List<string>>(data[0]);
            if (pics != null)
            {
                picsnum.Text = pics.Count().ToString();
                foreach (var item in pics)
                {
                    var view = new ScrollViewer();
                    view.ZoomMode = ZoomMode.Enabled;
                    view.HorizontalScrollMode = ScrollMode.Enabled;
                    view.VerticalScrollMode = ScrollMode.Enabled;
                    view.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                    view.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                    var image = new Image();
                    image.Tapped += Image_Tapped;
                    image.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(item));
                    view.Content = image;
                    flipview.Items.Add(view);
                }
                thispicnum.Text = "1";
                flipview.SelectionChanged += (sender,arg) =>
                {
                    var view = sender as FlipView;
                    thispicnum.Text = (view.SelectedIndex+1).ToString();
                };
                flipview.SelectedIndex = int.Parse(data[1]);
            }
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //LLM.Animator.Use(LLM.AnimationType.ZoomOut).SetDuration(TimeSpan.FromMilliseconds(800)).PlayOn(this);
            //await Task.Delay(800);
            Frame.GoBack();
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void SaveImage(object sender, RoutedEventArgs e)
        {
            var view = flipview.Items[flipview.SelectedIndex] as ScrollViewer;
            var image = view.Content as Image;
            var imgsource = image.Source as Windows.UI.Xaml.Media.Imaging.BitmapImage;
            if (Class.HttpPostUntil.isInternetAvailable)
            {
                var httpclient = new System.Net.Http.HttpClient();
                var bytes=await httpclient.GetByteArrayAsync(imgsource.UriSource.ToString());
                var foler = await KnownFolders.PicturesLibrary.CreateFolderAsync("保存的图片", CreationCollisionOption.OpenIfExists);
                var filename = DateTime.Now.ToFileTime().ToString();
                var file = await foler.CreateFileAsync(filename+".jpg",CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteBytesAsync(file, bytes);
                Class.Tools.ShowMsgAtFrame("图片已保存:"+file.Path);
            }
            else
            {
                Class.Tools.ShowMsgAtFrame("请检查网络连接");
            }
        }
    }
}
