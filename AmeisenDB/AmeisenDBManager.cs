using AmeisenMapping.objects;
using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace AmeisenDB
{
    public class AmeisenDBManager
    {
        #region Public Fields

        public const string TABLE_NAME_NODES = "ameisenbot_map_nodes";
        public const string TABLE_NAME_PATHS = "ameisenbot_map_paths";
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

        public bool Connect(string sqlConnectionString)
        {
            if (!IsConnected)
            {
                sqlConnection = new MySqlConnection(sqlConnectionString);

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

        public void Disconnect()
        {
            if (IsConnected)
            {
                sqlConnection.Close();
            }

            IsConnected = false;
        }

        public List<MapNode> GetNodes(int zoneID, int mapID)
        {
            if (IsConnected)
            {
                string sqlQuery =
                    "SELECT * FROM " + TABLE_NAME_NODES + " " +
                    "WHERE zone_id = " + zoneID + " AND " +
                    "map_id = " + mapID + ";";
                try
                {
                    return sqlConnection.Query<MapNode>(sqlQuery).AsList();
                }
                catch { return new List<MapNode>(); }
            }
            else return new List<MapNode>();
        }

        public void InitDB()
        {
            if (IsConnected)
            {
                string dbInit =
                "CREATE DATABASE IF NOT EXISTS `" + sqlConnection.Database + "` /*!40100 DEFAULT CHARACTER SET utf8 */;" +
                "USE `" + sqlConnection.Database + "`;" +
                "CREATE TABLE IF NOT EXISTS `ameisenbot_map_nodes` (" +
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
            else return 0;
        }

        public int UpdateOrAddPath(MapPath mapPath)
        {
            string sqlQueryGetNodeA = "SELECT * FROM " +
                TABLE_NAME_NODES + " " +
                "WHERE " +
                "x = @X AND " +
                "y = @Y AND " +
                "z = @Z;";

            string sqlQueryGetNodeB = "SELECT * FROM " +
                TABLE_NAME_NODES + " " +
                "WHERE " +
                "x = @X AND " +
                "y = @Y AND " +
                "z = @Z;";

            var nodeA = sqlConnection.QueryFirst(sqlQueryGetNodeA, mapPath.NodeA);
            var nodeB = sqlConnection.QueryFirst(sqlQueryGetNodeB, mapPath.NodeB);

            string sqlQueryInsert =
                "REPLACE INTO " +
                TABLE_NAME_PATHS + " (node_a, node_b, path_quality) " +
                "VALUES(" +
                "'" + nodeA.id + "'," +
                "'" + nodeB.id + "'," +
                "'" + mapPath.Quality + "');";

            return sqlConnection.Execute(sqlQueryInsert);
        }

        #endregion Public Methods
    }
}