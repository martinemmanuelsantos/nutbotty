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
    class YouTubeParser
    {

        const string YOUTUBE_API_KEY = "AIzaSyBvU27YJOALiCuQe9YVT7AzO1GFoMTwBLg";
        public const int IS_VIDEO = 1;
        public const int IS_PLAYLIST = 2;

        #region Methods
        /// <summary>
        /// Show the information of a YouTube video given a link. Information is pulled from the https://www.googleapis.com/youtube/v3/ endpoint
        /// </summary>
        /// <param name="link">Link to the YouTube video or playlist</param>
        /// <returns></returns>
        public static string GetYouTubeInfo(string link, int type)
        {

            // Parse the YouTube ID
            string ID;
            if (type == IS_VIDEO) { ID = GetYouTubeVideoID(link); }
            else if (type == IS_PLAYLIST) { ID = GetYouTubePlaylistID(link); }
            else return null;
            if (ID == null) return null;

            // Fetch the data from the Google API YouTube endpoint, using the API key in the Nutbotty class
            // Return null if both IDs are empty
            WebClient wc = new WebClient();
            string jsonString = null;

            if (type == IS_VIDEO)
            {
                jsonString = wc.DownloadString("https://www.googleapis.com/youtube/v3/videos?part=snippet&id=" + ID + "&key=" + YOUTUBE_API_KEY);
            }
            else if (type == IS_PLAYLIST)
            {
                jsonString = wc.DownloadString("https://www.googleapis.com/youtube/v3/playlists?part=snippet&id=" + ID + "&key=" + YOUTUBE_API_KEY);
            }
            else return null;

            JObject videoObject = JObject.Parse(jsonString);
            // Fetch the data in the fields "items" > "snippet" > "title" and "items" > "snippet" > "channelTitle"
            string info = String.Empty;
            try
            {
                JToken items = videoObject.SelectToken("items")[0].SelectToken("snippet");
                info = items["title"].ToString() + " | by " + items["channelTitle"].ToString();
            }
            catch (Exception e)
            {
                Log.Message("Could not retrieve YouTube Info: " + e, false);
            }
            return info;
        }

        /// <summary>
        /// Determine if a link is a YouTube video or not
        /// </summary>
        /// <param name="link">Link to the YouTube video or playlist</param>
        /// <returns></returns>
        public static string GetYouTubeVideoID(string link)
        {
            // Regex for video ID
            Regex rgxVideo = new Regex(@"youtu(?:\.be|be\.com)\/(?:.*v(?:\/|=)|(?:.*\/)?)([a-zA-Z0-9-_]+)");
            Match videoMatch = rgxVideo.Match(link);

            if (videoMatch.Success) { return videoMatch.Groups[1].Value; }
            else { return null; }
        }

        /// <summary>
        /// Determine if a link is a YouTube playlist or not 
        /// </summary>
        /// <param name="link">Link to the YouTube video or playlist</param>
        /// <returns></returns>
        public static string GetYouTubePlaylistID(string link)
        {
            // Regex for video ID
            Regex rgxPlaylist = new Regex(@"youtu(?:\.be|be\.com)\/.*list(?:\/|=)([a-zA-Z0-9-_]+)");
            Match playlistMatch = rgxPlaylist.Match(link);

            if (playlistMatch.Success) { return playlistMatch.Groups[1].Value; }
            else { return null; }
        }
        #endregion

    }
}
