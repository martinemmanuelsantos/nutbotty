using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nutbotty.Events
{
    class UnknownEvent : Event
    {
        string message;

        public UnknownEvent(string ircString)
        {
            this.message = ircString;
        }

        public override string ToString()
        {
            return "Unknown Chat Event: " + message;
        }
    }
}
