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
using Newtonsoft.Json;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

namespace WebSM;

public sealed partial class MainPage : Page
{
    public BrowserPage browserPage;
    private Dictionary<string, object>? _settings;
    private bool embedWebView2Ready = false;

    public MainPage()
    {
        InitializeComponent();
        _settings = LoadSettingsJSON();
        ApplySettings(_settings);
        SetupOrientationHandling();
#if ANDROID || IOS
        openWindowButton.Visibility = Visibility.Collapsed; // Not useful in phone
#endif
    }
    #region SetupStart
    private void SetupOrientationHandling()
    {
        this.SizeChanged += Page_SizeChanged;
        UpdateLayoutForOrientation(this.ActualWidth, this.ActualHeight);
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Subscribe to frame navigation so we capture the BrowserPage instance when it's created.
        mainFrame.Navigated += MainFrame_Navigated;
        mainFrame.Navigate(typeof(BrowserPage));

        // Ensure UI-related settings (theme, control states) are applied when XamlRoot is available.
        ApplySettings(_settings);
    }

    private void Page_SizeChanged(object sender, SizeChangedEventArgs args)
    {
        UpdateLayoutForOrientation(args.NewSize.Width, args.NewSize.Height);
    }

    private void UpdateLayoutForOrientation(double width, double height)
    {
        bool isLandscape = width > height;

#if ANDROID || IOS
        if (isLandscape)
        {
            navView.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
            navView.IsPaneOpen = false;
            var commandBarMargin = commandBar.Margin;
            commandBarMargin.Left = 48;
            commandBar.Margin = commandBarMargin;
            commandBar.OverflowButtonVisibility = CommandBarOverflowButtonVisibility.Collapsed;
            navView.Margin = new Thickness(0);
            browserPage.tabView.TabWidthMode = TabViewWidthMode.Compact;
        }
        else
        {
            navView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
            var commandBarMargin = commandBar.Margin;
            commandBarMargin.Left = 0;
            commandBar.Margin = commandBarMargin;
            commandBar.OverflowButtonVisibility = CommandBarOverflowButtonVisibility.Visible;
            browserPage.tabView.TabWidthMode = TabViewWidthMode.Equal;
#if ANDROID
            var topInsetDp = GetStatusBarHeightDp();
            if (topInsetDp > 0)
            {
                commandBar.Margin = new Thickness(0, 0, 0, topInsetDp);
                navView.Margin = new Thickness(0, topInsetDp, 0, 0);
            }
            else
            {
                navView.Margin = new Thickness(0);
            }
#else
            navView.Margin = new Thickness(0);
#endif
        }
#endif

#if DESKTOP
        var commandBarMarginDesktop = commandBar.Margin;
        commandBarMarginDesktop.Left = 48;
        commandBar.Margin = commandBarMarginDesktop;
        browserPage.tabView.TabWidthMode = TabViewWidthMode.Equal;
#endif
    }

    // When the frame navigates, keep a reference to the BrowserPage so button actions work.
    private void MainFrame_Navigated(object sender, NavigationEventArgs e)
    {
        if (e.Content is BrowserPage bp)
        {
            browserPage = bp;
        }
        else
        {
            browserPage = null;
        }
    }

#if ANDROID
    private double GetStatusBarHeightDp()
    {
        try
        {
            var res = Android.App.Application.Context.Resources;
            int id = res.GetIdentifier("status_bar_height", "dimen", "android");
            if (id > 0)
            {
                var px = res.GetDimensionPixelSize(id);
                var density = res.DisplayMetrics.Density;
                return px / density;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"GetStatusBarHeightDp failed: {ex}");
        }

        return 0;
    }
#endif
    #endregion
    #region UINavigation
    private void navView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            settingsView.IsPaneOpen = true;
            navView.SelectedItem = null;
            return;
        }
    }

    private async void openEmbedBrowserButton_Click(object sender, RoutedEventArgs e)
    {
        if (!embedWebView2Ready)
        {
            await embedWebView2.EnsureCoreWebView2Async();
            embedWebView2Ready = true;
        }
        embedBrowser.IsPaneOpen = true;
    }

    private void openWindowButton_Click(object sender, RoutedEventArgs e)
    {
        var window = new Window();
        window.Content = new MainPage();
        window.Activate();
    }

    private void homeButton_Click(object sender, RoutedEventArgs e)
    {
        browserPage?.Navigate("https://eu.startpage.com/");
    }

    private void searchButton_Click(object sender, RoutedEventArgs e)
    {
        SearchFunction(false);
    }

    private void backButton_Click(object sender, RoutedEventArgs e)
    {
        browserPage?.GoBack();
    }

    private void forwardButton_Click(object sender, RoutedEventArgs e)
    {
        browserPage?.GoForward();
    }

    private void refreshButton_Click(object sender, RoutedEventArgs e)
    {
        browserPage?.Reload();
    }
    #endregion
    #region Settings
    private void ApplySettings(Dictionary<string, object> settings)
    {
        if (settings.ContainsKey("Theme"))
        {
            int themeValue = Convert.ToInt32(settings["Theme"]);
            comboBox1.SelectedIndex = themeValue;
            switch (themeValue)
            {
                case 0:
                    SystemThemeHelper.SetApplicationTheme(this.XamlRoot, ElementTheme.Default);
                    break;
                case 1:
                    SystemThemeHelper.SetApplicationTheme(this.XamlRoot, ElementTheme.Light);
                    break;
                case 2:
                    SystemThemeHelper.SetApplicationTheme(this.XamlRoot, ElementTheme.Dark);
                    break;
                default:
                    Console.Error.WriteLine("Invalid Theme value");
                    break;
            }
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
            SystemThemeHelper.SetApplicationTheme(this.XamlRoot, ElementTheme.Default);
            SettingsHandler.SetDefaultTheme();
        }
        else if (comboBox1.SelectedIndex == 1) // <- Light theme
        {
            SystemThemeHelper.SetApplicationTheme(this.XamlRoot, ElementTheme.Light);
            SettingsHandler.SetLightTheme();
        }
        else if (comboBox1.SelectedIndex == 2) // <- Dark theme
        {
            SystemThemeHelper.SetApplicationTheme(this.XamlRoot, ElementTheme.Dark);
            SettingsHandler.SetDarkTheme();
        }
    }

    private async void AboutButton_Click(object sender, RoutedEventArgs e)
    {
        AboutDialog aboutDialog = new AboutDialog();
        aboutDialog.XamlRoot = this.XamlRoot;
        await aboutDialog.ShowAsync();
    }
    #endregion
    #region Embed Browser
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
    #endregion
    #region Search
    public async void SearchFunction(bool isEmbedSearch = false)
    {
        SearchDialog searchDialog = new SearchDialog();
        searchDialog.XamlRoot = this.XamlRoot;
        await searchDialog.ShowAsync();
        string input = searchDialog.searchTextBox.Text;
        WebView2 typeOfView = isEmbedSearch ? embedWebView2 : browserPage.webView2;
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
    #endregion
}
