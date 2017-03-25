using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nutbotty.Events
{
    class PingEvent : Event
    {
        public PingEvent()
        {

        }

        public override string ToString()
        {
            return "Sent PONG";
        }
    }
}
