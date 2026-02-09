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
    private readonly Dictionary<WebView2, TabViewItem> webviewToTab = new();
    private static readonly Random _random = new Random();
    public int currentTabId;
    private bool _initialTabCreated = false;

    public BrowserPage()
    {
        this.InitializeComponent();
        Loaded += Page_Loaded;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        tabView.Loaded += TabView_Loaded;
    }

    private void webView2_NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        progressRing.IsActive = true;
    }

    private async void webView2_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        progressRing.IsActive = false;
        TabViewItem? tabItem = tabView.SelectedItem as TabViewItem;
        if (!TryGetTabItemFromWebView(sender, out tabItem)) return;

        // Set the title to the tab
        string pageTitle = await sender.ExecuteScriptAsync("document.title");
        if (!string.IsNullOrEmpty(pageTitle))
        {
            pageTitle = pageTitle.Trim('"'); // Remove the quotes from the title
            tabItem.Header = pageTitle;
        }

        // Set the favicon to the tab
        string faviconUrl = await sender.ExecuteScriptAsync("document.querySelector('link[rel~=\"icon\"]')?.href || document.querySelector('link[rel~=\"shortcut icon\"]')?.href");
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
        await CreateNewTabAsync(0);
        tabView.SelectedIndex = 0;
    }

    private async void TabView_AddButtonClick(TabView sender, object args)
    {
        await CreateNewTabAsync(sender.TabItems.Count);
    }

    private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Tab is not TabViewItem tabItem) return;
        if (tabItem.Tag is not int tabId) return;

        if (tabViewTabItems.TryGetValue(tabId, out var webView))
        {
            webView.CoreWebView2?.Stop();
#if !WINAPPSDK_PACKAGED
            webView.Dispose();
#endif
        }

        tabViewTabItems.Remove(tabId);
        tabViewItems.Remove(tabId);
        sender.TabItems.Remove(tabItem);

        if (sender.TabItems.Count == 0)
        {
            Application.Current.Exit();
        }
    }

    private void tabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (tabView.SelectedItem is TabViewItem tabItem && tabItem.Tag is int id)
        {
            currentTabId = id;
        }
    }

    private async Task CreateNewTabAsync(int index)
    {
        progressRing.IsActive = true;

        int tabId = GenerateUniqueID();
        var webView = new WebView2();
        var tabItem = new TabViewItem
        {
            Tag = tabId,
            Content = webView,
            Header = "Nouvel onglet",
            IconSource = new SymbolIconSource { Symbol = Symbol.Globe }
        };

        tabViewTabItems.Add(tabId, webView);
        tabViewItems.Add(tabId, tabItem);

        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        {
            tabView.TabItems.Add(tabItem);
            tabView.SelectedIndex = index;
            currentTabId = tabId;
        });

        await webView.EnsureCoreWebView2Async();
        webView.Source = new Uri("https://eu.startpage.com/");
        webView.NavigationStarting += webView2_NavigationStarting;
        webView.NavigationCompleted += webView2_NavigationCompleted;
        webView.CoreWebView2.NewWindowRequested += webView2_NewWindowRequested;

        progressRing.IsActive = false;
    }

    private int GenerateUniqueID()
    {
        int id = _random.Next(0, int.MaxValue);
        while (tabViewTabItems.ContainsKey(id))
        {
            id = _random.Next(0, int.MaxValue);
        }
        return id;
    }

    public bool TryGetTabItemFromWebView(WebView2 webView, out TabViewItem? tabItem)
    {
        return webviewToTab.TryGetValue(webView, out tabItem);
    }

    private async void webView2_NewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
    {
        args.Handled = true; // Prevent the default behavior of opening a new window
        if (!TryGetCurrentWebView(out var webView)) return;
        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
        {
            await CreateNewTabAsync(tabView.TabItems.Count);
            webView.Source = new Uri(args.Uri);
        });
    }

    #region Wrapper
    public bool TryGetCurrentWebView(out WebView2 webView)
    {
        return tabViewTabItems.TryGetValue(currentTabId, out webView);
    }

    public void Navigate(string url)
    {
        if (TryGetCurrentWebView(out var webView) &&
            Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            webView.Source = uri;
        }
    }

    public void GoBack()
    {
        if (TryGetCurrentWebView(out var webView) && webView.CanGoBack)
        {
            webView.GoBack();
        }
    }

    public void GoForward()
    {
        if (TryGetCurrentWebView(out var webView) && webView.CanGoForward)
        {
            webView.GoForward();
        }
    }

    public void Reload()
    {
        if (TryGetCurrentWebView(out var webView))
        {
            webView.Reload();
        }
    }
    #endregion
}
