using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nutbotty.Events
{
    class HostEvent : Event
    {
        #region Member Variables
        string host;                    // The channel that is hosting
        string hostee;                  // The channel being hosted 
        #endregion

        #region Constructors
        public HostEvent(string ircString)
        {
            this.host = ParseHost(ircString);
            this.hostee = ParseHostee(ircString);
        }
        #endregion

        #region Methods
        internal string ParseHost(string ircString)
        {
            string host = ircString;
            int startIndex = host.LastIndexOf(':');
            host = host.Substring(startIndex + 1);
            int endIndex = host.IndexOf(' ');
            host = host.Substring(0, endIndex);
            return host;
        }

        internal string ParseHostee(string ircString)
        {
            string hostee = ircString;
            int startIndex = hostee.IndexOf(' ');
            hostee = hostee.Substring(startIndex + 1);
            startIndex = hostee.IndexOf(' ');
            hostee = hostee.Substring(startIndex + 1);
            int endIndex = hostee.IndexOf(' ');
            hostee = hostee.Substring(0, endIndex);
            return hostee;
        }
        public override string ToString()
        {
            return host + " is now hosting " + hostee;
        }
        #endregion

        #region Getters
        public string Host
        {
            get
            {
                return host;
            }
        }

        public string Hostee
        {
            get
            {
                return hostee;
            }
        }
        #endregion
    }
}
