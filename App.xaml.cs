﻿using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Gaming.XboxGameBar;
using YoutubeGameBarWidget.WebServer;
using System.IO;
using System.Linq;
using YoutubeGameBarWidget.Utilities;
using YoutubeGameBarWidget.Pages;
using System.Globalization;

namespace YoutubeGameBarOverlay
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private XboxGameBarWidget ytgbw;
        private WebServer ws;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            SetEnvVars();
            SetupLocalSettings();
            new Cabinet().Initialize();
            this.ws = new WebServer();
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }
        
        /// <summary>
        /// Searches for a .env file and sets the process' environment variables based on it.
        /// </summary>
        private void SetEnvVars()
        {
            string[] vars = File.ReadAllText("./.env").Split("\r\n");

            foreach(string var in vars)
            {
                string key = var.Split("=").First();
                string value = var.Split("=").Last();
                Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Process);
            }
        }

        /// <summary>
        /// Sets up the local settings of color and language based on stored config.
        /// Language preferences, if not already set, will be fetched by system's language.
        /// Color preferences, if not already set, will be failsafe default to Red theme.
        /// </summary>
        private void SetupLocalSettings()
        {
            string accentColor = (string) Utils.GetSettingValue(Constants.Settings.AccentColors["varname"]);
            string secondaryColor = (string) Utils.GetSettingValue(Constants.Settings.SecondaryColors["varname"]);
            string auxiliaryColor = (string) Utils.GetSettingValue(Constants.Settings.AuxiliaryColors["varname"]);
            string prefferedLanguage = (string) Utils.GetSettingValue(Constants.Settings.Languages["varname"]);
            string tipsPreference = (string) Utils.GetSettingValue(Constants.Settings.ShowTips["varname"]);
            string thumbnailsPreference = (string) Utils.GetSettingValue(Constants.Settings.ShowThumbnails["varname"]);

            if (accentColor == null)
            {
                Utils.SetSettingValue(Constants.Settings.AccentColors["varname"], Constants.Settings.AccentColors["Red"]);
            }

            if (secondaryColor == null)
            { 
                Utils.SetSettingValue(Constants.Settings.SecondaryColors["varname"], Constants.Settings.SecondaryColors["Red"]);
            }

            if (auxiliaryColor == null)
            {
                Utils.SetSettingValue(Constants.Settings.AuxiliaryColors["varname"], Constants.Settings.AuxiliaryColors["White"]);
            }

            if (prefferedLanguage == null)
            {
                Utils.SetSettingValue(Constants.Settings.Languages["varname"], CultureInfo.InstalledUICulture.Name);
            }

            if (tipsPreference == null)
            {
                Utils.SetSettingValue(Constants.Settings.ShowTips["varname"], Constants.Settings.ShowTips["True"]);
            }
            
            if (thumbnailsPreference == null)
            {
                Utils.SetSettingValue(Constants.Settings.ShowThumbnails["varname"], Constants.Settings.ShowThumbnails["True"]);
            }
        }

        /// <summary>
        /// The Game Bar Widget activations happen as a Protocol Activation
        /// with its URI scheme being "ms-gamebarwidget".
        /// 
        /// If it is the desired kind and equals to the string mentioned above we'll cast 
        /// the received arguments to XboxGameBarWidgetActivatedEventArgs type, initialize the communication
        /// between Xbox Game Bar and the Widget, then navigate to the desired page on our current window.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnActivated(IActivatedEventArgs args)
        {
            XboxGameBarWidgetActivatedEventArgs widgetArgs = null;

            if (args.Kind == ActivationKind.Protocol)
            {
                var protocolArgs = args as IProtocolActivatedEventArgs;
                string protocolScheme = protocolArgs.Uri.Scheme;

                if (protocolScheme.Equals("ms-gamebarwidget"))
                {
                    widgetArgs = args as XboxGameBarWidgetActivatedEventArgs;
                }
            }

            if (widgetArgs != null)
            {
                if (widgetArgs.IsLaunchActivation)
                {
                    var rootFrame = new Frame();
                    rootFrame.NavigationFailed += OnNavigationFailed;
                    Window.Current.Content = rootFrame;

                    ytgbw = new XboxGameBarWidget(widgetArgs, Window.Current.CoreWindow, rootFrame);
                    rootFrame.Navigate(typeof(MainPage));
                        
                    Window.Current.Closed += WidgetWindow_Closed;
                    Window.Current.Activate();
                }
            }
        }

        /// <summary>
        /// Handles the closing event sent to Widget's main window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WidgetWindow_Closed(object sender, Windows.UI.Core.CoreWindowEventArgs e)
        {
            ytgbw = null;
            ws = null;
            Window.Current.Closed -= WidgetWindow_Closed;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
