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
using Microsoft.Web.WebView2.Core;
using Windows.UI.Core;
using System.Diagnostics;

namespace WebSM.Lite;

public sealed partial class MainPage : Page
{
    public AppConfig appConfig;

    public MainPage()
    {
        InitializeComponent();
        ApplySettings();
#if ANDROID || IOS
        // Hide the navigation view pane on Android devices and add a Settings button to the app bar
        navView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
        var progressRingMargin = progressRing.Margin;
        progressRingMargin.Left = 0;
        progressRing.Margin = progressRingMargin;
#endif
#if DESKTOP
        // Modify dynamically the margin of commandBar for Settings button to work
        var commandBarMargin = commandBar.Margin;
        commandBarMargin.Left = 50;
        commandBar.Margin = commandBarMargin;
#endif
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // This code adjust the commandBar and navView margin to avoid overlapping
        // with the system navigation bar on Android devices.
        // If user has enabled the "three button navigation" in system settings.
#if ANDROID
        var topInsetDp = GetStatusBarHeightDp();
        if (topInsetDp > 0)
        {
            commandBar.Margin = new Thickness(0, 0, 0, topInsetDp);
            navView.Margin = new Thickness(0, topInsetDp, 0, 0);
        }
#endif

        // Launch WebView2
        await webView2.EnsureCoreWebView2Async();
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
            Debug.WriteLine($"GetStatusBarHeightDp failed: {ex}");
        }

        return 0;
    }
#endif

    private void webView2_NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        progressRing.IsActive = true;
        // Detect if the WebView2 is opening a new tab or window, open the link directly in the system default browser
        sender.CoreWebView2.NewWindowRequested += (s, e) =>
        {
            e.Handled = true;
            var uri = new Uri(e.Uri);
            _ = webView2.Source = uri;
        };
    }

    private void webView2_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        progressRing.IsActive = false;
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

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
        webView2.Source = new Uri("https://eu.startpage.com");
    }

    private async void searchButton_Click(object sender, RoutedEventArgs e)
    {
        Dialogs.SearchDialog searchDialog = new Dialogs.SearchDialog();
        searchDialog.XamlRoot = this.XamlRoot;
        await searchDialog.ShowAsync();
        // Maybe move this part of code to the SearchDialog class?
        if (String.IsNullOrEmpty(searchDialog.searchTextBox.Text)) return;
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
            webView2.Source = new Uri("https://eu.startpage.com/search?q=" + searchDialog.searchTextBox.Text);
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
    private void ApplySettings()
    {
        
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
        aboutDialog.XamlRoot = this.XamlRoot;
        await aboutDialog.ShowAsync();
    }
}
