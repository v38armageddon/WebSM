using Microsoft.Web.WebView2;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace WebSM
{
    public sealed partial class MainPage : Page
    {
        
        public MainPage()
        {
            this.InitializeComponent();
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values["Theme"];
            if (composite != null)
            {
                String theme = composite["Theme"] as string;
            }
        }

        private void webView2_NavigationStarting(Microsoft.UI.Xaml.Controls.WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        {
            // Fix Google connection blocked due to UserAgent, see https://github.com/MicrosoftEdge/WebView2Feedback/issues/1647
            var settings = webView2.CoreWebView2.Settings;
            if (webView2.Source.ToString().Contains("https://accounts.google.com"))
            {
                settings.UserAgent = GetMobileUserAgent();
            }
        }

        private string GetMobileUserAgent()
        {
            return "Chrome";
        }

        public void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                settingsView.IsPaneOpen = true;
            }
            else
            {
                NavigationViewItem Item = args.SelectedItem as NavigationViewItem;
                string sURL = webView2.Source.ToString();
                switch (Item.Tag)
                {
                    case "YouTube":
                        if (sURL.StartsWith("https://www.youtube.com"))
                        {
                            webView2.CoreWebView2.Resume();
                        }
                        else
                        {
                            webView2.CoreWebView2.Navigate("https://www.youtube.com");
                        }
                        break;
                    case "Twitch":
                        if (sURL.StartsWith("https://www.twitch.tv"))
                        {
                            webView2.CoreWebView2.Resume();
                        }
                        else
                        {
                            webView2.CoreWebView2.Navigate("https://www.twitch.tv");
                        }
                        break;
                    case "Discord":
                        if (sURL.StartsWith("https://discord.com/channels"))
                        {
                            webView2.CoreWebView2.Resume();
                        }
                        else
                        {
                            webView2.CoreWebView2.Navigate("https://discord.com/channels/@me");
                        }
                        break;
                    case "Twitter":
                        if (sURL.StartsWith("https://twitter.com"))
                        {
                            webView2.CoreWebView2.Resume();
                        }
                        else
                        {
                            webView2.CoreWebView2.Navigate("https://twitter.com/home");
                        }
                        break;
                    case "Reddit":
                        if (sURL.StartsWith("https://www.reddit.com"))
                        {
                            webView2.CoreWebView2.Resume();
                        }
                        else
                        {
                            webView2.CoreWebView2.Navigate("https://www.reddit.com");
                        }
                        break;
                    case "Spotify":
                        if (sURL.StartsWith("https://open.spotify.com"))
                        {
                            webView2.CoreWebView2.Resume();
                        }
                        else
                        {
                            webView2.CoreWebView2.Navigate("https://open.spotify.com");
                        }
                        break;
                }
            }
        }

        private async void openWindowButton_Click(object sender, RoutedEventArgs e)
        {
            var currentAV = ApplicationView.GetForCurrentView();
            var newAV = CoreApplication.CreateNewView();
            await newAV.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                async () =>
                {
                    var newWindow = Window.Current;
                    var newAppView = ApplicationView.GetForCurrentView();
                    var frame = new Frame();
                    
                    frame.Navigate(typeof(MainPage), null);
                    newWindow.Content = frame;
                    newWindow.Activate();
                    
                    await ApplicationViewSwitcher.TryShowAsStandaloneAsync(
                        newAppView.Id,
                        ViewSizePreference.UseMinimum,
                        currentAV.Id,
                        ViewSizePreference.UseMinimum);
                });
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            webView2.GoBack();
        }

        private void forwardButton_Click(object sender, RoutedEventArgs e)
        {
            webView2.GoForward();
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            webView2.Reload();
        }

        // Settings
        public void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.ApplicationDataCompositeValue composite = new Windows.Storage.ApplicationDataCompositeValue();
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    openWindowButton.Visibility = Visibility.Visible;
                    backButton.Visibility = Visibility.Visible;
                    forwardButton.Visibility = Visibility.Visible;
                    refreshButton.Visibility = Visibility.Visible;
                }
                else
                {
                    openWindowButton.Visibility = Visibility.Collapsed;
                    backButton.Visibility = Visibility.Collapsed;
                    forwardButton.Visibility = Visibility.Collapsed;
                    refreshButton.Visibility = Visibility.Collapsed;
                }
            }
        }
        
        public void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.ApplicationDataCompositeValue composite = new Windows.Storage.ApplicationDataCompositeValue();
            if (comboBox.SelectedIndex == 0)
            {
                this.RequestedTheme = ElementTheme.Default;
                string theme = App.Current.RequestedTheme.ToString();
                composite["Theme"] = theme;
                localSettings.Values["Theme"] = composite;
            }
            else if (comboBox.SelectedIndex == 1)
            {
                this.RequestedTheme = ElementTheme.Light;
                string theme = App.Current.RequestedTheme.ToString();
                composite["Theme"] = theme;
                localSettings.Values["Theme"] = composite;
            }
            else if (comboBox.SelectedIndex == 2)
            {
                this.RequestedTheme = ElementTheme.Dark;
                string theme = App.Current.RequestedTheme.ToString();
                composite["Theme"] = theme;
                localSettings.Values["Theme"] = composite;
            }
        }

        private void webDevButton_Click(object sender, RoutedEventArgs e)
        {
            webView2.CoreWebView2.OpenDevToolsWindow();
        }

        private async void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog aboutDialog = new AboutDialog();
            await aboutDialog.ShowAsync();
        }
    }
}
