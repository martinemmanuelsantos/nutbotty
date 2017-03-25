using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nutbotty.Events
{
    /// <summary>
    /// Message class for defining IRC message attributes
    /// IRC message format: :<user>!<user>@<user>.tmi.twitch.tv PRIVMSG #<channel> :<chat>
    /// </summary>
    class ChatEvent : Event
    {
        #region Member Variables
        private string user;                        // Username of the user that created the message
        private string channelName;                 // Channel that the message was received from 
        private string chatMessage;                 // Twitch chat portion of message

        private string userBadges;                  // Badges of the use
        private string userColor;                   // Color of the user as a hexadecimal
        private string displayname;                 // Display name of the user including capitalization
        private string emotes;                      // Emotes that used in the message
        private bool userIsSubscriber;              // Is the user a subscriber to this channel?
        private bool userHasTurbo;                  // Does the user have Twitch turbo?
        private long userId;                        // User ID number of the user
        private string roomId;                      // Room ID number of the message
        private string userType;                    // Type of the user (mod, global-mod, admin, staff)
        private bool userIsBroadcaster;             // Is the user the broadcaster in this channel?
        private bool userIsModerator;               // Is the user a moderator in this chhanel (or higher)?
        #endregion

        #region Constructor
        public ChatEvent(string ircString)
        {
            // Set main attributes of chat message
            this.user = ParseUser(ircString);
            this.channelName = ParseChannel(ircString);
            this.chatMessage = ParseMessage(ircString);

            // Set metatag attributes of chat message
            List<string> metatags = GetMetatags(ircString);
            userBadges = ParseUserBadges(metatags);
            userColor = ParseUserColor(metatags);
            displayname = ParseDisplayName(metatags);
            emotes = ParseEmotes(metatags);
            userIsSubscriber = ParseIsSubscriber(metatags);
            userHasTurbo = ParseHasTurbo(metatags);
            userId = ParseUserId(metatags);
            roomId = ParseRoomId(metatags);
            userType = ParseUserType(metatags);
            userIsBroadcaster = ParseIsBroadcaster(metatags);
            userIsModerator = ParseIsModerator(userBadges, metatags);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Parses the channel name that the chat message originated from
        /// </summary>
        /// <param name="ircString">The full IRC string</param>
        /// <returns></returns>
        internal static string ParseChannel(string ircString)
        {
            string channel = ircString;
            int startIndex = channel.IndexOf(' ');
            channel = channel.Substring(startIndex);
            startIndex = channel.IndexOf('#');
            int endIndex = channel.Length;
            int length = endIndex - startIndex - 1;
            channel = channel.Substring(startIndex + 1, length);                                                // Keep substring between first '#' to the end of the message
            endIndex = channel.IndexOf(' ');
            channel = channel.Substring(0, endIndex);                                                           // Keep substring between the start of the string and the first ' '
            return channel;
        }

        /// <summary>
        /// Parse the subscriber status of the user
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal bool ParseIsSubscriber(List<string> metatags)
        {
            string subscriberData = FindMetatagData(metatags, "subscriber=");
            if (subscriberData.Equals("1")) { return true; }
            else return false;
        }

        /// <summary>
        /// Parse the broadcaster status of the user
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal bool ParseIsBroadcaster(List<string> metatags)
        {
            if (this.User.Equals(this.Channel)) { return true; }
            else return false;
        }

        /// <summary>
        /// Parse the moderator status of the user
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal bool ParseIsModerator(string userBadges, List<string> metatags)
        {
            string moderatorData = FindMetatagData(metatags, "mod=");
            if (moderatorData.Equals("1")) { return true; }
            if (this.ParseUserBadges(metatags).Contains("broadcaster")) { return true; }
            else return false;
        }

        /// <summary>
        /// Override  ToString() method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[" + channelName + "] " + user + ": " + chatMessage;
        }
        #endregion

        #region Getters
        public string ChatMessage
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

        internal string Channel
        {
            get
            {
                return channelName;
            }
        }

        public string UserColor
        {
            get
            {
                return userColor;
            }
        }

        public string Displayname
        {
            get
            {
                return displayname;
            }
        }

        public string Emotes
        {
            get
            {
                return emotes;
            }
        }

        public bool UserIsSubscriber
        {
            get
            {
                return userIsSubscriber;
            }
        }

        public bool UserHasTurbo
        {
            get
            {
                return userHasTurbo;
            }
        }

        public long UserId
        {
            get
            {
                return userId;
            }
        }

        public string UserType
        {
            get
            {
                return userType;
            }

        }

        public bool UserIsBroadcaster
        {
            get
            {
                return userIsBroadcaster;
            }
        }

        public bool UserIsModerator
        {
            get
            {
                return userIsModerator;
            }
        }
        #endregion

    }
}
