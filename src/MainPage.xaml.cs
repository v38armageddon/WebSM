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
                        webView.Navigate("https://www.youtube.com");
                        webView.Refresh();
                        break;
                    case "Twitch":
                        webView.Navigate("https://www.twitch.tv");
                        webView.Refresh();
                        break;
                    case "Discord":
                        webView.Navigate("https://discord.com/channels/@me");
                        webView.Refresh();
                        break;
                    case "Twitter":
                        webView.Navigate("https://twitter.com/home");
                        webView.Refresh();
                        break;
                    case "Reddit":
                        webView.Navigate("https://www.reddit.com");
                        webView.Refresh();
                        break;
                    case "Spotify":
                        webView.Navigate("https://open.spotify.com");
                        webView.Refresh();
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
            webView.GoBack();
        }

        private void forwardButton_Click(object sender, RoutedEventArgs e)
        {
            webView.GoForward();
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            webView.Refresh();
        }
    }
}
