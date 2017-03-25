using nutbotty.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nutbotty
{
    class TwitchChatConnection
    {


        #region Member Variables
        private IrcClient ircClient;
        LinkedList<TwitchChatRoom> chatrooms = new LinkedList<TwitchChatRoom>();
        #endregion

        #region Constructors
        public TwitchChatConnection(IrcClient ircClient, bool isWhisperConnection)
        {
            this.ircClient = ircClient;
            this.ircClient.Connect();
            if (isWhisperConnection)
            {
                ircClient.SendIrcString("CAP REQ :twitch.tv/commands");
            }
        }
        #endregion

        #region Getters
        public IrcClient IrcClient
        {
            get
            {
                return ircClient;
            }

        }
        #endregion

        #region Methods
        internal void join(TwitchChatRoom channel)
        {
            ircClient.JoinChannel(channel.Channel.channelName);
            chatrooms.AddLast(channel);
        }

        internal void part(string channel)
        {
            ircClient.PartChannel(channel);
        }

        /// <summary>
        /// Continually check for messages from the IRC input stream, and respond according to the event typpe
        /// </summary>
        internal void Run()
        {
            while (true)
            {
                try
                {
                    Event chatEvent = GetNextEvent();
                    RespondToEvent(chatEvent);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }


        /// <summary>
        /// Determine the type of event given a full IRC string. Possible chat events include "PING", "WHISPER", "PRIVMSG", host events, otherwise unknown
        /// </summary>
        /// <param name="ircString">Complete IRC string</param>
        /// <returns></returns>
        private Event GetNextEvent()
        {
            // Retrieve the whole IRC message. If you are disconnected, repeatedly request to connect to the server indefinitely
            string ircString = null;
            while (ircString == null)
            {
                try
                {
                    ircString = this.ircClient.ReadIrcString();
                }
                catch (Exception e)
                {
                    Log.Message("You have been disconnected: " + e, true);
                    this.ircClient.Connect();
                }
            }

            return Event.parseEvent(ircString);
        }

        #region Bot Logic
        /// <summary>
        /// Logic for responding to chat events
        /// </summary>
        /// <param name="chatEvent"></param>
        private void RespondToEvent(Event chatEvent)
        {

            // Logic for responding to a "PING" event
            if (chatEvent.GetType().Equals(typeof(PingEvent)))
            {
                ircClient.SendIrcString("PONG");
                Log.Message(chatEvent.ToString(), false);
            }

            // Logic for responding to a "PRIVMSG" event
            else if (chatEvent.GetType().Equals(typeof(ChatEvent)))
            {
                // Check all the chat roomms currently connected, then respond to the correct channel
                ChatEvent chatMessage = (ChatEvent)chatEvent;
                foreach (TwitchChatRoom chatroom in chatrooms)
                {
                    if (chatMessage.Channel.Equals(chatroom.Channel.channelName))
                    {
                        chatroom.RespondToChatEvent(chatEvent);
                        break;
                    }
                }
            }

            // Logic for responding to a "WHISPER" event
            else if (chatEvent.GetType().Equals(typeof(WhisperEvent)))
            {
                Log.Message(chatEvent.ToString(), true);
            }

            // Logic for responding to a "HOST" event
            else if (chatEvent.GetType().Equals(typeof(HostEvent)))
            {
                HostEvent hostData = (HostEvent)chatEvent;
                ircClient.SendChatMessage(hostData.Hostee, chatEvent.ToString());
                Log.Message(hostData.ToString(), true);
            }

            // Logic for responding to an unknown event
            else
            {
                //Log.Message(chatEvent.ToString(), false);
            }
        }
        #endregion

        #endregion

    }
}
