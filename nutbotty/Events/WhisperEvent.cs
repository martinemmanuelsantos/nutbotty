using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nutbotty.Events
{
    class WhisperEvent : Event
    {
        #region Member Variables
        private string user;                        // Username of the user that created the message
        private string chatMessage;                 // Twitch chat portion of message

        private string userColor;                   // Color of the user as a hexadecimal
        private string displayname;                 // Display name of the user including capitalization
        private string threadId;                    // User ID number of the user
        private string emotes;                      // Emotes that used in the message
        private bool userHasTurbo;                  // Does the user have Twitch turbo?
        private long userId;                        // User ID number of the user
        private string userType;                    // Type of the user (mod, global-mod, admin, staff)
        #endregion

        #region Constructors
        public WhisperEvent(string ircString)
        {
            // Set main attributes of whisper
            this.chatMessage = ParseMessage(ircString);
            this.user = ParseUser(ircString);

            // Set metatag attributes of whisper message
            List<string> metatags = GetMetatags(ircString);
            userColor = ParseUserColor(metatags);
            displayname = ParseDisplayName(metatags);
            threadId = ParseThreadId(metatags);
            emotes = ParseEmotes(metatags);
            userHasTurbo = ParseHasTurbo(metatags);
            userId = ParseUserId(metatags);
            userType = ParseUserType(metatags);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Parse the thread ID of the whisper
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal string ParseThreadId(List<string> metatags)
        {
            return FindMetatagData(metatags, "thread-id=");
        }

        public override string ToString()
        {
            return "Whisper from " + user + ": " + chatMessage;
        }
        #endregion

        #region Getters
        internal string ChatMessage
        {
            get
            {
                return chatMessage;
            }
        }

        internal string User
        {
            get
            {
                return user;
            }
        }
        #endregion
    }
}
