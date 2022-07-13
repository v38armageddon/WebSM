using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
/* Uncomment when the issue is solved
 * using WebSM_SQLite_Database;
*/
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
    public sealed partial class NewSM : ContentDialog
    {
        public NewSM()
        {
            this.InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (textBox1.Text == "")
            {
                this.Hide();
                ContentDialog errorNoShortcut = new ErrorNoShortcut();
                await errorNoShortcut.ShowAsync();
            }
            else if (textBox2.Text == "")
            {
                this.Hide();
                ContentDialog errorNoURL = new ErrorNoURL();
                await errorNoURL.ShowAsync();
            }
            else
            {
                MainPage mainPage = new MainPage();
                mainPage.navView.MenuItems.Add(new NavigationViewItem() { Content = textBox1.Text, Icon = new SymbolIcon(Symbol.Globe) });
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Close Dialog
        }
    }
}
