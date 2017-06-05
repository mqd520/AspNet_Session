using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Web.Hosting;
using MySql.Data.MySqlClient;
using System.Configuration.Provider;
using System.IO;
using System.Web.SessionState;
using System.Web.Configuration;
using System.Web;
using System.Diagnostics;
using System.Threading;
using System.Security;
using System.Data;

namespace SessionProvider.Mysql
{
    /// <summary>
    /// This class allows ASP.NET applications to store and manage session state information in a
    /// MySQL database.
    /// Expired session data is periodically deleted from the database.
    /// </summary>
    public class MySqlSessionStateStore : SessionStateStoreProviderBase
    {
        string connectionString;
        ConnectionStringSettings connectionStringSettings;
        string eventSource = "MySQLSessionStateStore";
        string eventLog = "Application";
        string exceptionMessage = "An exception occurred. Please check the event log.";
        SessionStateSection sessionStateConfig;
        bool writeExceptionsToEventLog = false;

        bool inited = false;

        /// <summary>
        /// Indicates whether to write exceptions to event log
        /// </summary>
        public bool WriteExceptionsToEventLog
        {
            get { return writeExceptionsToEventLog; }
            set { writeExceptionsToEventLog = value; }
        }

        /// <summary>
        /// Handles MySql exception.
        /// If WriteExceptionsToEventLog is set, will write exception info
        /// to event log. 
        /// It throws provider exception (original exception is stored as inner exception)
        /// </summary>
        /// <param name="e">exception</param>
        /// <param name="action"> name of the function that throwed the exception</param>
        private void HandleMySqlException(MySqlException e, string action)
        {
            if (WriteExceptionsToEventLog)
            {
                using (EventLog log = new EventLog())
                {
                    log.Source = eventSource;
                    log.Log = eventLog;

                    string message = "An exception occurred communicating with the data source.\n\n";
                    message += "Action: " + action;
                    message += "Exception: " + e.ToString();
                    log.WriteEntry(message);
                }
            }
            throw new ProviderException(exceptionMessage, e);
        }

        /// <summary>
        /// Initializes the provider with the property values specified in the ASP.NET application configuration file
        /// </summary>
        /// <param name="name">The name of the provider instance to initialize.</param>
        /// <param name="config">Object that contains the names and values of configuration options for the provider.
        /// </param>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (inited)
            {
                return;
            }
            inited = true;

            //Initialize values from web.config.
            if (config == null)
            {
                throw new ArgumentException("sessionState.providers config error!");
            }
            if (name == null || name.Length == 0)
            {
                throw new ArgumentException("sessionState.providers config error!");
            }
            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config["description"] = "MySQL Session State Store Provider";
            }
            base.Initialize(name, config);

            // Get <sessionState> configuration element.
            Configuration webConfig = WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath);
            sessionStateConfig = (SessionStateSection)webConfig.SectionGroups["system.web"].Sections["sessionState"];

            // Initialize connection.
            connectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];
            if (connectionStringSettings == null || connectionStringSettings.ConnectionString.Trim() == "")
            {
                throw new HttpException("Connection string can not be null");
            }
            connectionString = connectionStringSettings.ConnectionString;
            writeExceptionsToEventLog = false;
            if (config["writeExceptionsToEventLog"] != null)
            {
                writeExceptionsToEventLog = (config["writeExceptionsToEventLog"].ToUpper() == "TRUE");
            }
        }

        /// <summary>
        /// This method creates a new SessionStateStoreData object for the current request.
        /// </summary>
        /// <param name="context">
        /// The HttpContext object for the current request.
        /// </param>
        /// <param name="timeout">
        /// The timeout value (in minutes) for the SessionStateStoreData object that is created.
        /// </param>
        public override SessionStateStoreData CreateNewStoreData(System.Web.HttpContext context, int timeout)
        {
            return new SessionStateStoreData(new SessionStateItemCollection(), SessionStateUtility.GetSessionStaticObjects(context), timeout);
        }

        /// <summary>
        /// This method adds a new session state item to the database.
        /// </summary>
        /// <param name="context">
        /// The HttpContext object for the current request.
        /// </param>
        /// <param name="id">
        /// The session ID for the current request.
        /// </param>
        /// <param name="timeout">
        /// The timeout value for the current request.
        /// </param>
        public override void CreateUninitializedItem(System.Web.HttpContext context, string id, int timeout)
        {
            MySqlConnection conn = new MySqlConnection(connectionString);
            try
            {
                string sql = "CreateNewSession";
                MySqlCommand cmd = new MySqlCommand(sql, conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@SessionID", id);
                cmd.Parameters.AddWithValue("@Timeout", timeout);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (MySqlException e)
            {
                conn.Close();
                HandleMySqlException(e, "CreateUninitializedItem");
            }
        }


        /// <summary>
        /// This method releases all the resources for this instance.
        /// </summary>
        public override void Dispose()
        {

        }

        /// <summary>
        /// This method allows the MySqlSessionStateStore object to perform any cleanup that may be 
        /// required for the current request.
        /// </summary>
        /// <param name="context">The HttpContext object for the current request</param>
        public override void EndRequest(System.Web.HttpContext context)
        {

        }

        /// <summary>
        /// This method returns a read-only session item from the database.
        /// </summary>
        public override SessionStateStoreData GetItem(System.Web.HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            return GetSessionStoreItem(false, context, id, out locked, out lockAge, out lockId, out actions);
        }

        /// <summary>
        /// This method locks a session item and returns it from the database
        /// </summary>
        /// <param name="context">The HttpContext object for the current request</param>
        /// <param name="id">The session ID for the current request</param>
        /// <param name="locked">
        /// true if the session item is locked in the database; otherwise, it is false.
        /// </param>
        /// <param name="lockAge">
        /// TimeSpan object that indicates the amount of time the session item has been locked in the database.
        /// </param>
        /// <param name="lockId">
        /// A lock identifier object.
        /// </param>
        /// <param name="actions">
        /// A SessionStateActions enumeration value that indicates whether or
        /// not the session is uninitialized and cookieless.
        /// </param>
        /// <returns></returns>
        public override SessionStateStoreData GetItemExclusive(System.Web.HttpContext context, string id,
            out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            return GetSessionStoreItem(true, context, id, out locked, out lockAge, out lockId, out actions);
        }

        /// <summary>
        ///  This method performs any per-request initializations that the MySqlSessionStateStore provider requires.
        /// </summary>
        public override void InitializeRequest(System.Web.HttpContext context)
        {

        }

        /// <summary>
        /// This method forcibly releases the lock on a session item in the database, if multiple attempts to 
        /// retrieve the session item fail.
        /// </summary>
        /// <param name="context">The HttpContext object for the current request.</param>
        /// <param name="id">The session ID for the current request.</param>
        /// <param name="lockId">The lock identifier for the current request.</param>
        public override void ReleaseItemExclusive(System.Web.HttpContext context, string id, object lockId)
        {
            MySqlConnection conn = new MySqlConnection(connectionString);
            try
            {
                MySqlCommand cmd = new MySqlCommand("UnlockSession", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Timeout", sessionStateConfig.Timeout.TotalMinutes);
                cmd.Parameters.AddWithValue("@SessionID", id);
                cmd.Parameters.AddWithValue("@LockId", lockId);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                cmd.Dispose();
            }
            catch (MySqlException e)
            {
                conn.Close();
                HandleMySqlException(e, "ReleaseItemExclusive");
            }
        }

        /// <summary>
        /// This method removes the specified session item from the database
        /// </summary>
        /// <param name="context">The HttpContext object for the current request</param>
        /// <param name="id">The session ID for the current request</param>
        /// <param name="lockId">The lock identifier for the current request.</param>
        /// <param name="item">The session item to remove from the database.</param>
        public override void RemoveItem(System.Web.HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            MySqlConnection conn = new MySqlConnection(connectionString);
            try
            {
                MySqlCommand cmd = new MySqlCommand("DelSession", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SessionID", id);
                cmd.Parameters.AddWithValue("@LockId", lockId);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                cmd.Dispose();
            }
            catch (MySqlException ex)
            {
                conn.Close();
                HandleMySqlException(ex, "RemoveItem Error: " + ex.Message);
            }
        }


        /// <summary>
        /// This method resets the expiration date and timeout for a session item in the database.
        /// </summary>
        /// <param name="context">The HttpContext object for the current request</param>
        /// <param name="id">The session ID for the current request</param>
        public override void ResetItemTimeout(System.Web.HttpContext context, string id)
        {
            MySqlConnection conn = new MySqlConnection(connectionString);
            try
            {
                MySqlCommand cmd = new MySqlCommand("UpdateSessionTimeout", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Timeout", sessionStateConfig.Timeout.TotalMinutes);
                cmd.Parameters.AddWithValue("@SessionID", id);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                cmd.Dispose();
            }
            catch (MySqlException e)
            {
                conn.Close();
                HandleMySqlException(e, "ResetItemTimeout");
            }
        }

        /// <summary>
        /// This method updates the session time information in the database with the specified session item,
        /// and releases the lock.
        /// </summary>
        /// <param name="context">The HttpContext object for the current request</param>
        /// <param name="id">The session ID for the current request</param>
        /// <param name="item">The session item containing new values to update the session item in the database with.
        /// </param>
        /// <param name="lockId">The lock identifier for the current request.</param>
        /// <param name="newItem">A Boolean value that indicates whether or not the session item is new in the database.
        /// A false value indicates an existing item.
        /// </param>
        public override void SetAndReleaseItemExclusive(System.Web.HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            MySqlConnection conn = new MySqlConnection(connectionString);
            try
            {
                string sessItems = Serialize((SessionStateItemCollection)item.Items);
                MySqlCommand cmd = new MySqlCommand("AddOrUpdateSessionAndUnlock", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@SessionID", id);
                cmd.Parameters.AddWithValue("@Timeout", item.Timeout);
                cmd.Parameters.AddWithValue("@LockId", lockId == null ? 0 : lockId);
                cmd.Parameters.AddWithValue("@SessionItems", sessItems);
                cmd.Parameters.AddWithValue("@NewItem", newItem);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                cmd.Dispose();
            }
            catch (MySqlException e)
            {
                conn.Close();
                HandleMySqlException(e, "SetAndReleaseItemExclusive");
            }
        }

        /// <summary>
        ///  GetSessionStoreItem is called by both the GetItem and  GetItemExclusive methods. GetSessionStoreItem 
        ///  retrieves the session data from the data source. If the lockRecord parameter is true (in the case of 
        ///  GetItemExclusive), then GetSessionStoreItem  locks the record and sets a New LockId and LockDate.
        /// </summary>
        private SessionStateStoreData GetSessionStoreItem(bool lockRecord,
               HttpContext context,
               string id,
               out bool locked,
               out TimeSpan lockAge,
               out object lockId,
               out SessionStateActions actionFlags)
        {
            // Initial values for return value and out parameters.
            SessionStateStoreData item = null;
            lockAge = TimeSpan.Zero;
            lockId = null;
            locked = false;
            actionFlags = SessionStateActions.None;

            // serialized SessionStateItemCollection.
            string serializedItems = null;
            // True if a record is found in the database.
            bool foundRecord = false;
            // True if the returned session item is expired and needs to be deleted.
            //bool deleteData = false;   
            // Timeout value from the data store.
            int timeout = 0;

            MySqlConnection conn = new MySqlConnection(connectionString);
            try
            {
                MySqlCommand cmd = new MySqlCommand("GetSession", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@LockRecord", locked);
                cmd.Parameters.AddWithValue("@SessionID", id);
                cmd.Parameters.Add(new MySqlParameter("@Locked", MySqlDbType.Bit) { Direction = ParameterDirection.Output });
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                if (reader.Read())
                {
                    DateTime now = reader.GetDateTime(0);
                    DateTime expires = reader.GetDateTime(1);
                    if (now.CompareTo(expires) > 0)
                    {
                        //The record was expired. Mark it as not locked.
                        locked = false;
                    }
                    else
                    {
                        foundRecord = true;
                    }
                    serializedItems = reader.GetValue(2) as string;
                    lockId = reader.GetValue(3) ?? 0;
                    actionFlags = (SessionStateActions)(reader.GetInt32(4));
                    timeout = reader.GetInt32(5);
                    DateTime lockDate = reader.GetDateTime(6);
                    lockAge = now.Subtract(lockDate);
                    // If it's a read-only session set locked to the current lock
                    // status (writable sessions have already done this)
                    if (!lockRecord)
                    {
                        locked = reader.GetBoolean(7);
                    }

                    conn.Close();
                    cmd.Dispose();

                    // The record was not found. Ensure that locked is false.
                    if (!foundRecord)
                    {
                        locked = false;
                    }

                    // If the record was found and you obtained a lock, then set 
                    // the lockId, clear the actionFlags,
                    // and create the SessionStateStoreItem to return.
                    if (foundRecord && !locked)
                    {
                        lockId = (int)(lockId) + 1;
                        cmd = new MySqlCommand("LockSession", conn) { CommandType = CommandType.StoredProcedure };
                        cmd.Parameters.AddWithValue("@LockId", lockId);
                        cmd.Parameters.AddWithValue("@SessionID", id);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        cmd.Dispose();
                        // If the actionFlags parameter is not InitializeItem, 
                        // deserialize the stored SessionStateItemCollection.
                        if (actionFlags == SessionStateActions.InitializeItem)
                        {
                            item = CreateNewStoreData(context, (int)sessionStateConfig.Timeout.TotalMinutes);
                        }
                        else
                        {
                            item = Deserialize(context, serializedItems, timeout);
                        }
                    }
                }
                else
                {
                    conn.Close();
                    cmd.Dispose();
                }
            }
            catch (MySqlException e)
            {
                conn.Close();
                HandleMySqlException(e, "GetSessionStoreItem");
            }
            return item;
        }

        /// <summary>
        /// This method sets the reference for the ExpireCallback delegate if setting is enabled.
        /// </summary>
        /// <param name="expireCallback"></param>
        /// <returns>false </returns>
        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            return false;
        }

        ///<summary>
        /// Serialize is called by the SetAndReleaseItemExclusive method to 
        /// convert the SessionStateItemCollection into a byte array to
        /// be stored in the blob field.
        /// </summary>
        private string Serialize(SessionStateItemCollection items)
        {
            MemoryStream ms = new MemoryStream();
            if (items != null)
            {
                BinaryWriter writer = new BinaryWriter(ms);
                items.Serialize(writer);
                writer.Close();
                ms.Close();
                byte[] buf = ms.ToArray();
                string result = Convert.ToBase64String(buf, 0, buf.Length);
                return result;
            }
            else
            {
                return "";
            }
        }

        ///<summary>
        /// Deserialize is called by the GetSessionStoreItem method to 
        /// convert the byte array stored in the blob field to a 
        /// SessionStateItemCollection.
        /// </summary>
        private SessionStateStoreData Deserialize(HttpContext context, string serializedItems, int timeout)
        {
            SessionStateItemCollection sessionItems = new SessionStateItemCollection();
            if (!string.IsNullOrEmpty(serializedItems))
            {
                byte[] decrypt = Convert.FromBase64String(serializedItems);
                MemoryStream ms = new MemoryStream(decrypt);
                if (ms.Length > 0)
                {
                    BinaryReader br = new BinaryReader(ms);
                    sessionItems = SessionStateItemCollection.Deserialize(br);
                    br.Close();
                    ms.Close();
                }
            }
            return new SessionStateStoreData(sessionItems, SessionStateUtility.GetSessionStaticObjects(context), timeout);
        }
    }
}