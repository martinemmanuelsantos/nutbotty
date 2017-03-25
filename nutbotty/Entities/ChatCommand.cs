using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nutbotty.Entities
{

    class ChatCommand
    {
        #region Member Variables
        public string TriggerText { get; }
        public string ResponseText { get; }
        public string ChannelName { get; }
        public bool IsUniversal { get; }
        public bool MustBeExact { get; }
        public bool WhisperResponse { get; }
        public bool SubscriberOnly { get; }
        public bool ModeratorOnly { get; }
        public bool BroadcasterOnly { get; }
        #endregion

        #region Constructors
        public ChatCommand(string triggerText, string responseText, string channelName,
            bool isUniversal, bool mustBeExact, bool whisperResponse,
            bool subscriberOnly, bool moderatorOnly, bool broadcasterOnly)
        {
            this.TriggerText = triggerText;
            this.ResponseText = responseText;
            this.ChannelName = channelName;
            this.IsUniversal = isUniversal;
            this.MustBeExact = mustBeExact;
            this.WhisperResponse = whisperResponse;
            this.SubscriberOnly = subscriberOnly;
            this.ModeratorOnly = moderatorOnly;
            this.BroadcasterOnly = broadcasterOnly;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return TriggerText + ": " + ResponseText + " | " + ChannelName;
        }
        #endregion

    }
}
