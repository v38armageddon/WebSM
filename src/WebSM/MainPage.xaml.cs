using HtmlAgilityPack;
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

            // Init WebView2
            await webView2.EnsureCoreWebView2Async();
            webView2.Source = new Uri("https://www.bing.com");
            webView2.NavigationStarting += webView2_NavigationStarting;
            webView2.NavigationCompleted += webView2_NavigationCompleted;

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
            });
            progressRing.IsActive = false;
        }

        private async void webView2_NavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        {
            progressRing.IsActive = true;
            await Task.Delay(0);
        }

        private void webView2_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            progressRing.IsActive = false;
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
            }
            else
            {
                NavigationViewItem item = args.SelectedItem as NavigationViewItem;

                switch (item.Tag)
                {
                    case "home":
                        webView2.Source = new Uri("https://www.bing.com");
                        break;
                    case "openEmbedBrowser":
                        embedBrowser.IsPaneOpen = true;
                        break;
                    case "Favourite":
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

        private async void searchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchDialog searchDialog = new SearchDialog();
            await searchDialog.ShowAsync();
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

        private void accessLink_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void pinEmbedBrowser_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
