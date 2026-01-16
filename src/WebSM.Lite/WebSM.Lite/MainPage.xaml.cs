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
using System.Diagnostics;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Windows.UI.Core;

namespace WebSM.Lite;

public sealed partial class MainPage : Page
{
    public BrowserPage browserPage;
    public MainPage()
    {
        InitializeComponent();
        var settings = LoadSettingsJSON();
        if (settings != null && settings.ContainsKey("Theme"))
        {
            int themeValue = Convert.ToInt32(settings["Theme"]);
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
                    Console.Error.WriteLine("Invalid value Theme.");
                    break;
            }
        }
        ApplySettings(settings);
        // Setup dynamic orientation/size handling (mobile & desktop)
        SetupOrientationHandling();
    }

    private void SetupOrientationHandling()
    {
        this.SizeChanged += MainPage_SizeChanged;
        UpdateLayoutForOrientation(this.ActualWidth, this.ActualHeight);
    }

    private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateLayoutForOrientation(e.NewSize.Width, e.NewSize.Height);
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
            navView.Margin = new Thickness(0);
        }
        else
        {
            navView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
            var commandBarMargin = commandBar.Margin;
            commandBarMargin.Left = 0;
            commandBar.Margin = commandBarMargin;
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
#endif
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        var settingsHandler = new SettingsHandler();
        if (!settingsHandler.IsInternetAvailable())
        {
            mainFrame.Navigate(typeof(OfflinePage));
            // ensure browserPage is null if offline page shown
            browserPage = null;
            return;
        }

        // Subscribe to frame navigation so we capture the BrowserPage instance when it's created.
        mainFrame.Navigated += MainFrame_Navigated;
        mainFrame.Navigate(typeof(BrowserPage));
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

    private void navView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            settingsView.IsPaneOpen = true;
            navView.SelectedItem = null;
            return;
        }
    }

    private void homeButton_Click(object sender, RoutedEventArgs e)
    {
        browserPage?.Navigate("https://eu.startpage.com");
    }

    private async void searchButton_Click(object sender, RoutedEventArgs e)
    {
        Dialogs.SearchDialog searchDialog = new Dialogs.SearchDialog();
        searchDialog.XamlRoot = this.XamlRoot;
        await searchDialog.ShowAsync();
        if (String.IsNullOrEmpty(searchDialog.searchTextBox.Text)) return;
        if (searchDialog.searchTextBox.Text.Contains("://"))
        {
            browserPage?.Navigate(searchDialog.searchTextBox.Text);
        }
        else if (searchDialog.searchTextBox.Text.Contains("."))
        {
            browserPage?.Navigate("https://" + searchDialog.searchTextBox.Text);
        }
        else
        {
            browserPage?.Navigate("https://eu.startpage.com/search?q=" + searchDialog.searchTextBox.Text);
        }
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
        Dialogs.AboutDialog aboutDialog = new Dialogs.AboutDialog();
        aboutDialog.XamlRoot = this.XamlRoot;
        await aboutDialog.ShowAsync();
    }
}
