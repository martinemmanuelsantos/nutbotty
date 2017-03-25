using nutbotty.APIs;
using nutbotty.Entities;
using nutbotty.Events;
using nutbotty.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace nutbotty
{
    class TwitchChatRoom
    {

        DateTime timeOfLastMessage = DateTime.Now;
        bool quoteListHasChanges = true;
        string currenQuotePasteBin;

        #region Member Variables
        TwitchChatConnection chatConnection;
        TwitchChatConnection whisperConnection;
        Channel channel;
        Random RNG = new Random();
        #endregion

        #region Constructors
        public TwitchChatRoom(TwitchChatConnection chatConnection, TwitchChatConnection whisperConnection, Channel channel)
        {
            this.chatConnection = chatConnection;
            this.whisperConnection = whisperConnection;
            this.channel = channel;

            chatConnection.join(this);
        }
        #endregion

        #region Getters
        internal TwitchChatConnection ChatConnection
        {
            get
            {
                return chatConnection;
            }
        }

        internal TwitchChatConnection WhisperConnection
        {
            get
            {
                return whisperConnection;
            }
        }

        internal Channel Channel
        {
            get
            {
                return channel;
            }
        }
        #endregion

        internal void RespondToChatEvent(Event chatEvent)
        {

            #region Bot Logic
            // Logic for responding to a "PRIVMSG" event
            if (chatEvent.GetType().Equals(typeof(ChatEvent)))
            {
                // Create a reference for the chat data
                ChatEvent chatData = (ChatEvent)(chatEvent);
                string message = chatData.ChatMessage;
                string user = chatData.User;
                string channelName = chatData.Channel;

                Program.nutbotty.Invoke(new Action(() =>
                {
                    Program.nutbotty.chatBox.Text += string.Format("<{0}> {1}" + Environment.NewLine, user, message);
                }));

                #region JOIN and PART commands
                if (message.Equals("!join") && channelName.Equals(Program.BOTNAME))
                {
                    Log.Message(user + " requested " + Program.BOTNAME + " to join their channel.", true);
                    if (!DatabaseHandler.ChannelExists(user))
                    {
                        Channel channel = new Channel(user);
                        DatabaseHandler.InsertChannel(channel);
                        new TwitchChatRoom(chatConnection, whisperConnection, channel);
                        SendChatMessage(Program.BOTNAME + " is now available for " + user + ". Type !commands for a list of commands you can use.");
                    }
                    else
                    {
                        SendChatMessage(Program.BOTNAME + " is already available for " + user + ".");
                    }
                }

                if (message.Equals("!part") && channelName.Equals(Program.BOTNAME))
                {
                    Log.Message(user + " requested " + Program.BOTNAME + " to part their channel.", true);
                    if (DatabaseHandler.ChannelExists(user))
                    {
                        Channel channel = new Channel(user);
                        DatabaseHandler.DeleteChannel(channel.channelName);
                        chatConnection.part(user);
                        SendChatMessage("@" + user + ", thank you for using " + Program.BOTNAME + ".Type !join if you ever want to use " + Program.BOTNAME + " again.");
                    }
                    else
                    {
                        SendChatMessage(Program.BOTNAME + " is not in #" + user + ".");
                    }
                }
                #endregion

                #region Generic Commands
                // Iterate through table rows in database and check if the trigger text matches the message
                for (int i = 0; i < DatabaseHandler.CommandsCount(); i++)
                {
                    //Retrieve command from the database and replace the appropriate strings
                    ChatCommand command = DatabaseHandler.GetCommandAtIndex(i);

                    string responseText = command.ResponseText;
                    responseText = responseText.Replace("$channel", channelName);
                    responseText = responseText.Replace("$user", user);

                    // Check if the command needs to be matched exactly or "loosely"
                    if ((command.MustBeExact && message.Equals(command.TriggerText)) || (!command.MustBeExact && message.Contains(command.TriggerText)))
                    {
                        // Check if the command is universal, or if the command is in the correct channel
                        if (command.IsUniversal || channelName.Equals(command.ChannelName))
                        {
                            // Check if the user is the subscriber (iff the command is subscriber only)
                            if ((command.SubscriberOnly && chatData.UserIsSubscriber) || !(command.SubscriberOnly))
                            {
                                // Check if the user is the moderator (iff the command is moderator only)
                                if ((command.ModeratorOnly && chatData.UserIsModerator) || !(command.ModeratorOnly))
                                {
                                    // Check if the user is the broadcaster (iff the command is broadcaster only)
                                    if ((command.BroadcasterOnly && chatData.UserIsBroadcaster) || !(command.BroadcasterOnly))
                                    {
                                        // Check if the command is whisper only
                                        if (command.WhisperResponse) { SendWhisper(user, responseText); }
                                        else { SendChatMessage(responseText); }
                                    }
                                    else
                                    {
                                        SendWhisper(user, command.TriggerText + " is only available to the broadcaster.");
                                    }
                                }
                                else
                                {
                                    SendWhisper(user, command.TriggerText + " is only available to moderators.");
                                }
                            }
                            else
                            {
                                SendWhisper(user, command.TriggerText + " is only available to subscribers.");
                            }
                        }
                    }
                }
                #endregion

                #region BLOCKED PHRASE Commands
                for (int i = 0; i < DatabaseHandler.PhraseCount(); i++)
                {
                    string phrase = DatabaseHandler.GetPhraseAtIndex(i).phrase;
                    if (message.Contains(phrase))
                    {
                        if (!chatData.UserIsModerator)
                        {
                            SendChatMessageNoAction(".timeout " + user + " 1");
                            SendWhisper(user, "Your messages have been purged from " + channelName + " for using the phrase \"" + phrase + "\".");
                            Log.Message(user + " has been timed out from " + channelName + " for using the phrase \"" + phrase + "\".", true);
                        }
                    }
                }

                // Add a phrase to the BLOCKED_PHRASES table
                if (message.StartsWith("!block "))
                {
                    // Parse the phrase text data
                    string phrase_text = message.Substring("!block ".Length);

                    // If the user is a moderator, add the quote to the database, else do nothing
                    if (chatData.UserIsModerator)
                    {
                        // Assume the command has no arguments, then split on space characters
                        bool hasArgs = false;
                        string[] args = message.Split(' ');

                        // If there is at least one argument, continue, otherwise end if
                        if (args.Length > 1) { hasArgs = true; }
                        else { Log.Message("<" + channelName + "> " + user + " attempted to block phrase, but there was not enough arguments.", true); }
                        // Add phrase to database if there were arguments and phrase doesn't already exist in the database
                        if (hasArgs)
                        {
                            if (DatabaseHandler.PhraseExists(phrase_text))
                            {
                                SendChatMessage(user + ", the phrase \"" + phrase_text + "\" is already blocked.");
                                Log.Message("<" + channelName + "> " + user + " attempted to block a phrase, but it already exists --> " + phrase_text, true);
                            }
                            else
                            {
                                BlockedPhrase phrase = new BlockedPhrase(phrase_text);
                                DatabaseHandler.InsertPhrase(phrase);
                                SendChatMessage(user + " blocked the phrase [" + (DatabaseHandler.PhraseCount() - 1) + "]: " + phrase_text);
                                Log.Message("<" + channelName + "> " + user + " blocked a phrase: " + phrase_text, true);
                            }
                        }
                    }
                    else
                    {
                        SendWhisper(user, "!block is only available to moderators");
                        Log.Message(user + " attempted to block a phrase but is not a moderator --> " + phrase_text, true);
                    }
                }

                // Delete a quote to the QUOTE table by searching the QuoteText column
                if (message.StartsWith("!unblock "))
                {
                    // Parse the quote text data
                    string phrase_text = message.Substring("!unblock ".Length);

                    // If the user is a moderator, add the quote to the database, else do nothing
                    if (chatData.UserIsModerator)
                    {
                        // Assume the command has no arguments
                        bool hasArgs = false;
                        // Split the command on space characters
                        string[] args = message.Split(' ');
                        // If there is at least one argument, continue, otherwise end if
                        if (args.Length > 1) { hasArgs = true; }
                        else { Log.Message("<" + channelName + "> " + user + " attempted to unblock a phrase, but there was not enough arguments.", true); }
                        // Add quote to database if there were arguments and the quote exists
                        if (hasArgs)
                        {
                            if (DatabaseHandler.PhraseExists(phrase_text))
                            {
                                DatabaseHandler.DeletePhrase(phrase_text);
                                SendChatMessage(user + " unblocked a phrase: " + phrase_text);
                                Log.Message("<" + channelName + "> " + user + " unblocked a phrase: " + phrase_text, true);
                            }
                            else
                            {
                                SendChatMessage(user + ", that phrase is already unblocked.");
                                Log.Message("<" + channelName + "> " + user + " attempted to block a phrase, but it does not exist --> " + phrase_text, true);
                            }
                        }
                    }
                    else
                    {
                        SendWhisper(user, "!unblock is only available to moderators");
                        Log.Message(user + " attempted to unblock a phrase but is not a moderator --> " + phrase_text, true);
                    }
                }
                #endregion

                #region QUOTE Commands
                // Pull a random quote from the QUOTES table
                Regex quoteRgx = new Regex(@"^!quote$");
                Regex quoteNumRgx = new Regex(@"^!quote [0-9]{1,}$");
                Quote foundQuote;
                
                if (quoteRgx.IsMatch(message))
                {
                    int ID = RNG.Next(0, DatabaseHandler.QuoteCount());
                    foundQuote = DatabaseHandler.GetQuoteAtIndex(ID);
                    Log.Message(user + " requested a random quote from the database.", true);
                    SendChatMessage(string.Format("[{0}] {1}", ID, foundQuote.quoteText));
                }
                else if (quoteNumRgx.IsMatch(message))
                {
                    string quoteNum = message.Remove(0, "!quote ".Length);
                    int ID = Convert.ToInt32(quoteNum);

                    if (ID >= 0 && ID < DatabaseHandler.QuoteCount()) {
                        foundQuote = DatabaseHandler.GetQuoteAtIndex(ID);
                        Log.Message(user + " requested for quote #" + ID + " from the database.", true);
                        SendChatMessage(string.Format("[{0}] {1}", ID, foundQuote.quoteText));
                    } else
                    {
                        SendWhisper(user, "There are only " + DatabaseHandler.QuoteCount() + " quotes in the database.");
                    }
                }
                else if (message.StartsWith("!quote "))
                {
                    string searchString = message.Remove(0, "!quote ".Length);
                    List<Quote> result = DatabaseHandler.SearchQuoteList(searchString);

                    if (result.Count > 0)
                    {
                        int ID = RNG.Next(0, result.Count);
                        foundQuote = result[ID];
                        Log.Message(user + " searched quote from the database.", true);
                        SendChatMessage(string.Format("{0}", foundQuote.quoteText));
                    }
                    else
                    {
                        SendWhisper(user, string.Format("The phease '{0}' was not found in the database.", searchString));
                    }
                }

                // Add a quote to the QUOTE table
                if (message.StartsWith("!addquote "))
                {
                    // Parse the quote text data
                    string quoteText = message.Substring("!addquote ".Length);
                    Quote quote = new Quote(quoteText, channelName, user, DateTime.Now);

                    // If the user is a moderator, add the quote to the database, else do nothing
                    if (chatData.UserIsModerator)
                    {
                        // Assume the command has no arguments, then split on space characters
                        bool hasArgs = false;
                        string[] args = message.Split(' ');

                        // If there is at least one argument, continue, otherwise end if
                        if (args.Length > 1) { hasArgs = true; }
                        else { Log.Message("<" + channelName + "> " + user + " attempted to add quote, but there was not enough arguments.", true); }
                        // Add quote to database if there were arguments and quote doesn't already exist in the database
                        if (hasArgs)
                        {
                            if (DatabaseHandler.QuoteExists(quoteText))
                            {
                                SendChatMessage(user + ", that quote is already in the database.");
                                Log.Message("<" + channelName + "> " + user + " attempted to add quote, but it already exists --> " + quoteText, true);
                            }
                            else
                            {
                                DatabaseHandler.InsertQuote(quote);
                                quoteListHasChanges = true;
                                SendChatMessage(user + " added quote [" + (DatabaseHandler.QuoteCount() - 1) + "]: " + quoteText);
                                Log.Message("<" + channelName + "> " + user + " added quote: " + quoteText, true);
                            }
                        }
                    }
                    else
                    {
                        SendWhisper(user, "!addquote is only available to moderators");
                        Log.Message(user + " attempted to add a quote but is not a moderator --> " + quoteText, true);
                    }
                }

                // Delete a quote to the QUOTE table by searching the QuoteText column
                if (message.StartsWith("!delquote "))
                {
                    // Parse the quote text data
                    string quoteText = message.Substring("!delquote ".Length);

                    // If the user is a moderator, add the quote to the database, else do nothing
                    if (chatData.UserIsModerator)
                    {
                        // Assume the command has no arguments
                        bool hasArgs = false;
                        // Split the command on space characters
                        string[] args = message.Split(' ');
                        // If there is at least one argument, continue, otherwise end if
                        if (args.Length > 1) { hasArgs = true; }
                        else { Log.Message("<" + channelName + "> " + user + " attempted to delete a quote, but there was not enough arguments.", true); }
                        // Add quote to database if there were arguments and the quote exists
                        if (hasArgs)
                        {
                            if (DatabaseHandler.QuoteExists(quoteText))
                            {
                                DatabaseHandler.DeleteQuote(quoteText);
                                quoteListHasChanges = true;
                                SendChatMessage(user + " deleted quote: " + quoteText);
                                Log.Message("<" + channelName + "> " + user + " deleted quote: " + quoteText, true);
                            }
                            else
                            {
                                SendChatMessage(user + ", that quote was not found in the database.");
                                Log.Message("<" + channelName + "> " + user + " attempted to deleted quote, but it does not exist --> " + quoteText, true);
                            }
                        }
                    }
                    else
                    {
                        SendWhisper(user, "!delquote is only available to moderators");
                        Log.Message(user + " attempted to add a quote but is not a moderator --> " + quoteText, true);
                    }
                }

                if (message.Equals("!quotelist"))
                {

                    if (quoteListHasChanges || currenQuotePasteBin.Equals(null))
                    {

                        string pastebinTitle = Program.BOTNAME + " quote list as of " + DateTime.Now;
                        string pastebinContent = "";

                        List<Quote> quoteList = DatabaseHandler.GetQuoteList();

                        for (int i = 0; i < quoteList.Count; i++)
                        {
                            pastebinContent += "[" + i + "] " + quoteList[i].quoteText + Environment.NewLine;
                        }

                        quoteListHasChanges = false;
                        currenQuotePasteBin = "Click here for a list of quotes ➤ " + Pastebin.Post(pastebinTitle, pastebinContent).Result;
                        SendChatMessage(currenQuotePasteBin);

                    } else
                    {

                        SendChatMessage(currenQuotePasteBin);
                        
                    }

                }
                #endregion

                #region STRAWPOLL Parser
                if (message.Contains("strawpoll.me/"))
                {
                    if (StrawpollParser.GetStrawpollInfo(message) == null)
                    {
                        SendChatMessage(user + ", that is not a valid Strawpoll");
                    }
                    else
                    {
                        SendChatMessage(user + " pasted a Strawpoll ➤ " + StrawpollParser.GetStrawpollInfo(message));
                    }
                }
                #endregion

                #region YOUTUBE Parser
                if (message.Contains("youtube.com/") || message.Contains("youtu.be/"))
                {
                    Console.WriteLine("YouTube Link detected: " + message);
                    if (YouTubeParser.GetYouTubeVideoID(message) != null)
                    {
                        SendChatMessage(user + " pasted a YouTube video ➤ " + YouTubeParser.GetYouTubeInfo(message, YouTubeParser.IS_VIDEO));
                    }
                    if (YouTubeParser.GetYouTubePlaylistID(message) != null)
                    {
                        SendChatMessage(user + " pasted a YouTube playlist ➤ " + YouTubeParser.GetYouTubeInfo(message, YouTubeParser.IS_PLAYLIST));
                    }
                }
                #endregion

                #region GUESSING Commands
                // CeresBot Guesses
                if (message.Contains(@"Round Started. Type !guess") && user.Equals("ceresbot"))
                {
                    int seed = RNG.Next(100);

                    // 30% chance of 45 seconds | 65% chance of 46 seconds | 5% chance of 47 seconds
                    int seconds;
                    if (seed < 30) { seconds = 45; }
                    else if (seed < 95) { seconds = 46; }
                    else { seconds = 47; }

                    // if 45-46 seconds, milliseconds between 0-99, else between 0-25
                    int milliseconds;
                    if (seconds < 47) { milliseconds = RNG.Next(100); }
                    else { milliseconds = RNG.Next(25); }

                    // Make the guess
                    SendChatMessageNoAction("!guess " + seconds + "\"" + milliseconds.ToString("00"));
                }

                // Phantoon guesses
                if (message.Equals("!phantoon"))
                {
                    int[] rands = new int[2];
                    string label = null;
                    int total = 0;

                    // Calculate prediction
                    for (int i = 0; i < rands.Length; i++)
                    {
                        rands[i] = RNG.Next(1, 4);
                        total += rands[i];
                        if (rands[i] == 1) { label = label + " FAST"; }
                        else if (rands[i] == 2) { label = label + " MID"; }
                        else { label = label + " SLOW"; }
                    }

                    // Send chat message
                    if (total <= 2) { SendChatMessage("predicts " + label + ". THE RNG LORDS ARE WITH US PogChamp"); }
                    else if (total > 2 && total <= 3) { SendChatMessage("predicts " + label + ". Praise Jesus BloodTrail"); }
                    else if (total > 3 && total <= 4) { SendChatMessage("predicts " + label + ". Maybe this won't be a reset after all OMGScoots"); }
                    else if (total > 5 && total <= 6) { SendChatMessage("predicts " + label + ". Phantoon please BibleThump"); }
                    else if (total == 6) { SendChatMessage("predicts " + label + ". You motherfucker. RESET RESET RESET SwiftRage"); }
                }

                // Eyedoor guesses
                if (message.Equals("!eyedoor"))
                {
                    int rand = RNG.Next(0, 5);

                    // Send chat message
                    if (rand == 0) { SendChatMessage("predicts... ZERO beams. THE RNG GODS ARE WITH YOU PogChamp"); }
                    else if (rand == 1) { SendChatMessage("predicts... ONE beam. Allelujah! BloodTrail"); }
                    else if (rand == 2) { SendChatMessage("predicts... TWO beams. You're lucky this time OMGScoots"); }
                    else if (rand == 3) { SendChatMessage("predicts... THREE beams. Come on eye door! DansGame"); }
                    else if (rand == 4) { SendChatMessage("predicts... FOUR beams. DAFUQ BITCH?! SwiftRage"); }
                }
                #endregion

                #region OTHER Commands
                // Show the uptime for the stream. Information is pulled from DecAPI API by Alex Thomassen
                if (message.Equals("!uptime"))
                {
                    string uptime = GetUptime(chatData.Channel);
                    SendChatMessage("@" + user + ": " + uptime);
                    Log.Message(user + " checked the uptime for #" + chatData.Channel + ": " + uptime, true);
                }

                // Check how many points Nutbotty has on ceresbot
                if (message.Contains(Program.BOTNAME) && message.Contains("how many points") && channelName.Equals("oatsngoats"))
                {
                    SendChatMessageNoAction("!points");
                }

                // Commit sudoku
                if (message.Equals("!sudoku"))
                {
                    if (!chatData.UserIsModerator)
                    {
                        SendChatMessageNoAction(".timeout " + user + " 1");
                        SendChatMessage(user + " committed sudoku.");
                    }
                }

                if (message.Equals("!foosdaraid"))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        SendChatMessage("Foosda Raid ( ͡° ͜ʖ ͡°)");
                    }
                }

                // Block thinking emoji
                if (message.Equals("🤔"))
                {
                    SendChatMessageNoAction(".timeout " + user + " 1");
                    SendWhisper(user, "Your messages have been purged from " + channelName + " for using the thinking emoji. Go sit in the corner.");
                    Log.Message(user + " has been timed out from " + channelName + " for using the thinking emoji. Go sit in the corner.", true);
                }

                // FAQ
                if ((message.Contains("What") || message.Contains("what")) && (message.Contains(" rbo") || message.Contains(" RBO") || message.Contains(" rbo ") || message.Contains(" RBO ")))
                {
                    SendChatMessage("RBO stand for Reverse Boss Order. It requires beating the four statue bosses in the following order: Ridley, Draygon, Phantoon, Kraid.");
                }

                if (message.StartsWith("I think") || message.StartsWith("i think"))
                {
                    SendChatMessage("Nobody care what you think, " + user);
                }

                #endregion
            }

            // Logic for responding to an unknown event
            else
            {
                //Log.Message(chatEvent.ToString(), false);
            }
            #endregion
        
    }

        #region Helper Methods
        /// <summary>
        /// Send a chat message to the chat room
        /// </summary>
        /// <param name="message">Message to send to the chat room</param>
        internal void SendChatMessage(string message)
        {
            this.chatConnection.IrcClient.SendChatMessage(this.channel.channelName, message);

            if (Program.nutbotty != null)
            {
                //Console.WriteLine("[" + DateTime.Now + "] " + message);
                Program.nutbotty.Invoke(new Action(() =>
                {
                    Program.nutbotty.chatBox.Text += string.Format("<{0}> {1}" + Environment.NewLine, this.channel.channelName, message);
                }));
            }

            Log.Message(string.Format("<{0}> {1}", this.channel.channelName, message), true);
        }

        /// <summary>
        /// Send a chat message to the chat room
        /// </summary>
        /// <param name="message">Message to send to the chat room</param>
        internal void SendChatMessageNoAction(string message)
        {
            this.chatConnection.IrcClient.SendChatMessageNoAction(this.channel.channelName, message);

            if (Program.nutbotty != null)
            {
                //Console.WriteLine("[" + DateTime.Now + "] " + message);
                Program.nutbotty.Invoke(new Action(() =>
                {
                    Program.nutbotty.chatBox.Text += string.Format("<{0}> {1}" + Environment.NewLine, this.channel.channelName, message);
                }));
            }

            Log.Message(string.Format("<{0}> {1}", this.channel.channelName, message), true);
        }

        /// <summary>
        /// Send a whisper to a designated user
        /// </summary>
        /// <param name="user">Username of recpient</param>
        /// <param name="message">Message to whisper</param>
        internal void SendWhisper(string user, string message)
        {
            this.whisperConnection.IrcClient.SendWhisper(user, message);

            if (Program.nutbotty != null)
            {
                //Console.WriteLine("[" + DateTime.Now + "] " + message);
                Program.nutbotty.Invoke(new Action(() =>
                {
                    Program.nutbotty.consoleBox.Text += string.Format("{0} >> {1}: {2}" + Environment.NewLine, Program.BOTNAME, user, message);
                }));
            }

            Log.Message(string.Format("{0} >> {1}: {2}", Program.BOTNAME, user, message), true);
        }


        /// <summary>
        /// Show the uptime+ for the stream. Information is pulled from DecAPI.me API by Alex Thomassen
        /// </summary>
        /// <param name="channel">The channel to check</param>
        /// <param name="irc">IRC client</param>
        /// <param name="user">User that is requesting the uptime</param>
        internal string GetUptime(string channel)
        {
            string urlData = String.Empty;
            WebClient wc = new WebClient();
            urlData = wc.DownloadString("https://decapi.me/twitch/uptime.php?channel=" + channel);

            if (urlData.Equals("Channel is not live.")) { return (channel + " is currently offline."); }
            else { return channel + " has been live for " + urlData; }
        }
        #endregion

    }
}
