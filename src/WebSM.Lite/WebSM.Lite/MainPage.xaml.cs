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
using System.Xml.Linq;
using Microsoft.Web.WebView2.Core;
using Windows.UI.Core;

namespace WebSM.Lite;

public sealed partial class MainPage : Page
{
    public BrowserPage browserPage;
    private Dictionary<string, object>? _settings;

    public MainPage()
    {
        InitializeComponent();
        _settings = LoadSettingsXML();
        ApplySettings(_settings);
        SetupOrientationHandling();
    }

    #region SetupStart
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
        // Subscribe to frame navigation so we capture the BrowserPage instance when it's created.
        mainFrame.Navigated += MainFrame_Navigated;
        mainFrame.Navigate(typeof(BrowserPage));

        // Ensure UI-related settings (theme, control states) are applied when XamlRoot is available.
        ApplySettings(_settings);
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

    private void homeButton_Click(object sender, RoutedEventArgs e)
    {
        browserPage?.Navigate("https://eu.startpage.com");
    }

    private async void searchButton_Click(object sender, RoutedEventArgs e)
    {
        Dialogs.SearchDialog searchDialog = new Dialogs.SearchDialog();
        searchDialog.XamlRoot = this.XamlRoot;
        await searchDialog.ShowAsync();
        string input = searchDialog.searchTextBox.Text;
        if (String.IsNullOrEmpty(input)) return;
        switch (input)
        {
            case string s when s.Contains("://"):
                browserPage?.Navigate(s);
                break;
            case string s when s.Contains("."):
                browserPage?.Navigate("https://" + s);
                break;
            default:
                browserPage?.Navigate("https://eu.startpage.com/search?q=" + input);
                break;
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

    private Dictionary<string, object> LoadSettingsXML()
    {
        string filePath = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "settings.xml");
        if (!File.Exists(filePath))
        {
            var doc = new XDocument(
                new XElement("Settings",
                    new XElement("Theme", (int)ElementTheme.Default) // 0 = Default, 1 = Light, 2 = Dark
                )
            );
            doc.Save(filePath);
        }
        Dictionary<string, object> settings = new Dictionary<string, object>();
        try
        {
            XDocument doc = XDocument.Load(filePath);
            if (doc.Root != null)
            {
                foreach (XElement element in doc.Root.Elements())
                {
                    settings[element.Name.LocalName] = element.Value;
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading settings: {ex}");
        }
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
    #endregion
}
