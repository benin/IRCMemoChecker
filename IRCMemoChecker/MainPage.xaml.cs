using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

using NotifyType = IRCMemoChecker.IRCClient.NotifyType;

namespace IRCMemoChecker
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private IRCClient ircClient;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            ircClient = new IRCClient(this.Dispatcher);
            ircClient.OnNotification += NotifyUser;
            ircClient.OnConnectionChanged += OnConnectionStateChanged;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            switch (ircClient.ConnectionState)
            {
                case IRCClient.ConnectionStateType.Disconnected:
                    // Start connecting
                    ircClient.Connect(new Uri(ServerHostname.Text));
                    break;
                case IRCClient.ConnectionStateType.Connecting:
                case IRCClient.ConnectionStateType.Connected:
                    ircClient.Disconnect();
                    break;
                default:
                    break;
            }    
        }

        private void OnConnectionStateChanged(IRCClient.ConnectionStateType type)
        {
            switch (type)
            {
                case IRCClient.ConnectionStateType.Disconnected:
                    ConnectButton.Content = "Connect";
                    break;
                case IRCClient.ConnectionStateType.Connecting:
                    ConnectButton.Content = "Cancel";
                    break;
                case IRCClient.ConnectionStateType.Connected:
                    ConnectButton.Content = "Disconnect";
                    break;
                default:
                    break;
            }
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            ircClient.Send(SendText.Text);
        }

        private void NotifyUser(string strMessage, NotifyType type)
        {
            if (type == NotifyType.DataMessage)
            {
                OutputLog.Text += strMessage + Environment.NewLine;

                // Scroll to view data
                OutputLogViewer.UpdateLayout();
                OutputLogViewer.ChangeView(null, OutputLogViewer.ScrollableHeight, null);
            }
            else if (StatusBlock != null)
            {
                switch (type)
                {
                    case NotifyType.StatusMessage:
                        StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                        break;
                    case NotifyType.ErrorMessage:
                        StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                        break;
                }
                StatusBlock.Text = strMessage;

                // Collapse the StatusBlock if it has no text to conserve real estate.
                if (StatusBlock.Text != String.Empty)
                {
                    StatusBorder.Visibility = Visibility.Visible;
                }
                else
                {
                    StatusBorder.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void StatusBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StatusBorder.Visibility = Visibility.Collapsed;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //
        }

        private void AppBarLogButton_Clicked(object sender, RoutedEventArgs e)
        {
            OutputLog.Visibility = ButtonLog.IsChecked == true ?
                Visibility.Visible : Visibility.Collapsed;
        }
    }
}
