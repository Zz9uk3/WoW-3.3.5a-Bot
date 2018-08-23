using AmeisenUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenUtilities
{
    /// <summary>
    /// Class containing the default and loaded settings
    /// </summary>
    public class Settings
    {
        public int dataRefreshRate = 250;
        public int botMaxThreads = 2;

        public string accentColor = "#FFAAAAAA";
        public string fontColor = "#FFFFFFFF";
        public string backgroundColor = "#FF303030";

        public string ameisenServerIP = "127.0.0.1";
        public string ameisenServerPort = "16200";
        public string ameisenServerName = Utils.GenerateRandonString(12, "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");

        public string combatClassPath = "none";

        public bool behaviourAttack = false;
        public bool behaviourHeal = false;
        public bool behaviourTank = false;

        public bool followMaster = false;

        public string masterName = "";
    }
}
