using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace Instant_Gist
{
    public class Database
    {
        public string FileName  { get; }
        public string Directory { get; }
        public SQLiteConnection DbConnection { get; }

        public struct Data
        {
            public string Date;
            public string URL;
            public string FileName;
        }
        /// <summary>
        /// Initialize a new database handler
        /// </summary>
        /// <param name="dbName">Name of the database (Do not use extensions).</param>
        /// <param name="dir">Directory the file should be placed.</param>
        public Database(string dbName, string dir)
        {
            FileName = dbName;
            Directory = dir;
            DbConnection = new SQLiteConnection("Data Source=\"" + Directory + "\\" + FileName + ".sqlite\";Version=3;");

            if (File.Exists(Directory + "\\" + FileName + ".sqlite")) return;
            SQLiteConnection.CreateFile(Directory + "\\" + FileName + ".sqlite");
            const string sql = "create table history (Date text, URL text, Filename text)";
            var command = new SQLiteCommand(sql, DbConnection); ;
            DbConnection.Open();
            command.ExecuteNonQuery();
            command.Dispose();
            DbConnection.Close();
        }
        /// <summary>
        /// Add a database entry.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="URL"></param>
        /// <param name="filename"></param>
        public async void AddHistory(DateTime date, string URL, string filename)
        {
            try
            {
                var sql = "insert into history (Date, URL, Filename) values (" + date.ToLongDateString() + "," + URL +
                     "," + filename + ")";
                var command = new SQLiteCommand(sql, DbConnection);
                DbConnection.Open();
                await command.ExecuteNonQueryAsync();
                command.Dispose();
                DbConnection.Close();
            }
            catch (Exception)
            {              
                // ignored
            }
        }
        /// <summary>
        /// Get all database objects in table.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Data>> GetAllItems()
        {
            try
            {
                var data = new List<Data>();
                const string sql = "select * from history order by date desc";
                var command = new SQLiteCommand(sql, DbConnection);
                DbConnection.Open();
                var reader = await command.ExecuteReaderAsync();
                command.Dispose();
                DbConnection.Close();
                while (reader.Read())
                {
                    var date = (string)reader["Date"];
                    var url = (string)reader["URL"];
                    var filename = (string)reader["Filename"];
                    var dataStruct = new Data
                    {
                        Date = date,
                        URL = url,
                        FileName = filename
                    };
                    data.Add(dataStruct);
                }
                return data;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
