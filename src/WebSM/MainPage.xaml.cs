using Microsoft.UI.Xaml.Controls;
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
#region aliases
using NavigationView = Windows.UI.Xaml.Controls.NavigationView;
using NavigationViewBackRequestedEventArgs = Windows.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs;
using NavigationViewSelectionChangedEventArgs = Windows.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs;
using NavigationViewItem = Windows.UI.Xaml.Controls.NavigationViewItem;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Web.WebView2.Core;
using System.Net.Http;
#endregion

namespace WebSM
{
    public sealed partial class MainPage : Page
    {
        private Dictionary<int, WebView2> tabViewTabItems = new Dictionary<int, WebView2>();
        WebView2 webView2 = new WebView2();

        public MainPage()
        {
            this.InitializeComponent();
            LoadSettings();
        }

        private void SaveSettings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            localSettings.Values["ComboBoxValue"] = comboBox1.SelectedIndex;
        }


        private void LoadSettings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            // Load theme
            if (localSettings.Values.ContainsKey("ComboBoxValue"))
            {
                int selectedIndex = (int)localSettings.Values["ComboBoxValue"];
                comboBox1.SelectedIndex = selectedIndex;
            }
        }


        private async void TabView_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 1; i++)
            {
                await CreateNewTabAsync(i);
                tabView.SelectedIndex = i;
            }
        }

        private async void TabView_AddButtonClick(TabView sender, object args)
        {
            await CreateNewTabAsync(sender.TabItems.Count);
        }

        private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            sender.TabItems.Remove(args.Tab);
            if (sender.TabItems.Count == 0)
            {
                Application.Current.Exit();
            }
        }

        private async Task CreateNewTabAsync(int index)
        {
            progressRing.IsActive = true;
            TabViewItem newItem = new TabViewItem();

            if (tabViewTabItems.ContainsKey(index))
            {
                webView2 = tabViewTabItems[index];
            }
            else
            {
                webView2 = new WebView2();

                // Init WebView2
                await webView2.EnsureCoreWebView2Async();
                webView2.Source = new Uri("https://www.bing.com");
                webView2.NavigationStarting += webView2_NavigationStarting;
                webView2.NavigationCompleted += webView2_NavigationCompleted;

                tabViewTabItems.Add(index, webView2);
            }

            var search_dialog = new SearchDialog();

            newItem.Content = webView2;
            string pageTitle = await webView2.CoreWebView2.ExecuteScriptAsync("document.title");
            if (pageTitle != null)
            {
                newItem.Header = pageTitle;
            }

            string faviconUrl = await webView2.CoreWebView2.ExecuteScriptAsync("document.querySelector('link[rel~=\"icon\"]')?.href || document.querySelector('link[rel~=\"shortcut icon\"]')?.href");
            Uri iconUri;
            if (!string.IsNullOrEmpty(faviconUrl) && Uri.TryCreate(faviconUrl, UriKind.Absolute, out iconUri))
            {
                newItem.IconSource = new Microsoft.UI.Xaml.Controls.BitmapIconSource() { UriSource = iconUri };
            }
            else
            {
                newItem.IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Globe };
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tabView.TabItems.Add(newItem);
                tabView.SelectedIndex = index;
            });
            progressRing.IsActive = false;
        }

        private async void webView2_NavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        {
            progressRing.IsActive = true;
            await Task.Delay(0);
        }

        private async void webView2_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            progressRing.IsActive = false;
            TabViewItem tabItem = tabView.SelectedItem as TabViewItem;
            string pageTitle = await webView2.CoreWebView2.ExecuteScriptAsync("document.title");
            if (pageTitle != null)
            {
                tabItem.Header = pageTitle;
            }

            string faviconUrl = await webView2.CoreWebView2.ExecuteScriptAsync("document.querySelector('link[rel~=\"icon\"]')?.href || document.querySelector('link[rel~=\"shortcut icon\"]')?.href");
            Uri iconUri;
            if (!string.IsNullOrEmpty(faviconUrl) && Uri.TryCreate(faviconUrl, UriKind.Absolute, out iconUri))
            {
                tabItem.IconSource = new Microsoft.UI.Xaml.Controls.BitmapIconSource() { UriSource = iconUri };
            }
            else
            {
                tabItem.IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Globe };
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void navView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            webView2.GoBack();
        }

        private void navView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                settingsView.IsPaneOpen = true;
                navView.SelectedItem = null;
            }
            else
            {
                NavigationViewItem item = args.SelectedItem as NavigationViewItem;

                switch (item.Tag)
                {
                    // Add here all the favorites links
                }
            }
        }

        private void openEmbedBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            embedBrowser.IsPaneOpen = true;
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

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            webView2.Source = new Uri("https://www.bing.com");
        }

        private async void searchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchDialog searchDialog = new SearchDialog();
            await searchDialog.ShowAsync();
            if (String.IsNullOrEmpty(searchDialog.searchTextBox.Text))
            {
                return;
            }
            if (searchDialog.searchTextBox.Text.StartsWith("https://") || searchDialog.searchTextBox.Text.StartsWith("http://"))
            {
                webView2.Source = new Uri(searchDialog.searchTextBox.Text);
            }
            else if (searchDialog.searchTextBox.Text.Contains("."))
            {
                webView2.Source = new Uri("https://" + searchDialog.searchTextBox.Text);
            }
            else
            {
                webView2.Source = new Uri("https://www.bing.com/search?q=" + searchDialog.searchTextBox.Text);
            }
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
            SaveSettings();
        }

        private void userAgentSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (userAgentSwitch.IsOn == true)
            {
                ChangeUserAgent();
            }
            else
            {
                ResetUserAgent();
            }
        }

        private void ChangeUserAgent()
        {
            webView2.CoreWebView2.Settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.5666.197 Safari/537.36";
        }

        private async void ResetUserAgent()
        {
            WebView2 defaultWebView2 = new WebView2();
            await defaultWebView2.EnsureCoreWebView2Async(); // We need to create a new WebView2 to get the default UserAgent
            webView2.CoreWebView2.Settings.UserAgent = defaultWebView2.CoreWebView2.Settings.UserAgent;
            webView2.Reload();
        }

        private async void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog aboutDialog = new AboutDialog();
            await aboutDialog.ShowAsync();
        }

        // Embed browser
        private void closeEmbedBrowser_Click(object sender, RoutedEventArgs e)
        {
            embedBrowser.IsPaneOpen = false;
        }

        private async void accessLink_Click(object sender, RoutedEventArgs e)
        {
            SearchDialog searchDialog = new SearchDialog();
            await searchDialog.ShowAsync();
            if (searchDialog.searchTextBox.Text.StartsWith("https://") || searchDialog.searchTextBox.Text.StartsWith("http://"))
            {
                embedWebView2.Source = new Uri(searchDialog.searchTextBox.Text);
            }
            else
            {
                embedWebView2.Source = new Uri("https://www.bing.com/search?q=" + searchDialog.searchTextBox.Text);
            }
        }

        private void pinEmbedBrowser_Click(object sender, RoutedEventArgs e)
        {
            if (pinEmbedBrowser.IsChecked == true)
            {
                embedBrowser.DisplayMode = SplitViewDisplayMode.Inline;
            }
            else
            {
                embedBrowser.DisplayMode = SplitViewDisplayMode.Overlay;
            }
        }
    }
}
