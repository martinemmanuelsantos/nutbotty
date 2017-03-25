using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace nutbotty.Events
{

    public abstract class Event
    {

        internal static Event parseEvent(string ircString)
        {
            #region Event Regular Expressions
            // PING
            Regex pingRgx = new Regex(@"PING.*$");

            // PRIVMSG (for regular chat messages) (use #[.0-9a-fA-F]{6} for hexadecimal strings)
            Regex chatRgx =
                new Regex(@"@badges=.*;color=.*;display-name=.*;emotes=.*;id=.*;mod=[0-1];room-id=[0-9]*(;sent-ts=[0-9]*)?;subscriber=[0-1];tmi-sent-ts=.*;turbo=[0-1];user-id=[0-9]*;user-type=.* :.+!.+@.+\.tmi\.twitch\.tv PRIVMSG #.+ :.*$");

            // WHISPER (for whisper messages) (use #[.0-9a-fA-F]{6} for hexadecimal strings)
            Regex whisperRgx =
                new Regex(@"@badges=.*;color=.*;display-name=.*;emotes=.*;message-id=[0-9]*;thread-id=.*;turbo=[0-1];user-id=[0-9]*;user-type=.* :.+!.+@.+\.tmi\.twitch\.tv WHISPER .+ :.*$");

            // Host event
            Regex hostRgx = new Regex(@"jtv!jtv@jtv\.tmi\.twitch\.tv PRIVMSG .+ :.+ is now hosting you\.$");
            #endregion

            // Check what the event type is, then pass the relevant instance
            if (pingRgx.IsMatch(ircString)) { return new PingEvent(); }
            else if (chatRgx.IsMatch(ircString)) { return new ChatEvent(ircString); }
            else if (whisperRgx.IsMatch(ircString)) { return new WhisperEvent(ircString); }
            else if (hostRgx.IsMatch(ircString)) { return new HostEvent(ircString); }
            else { return new UnknownEvent(ircString); }
        }

        /// <summary>
        /// Parses the chat portion given a full IRC string
        /// </summary>
        /// <param name="ircString">The full IRC string</param>
        /// <returns></returns>
        internal static string ParseMessage(string ircString)
        {
            // Regex for chat message or whisper events
            Regex rgxMessage = new Regex(@".+ (?:PRIVMSG|WHISPER) #.+ :(.+)");
            Match messageMatch = rgxMessage.Match(ircString);

            if (messageMatch.Success)
            {
                string message = messageMatch.Groups[1].Value;
                if (message.StartsWith("\x01" + "ACTION "))
                {
                    message = Regex.Replace(message, "\x01", "", RegexOptions.Compiled);
                    message = message.Remove(0, "ACTION ".Length);
                }
                return message;
            }
            else { return ""; }
        }

        /// <summary>
        /// Parses the username of the user that sent the chat message
        /// </summary>
        /// <param name="ircString">The full IRC string</param>
        /// <returns></returns>
        internal static string ParseUser(string ircString)
        {
            string user = ircString;                                                                            // Begin with the IRC message
            int startIndex = user.IndexOf(' ');
            user = user.Substring(startIndex);
            startIndex = user.IndexOf(':');
            int endIndex = user.IndexOf('!');
            int length = endIndex - startIndex - 1;
            user = user.Substring(startIndex + 1, length);                                                      // Keep substring between first ':' and first '!'
            return user;
        }

        #region Metatag Parser Methods
        /// <summary>
        /// Get metatags for chat message
        /// </summary>
        /// <param name="ircString">Full IRC string</param>
        /// <returns></returns>
        internal List<string> GetMetatags(string ircString)
        {
            int space = ircString.IndexOf(' ');
            string metatagString = ircString.Substring(0, space);
            List<string> metatags = metatagString.Split(';').ToList();
            return metatags;
        }

        /// <summary>
        /// Find the string data associated with a given metatag, given a list of metatags
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal string FindMetatagData(List<string> metatags, string tag)
        {
            string metatagData = "";
            foreach (string metatag in metatags)
            {
                if (metatag.StartsWith(tag))
                {
                    metatagData = metatag.Replace(tag, "");
                    break;
                }
            }
            return metatagData;
        }

        /// <summary>
        /// Parse the user's badges
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal string ParseUserBadges(List<string> metatags)
        {
            return FindMetatagData(metatags, "@badges=");
        }

        /// <summary>
        /// Parse the user's color as a hexadecimal string
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal string ParseUserColor(List<string> metatags)
        {
            return FindMetatagData(metatags, "color=");
        }

        /// <summary>
        /// Parse the user's display name, including all capitalization
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal string ParseDisplayName(List<string> metatags)
        {
            return FindMetatagData(metatags, "display-name=");
        }

        /// <summary>
        /// Parse the emotes used in the string
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal string ParseEmotes(List<string> metatags)
        {
            return FindMetatagData(metatags, "emotes=");
        }

        /// <summary>
        /// Parse the moderator status of the user
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal bool ParseHasMod(List<string> metatags)
        {
            string turboData = FindMetatagData(metatags, "mod=");
            if (turboData.Equals("1")) { return true; }
            else return false;
        }

        /// <summary>
        /// Parse the room ID
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal string ParseRoomId(List<string> metatags)
        {
            return FindMetatagData(metatags, "room-id=");
        }

        /// <summary>
        /// Parse the Twitch turbo status of the user
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal bool ParseHasTurbo(List<string> metatags)
        {
            string turboData = FindMetatagData(metatags, "turbo=");
            if (turboData.Equals("1")) { return true; }
            else return false;
        }

        /// <summary>
        /// Parse the user ID
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal long ParseUserId(List<string> metatags)
        {
            return Convert.ToInt64(FindMetatagData(metatags, "user-id="));
        }

        /// <summary>
        /// Parse the user type (mod, global-mod, admin, staff)
        /// </summary>
        /// <param name="metatags">Complete list of metatags</param>
        /// <returns></returns>
        internal string ParseUserType(List<string> metatags)
        {
            return FindMetatagData(metatags, "user-type=");
        }
        #endregion

    }
}
