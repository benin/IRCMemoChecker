using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using IRCMemoChecker.Data;
using System.Collections.Specialized;

namespace IRCMemoChecker
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ServerInfoPage : Page
    {
        private IRCServerData ServerData;

        public ServerInfoPage()
        {
            this.InitializeComponent();

            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            // Listen for ServerData changes
            IRCServers.Instance.Servers.CollectionChanged += this.OnCollectionChanged;
        }

        void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ServerData = (IRCServerData)e.Parameter;

            if (ServerData != null)
            {
                TextBlockName.DataContext = ServerData;
            }
        }

        private void AppBarButton_ClearClick(object sender, RoutedEventArgs e)
        {

        }

        private void AppBarButton_RefreshClick(object sender, RoutedEventArgs e)
        {

        }

        private void AppBarButton_EditServerClick(object sender, RoutedEventArgs e)
        {
            if (ServerData != null)
            {
                Frame.Navigate(typeof(ServerSettingsPage), ServerData);
            }
        }

        private void AppBarButton_RemoveServerClick(object sender, RoutedEventArgs e)
        {
            if (ServerData != null)
            {
                // Remove server
                IRCServers.Instance.Remove(ServerData);
                
                // Go back
                //Frame rootFrame = Window.Current.Content as Frame;
                //if (rootFrame != null && rootFrame.CanGoBack)
                //{
                //    rootFrame.GoBack();
                //}
            }
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
            }

            if (e.OldItems != null)
            {
                foreach (IRCServerData oldItem in e.OldItems)
                {
                    if (oldItem == ServerData)
                    {
                        // Go back
                        Frame rootFrame = Window.Current.Content as Frame;
                        if (rootFrame != null && rootFrame.CanGoBack)
                        {
                            rootFrame.GoBack();
                        }
                    }
                }
            }
        }
    }
}
