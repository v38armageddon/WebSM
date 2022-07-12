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

namespace WebSM
{
    public sealed partial class RemoveSM : ContentDialog
    {
        public RemoveSM()
        {
            this.InitializeComponent();
            MainPage mainPage = new MainPage();
            //selectedShortcut.Text = mainPage.navView.MenuItems.Select(mainPage.navView.SelectedItem);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            MainPage mainPage = new MainPage();
            mainPage.navView.MenuItems.Remove(mainPage.navView.SelectedItem);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Close Dialog
        }
    }
}
