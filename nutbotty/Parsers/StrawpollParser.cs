using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace nutbotty.Parsers
{
    class StrawpollParser
    {

        /// <summary>
        /// Show the title given a strawpoll link. Information is pulled from the https://strawpoll.me/api/v2/ endpoint
        /// </summary>
        /// <param name="link">Link to the strawpoll</param>
        /// <returns></returns>
        public static string GetStrawpollInfo(string link)
        {
            // Parse the Strawpoll ID
            string ID;
            Regex rgxStrawpoll = new Regex(@"strawpoll.me\/([a-zA-Z0-9-_]+)");
            Match strawpollMatch = rgxStrawpoll.Match(link);
            if (strawpollMatch.Success) { ID = strawpollMatch.Groups[1].Value; }
            else { return null; }
            if (ID == null) return null;

            // Fetch the data from the strawpoll API endpoint, then create a JSON object using the data
            WebClient wc = new WebClient();
            string jsonString = null;
            try
            {
                jsonString = wc.DownloadString("https://strawpoll.me/api/v2/polls/" + ID);
                JObject jsonObject = JObject.Parse(jsonString);
                // Fetch the data matching the field "title"
                string questionString = jsonObject["title"].ToString();

                // ... and "option" (then parse the data into a single line)
                JToken[] options = jsonObject["options"].ToArray();
                string optionString = null;
                foreach (JToken option in options)
                {
                    optionString = optionString + option + " — ";
                }
                optionString = optionString.Remove(optionString.LastIndexOf("—") - 1, 3);

                // Finally, return the data in a formatted string
                return questionString + " (Available options: " + optionString + ")";
            }
            catch (Exception e)
            {
                Log.Message(e.Message, true);
            }
            return null;
        }
    }
}
