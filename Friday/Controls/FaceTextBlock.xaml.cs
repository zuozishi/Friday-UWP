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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Friday.Controls
{
    public sealed partial class FaceTextBlock : UserControl
    {
        public new Brush Foreground
        {
            get
            {
                return (Brush)GetValue(ForegroundProperty);
            }
            set
            {
                SetValue(ForegroundProperty, value);
            }
        }
        public static readonly new DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Brush), typeof(FaceTextBlock), new PropertyMetadata(default(Brush)));
        public TextWrapping TextWrapping
        {
            get
            {
                return (TextWrapping)GetValue(TextWrappingProperty);
            }
            set
            {
                SetValue(TextWrappingProperty, value);
            }
        }
        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(FaceTextBlock), new PropertyMetadata(default(TextWrapping)));
        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(FaceTextBlock), new PropertyMetadata(default(string)));

        public Windows.UI.Xaml.Documents.Paragraph ResultUI
        {
            get
            {
                var result = new Windows.UI.Xaml.Documents.Paragraph();
                if (Text.Contains("$"))
                {
                    var strs = Text.Split('$');
                    foreach (var item in strs)
                    {
                        var image = Class.Model.FaceIcon.GetFaceImage(item);
                        if (image != null)
                        {
                            var uiconter = new Windows.UI.Xaml.Documents.InlineUIContainer();
                            uiconter.Child = image;
                        }
                        else
                        {
                            result.Inlines.Add(new Windows.UI.Xaml.Documents.Run() { Text = item });
                        }
                    }
                }
                else
                {
                    result.Inlines.Add(new Windows.UI.Xaml.Documents.Run() { Text = Text });
                }
                return result;
            }
        }

        public FaceTextBlock()
        {
            this.InitializeComponent();
        }

        private Windows.UI.Xaml.Documents.Paragraph LoadText(string value)
        {
            var result = new Windows.UI.Xaml.Documents.Paragraph();
            if (value.Contains("$"))
            {
                var strs = value.Split('$');
                foreach (var item in strs)
                {
                    var image = Class.Model.FaceIcon.GetFaceImage(item);
                    if (image != null)
                    {
                        var uiconter = new Windows.UI.Xaml.Documents.InlineUIContainer();
                        uiconter.Child = image;
                    }
                    else
                    {
                        result.Inlines.Add(new Windows.UI.Xaml.Documents.Run() { Text = item });
                    }
                }
            }
            else
            {
                result.Inlines.Add(new Windows.UI.Xaml.Documents.Run() { Text = value });
            }
            return result;
        }

        private string GetText()
        {
            string result = "";
            if (textpatagraph.Inlines != null && textpatagraph.Inlines.Count > 0)
            {
                foreach (var item in textpatagraph.Inlines)
                {
                    if ((item as Windows.UI.Xaml.Documents.Run) != null)
                    {
                        result = result + (item as Windows.UI.Xaml.Documents.Run).Text;
                    }
                    if ((item as Windows.UI.Xaml.Documents.InlineUIContainer) != null)
                    {
                        var image = (item as Windows.UI.Xaml.Documents.InlineUIContainer).Child as Image;
                        var source = (image.Source as Windows.UI.Xaml.Media.Imaging.BitmapImage).UriSource.ToString();
                        result = result + Class.Model.FaceIcon.GetFaceText(source);
                    }
                }
            }
            return result;
        }
    }
}
