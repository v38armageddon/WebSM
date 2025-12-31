/*
 * WebSM - A simply minimalist web browser.
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
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.Web.WebView2.Core;

namespace WebSM
{
    public sealed partial class MainPage : Page
    {
        private Dictionary<int, WebView2> tabViewTabItems = new Dictionary<int, WebView2>();
        private Dictionary<int, TabViewItem> tabViewItems = new Dictionary<int, TabViewItem>();
        private int currentTabId;
        WebView2 webView2 = new WebView2();

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
            var tabItem = args.Tab as TabViewItem;
            int tabId = (int)tabItem.Tag;
            sender.TabItems.Remove(tabItem);
            tabViewTabItems.Remove(tabId);
            tabViewItems.Remove(tabId);

            if (sender.TabItems.Count == 0)
            {
                Application.Current.Exit();
            }
            else
            {
                var newSelectedTab = sender.TabItems[0] as TabViewItem;
                currentTabId = (int)newSelectedTab.Tag;
                webView2 = tabViewTabItems[currentTabId];
            }
        }

        private void tabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabViewItem? tabItem = tabView.SelectedItem as TabViewItem;
            currentTabId = (int)tabItem.Tag;
            webView2 = tabViewTabItems[currentTabId];
        }

        private async Task CreateNewTabAsync(int index)
        {
            progressRing.IsActive = true;
            TabViewItem newItem = new TabViewItem();

            int newIndex = GenerateUniqueID(); // Generate a unique ID for the new tab

            if (tabViewTabItems.ContainsKey(newIndex))
            {
                webView2 = tabViewTabItems[newIndex];
            }
            else
            {
                webView2 = new WebView2();

                // Init WebView2
                await webView2.EnsureCoreWebView2Async();
                webView2.Source = new Uri("https://eu.startpage.com/");
                webView2.NavigationStarting += webView2_NavigationStarting;
                webView2.NavigationCompleted += webView2_NavigationCompleted;
                webView2.CoreWebView2.NewWindowRequested += webView2_NewWindowRequested;

                tabViewTabItems.Add(newIndex, webView2);
            }

            newItem.Tag = newIndex;
            newItem.Content = webView2;
            tabViewItems.Add(newIndex, newItem);

            // Init Name of website and favicon
            string pageTitle = await webView2.CoreWebView2.ExecuteScriptAsync("document.title");
            pageTitle = pageTitle.Trim('"');
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
                currentTabId = newIndex;
            });
            progressRing.IsActive = false;
        }

        private int GenerateUniqueID()
        {
            Random random = new Random();
            int id = random.Next(0, int.MaxValue);
            while (tabViewTabItems.ContainsKey(id))
            {
                id = random.Next(0, int.MaxValue);
            }
            return id;
        }

        private async void webView2_NewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
        {
            args.Handled = true; // Prevent the default behavior of opening a new window
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await CreateNewTabAsync(tabView.TabItems.Count);
                webView2.Source = new Uri(args.Uri);
            });
        }

        private void webView2_NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            progressRing.IsActive = true;
        }

        private async void webView2_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            progressRing.IsActive = false;
            TabViewItem? tabItem = tabView.SelectedItem as TabViewItem;

            // Set the title to the tab
            string pageTitle = await webView2.CoreWebView2.ExecuteScriptAsync("document.title");
            if (!string.IsNullOrEmpty(pageTitle))
            {
                pageTitle = pageTitle.Trim('"'); // Remove the quotes from the title
                tabItem.Header = pageTitle;
            }

            // Set the favicon to the tab
            string faviconUrl = await webView2.CoreWebView2.ExecuteScriptAsync("document.querySelector('link[rel~=\"icon\"]')?.href || document.querySelector('link[rel~=\"shortcut icon\"]')?.href");
            faviconUrl = faviconUrl.Trim('"');
            Uri iconUri;
            if (!string.IsNullOrEmpty(faviconUrl) && Uri.TryCreate(faviconUrl, UriKind.Absolute, out iconUri))
            {
                tabItem.IconSource = new Microsoft.UI.Xaml.Controls.BitmapIconSource() { UriSource = iconUri, ShowAsMonochrome = false };
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
            NavigationViewItem? item = args.SelectedItem as NavigationViewItem;
            switch (item.Tag)
            {
                case "Downloads":
                    webView2.CoreWebView2.OpenDefaultDownloadDialog();
                    break;
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

            await newAV.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
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
            webView2.Source = new Uri("https://eu.startpage.com/");
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchFunction(false);
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

            if (settings.ContainsKey("UserAgent") && settings["UserAgent"] is bool userAgentValue)
            {
                userAgentSwitch.IsOn = userAgentValue;
            }
        }

        private Dictionary<string, object> LoadSettingsJSON()
        {
            string filePath = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "settings.json");
            if (!File.Exists(filePath))
            {
                var settingsCreation = new Dictionary<string, object>
                {
                    { "Theme", (int)ElementTheme.Default }, // 0 = Default, 1 = Light, 2 = Dark
                    { "UserAgent", false }
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
            webView2.CoreWebView2.Settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36 Edg/114.0.1788.0";
        }

        private async void ResetUserAgent()
        {
            WebView2 defaultWebView2 = new WebView2();
            await defaultWebView2.EnsureCoreWebView2Async(); // We need to create a new WebView2 to get the default UserAgent
            webView2.CoreWebView2.Settings.UserAgent = defaultWebView2.CoreWebView2.Settings.UserAgent;
            webView2.Reload();
            // Destroy the generated webView2
            defaultWebView2.Close();
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
            SearchFunction(true);
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

        // Search Function
        // TODO: Maybe optimize it by putting in a separate file?
        public async void SearchFunction(bool isEmbedSearch = false)
        {
            SearchDialog searchDialog = new SearchDialog();
            await searchDialog.ShowAsync();
            string input = searchDialog.searchTextBox.Text;
            WebView2 typeOfView = isEmbedSearch ? embedWebView2 : webView2;
            if (string.IsNullOrEmpty(input))
            {
                return;
            }
            switch (input)
            {
                case string s when s.StartsWith("https://") || s.StartsWith("http://"):
                    typeOfView.Source = new Uri(s);
                    break;
                case string s when s.Contains("."):
                    typeOfView.Source = new Uri("https://" + s);
                    break;
                default:
                    typeOfView.Source = new Uri("https://eu.startpage.com/search?q=" + input);
                    break;
            }
        }
    }
}
