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

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Friday.Controls
{
    public sealed partial class DialogBox : ContentDialog
    {
        public TextBlock mainTextBlock { get; set; }
        public Grid mainDialogGrid { get; set; }
        public DialogBox()
        {
            this.InitializeComponent();
            mainTextBlock = mainText;
            mainDialogGrid = mainGrid;
        }
    }
}
