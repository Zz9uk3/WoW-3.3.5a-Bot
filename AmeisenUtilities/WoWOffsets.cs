namespace AmeisenUtilities
{
    /// <summary>
    /// Abstract class that contains all the offsets
    /// </summary>
    public abstract class WoWOffsets
    {
        public static uint arenaPlayer1 = 0xBE9F48;
        public static uint arenaPlayer2 = arenaPlayer1 + 0x8;
        public static uint arenaPlayer3 = arenaPlayer2 + 0x8;
        public static uint arenaPlayer4 = arenaPlayer3 + 0x8;
        public static uint arenaPlayer5 = arenaPlayer4 + 0x8;
        public static uint characterSlotSelected = 0x6C436C;
        public static uint clientGameUITarget = 0x524BF0;
        public static uint clientObjectManagerGetActivePlayerObject = 0x4038F0;
        public static uint continentName = 0xCE06D0;
        public static uint corpseX = 0xBD0A58;
        public static uint corpseY = corpseX + 0x4;
        public static uint corpseZ = corpseY + 0x4;
        public static uint ctmABase = 0xCA11D8;
        public static uint ctmAction = ctmABase + 0x1C;
        public static uint ctmDistance = ctmABase + 0xC;
        public static uint ctmGUID = ctmABase + 0x20;
        public static uint ctmX = ctmABase + 0x8C;
        public static uint ctmY = ctmABase + 0x90;
        public static uint ctmZ = ctmABase + 0x94;
        public static uint currentClientConnection = 0xC79CE0;
        public static uint currentManagerLocalGUID = 0xC0;
        public static uint currentManagerOffset = 0x2ED0;
        public static uint devicePtr1 = 0xC5DF88;
        public static uint devicePtr2 = 0x397C;
        public static uint dynamicObjectBytes = 0x8;
        public static uint dynamicObjectCaster = 0x6;
        public static uint dynamicObjectCastTime = 0xB;
        public static uint dynamicObjectRadius = 0xA;
        public static uint dynamicObjectSpellID = 0x9;
        public static uint endScene = 0xA8;
        public static uint firstObjectOffset = 0xAC;
        public static uint gameobjectGUIDOffset = 0x30;
        public static uint gameobjectTypeOffset = 0x14;
        public static uint gameState = 0xB6A9E0;
        public static uint isBattlegroundOver = 0xBEA588;
        public static uint isLoading = 0xB6AA30;
        public static uint localComboPoint = 0xBD0845;
        public static uint localLastTarget = 0xBD07B0;
        public static uint localLootWindowOpen = 0xBFA8D0;
        public static uint localMouseoverGUID = 0xBD07B0;
        public static uint localPlayerCharacterState = 0x6DACA4;
        public static uint localPlayerCharacterStateOffset1 = 0xC;
        public static uint localPlayerCharacterStateOffset2 = 0x94;
        public static uint localPlayerCharacterStateOffset3 = 0x90;
        public static uint localPlayerGUID = 0xCA1238;
        public static uint localTargetGUID = 0xBD07B0;
        public static uint luaDoString = 0x819210;
        public static uint luaGetLocalizedText = 0x7225E0;
        public static uint mapID = 0xBD0804;
        public static uint nameBase = 0x1C;
        public static uint nameMask = 0x24;
        public static uint nameStore = 0xC5D938 + 0x8;
        public static uint nameString = 0x20;
        public static uint nextObjectOffset = 0x3C;
        public static uint partyLeader = 0xBD1968;
        public static uint partyPlayer1 = 0xBD1948;
        public static uint partyPlayer2 = partyPlayer1 + 0x8;
        public static uint partyPlayer3 = partyPlayer2 + 0x8;
        public static uint partyPlayer4 = partyPlayer3 + 0x8;
        public static uint petGUID = 0xC234D0;
        public static uint playerBase = 0xCD87A8;
        public static uint playerCorpseX = 0xBD0A58;
        public static uint playerCorpseY = playerCorpseX + 0x4;
        public static uint playerCorpseZ = playerCorpseY + 0x4;
        public static uint playerHealth = 0x19B8;
        public static uint playerIsIngame = 0xBD0792;
        public static uint playerIsLoadingscreen = 0xB6AA38;
        public static uint playerName = 0xC79D18;
        public static uint realmName = 0xC79B9E;
        public static uint sendMovementPacket = 0x7413F0;
        public static uint setFacing = 0x9606E0;
        public static uint staticCastingstate = 0x6F5250;
        public static uint subZoneText = 0xBD0784;
        public static uint tickCount = 0xB499A4;
        public static uint timestamp = 0xB1D618;
        public static uint worldLoaded = 0xBEBA40;
        public static uint wowChat = 0xB75A60;
        public static uint wowChatNextMsg = 0x17C0;
        public static uint zoneID = 0xBD080C;
        public static uint zoneText = 0xBD0788;
    }
}