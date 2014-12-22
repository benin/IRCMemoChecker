using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace IRCMemoChecker.Data
{
    class IRCServers
    {
        static IRCServers instance = null;
        private ObservableCollection<IRCServerData> servers;

        IRCServers()
        {
            Servers = new ObservableCollection<IRCServerData>();
        }

        public static IRCServers Instance
        {
            get
            {                
                if (instance == null)
                {
                    instance = new IRCServers();
                }
                return instance;
            }
        }

        public ObservableCollection<IRCServerData> Servers
        {
            get
            {
                return servers;
            }
            set
            {
                if (servers != value)
                {
                    servers = value;
                }
            }
        }

        // Accessors
        public void Add(IRCServerData ServerData)
        {
            Servers.Add(ServerData);
        }

        public async void Remove(IRCServerData ServerData)
        {
            MessageDialog PromptDialog = new MessageDialog(ServerData.Name + " will be deleted from the base.", "Delete server?");

            bool? result = null;
            PromptDialog.Commands.Add(new UICommand(
                "Yes", new UICommandInvokedHandler((cmd) => result = true)));
            PromptDialog.Commands.Add(new UICommand(
                "Cancel", new UICommandInvokedHandler((cmd) => result = false)));

            // Set the command that will be invoked by default
            PromptDialog.DefaultCommandIndex = 0;

            // Set the command to be invoked when escape is pressed
            PromptDialog.CancelCommandIndex = 1;

            // Show the message dialog
            await PromptDialog.ShowAsync();

            if (result == true)
            {
                Servers.Remove(ServerData);
            }
        }
    }

    class IRCServerData
    {
        public IRCServerData()
        {
            UnreadedMessages = 0;
        }

        public IRCServerData(string SrvName, Uri SrvUrl)
        {
            Name = SrvName;
            Url = SrvUrl;
        }

        // Basic properties
        public string Name { get; set; }
        public Uri Url { get; set; }

        public string User { get; set; }
        public string Password { get; set; }

        public int UnreadedMessages { get; set; }

        // Messages
        public ObservableCollection<IRCMemoServerMessage> Messages { get; set;}

        public override string ToString()
        {
            return Name;
        }
    }

    class IRCMemoServerMessage
    {
        public string From { get; set; }
        public DateTime When { get; set; }
        public string Message { get; set; }
    }
}
