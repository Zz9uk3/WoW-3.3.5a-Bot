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

        #endregion Public Fields
    }
}