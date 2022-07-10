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
using Microsoft.Web.WebView2;

namespace WebSM
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                NavigationViewItem Item = args.SelectedItem as NavigationViewItem;

                switch (Item.Tag)
                {
                    case "YouTube":
                        webView2.CoreWebView2.Navigate("https://www.youtube.com");
                        break;
                    case "Twitch":
                        webView2.CoreWebView2.Navigate("https://www.twitch.tv");
                        break;
                    case "Discord":
                        webView2.CoreWebView2.Navigate("https://discord.com/channels/@me");
                        break;
                    case "Twitter":
                        webView2.CoreWebView2.Navigate("https://twitter.com/home");
                        break;
                    case "Reddit":
                        webView2.CoreWebView2.Navigate("https://www.reddit.com");
                        break;
                    case "Spotify":
                        webView2.CoreWebView2.Navigate("https://open.spotify.com");
                        break;
                }
            }
        }

        /* For a futur update
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(NewSM));
        }
        */
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
    }
}
