/*
 * WebSM Lite - A simply minimalist web browser.
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
namespace WebSM.Lite;

public sealed partial class BrowserPage : Page
{
    private bool _ready;

    public BrowserPage()
    {
        this.InitializeComponent();
        Loaded += Page_Loaded;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await webView2.EnsureCoreWebView2Async();
        _ready = true;
    }

    private void webView2_NavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
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

    private void webView2_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
    {
        progressRing.IsActive = false;
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    // Wrapper for MainPage to navigate correctly
    public void Navigate(string url)
    {
        if (_ready)
        {
            webView2.Source = new Uri(url);
        }
    }

    public void GoBack()
    {
        if (_ready && webView2.CanGoBack)
        {
            webView2.GoBack();
        }
    }

    public void GoForward()
    {
        if (_ready && webView2.CanGoForward)
        {
            webView2.GoForward();
        }
    }

    public void Reload()
    {
        if (_ready)
        {
            webView2.Reload();
        }
    }
}
