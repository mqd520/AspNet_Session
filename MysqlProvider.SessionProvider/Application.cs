using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Configuration.Provider;
using MysqlProvider.SessionProvider.Properties;

namespace MysqlProvider.SessionProvider
{
    internal class Application
    {
        private long _id;
        private string _desc;

        public Application(string name, string desc)
        {
            Id = -1;
            Name = name;
            Description = desc;
        }
        public long Id
        {
            get { return _id; }
            private set { _id = value; }
        }
        public string Name;

        public string Description
        {
            get { return _desc; }
            private set { _desc = value; }
        }

        public long FetchId(MySqlConnection connection)
        {
            if (Id == -1)
            {
                MySqlCommand cmd = new MySqlCommand(
                    @"SELECT id FROM my_aspnet_applications WHERE name=@name", connection);
                cmd.Parameters.AddWithValue("@name", Name);
                object id = cmd.ExecuteScalar();
                Id = id == null ? -1 : Convert.ToInt64(id);
            }
            return Id;
        }

        /// <summary>
        /// Creates the or fetch application id.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="applicationId">The application id.</param>
        /// <param name="applicationDesc">The application desc.</param>
        /// <param name="connection">The connection.</param>
        public long EnsureId(MySqlConnection connection)
        {
            // first try and retrieve the existing id
            if (FetchId(connection) <= 0)
            {
                MySqlCommand cmd = new MySqlCommand(
                    "INSERT INTO my_aspnet_applications VALUES (NULL, @appName, @appDesc)", connection);
                cmd.Parameters.AddWithValue("@appName", Name);
                cmd.Parameters.AddWithValue("@appDesc", Description);
                int recordsAffected = cmd.ExecuteNonQuery();
                if (recordsAffected != 1)
                    throw new ProviderException(Resources.UnableToCreateApplication);
                Id = cmd.LastInsertedId;
            }
            return Id;
        }
    }
}
