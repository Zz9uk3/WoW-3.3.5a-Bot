namespace AmeisenUtilities
{
    /// <summary>
    /// Class containing the default and loaded settings
    /// </summary>
    public class Settings
    {
        #region Public Fields

        public string accentColor = "#FFAAAAAA";
        public string ameisenServerIP = "127.0.0.1";
        public string ameisenServerName = Utils.GenerateRandonString(12, "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");
        public string ameisenServerPort = "16200";
        public string backgroundColor = "#FF303030";
        public bool behaviourAttack = false;
        public bool behaviourHeal = false;
        public bool behaviourTank = false;
        public int botMaxThreads = 2;
        public string combatClassPath = "none";
        public int dataRefreshRate = 250;
        public bool followMaster = false;
        public string fontColor = "#FFFFFFFF";
        public string masterName = "";
        public string picturePath = "";
        public string serverIP = "127.0.0.1";
        public int serverPort = 16200;
        public bool serverAutoConnect = false;
        public string databaseIP = "127.0.0.1";
        public int databasePort = 3306;
        public string databaseName = "ameisenbot";
        public string databaseUsername = "ameisenbot";
        public string databasePasswort = "AmeisenPassword";
        public bool databaseAutoConnect = false;
        public string walkableNodeColor = "#FFA0FF00";
        public string meNodeColor = "#FF00FFFF";

        #endregion Public Fields
    }
}