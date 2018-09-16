using AmeisenBotMapping.objects;
using AmeisenBotUtilities;
using AmeisenBotUtilities.Enums;
using AmeisenBotUtilities.Objects;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmeisenBotDB
{
    public class AmeisenDBManager
    {
        public const string TABLE_NAME_NODES = "ameisenbot_map_nodes";
        public const string TABLE_NAME_REMEMBERED_UNITS = "ameisenbot_remembered_units";
        public string DBName = "ameisenbot";
        private MySqlConnection sqlConnection;
        public bool IsConnected { get; private set; }
        private string MysqlConnectionString { get; set; }

        public AmeisenDBManager()
        {
            IsConnected = false;
        }

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
                MysqlConnectionString = mysqlConnectionString;
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

        /// <summary> Get all saved nodes by the zone & map id </summary> <param name="zoneID">zone
        /// id to get the nodes from</param> <param name="mapID">map id to get the nodes from</param>
        /// <returns>list containing all the MapNodes</returns>
        public List<MapNode> GetNodes(int zoneID, int mapID, int maxX = 0, int minX = 0, int maxY = 0, int minY = 0)
        {
            MySqlConnection sqlConnection = new MySqlConnection(MysqlConnectionString);
            sqlConnection.Open();

            if (IsConnected)
            {
                StringBuilder sqlQuery = new StringBuilder();
                sqlQuery.Append($"SELECT * FROM {TABLE_NAME_NODES} ");
                sqlQuery.Append($"WHERE zone_id = {zoneID} AND ");
                sqlQuery.Append($"map_id = {mapID} ");

                if (maxX >= minX)
                    sqlQuery.Append($"AND (x BETWEEN {minX} AND {maxX})");
                else
                    sqlQuery.Append($"AND (x BETWEEN {maxX} AND {minX})");

                if (maxX >= minX)
                    sqlQuery.Append($"AND (y BETWEEN {minY} AND {maxY})");
                else
                    sqlQuery.Append($"AND (y BETWEEN {maxY} AND {minY})");

                sqlQuery.Append(";");

                List<MapNode> nodeList = sqlConnection.Query<MapNode>(sqlQuery.ToString()).AsList();
                return nodeList;
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
                StringBuilder dbInit = new StringBuilder();

                dbInit.Append($"CREATE DATABASE IF NOT EXISTS `{sqlConnection.Database}` /*!40100 DEFAULT CHARACTER SET utf8 */;");
                dbInit.Append($"USE `{sqlConnection.Database}`;");
                dbInit.Append($"CREATE TABLE IF NOT EXISTS `{TABLE_NAME_NODES}` (");
                dbInit.Append("`id` int(11) NOT NULL AUTO_INCREMENT, ");
                dbInit.Append("`x` int(11) DEFAULT NULL, ");
                dbInit.Append("`y` int(11) DEFAULT NULL, ");
                dbInit.Append("`z` int(11) DEFAULT NULL, ");
                dbInit.Append("`zone_id` int(11) DEFAULT NULL,");
                dbInit.Append("`map_id` int(11) DEFAULT NULL,");
                dbInit.Append("PRIMARY KEY(`id`), ");
                dbInit.Append("UNIQUE KEY `coordinates` (`x`,`y`,`z`,`zone_id`,`map_id`) ");
                dbInit.Append(") ENGINE = InnoDB DEFAULT CHARSET = utf8;");

                dbInit.Append($"CREATE DATABASE IF NOT EXISTS `{sqlConnection.Database}` /*!40100 DEFAULT CHARACTER SET utf8 */;");
                dbInit.Append($"USE `{sqlConnection.Database}`;");
                dbInit.Append($"CREATE TABLE IF NOT EXISTS `{TABLE_NAME_REMEMBERED_UNITS}` (");
                dbInit.Append("`id` int(11) NOT NULL AUTO_INCREMENT, ");
                dbInit.Append("`name` text, ");
                dbInit.Append("`x` double DEFAULT '0', ");
                dbInit.Append("`y` double DEFAULT '0', ");
                dbInit.Append("`z` double DEFAULT '0', ");
                dbInit.Append("`zone_id` int(11) DEFAULT '0', ");
                dbInit.Append("`map_id` int(11) DEFAULT '0', ");
                dbInit.Append("`traits` json DEFAULT NULL, ");
                dbInit.Append("PRIMARY KEY(`id`), ");
                dbInit.Append("UNIQUE KEY `guid` (`x`,`y`,`z`,`zone_id`,`map_id`) ");
                dbInit.Append(") ENGINE=InnoDB DEFAULT CHARSET=utf8;");

                sqlConnection.Execute(dbInit.ToString());
                sqlConnection.Close();
            }
        }

        /// <summary>
        /// Add a MapNode to the database, duplicate nodes will be ignored
        /// </summary>
        /// <param name="mapNode">Node to add</param>
        /// <returns>affected SQL rows</returns>
        public int UpdateOrAddNode(MapNode mapNode)
        {
            MySqlConnection sqlConnection = new MySqlConnection(MysqlConnectionString);
            sqlConnection.Open();

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

        public void RememberUnit(RememberedUnit rememberedUnit)
        {
            MySqlConnection sqlConnection = new MySqlConnection(MysqlConnectionString);
            sqlConnection.Open();

            if (IsConnected)
            {
                rememberedUnit.UnitTraitsString = JsonConvert.SerializeObject(rememberedUnit.UnitTraits);

                string sqlQuery =
                    $"INSERT INTO {TABLE_NAME_REMEMBERED_UNITS} (name, x, y, z, zone_id, map_id, traits) " +
                    $"VALUES(\"{rememberedUnit.Name}\"," +
                    $"{(int)rememberedUnit.Position.X}," +
                    $"{(int)rememberedUnit.Position.Y}," +
                    $"{(int)rememberedUnit.Position.Z}," +
                    $"{rememberedUnit.ZoneID}," +
                    $"{rememberedUnit.MapID}," +
                    $"\"{rememberedUnit.UnitTraitsString}\");";

                try
                {
                    sqlConnection.Execute(sqlQuery);
                }
                catch { }
            }
        }

        public RememberedUnit CheckForRememberedUnit(string name, int zoneID, int mapID)
        {
            MySqlConnection sqlConnection = new MySqlConnection(MysqlConnectionString);
            sqlConnection.Open();

            if (IsConnected)
            {
                StringBuilder sqlQuery = new StringBuilder();
                sqlQuery.Append($"SELECT * FROM {TABLE_NAME_REMEMBERED_UNITS} ");
                sqlQuery.Append($"WHERE zone_id = {zoneID} AND ");
                sqlQuery.Append($"map_id = {mapID} AND ");
                sqlQuery.Append($"name = \"{name}\";");

                try
                {
                    dynamic rawUnit = sqlConnection.Query(sqlQuery.ToString()).FirstOrDefault();

                    RememberedUnit rememberedUnit = new RememberedUnit
                    {
                        Name = rawUnit.name,
                        ZoneID = rawUnit.zone_id,
                        MapID = rawUnit.map_id,
                        Position = new Vector3(rawUnit.x, rawUnit.y, rawUnit.z),
                        UnitTraitsString = rawUnit.traits
                    };

                    rememberedUnit.UnitTraits = JsonConvert.DeserializeObject<List<UnitTrait>>(rememberedUnit.UnitTraitsString);
                    return rememberedUnit;
                }
                catch { }
            }
            return null;
        }
    }
}