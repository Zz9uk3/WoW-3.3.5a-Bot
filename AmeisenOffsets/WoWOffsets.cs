using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmeisenOffsets
{
    public class WoWOffsets
    {
        public static int playerName = 0x879D18;
        public static int localPlayerGUID = 0x8A1238;
        public static int localTargetGUID = 0x7D07B0;

        public static int currentClientConnection = 0x879CE0;
        public static int currentManagerOffset = 0x2ED0;
        public static int firstObjectOffset = 0xAC;
        public static int nextObjectOffset = 0x3C;
        public static int gameobjectTypeOffset = 0x14;
        public static int gameobjectGUIDOffset = 0x30;

        public static int playerBase = 0x8D87A8;
        public static int playerHealth = 0x19B8;

        public static int nameStore = 0x00C5D938 + 0x8;
        public static int nameMask = 0x24;
        public static int nameBase = 0x1C;
        public static int nameString = 0x20;

        public static int mapID = 0x6B63BC;
        public static int zoneID = 0x7D080C;

        public static int partyLeader = 0xBD1968;
        public static int partyplayer1 = 0xBD1948;
        public static int partyplayer2 = partyplayer1 + 0x8;
        public static int partyplayer3 = partyplayer2 + 0x8;
        public static int partyplayer4 = partyplayer3 + 0x8;

        public static int ctmBase = 0x8A11D8;
        public static int ctmX = ctmBase + 0x8C;
        public static int ctmY = ctmBase + 0x90;
        public static int ctmZ = ctmBase + 0x94;
        public static int ctmAction = ctmBase + 0x1C;
    }
}
