/*
 * WebSM Lite - A simply minimalist web browser.
 * Copyright (C) 2022 - 2024 - v38armageddon
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
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
using System.Net.Http;
using System.Threading;
using Windows.UI.Xaml.Media.Animation;
using Newtonsoft.Json;
using System.Diagnostics;
#endregion

namespace WebSM.Lite
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            var settings = LoadSettingsJSON();
            if (settings != null && settings.ContainsKey("Theme"))
            {
                int themeValue = Convert.ToInt32(settings["Theme"]);
                switch (themeValue)
                {
                    case 0:
                        RequestedTheme = ElementTheme.Default;
                        break;
                    case 1:
                        RequestedTheme = ElementTheme.Light;
                        break;
                    case 2:
                        RequestedTheme = ElementTheme.Dark;
                        break;
                    default:
                        Debug.WriteLine("Invalid value Theme.");
                        break;
                }
            }
            InitializeComponent();
            ApplySettings(settings);
            webView2.CoreWebView2Initialized += WebView2_CoreWebView2InitializationCompleted;
        }

        private void WebView2_CoreWebView2InitializationCompleted(WebView2 sender, CoreWebView2InitializedEventArgs args)
        {
            webView2.CoreWebView2.NewWindowRequested += webView2_NewWindowRequested;
        }

        private async void webView2_NewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
        {
            args.Handled = true; // Prevent the default behavior of opening a new window
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                webView2.Source = new Uri(args.Uri);
            });
        }

        private void webView2_NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            progressRing.IsActive = true;
        }

        private void webView2_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            progressRing.IsActive = false;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void navView_BackRequested(Windows.UI.Xaml.Controls.NavigationView sender, Windows.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args)
        {
            webView2.GoBack();
        }

        private void navView_SelectionChanged(Windows.UI.Xaml.Controls.NavigationView sender, Windows.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                settingsView.IsPaneOpen = true;
                navView.SelectedItem = null;
            }
            NavigationViewItem item = args.SelectedItem as Windows.UI.Xaml.Controls.NavigationViewItem;
            if (item != null && item.Tag != null)
            {
                switch (item.Tag.ToString())
                {
                    case "Downloads":
                        webView2.CoreWebView2.OpenDefaultDownloadDialog();
                        break;
                }
            }
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            webView2.Source = new Uri("https://www.bing.com");
        }

        private async void searchButton_Click(object sender, RoutedEventArgs e)
        {
            Dialogs.SearchDialog searchDialog = new Dialogs.SearchDialog();
            await searchDialog.ShowAsync();
            // Maybe move this part of code to the SearchDialog class?
            if (String.IsNullOrEmpty(searchDialog.searchTextBox.Text))
            {
                return;
            }
            if (searchDialog.searchTextBox.Text.Contains("://"))
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
        private void ApplySettings(Dictionary<string, object> settings)
        {
            if (settings.ContainsKey("Theme"))
            {
                int themeValue = Convert.ToInt32(settings["Theme"]);
                comboBox1.SelectedIndex = themeValue;
            }
        }

        private Dictionary<string, object> LoadSettingsJSON()
        {
            string filePath = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "settings.json");
            if (!File.Exists(filePath))
            {
                var settingsCreation = new Dictionary<string, object>
                {
                    { "Theme", (int)ElementTheme.Default } // 0 = Default, 1 = Light, 2 = Dark
                };
                string jsonCreation = JsonConvert.SerializeObject(settingsCreation, Formatting.Indented);
                File.WriteAllText(filePath, jsonCreation);
            }
            string json = File.ReadAllText(filePath);
            Dictionary<string, object> settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return settings;
        }

        public void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox1.SelectedIndex == 0) // <- Default theme from the system
            {
                RequestedTheme = ElementTheme.Default;
                SettingsTheme.SetDefaultTheme();
            }
            else if (comboBox1.SelectedIndex == 1) // <- Light theme
            {
                RequestedTheme = ElementTheme.Light;
                SettingsTheme.SetLightTheme();
            }
            else if (comboBox1.SelectedIndex == 2) // <- Dark theme
            {
                RequestedTheme = ElementTheme.Dark;
                SettingsTheme.SetDarkTheme();
            }
        }

        private async void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            Dialogs.AboutDialog aboutDialog = new Dialogs.AboutDialog();
            await aboutDialog.ShowAsync();
        }
    }
}
