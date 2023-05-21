using Microsoft.Web.WebView2;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
using Microsoft.UI.Xaml.Controls;
#region aliases
using NavigationView = Windows.UI.Xaml.Controls.NavigationView;
using NavigationViewBackRequestedEventArgs = Windows.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs;
using NavigationViewSelectionChangedEventArgs = Windows.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs;
using NavigationViewItem = Windows.UI.Xaml.Controls.NavigationViewItem;
#endregion

namespace WebSM
{
    public sealed partial class MainPage : Page
    {
        WebView2 webView2 = new WebView2();

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void TabView_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 1; i++)
            {
                (sender as TabView).TabItems.Add(CreateNewTab(i));
            }
        }

        private void TabView_AddButtonClick(TabView sender, object args)
        {
            sender.TabItems.Add(CreateNewTab(sender.TabItems.Count));
        }

        private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            sender.TabItems.Remove(args.Tab);
        }

        private TabViewItem CreateNewTab(int index)
        {
            TabViewItem newItem = new TabViewItem();

            newItem.Header = $"Document {index}";
            newItem.IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Document };

            WebView2 webView = new WebView2();

            // Init WebView2
            webView.Source = new Uri("https://www.example.com");
            webView.NavigationStarting += webView2_NavigationStarting;

            newItem.Content = webView;

            return newItem;
        }

        private async void webView2_NavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        {
            progressRing.IsActive = true;
            GetMobileUserAgent();
            await Task.Delay(0);
        }

        private void webView2_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            progressRing.IsActive = false;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private string GetMobileUserAgent()
        {
            return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.5026.0 Safari/537.36 Edg/103.0.1254.0";
        }

        private void navView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            webView2.GoBack();
        }

        public void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                if (settingsView.IsPaneOpen == false)
                {
                    settingsView.IsPaneOpen = true;
                }
                else if (settingsView.IsPaneOpen == true)
                {
                    settingsView.IsPaneOpen = false;
                }
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
            if (comboBox1.SelectedIndex == 0) // <- Default theme from the system
            {
                this.RequestedTheme = ElementTheme.Default;
            }
            else if (comboBox1.SelectedIndex == 1) // <- Light theme
            {
                this.RequestedTheme = ElementTheme.Light;
            }
            else if (comboBox1.SelectedIndex == 2) // <- Dark theme
            {
                this.RequestedTheme = ElementTheme.Dark;
            }
        }

        private async void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog aboutDialog = new AboutDialog();
            await aboutDialog.ShowAsync();
        }
    }
}
