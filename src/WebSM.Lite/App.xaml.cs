﻿/*
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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace WebSM.Lite
{
    sealed partial class App : Application
    {
        public string URL { get; set; }

        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Protocol)
            {
                var protocolArgs = args as ProtocolActivatedEventArgs;
                if (protocolArgs != null)
                {
                    URL = protocolArgs.Uri.AbsoluteUri;
                }
                else
                {
                    URL = "https://www.bing.com";
                }
            }

            base.OnActivated(args);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Okay this is last time I'm gonna use this
                    // FFS for your Terminated is saying launching the app or closing the app?
                    // I will need to review all the projects if it resolve everything
                    // And oh god it will take a lot of time!
                }

                MainPage mainPage = new MainPage();
                mainPage.webView2.Source = new Uri(URL);

                if (File.Exists(Path.Combine(ApplicationData.Current.RoamingFolder.Path, "settings.json")))
                {
                    // Load the settings.json file
                    string filePath = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "settings.json");
                    string json = File.ReadAllText(filePath);

                    Dictionary<string, object> settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                    Debug.WriteLine(json);
                    Debug.WriteLine(settings);
                    Debug.WriteLine(settings["Theme"]);
                    Debug.WriteLine(settings["FakeUserAgent"]);

                    // Apply the settings to the app
                    RequestedTheme = (ApplicationTheme)settings["Theme"];
                    if ((bool)settings["FakeUserAgent"] == true)
                    {
                        mainPage.webView2.CoreWebView2.Settings.UserAgent = default; // Is this gonna even work?
                    }
                    else if ((bool)settings["FakeUserAgent"] == false)
                    {
                        mainPage.webView2.CoreWebView2.Settings.UserAgent = "WebSM/4.1 Lite Edition (Based on Microsoft WebView2)";
                    }
                }
                else
                {
                    // Create the settings.json file which contains settings for the app
                    Dictionary<string, object> settings = new Dictionary<string, object>
                        {
                            { "Theme", RequestedTheme = (ApplicationTheme)ElementTheme.Default },
                            { "FakeUserAgent", false }
                        };

                    // Format and combine all the settings into a JSON file
                    string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    string filePath = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "settings.json");

                    Debug.Write(filePath);

                    File.WriteAllText(filePath, json);
                }

                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                Window.Current.Activate();
            }
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}
