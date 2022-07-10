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
    public sealed partial class SettingsPage : Page
    {

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    MainPage mainPage = new MainPage();
                    mainPage.backButton.Visibility = Visibility.Visible;
                    mainPage.forwardButton.Visibility = Visibility.Visible;
                    mainPage.refreshButton.Visibility = Visibility.Visible;
                }
                else
                {
                    MainPage mainPage = new MainPage();
                    mainPage.backButton.Visibility = Visibility.Collapsed;
                    mainPage.forwardButton.Visibility = Visibility.Collapsed;
                    mainPage.refreshButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        // Command Bar
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage));
        }

        private async void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog aboutDialog = new AboutDialog();
            await aboutDialog.ShowAsync();
        }
    }
}
