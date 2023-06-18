using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

namespace WebSM_Linux
{
    public sealed partial class MainPage : Page
    {
        WebView2 webView2 = new WebView2();

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void TabView_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 1; i++)
            {
                (sender as TabView).TabItems.Add(CreateNewTab(i));
            }
        }

        private void TabView_AddButtonClick(TabView sender, object args)
        {
            sender.TabItems.Add(CreateNewTab(sender.TabItems.Count));
        }

        private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            sender.TabItems.Remove(args.Tab);
            if (tabView.TabItems.Count == 0)
            {
                Application.Current.Exit();
            }
        }

        private TabViewItem CreateNewTab(int index)
        {
            TabViewItem newItem = new TabViewItem();

            newItem.Header = $"Document {index}";
            newItem.IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Document };

            WebView2 webView = new WebView2();

            // Init WebView2
            webView.Source = new Uri("https://www.example.com");
            webView.NavigationStarting += webView2_NavigationStarting;

            newItem.Content = webView;

            return newItem;
        }

        private async void webView2_NavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        {
            progressRing.IsActive = true;
            await Task.Delay(0);
        }

        private void webView2_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            progressRing.IsActive = false;
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
            }
            else
            {
                NavigationViewItem item = args.SelectedItem as NavigationViewItem;

                switch (item.Tag)
                {
                    case "Open sidebar":
                        break;
                    case "Favorites":
                        break;
                }
            }
        }

        private async void openWindowButton_Click(object sender, RoutedEventArgs e)
        {
            var currentAV = ApplicationView.GetForCurrentView();
            var newAV = CoreApplication.CreateNewView();
            await newAV.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                async () =>
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

        private async void searchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchDialog searchDialog = new SearchDialog();
            await searchDialog.ShowAsync();
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
        public void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox1.SelectedIndex == 0) // <- Default theme from the system
            {
                this.RequestedTheme = ElementTheme.Default;
            }
            else if (comboBox1.SelectedIndex == 1) // <- Light theme
            {
                this.RequestedTheme = ElementTheme.Light;
            }
            else if (comboBox1.SelectedIndex == 2) // <- Dark theme
            {
                this.RequestedTheme = ElementTheme.Dark;
            }
        }

        private async void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog aboutDialog = new AboutDialog();
            await aboutDialog.ShowAsync();
        }
    }
}