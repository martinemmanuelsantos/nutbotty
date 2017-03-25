using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nutbotty
{
    class Channel
    {
        #region Member Variables
        public string channelName { get; set; }
        #endregion

        #region Constructors
        public Channel(string channelName)
        {
            this.channelName = channelName;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return this.channelName;
        }
        #endregion

        #region Database Methods
        //public static void AddChannelToDB(Channel channel)
        //{
        //    string tableName = "CHANNELS";
        //    string columnNames = "ChannelName";
        //    string parameterNames = @"@channelName";

        //    Action<SqlCommand> addParameters = (insertCommand) =>
        //    {
        //        insertCommand.Parameters.AddWithValue("@channelName", channel.ChannelName);
        //    };

        //    Database.InsertEntry(tableName, columnNames, parameterNames, addParameters);
        //}

        //public static void DeleteChannelFromDB(string channel)
        //{
        //    Database.DeleteEntry("CHANNELS", "ChannelName", channel);
        //}

        //public static bool ChannelExistsInDB(string channel)
        //{
        //    return Database.EntryExists("CHANNELS", "ChannelName", channel);
        //}

        //public static int ChannelCountInDB()
        //{
        //    return Database.CountRows("CHANNELS");
        //}
        //#endregion

        //#region Getters
        //public string ChannelName
        //{
        //    get
        //    {
        //        return channelName;
        //    }
        //}
        #endregion
    }
}
