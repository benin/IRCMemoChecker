using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace IRCMemoChecker
{
    class IRCClient
    {
        // Basic notification for owner
        public delegate void NotificationContainer(string strMessage, NotifyType type);
        public delegate void ConnectionChanged(ConnectionStateType type);

        public event NotificationContainer OnNotification;
        public event ConnectionChanged OnConnectionChanged;

        public bool IsConnected { get { return ConnectionState == ConnectionStateType.Connected; } }

        public ConnectionStateType ConnectionState
        {
            get { return connState; }
            set
            {
                connState = value;
                if (OnConnectionChanged != null)
                    OnConnectionChanged(connState);
            }
        }
        private ConnectionStateType connState = ConnectionStateType.Disconnected;

        // client data
        private StreamSocket clientSocket = null;
        CancellationToken tokenCancel;
        
        private DataWriter dataWriter = null;
        private DataReader dataReader = null;

        CoreDispatcher parentDispatcher;

        private string strPrefDataBuffer = string.Empty;

        // Notification
        public enum NotifyType
        {
            DataMessage,
            StatusMessage,
            ErrorMessage
        };
        
        public enum ConnectionStateType
        {
            Disconnected,
            Connecting,
            Connected
        }

        public IRCClient(CoreDispatcher dispatcher)
        {
            parentDispatcher = dispatcher;
            OnConnectionChanged += OnClientConnectionChanged;
        }

        private void Notify(string strMessage, NotifyType type)
        {
            if (OnNotification != null)
            {
                var ignore = parentDispatcher.RunAsync(
                    CoreDispatcherPriority.Normal, () => OnNotification(strMessage, type));
            }
        }

        public async void Connect(Uri uri)
        {
            if (ConnectionState != ConnectionStateType.Disconnected)
            {

                switch (ConnectionState)
                {
                    case ConnectionStateType.Connecting:
                        Notify("Connecting...", NotifyType.ErrorMessage);
                        break;
                    case ConnectionStateType.Connected:
                        Notify("Already connected.", NotifyType.ErrorMessage);
                        break;
                    default:
                        break;
                }
                return;
            }

            try
            {
                Notify("Trying to connect ...", NotifyType.StatusMessage);

                if (clientSocket == null)
                    clientSocket = new StreamSocket();

                ConnectionState = ConnectionStateType.Connecting;

                // Set timeout 3 sec
                CancellationTokenSource canclConnect = new CancellationTokenSource();
                canclConnect.CancelAfter(1000);
                tokenCancel = canclConnect.Token;

                // Try to connect to the 
                await clientSocket.ConnectAsync(new HostName(uri.Host), uri.Port.ToString(),
                    SocketProtectionLevel.PlainSocket).AsTask(canclConnect.Token);

                ConnectionState = ConnectionStateType.Connected;

                Notify("Connection established", NotifyType.StatusMessage);
                
            }
            catch (TaskCanceledException exception)
            {
                Notify("Connection timedout: " + exception.Message, NotifyType.ErrorMessage);

                // the Close method is mapped to the C# Dispose
                if (clientSocket != null)
                    clientSocket.Dispose();
                clientSocket = null;

                ConnectionState = ConnectionStateType.Disconnected;
            }
            catch (Exception exception)
            {
                // If this is an unknown status, 
                // it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }

                Notify("Connect failed with error: " + exception.Message, NotifyType.ErrorMessage);

                // the Close method is mapped to the C# Dispose
                if (clientSocket != null)
                    clientSocket.Dispose();
                clientSocket = null;

                ConnectionState = ConnectionStateType.Disconnected;
            }
        }

        public void Disconnect()
        {
            if (ConnectionState == ConnectionStateType.Disconnected|| clientSocket == null)
            {
                Notify("Not connected.", NotifyType.ErrorMessage);
                return;
            }

            try
            {
                Notify("Disconnecting...", NotifyType.StatusMessage);

                if (ConnectionState == ConnectionStateType.Connected)
                {
                    Send("QUIT buy buy");
                }

                if (ConnectionState == ConnectionStateType.Connecting)
                    tokenCancel.ThrowIfCancellationRequested();

                // Try to connect to the 
                clientSocket.Dispose();
                clientSocket = null;

                ConnectionState = ConnectionStateType.Disconnected;

                Notify("Connection closed", NotifyType.StatusMessage);
            }
            catch (Exception exception)
            {
                // If this is an unknown status, 
                // it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }

                Notify("Disconnect failed with error: " + exception.Message, NotifyType.ErrorMessage);

                // the Close method is mapped to the C# Dispose
                clientSocket = null;
            }
        }

        private async void OnClientConnectionChanged(ConnectionStateType type)
        {
            switch (type)
            {
                case ConnectionStateType.Connected:
                    dataReader = new DataReader(clientSocket.InputStream);
                    dataReader.InputStreamOptions = InputStreamOptions.Partial;
                    dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    dataReader.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;

                    //canclRead = new CancellationTokenSource();

                    dataWriter = new DataWriter(clientSocket.OutputStream);

                    Handshake();

                    if (parentDispatcher != null)
                        await parentDispatcher.RunAsync(CoreDispatcherPriority.Normal, () => OnListen());

                    break;
                case ConnectionStateType.Disconnected:
                    //canclRead.Cancel();

                    //canclRead.Dispose();
                    dataWriter = null;
                    break;
                default:
                    break;
            }
        }

        private void Handshake()
        {
            Send("NICK IRCClient");
            Send("USER IRCClient 0 * :IRCClient");
        }

        private async void OnListen()
        {
            if (IsConnected && clientSocket != null)
            {
                bool listen = true;
                while (listen)
                {
                    // Reading data from server
                    string receivedData;

                    try
                    {
                        var count = await dataReader.LoadAsync(1024);

                        if (count > 0)
                        {
                            receivedData = dataReader.ReadString(count);

                            // Divide onto lines
                            FormatData(receivedData);

                            //Notify(receivedData, NotifyType.DataMessage);
                        }
                    }
                    catch (Exception)
                    {
                        listen = false;
                    }
                }
                
            }
        }

        // Format data, split on lines
        private void FormatData(string strData)
        {
            // split the data into parts
            string[] ircData = strData.Split(new string[] { "\n", "\r\n" }, 
                StringSplitOptions.RemoveEmptyEntries);

            // Check endline, if not - wait for next data
            string strTempBuf = string.Empty;
            if (!strData.EndsWith(Environment.NewLine))
            {
                strTempBuf = ircData[ircData.Length - 1];
                Array.Clear(ircData, ircData.Length - 1, 1);
            }
            
            foreach (string s in ircData)
            {
                if (String.IsNullOrEmpty(s))
                    continue;

                if (!String.IsNullOrEmpty(strPrefDataBuffer))
                {
                    ParseData(strPrefDataBuffer + s);
                    strPrefDataBuffer = string.Empty;
                    continue;
                }

                ParseData(s);
            }

            if (strTempBuf != string.Empty)
                strPrefDataBuffer = strTempBuf;
        }

        // Parse lined data
        private void ParseData(string strData)
        {
            Notify(strData, NotifyType.DataMessage);

            // split the data into parts
            string[] ircData = strData.Split(' ');

            // if the message starts with PING we must PONG back
            if (strData.Length > 4)
            {
                if (strData.Substring(0, 4) == "PING")
                {
                    Send("PONG " + ircData[1]);
                    return;
                }
            }
        }

        public async void Send(string sendString)
        {
            if (!IsConnected)
            {
                Notify("Is not connected!", NotifyType.ErrorMessage);
                return;
            }

            // Writing data to the writer will just store data in memory.
            dataWriter.WriteString(sendString + Environment.NewLine);

            try
            {
                // Call StoreAsync method to store the data to a backing stream
                await dataWriter.StoreAsync();
            }
            catch (Exception exception)
            {
                // If this is an unknown status, 
                // it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }

                Notify("Send data or receive failed with error: " + exception.Message, NotifyType.ErrorMessage);
            }
        }
    }
}
