using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace WebSM
{
    public class NewTabs
    {
        public async Task NewTab()
        {
            var webView = new WebView2();
            webView.Margin = new Thickness(-12,0,-12,0);
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.Navigate("file:///C:/Users/v38armageddon/Documents/GitHub/v38armageddon/WebSM/src/Pages/NewTab.html");
        }
    }
}
