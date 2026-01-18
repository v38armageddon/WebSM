/*
 * WebSM - A simply minimalist web browser.
 * Copyright (C) 2022 - 2026 - v38armageddon
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

namespace WebSM;

public sealed partial class BrowserPage : Page
{
    private Dictionary<int, WebView2> tabViewTabItems = new Dictionary<int, WebView2>();
    private Dictionary<int, TabViewItem> tabViewItems = new Dictionary<int, TabViewItem>();
    private int currentTabId;
    public WebView2 webView2 = new WebView2();
    private bool _ready;
    private bool _initialTabCreated = false; // Ajout du champ indicateur

    public BrowserPage()
    {
        this.InitializeComponent();
        Loaded += Page_Loaded;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await webView2.EnsureCoreWebView2Async();
        _ready = true;
        tabView.Loaded += TabView_Loaded;
        if (!_initialTabCreated && tabView != null && tabView.TabItems.Count == 0)
        {
            _initialTabCreated = true;
            await CreateNewTabAsync(0);
            tabView.SelectedIndex = 0;
        }
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
            tabItem.IconSource = new BitmapIconSource() { UriSource = iconUri, ShowAsMonochrome = false };
        }
        else
        {
            tabItem.IconSource = new SymbolIconSource() { Symbol = Symbol.Globe };
        }
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    private async void TabView_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialTabCreated) return;
        _initialTabCreated = true;
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
            newItem.IconSource = new BitmapIconSource() { UriSource = iconUri };
        }
        else
        {
            newItem.IconSource = new SymbolIconSource() { Symbol = Symbol.Globe };
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

    #region Wrapper
    public void Navigate(string url)
    {
        if (webView2 != null)
        {
            webView2.Source = new Uri(url);
        }
    }

    public void GoBack()
    {
        if (webView2 != null && webView2.CanGoBack)
        {
            webView2.GoBack();
        }
    }

    public void GoForward()
    {
        if (webView2 != null && webView2.CanGoForward)
        {
            webView2.GoForward();
        }
    }

    public void Reload()
    {
        if (webView2 != null)
        {
            webView2.Reload();
        }
    }
    #endregion
}
