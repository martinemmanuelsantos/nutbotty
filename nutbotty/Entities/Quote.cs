using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nutbotty.Entities
{
    class Quote
    {

        #region Member Variables
        public string quoteText { get; }                       // Contents of the quote
        public string channel { get; }                         // Channel that the quote originated from
        public string addedBy { get; }                         // Username of the user that added the quote
        public DateTime dateAdded { get; } = DateTime.Now;
        #endregion

        #region Constructors
        public Quote(string quoteText, string channel, string addedBy, DateTime dateAdded)
        {
            this.quoteText = quoteText;
            this.channel = channel;
            this.addedBy = addedBy;
            this.dateAdded = dateAdded;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return "<" + channel + "> " + quoteText + " | added by " + addedBy + " @" + dateAdded;
        }
        #endregion

    }
}
