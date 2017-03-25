using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
namespace nutbotty
{
    class IrcClient
    {
        private string ip;
        private int port;
        private string username;                            // Username to login with
        private string password;                            // Oauth Token of the user
        private int reconnectTime;                          // Number of seconds to wait between reconnects

        private TcpClient tcpClient;
        private StreamReader inputStream;
        private StreamWriter outputStream;


        /// <summary>
        /// IrcClient class constructor
        /// Logs in with the provided username and Oauth password
        /// </summary>
        /// <param name="ip">IP address of IRC server</param>
        /// <param name="port">Port number of server</param>
        /// <param name="reconnectTime">Number of seconds to wait between reconnects</param>
        /// <param name="username">Username to login with</param>
        /// <param name="password">Oauth Token of the user</param>
        public IrcClient(string ip, int port, int reconnectTime, string username, string password)
        {
            this.ip = ip;
            this.port = port;
            this.reconnectTime = reconnectTime;
            this.username = username;
            this.password = password;
        }

        public void Connect()
        {
            Log.Message("Attempting to connect to " + ip + " | Port: " + port, true);

            // Try connection, if it fails, reconnect indefinitely
            // Wait time is specified by reconnectTime
            bool connected = false;
            while (!connected)
            {
                try
                {
                    tcpClient = new TcpClient(ip, port);
                    if (tcpClient != null) connected = true;
                }
                catch (Exception e)
                {
                    Log.Message("Cannot connect to " + ip + ", retrying in " + reconnectTime + " seconds.", true);
                    System.Threading.Thread.Sleep(1000 * reconnectTime);
                }
            }


            inputStream = new StreamReader(tcpClient.GetStream());
            outputStream = new StreamWriter(tcpClient.GetStream());

            outputStream.WriteLine("PASS " + password);
            outputStream.WriteLine("NICK " + username);
            outputStream.WriteLine("USER " + username + " 8 * :" + username);
            outputStream.Flush();

            Log.Message("Connected to IRC server: " + ip, true);
            Log.Message("Logged in as " + username, true);



            SendIrcString("CAP REQ :twitch.tv/tags");
            SendIrcString("CAP REQ :twitch.tv/membership");

        }

        /// <summary>
        /// Join a given channel
        /// </summary>
        /// <param name="channel">The channel to join</param>
        public void JoinChannel(string channel)
        {
            outputStream.WriteLine("JOIN #" + channel);
            outputStream.Flush();
            Log.Message("Joined #" + channel, true);
        }

        /// <summary>
        /// Join a given channel
        /// </summary>
        /// <param name="channel"></param>
        public void PartChannel(string channel)
        {
            outputStream.WriteLine("PART #" + channel);
            outputStream.Flush();
            Log.Message("Parted #" + channel, true);
        }

        /// <summary>
        /// Wrapper method for sending IRC messages
        /// </summary>
        /// <param name="message">Raw string to send to IRC server</param>
        public void SendIrcString(string message)
        {
            outputStream.WriteLine(message);
            outputStream.Flush();
            Log.Message(message, false);
        }

        /// <summary>
        /// Retrieves entire IRC message
        /// </summary>
        /// <returns>Entire IRC message</returns>
        public string ReadIrcString()
        {
            String message = inputStream.ReadLine();
            Log.Message(message, false);
            return message;
        }

        /// <summary>
        /// Method for sending pre-formatted Twitch chat messages
        /// </summary>
        /// <param name="channel">Channel to send chat message</param>
        /// <param name="message">Chat message</param>
        public void SendChatMessage(string channel, string message)
        {
            SendIrcString(":" + username + "!" + username + "@" + username + ".tmi.twitch.tv PRIVMSG #" + channel + " :/me " + message);
            outputStream.Flush();
            Log.Message(message, false);
        }

        /// <summary>
        /// Method for sending pre-formatted Twitch chat messages (with /me string)
        /// </summary>
        /// <param name="channel">Channel to send chat message</param>
        /// <param name="message">Chat message</param>
        public void SendChatMessageNoAction(string channel, string message)
        {
            SendIrcString(":" + username + "!" + username + "@" + username + ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message);
            outputStream.Flush();
            Log.Message(message, false);
        }

        /// <summary>
        /// Mehod for sending pre-formatted whisper message
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        public void SendWhisper(string user, string message)
        {
            SendIrcString("PRIVMSG #jtv :/w " + user + " /me " + message);
            outputStream.Flush();
            Log.Message(message, false);
        }

    }
}
