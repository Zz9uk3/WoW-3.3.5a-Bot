using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmeisenOffsets
{
    /// <summary>
    /// Abstract class that contains all the offsets
    /// </summary>
    public abstract class WoWOffsets
    {
        public static uint playerName = 0x879D18;
        public static uint localPlayerGUID = 0x8A1238;
        public static uint localTargetGUID = 0x7D07B0;

        public static uint currentClientConnection = 0x879CE0;
        public static uint currentManagerOffset = 0x2ED0;
        public static uint firstObjectOffset = 0xAC;
        public static uint nextObjectOffset = 0x3C;
        public static uint gameobjectTypeOffset = 0x14;
        public static uint gameobjectGUIDOffset = 0x30;

        public static uint playerBase = 0x8D87A8;
        public static uint playerHealth = 0x19B8;

        public static uint nameStore = 0x00C5D938 + 0x8;
        public static uint nameMask = 0x24;
        public static uint nameBase = 0x1C;
        public static uint nameString = 0x20;

        public static uint mapID = 0x6B63BC;
        public static uint zoneID = 0x7D080C;

        public static uint partyLeader = 0xBD1968;
        public static uint partyplayer1 = 0xBD1948;
        public static uint partyplayer2 = partyplayer1 + 0x8;
        public static uint partyplayer3 = partyplayer2 + 0x8;
        public static uint partyplayer4 = partyplayer3 + 0x8;

        public static uint ctmBase = 0x8A11D8;
        public static uint ctmX = ctmBase + 0x8C;
        public static uint ctmY = ctmBase + 0x90;
        public static uint ctmZ = ctmBase + 0x94;
        public static uint ctmAction = ctmBase + 0x1C;
        public static uint ctmGUID = ctmBase + 0x20;
        public static uint ctmDistance = ctmBase + 0xC;

        public static uint FrameScriptExecute = 0x819210;

        public static uint devicePtr1 = 0xC5DF88;
        public static uint devicePtr2 = 0x397C;
        public static uint endScene = 0xA8;

        public static uint clientObjectManagerGetActivePlayerObject = 0x4038F0;
        public static uint frameScriptGetLocalizedText = 0x7225E0;

        public static uint LuaDoString = 0x819210;
        public static uint LuaGetLocalizedText = 0x7225E0;
    }
}
