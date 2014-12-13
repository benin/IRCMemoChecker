using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRCMemoChecker.Data
{
    class IRCMemoServerMessage
    {
        public string From { get; set; }
        public DateTime When { get; set; }
        public string Message { get; set; }
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
}
