using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace WebSM.WebUtilities
{
    public class UserAgent
    {
        public WebView2 webView2 { get; set; }
        public PivotItem pivotItem { get; set; }

        public void UserAgents()
        {
            var settings = webView2.CoreWebView2.Settings;
            if (webView2.Source.ToString().Contains("https://accounts.google.com"))
            {
                settings.UserAgent = GetMobileUserAgent();
            }
            else
            {
                settings.UserAgent = DefaultUserAgent();
            }
        }

        private string GetMobileUserAgent()
        {
            return "Chrome";
        }

        private string DefaultUserAgent()
        {
            return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.5060.134 Safari/537.36 Edge/103.0.1264.77";
        }
    }
}
