using nutbotty.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nutbotty
{
    class DatabaseHandler
    {

        public static String dbConnection = "nutbotty.sqlite";

        #region Constructors

        public DatabaseHandler()
        {

            if (!System.IO.File.Exists("nutbotty.sqlite"))
            {
                Log.Message("No database found, creating nutbotty.sqlite.", true);
                SQLiteConnection.CreateFile(dbConnection);
            }

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + dbConnection))
            {
                conn.Open();

                new SQLiteCommand(
                    @"CREATE TABLE IF NOT EXISTS channels (channel_id INTEGER PRIMARY KEY AUTOINCREMENT, 
                        channel_name TEXT)"
                    , conn).ExecuteNonQuery();

                new SQLiteCommand(
                    @"CREATE TABLE IF NOT EXISTS quotes (quote_id INTEGER PRIMARY KEY AUTOINCREMENT, 
                        quote_text TEXT, 
                        channel_name TEXT, 
                        added_by TEXT, 
                        date_added TEXT)"
                    , conn).ExecuteNonQuery();

                new SQLiteCommand(
                    @"CREATE TABLE IF NOT EXISTS blocked_phrases (phrase_id INTEGER PRIMARY KEY AUTOINCREMENT, 
                        phrase TEXT)"
                    , conn).ExecuteNonQuery();

                new SQLiteCommand(
                    @"CREATE TABLE IF NOT EXISTS commands (command_id INTEGER PRIMARY KEY AUTOINCREMENT, 
                        trigger_text TEXT, 
                        response_text TEXT, 
                        channel_name TEXT, 
                        is_universal BIT, 
                        must_be_exact BIT, 
                        whisper_response BIT, 
                        subscriber_only BIT, 
                        moderator_only BIT, 
                        broadcaster_only BIT)"
                    , conn).ExecuteNonQuery();

            }

        }

        #endregion

        #region Generic Database Methods

        private static DataTable GetDataTable(string tablename)
        {

            DataTable dataTable = new DataTable();

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + dbConnection))
            {
                conn.Open();

                SQLiteCommand command = conn.CreateCommand();

                command.CommandText = string.Format("SELECT * FROM {0}", tablename);

                SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);

                adapter.AcceptChangesDuringFill = false;

                adapter.Fill(dataTable);

                conn.Close();

                return dataTable;

            }

        }

        private static DataTable SearchDataTable(string tablename, string columnname, string searchstring)
        {

            DataTable dataTable = new DataTable();

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + dbConnection))
            {
                conn.Open();

                SQLiteCommand command = conn.CreateCommand();

                command.CommandText = string.Format("SELECT * FROM {0} WHERE {1} LIKE '%{2}%'", tablename, columnname, searchstring);

                SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);

                adapter.AcceptChangesDuringFill = false;

                adapter.Fill(dataTable);

                conn.Close();

                return dataTable;

            }

        }

        private static DataRow GetDataRow(string tablename, int index)
        {

            DataTable dataTable = GetDataTable(tablename);

            if (index < dataTable.Rows.Count)
            {
                return dataTable.Rows[index];
            } else
            {
                return null;
            }

        }

        private static bool DoesRecordExist(string tablename, string columnname, string value)
        {

            int result;

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + dbConnection))
            {
                conn.Open();

                SQLiteCommand command = conn.CreateCommand();

                command.CommandText = string.Format(@"SELECT COUNT(*) FROM {0} WHERE {1}=@value", tablename, columnname);
                command.Parameters.Add("@value", DbType.String).Value = value;

                result = Convert.ToInt32(command.ExecuteScalar());

                conn.Close();

            }

            return (result != 0) ? true : false;

        }

        private static int RecordsCount(string tablename)
        {

            int result;

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + dbConnection))
            {
                conn.Open();

                SQLiteCommand command = conn.CreateCommand();

                command.CommandText = string.Format(@"SELECT COUNT(*) FROM {0}", tablename);

                result = Convert.ToInt32(command.ExecuteScalar());

                conn.Close();

            }

            return result;

        }

        private static void InsertEntry(string tableName, string columnNames, string parameterNames, Action<SQLiteCommand> addParameters)
        {

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + dbConnection))
            {

                SQLiteCommand command = conn.CreateCommand();

                command.CommandText = string.Format(@"INSERT INTO {0} ({1}) VALUES ({2})", tableName, columnNames, parameterNames);
                addParameters(command);

                // Execute insert command
                try { conn.Open(); command.ExecuteNonQuery(); }
                catch (SQLiteException e) { Log.Message(e.Message, true); }
                finally { conn.Close(); }

            }

        }

        private static void DeleteEntry(string tableName, string columnName, string parameterString)
        {

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + dbConnection))
            {

                // Build up the SQL command
                string parameter = parameterString.Replace("'", "''");                   // Escape ' characters with ''
                string deleteStatement = string.Format(@"DELETE FROM {0} WHERE {1} = '{2}'", tableName, columnName, parameter);

                // Add the parameters of the object to be saved into database
                SQLiteCommand command = new SQLiteCommand(deleteStatement, conn);

                // Execute delete command
                try { conn.Open(); command.ExecuteNonQuery(); }
                catch (SQLiteException e) { Log.Message(e.Message, true); }
                finally { conn.Close(); }

            }

        }

        #endregion

        #region Channels Table

        public static List<Channel> GetChannelList()
        {

            DataTable channelTable = GetDataTable("channels");
            List<Channel> channelList = new List<Channel>();

            foreach (DataRow row in channelTable.Rows)
            {
                Channel channel = new Channel(row["channel_name"].ToString());
                channelList.Add(channel);
            }

            return channelList;

        }

        public static Channel GetChannelAtIndex(int index)
        {
            DataRow row = GetDataRow("channels", index);

            Channel channel = new Channel(row["channel_name"].ToString());
            return channel;
        }

        public static bool ChannelExists(string channelname)
        {
            return DoesRecordExist("channels", "channel_name", channelname);
        }

        public static int ChannelsCount()
        {
            return RecordsCount("channels");
        }

        public static void InsertChannel(Channel channel)
        {
            string tableName = "channels";
            string columnNames = "channel_name";
            string parameterNames = @"@channel_name";

            Action<SQLiteCommand> addParameters = (insertCommand) =>
            {
                insertCommand.Parameters.AddWithValue("@channel_name", channel.channelName);
            };

            InsertEntry(tableName, columnNames, parameterNames, addParameters);
        }

        public static void DeleteChannel(string channel_name)
        {
            DeleteEntry("channels", "channel_name", channel_name);
        }

        #endregion

        #region Quotes Table

        public static List<Quote> GetQuoteList()
        {

            DataTable quoteTable = GetDataTable("quotes");
            List<Quote> quoteList = new List<Quote>();

            foreach (DataRow row in quoteTable.Rows)
            {
                Quote quote = new Quote(
                    row["quote_text"].ToString(),
                    row["channel_name"].ToString(),
                    row["added_by"].ToString(),
                    Convert.ToDateTime(row["date_added"].ToString())
                    );
                quoteList.Add(quote);
            }

            return quoteList;

        }

        public static List<Quote> SearchQuoteList(string searchstring)
        {

            DataTable quoteTable = SearchDataTable("quotes", "quote_text", searchstring);
            List<Quote> quoteList = new List<Quote>();

            foreach (DataRow row in quoteTable.Rows)
            {
                Quote quote = new Quote(
                    row["quote_text"].ToString(),
                    row["channel_name"].ToString(),
                    row["added_by"].ToString(),
                    Convert.ToDateTime(row["date_added"].ToString())
                    );
                quoteList.Add(quote);
            }

            return quoteList;

        }

        public static Quote GetQuoteAtIndex(int index)
        {
            DataRow row = GetDataRow("quotes", index);

            Quote quote = new Quote(
                    row["quote_text"].ToString(),
                    row["channel_name"].ToString(),
                    row["added_by"].ToString(),
                    Convert.ToDateTime(row["date_added"].ToString())
                    );

            return quote;
        }

        public static bool QuoteExists(string quote_text)
        {
            return DoesRecordExist("quotes", "quote_text", quote_text);
        }

        public static int QuoteCount()
        {
            return RecordsCount("quotes");
        }

        public static void InsertQuote(Quote quote)
        {
            string tableName = "quotes";
            string columnNames = "quote_text, channel_name, added_by, date_added";
            string parameterNames = @"@quote_text, @channel_name, @added_by, @date_added";

            Action<SQLiteCommand> addParameters = (insertCommand) =>
            {
                insertCommand.Parameters.AddWithValue("@quote_text", quote.quoteText);
                insertCommand.Parameters.AddWithValue("@channel_name", quote.channel);
                insertCommand.Parameters.AddWithValue("@added_by", quote.addedBy);
                insertCommand.Parameters.AddWithValue("@date_added", quote.dateAdded);
            };

            InsertEntry(tableName, columnNames, parameterNames, addParameters);
        }

        public static void DeleteQuote(string quote_text)
        {
            DeleteEntry("quotes", "quote_text", quote_text);
        }

        #endregion

        #region Commands Table

        public static List<ChatCommand> GetCommandList()
        {

            DataTable comandsTable = GetDataTable("commands");
            List<ChatCommand> commandsList = new List<ChatCommand>();

            foreach (DataRow row in comandsTable.Rows)
            {
                ChatCommand command = new ChatCommand(
                    row["trigger_text"].ToString(),
                    row["response_text"].ToString(),
                    row["channel_name"].ToString(),
                    (bool)row["is_universal"],
                    (bool)row["must_be_exact"],
                    (bool)row["whisper_response"],
                    (bool)row["subscriber_only"],
                    (bool)row["moderator_only"],
                    (bool)row["broadcaster_only"]
                    );
                commandsList.Add(command);
            }

            return commandsList;

        }

        public static ChatCommand GetCommandAtIndex(int index)
        {
            DataRow row = GetDataRow("commands", index);

            ChatCommand command = new ChatCommand(
                    row["trigger_text"].ToString(),
                    row["response_text"].ToString(),
                    row["channel_name"].ToString(),
                    (bool)row["is_universal"],
                    (bool)row["must_be_exact"],
                    (bool)row["whisper_response"],
                    (bool)row["subscriber_only"],
                    (bool)row["moderator_only"],
                    (bool)row["broadcaster_only"]
                    );

            return command;
        }

        public static int CommandsCount()
        {
            return RecordsCount("commands");
        }

        #endregion

        #region Blocked Phrases Table

        public static List<BlockedPhrase> GetBlockedPhraseList()
        {

            DataTable phrasesTable = GetDataTable("blocked_phrases");
            List<BlockedPhrase> phraseList = new List<BlockedPhrase>();

            foreach (DataRow row in phrasesTable.Rows)
            {
                BlockedPhrase phrase = new BlockedPhrase(row["phrase"].ToString());
                phraseList.Add(phrase);
            }

            return phraseList;

        }

        public static BlockedPhrase GetPhraseAtIndex(int index)
        {
            DataRow row = GetDataRow("blocked_phrases", index);

            BlockedPhrase phrase = new BlockedPhrase(row["phrase"].ToString());

            return phrase;
        }

        public static bool PhraseExists(string phrase)
        {
            return DoesRecordExist("blocked_phrases", "phrase", phrase);
        }

        public static int PhraseCount()
        {
            return RecordsCount("blocked_phrases");
        }


        public static void InsertPhrase(BlockedPhrase phrase)
        {
            string tableName = "blocked_phrases";
            string columnNames = "phrase";
            string parameterNames = @"@phrase";

            Action<SQLiteCommand> addParameters = (insertCommand) =>
            {
                insertCommand.Parameters.AddWithValue("@phrase", phrase.phrase);
            };

            InsertEntry(tableName, columnNames, parameterNames, addParameters);
        }

        public static void DeletePhrase(string phrase_text)
        {
            DeleteEntry("blocked_phrases", "phrase", phrase_text);
        }

        #endregion

    }
}
