using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nutbotty.Entities
{
    class BlockedPhrase
    {
        #region Member Variables
        public string phrase { get; set; }
        #endregion

        #region Constructors
        public BlockedPhrase(string blockedPhrase)
        {
            this.phrase = blockedPhrase;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return this.phrase;
        }
        #endregion

    }
}
