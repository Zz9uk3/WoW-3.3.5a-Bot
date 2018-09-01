using AmeisenMapping.objects;
using Dapper;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Text;

namespace AmeisenDB
{
    public class AmeisenDBManager
    {
        #region Public Fields

        public const string TABLE_NAME_NODES = "ameisenbot_map_nodes";
        public string DBName = "ameisenbot";

        #endregion Public Fields

        #region Private Fields

        private static readonly object padlock = new object();
        private static AmeisenDBManager instance = null;
        private MySqlConnection sqlConnection;

        #endregion Private Fields

        #region Private Constructors

        private AmeisenDBManager()
        {
            IsConnected = false;
        }

        #endregion Private Constructors

        #region Public Properties

        public static AmeisenDBManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new AmeisenDBManager();
                    }

                    return instance;
                }
            }
        }

        public bool IsConnected { get; private set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Connect to a MySQL database
        /// </summary>
        /// <param name="mysqlConnectionString">mysql connection string</param>
        /// <returns>true if connected, false if not</returns>
        public bool ConnectToMySQL(string mysqlConnectionString)
        {
            if (!IsConnected)
            {
                sqlConnection = new MySqlConnection(mysqlConnectionString);

                try
                {
                    sqlConnection.Open();
                    IsConnected = true;
                    InitDB();
                }
                catch { }
            }
            return IsConnected;
        }

        /// <summary>
        /// Disconnect if we are connected
        /// </summary>
        public void Disconnect()
        {
            if (IsConnected)
            {
                sqlConnection.Close();
            }

            IsConnected = false;
        }

        /// <summary>
        /// Get all saved nodes by the zone & map id
        /// </summary>
        /// <param name="zoneID">zone id to get the nodes from</param>
        /// <param name="mapID">map id to get the nodes from</param>
        /// <returns>list containing all the MapNodes</returns>
        public List<MapNode> GetNodes(int zoneID, int mapID, int maxX = 0, int minX = 0, int maxY = 0, int minY = 0)
        {
            if (IsConnected)
            {
                StringBuilder sqlQuery = new StringBuilder();
                sqlQuery.Append("SELECT * FROM " + TABLE_NAME_NODES + " ");
                sqlQuery.Append("WHERE zone_id = " + zoneID + " AND ");
                sqlQuery.Append("map_id = " + mapID);

                if (maxX != 0)
                    sqlQuery.Append(" AND x < " + maxX);
                if (maxY != 0)
                    sqlQuery.Append(" AND y < " + maxY);
                if (minX != 0)
                    sqlQuery.Append(" AND x > " + minX);
                if (minY != 0)
                    sqlQuery.Append(" AND y > " + minY);

                sqlQuery.Append(";");
                try
                {
                    return sqlConnection.Query<MapNode>(sqlQuery.ToString()).AsList();
                }
                catch { return new List<MapNode>(); }
            }
            else
            {
                return new List<MapNode>();
            }
        }

        /// <summary>
        /// Initialise the database with Tables
        /// </summary>
        public void InitDB()
        {
            if (IsConnected)
            {
                string dbInit =
                "CREATE DATABASE IF NOT EXISTS `" + sqlConnection.Database + "` /*!40100 DEFAULT CHARACTER SET utf8 */;" +
                "USE `" + sqlConnection.Database + "`;" +
                "CREATE TABLE IF NOT EXISTS `" + TABLE_NAME_NODES + "` (" +
                "`id` int(11) NOT NULL AUTO_INCREMENT, " +
                "`x` int(11) DEFAULT NULL, " +
                "`y` int(11) DEFAULT NULL, " +
                "`z` int(11) DEFAULT NULL, " +
                "`zone_id` int(11) DEFAULT NULL," +
                "`map_id` int(11) DEFAULT NULL," +
                "PRIMARY KEY(`id`), " +
                "UNIQUE KEY `coordinates` (`x`,`y`,`z`,`zone_id`,`map_id`) " +
                "ENGINE = InnoDB AUTO_INCREMENT = 0 DEFAULT CHARSET = utf8;";

                sqlConnection.Execute(dbInit);
            }
        }

        /// <summary>
        /// Add a MapNode to the database, duplicate nodes will be ignored
        /// </summary>
        /// <param name="mapNode">Node to add</param>
        /// <returns>affected SQL rows</returns>
        public int UpdateOrAddNode(MapNode mapNode)
        {
            if (IsConnected)
            {
                string sqlQuery =
                    "INSERT INTO " +
                    TABLE_NAME_NODES + " (x, y, z, zone_id, map_id) " +
                    "VALUES(@X,@Y,@Z,@ZoneID,@MapID);";
                try
                {
                    return sqlConnection.Execute(sqlQuery, mapNode);
                }
                catch { return 0; }
            }
            else
            {
                return 0;
            }
        }

        #endregion Public Methods
    }
}