using AmeisenMapping.objects;
using Dapper;
using MySql.Data.MySqlClient;
using System;

namespace AmeisenDB
{
    public class AmeisenDBManager
    {
        private static readonly object padlock = new object();
        private static AmeisenDBManager instance = null;
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

        public string DBName = "ameisenbot";

        public const string TABLE_NAME_NODES = "ameisenbot_map_nodes";
        public const string TABLE_NAME_PATHS = "ameisenbot_map_paths";

        private MySqlConnection sqlConnection;

        private AmeisenDBManager()
        {
            IsConnected = false;
        }

        public void InitDB()
        {
            if (IsConnected)
            {
                string dbInit =
                "/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */" +
                "/*!40101 SET NAMES utf8 */;" +
                "/*!50503 SET NAMES utf8mb4 */;" +
                "/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;" +
                "/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;" +

                "CREATE DATABASE IF NOT EXISTS `" + sqlConnection.Database + "` /*!40100 DEFAULT CHARACTER SET utf8 */;" +
                "USE `" + sqlConnection.Database + "`;" +

                "CREATE TABLE IF NOT EXISTS `" + TABLE_NAME_NODES + "` (" +
                "  `id` int (11) NOT NULL AUTO_INCREMENT," +
                "  `x` int (11) DEFAULT NULL," +
                "   `y` int (11) DEFAULT NULL," +
                "    `z` int (11) DEFAULT NULL," +
                "     PRIMARY KEY(`id`)," +
                "  UNIQUE KEY `coordinates` (`x`,`y`,`z`)" +
                ") ENGINE=InnoDB DEFAULT CHARSET=utf8;" +

                "CREATE TABLE IF NOT EXISTS `" + TABLE_NAME_PATHS + "` (" +
                "  `id` int (11) NOT NULL AUTO_INCREMENT," +
                "  `node_a` int (11) NOT NULL DEFAULT '0'," +
                "  `node_b` int (11) NOT NULL DEFAULT '0'," +
                "  `path_quality` int (11) NOT NULL DEFAULT '0'," +
                "  PRIMARY KEY(`id`)," +
                "  KEY `node_a_key` (`node_a`)," +
                "  KEY `node_b_key` (`node_b`)," +
                "  CONSTRAINT `node_a_key` FOREIGN KEY(`node_a`) REFERENCES `" + TABLE_NAME_NODES + "` (`id`)," +
                "  CONSTRAINT `node_b_key` FOREIGN KEY(`node_b`) REFERENCES `" + TABLE_NAME_NODES + "` (`id`)" +
                ") ENGINE=InnoDB DEFAULT CHARSET=utf8;" +

                "/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;" +
                "/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;" +
                "/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;";

                sqlConnection.Execute(dbInit);
            }
        }

        public bool Connect(string sqlConnectionString)
        {
            if (!IsConnected)
            {
                sqlConnection = new MySqlConnection(sqlConnectionString);

                try
                {
                    sqlConnection.Open();
                    IsConnected = true;
                }
                catch (Exception e) { throw e; }
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

        public int UpdateOrAddNode(MapNode mapNode)
        {
            string sqlQuery =
                "INSERT IGNORE INTO " +
                TABLE_NAME_NODES + " (x, y, z) " +
                "VALUES(@X,@Y,@Z);";

            return sqlConnection.Execute(sqlQuery, mapNode);
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
                "x = " + mapPath.NodeB.X + " AND " +
                "y = " + mapPath.NodeB.Y + " AND " +
                "z = " + mapPath.NodeB.Z + ";";

            var nodeA = sqlConnection.QueryFirst(sqlQueryGetNodeA);
            var nodeB = sqlConnection.QueryFirst(sqlQueryGetNodeB);

            string sqlQueryInsert =
                "REPLACE INTO " +
                TABLE_NAME_PATHS + " (node_a, node_b, path_quality) " +
                "VALUES(" +
                "'" + nodeA.id + "'," +
                "'" + nodeB.id + "'," +
                "'" + mapPath.Quality + "');";

            return sqlConnection.Execute(sqlQueryInsert);
        }
    }
}
