namespace AmeisenUtilities
{
    /// <summary>
    /// Class containing the default and loaded settings
    /// </summary>
    public class Settings
    {
        public string accentColor = "#FFAAAAAA";
        public string ameisenServerIP = "127.0.0.1";
        public string ameisenServerName = Utils.GenerateRandonString(12, "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");
        public int ameisenServerPort = 16200;
        public string backgroundColor = "#FF303030";
        public bool behaviourAttack = false;
        public bool behaviourHeal = false;
        public bool behaviourTank = false;
        public int botMaxThreads = 2;
        public string combatClassPath = "none";
        public bool databaseAutoConnect = true;
        public string databaseIP = "127.0.0.1";
        public string databaseName = "ameisenbot";
        public string databasePasswort = "AmeisenPassword";
        public int databasePort = 3306;
        public string databaseUsername = "ameisenbot";
        public int dataRefreshRate = 250;
        public string energyColor = "#FFFFA160";
        public string expColor = "#FFD4A0FF";
        public bool followMaster = false;
        public string fontColor = "#FFFFFFFF";
        public string healthColor = "#FFFF6060";
        public string masterName = "";
        public string meNodeColor = "#FF00FFFF";
        public string picturePath = "";
        public bool serverAutoConnect = true;
        public string targetEnergyColor = "#FFFFA160";
        public string targetHealthColor = "#FFFF6060";
        public string threadsColor = "#FFFFFF60";
        public string walkableNodeColor = "#FFA0FF00";
        public double followDistance = 3.0;
    }
}