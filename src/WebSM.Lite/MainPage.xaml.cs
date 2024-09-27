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
#endregion

namespace WebSM.Lite
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            // Detect if the app is called via a protocol
            if (Application.Current is App app)
            {
                if (app.URL != null)
                {
                    webView2.Source = new Uri(app.URL);
                }
                webView2.Source = new Uri("https://www.bing.com");
            }

            // Load the settings
            string filePath = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "settings.json");
            string json = File.ReadAllText(filePath);
            Dictionary<string, object> settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            // Apply the settings to the app
            switch (settings["Theme"])
            {
                case 0:
                    RequestedTheme = ElementTheme.Default;
                    comboBox1.SelectedIndex = 0;
                    break;
                case 1:
                    RequestedTheme = ElementTheme.Light;
                    comboBox1.SelectedIndex = 1;
                    break;
                case 2:
                    RequestedTheme = ElementTheme.Dark;
                    comboBox1.SelectedIndex = 2;
                    break;
                default:
                    throw new Exception("Settings loader bitching again!");
            }
            if ((bool)settings["FakeUserAgent"] == true)
            {
                userAgentSwitch.IsOn = true;
                ChangeUserAgent();
            }
        }

        private void webView2_NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            progressRing.IsActive = true;
            webView2.CoreWebView2.Settings.UserAgent = "WebSM/4.1 Lite Edition (Based on Microsoft WebView2)";
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

        // WARNING: Complicated code here! Not present on normal version!
        private void navView_SelectionChanged(Windows.UI.Xaml.Controls.NavigationView sender, Windows.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                settingsView.IsPaneOpen = true;
                navView.SelectedItem = null;
            }
            Windows.UI.Xaml.Controls.NavigationViewItem item = args.SelectedItem as Windows.UI.Xaml.Controls.NavigationViewItem;
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
                RequestedTheme = ElementTheme.Default;
                string filePath = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "settings.json");
                if (File.Exists(filePath)) // This should never fail, but if it fails, it's user's fault
                {
                    string json = File.ReadAllText(filePath);
                    Dictionary<string, object> settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    settings["Theme"] = 0;
                    string newJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    File.WriteAllText(filePath, newJson);
                }
            }
            else if (comboBox1.SelectedIndex == 1) // <- Light theme
            {
                RequestedTheme = ElementTheme.Light;
                string filePath = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "settings.json");
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    Dictionary<string, object> settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    settings["Theme"] = 1;
                    string newJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    File.WriteAllText(filePath, newJson);
                }
            }
            else if (comboBox1.SelectedIndex == 2) // <- Dark theme
            {
                RequestedTheme = ElementTheme.Dark;
                string filePath = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "settings.json");
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    Dictionary<string, object> settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    settings["Theme"] = 2;
                    string newJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    File.WriteAllText(filePath, newJson);
                }
            }
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

        private async void ChangeUserAgent()
        {
            WebView2 defaultWebView2 = new WebView2();
            await defaultWebView2.EnsureCoreWebView2Async(); // We need to create a new WebView2 to get the default UserAgent
            webView2.CoreWebView2.Settings.UserAgent = defaultWebView2.CoreWebView2.Settings.UserAgent;
            webView2.Reload();
            // Destroy the generated webView2
            defaultWebView2.Close();
            
        }

        private void ResetUserAgent()
        {
            webView2.CoreWebView2.Settings.UserAgent = "WebSM/4.1 Lite Edition (Based on Microsoft WebView2)";
        }

        private async void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            Dialogs.AboutDialog aboutDialog = new Dialogs.AboutDialog();
            await aboutDialog.ShowAsync();
        }
    }
}
