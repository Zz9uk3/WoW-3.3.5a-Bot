namespace AmeisenUtilities
{
    /// <summary>
    /// Abstract class that contains all the offsets
    /// </summary>
    public abstract class Offsets
    {
        public static readonly uint arenaPlayer1 = 0xBE9F48;
        public static readonly uint arenaPlayer2 = arenaPlayer1 + 0x8;
        public static readonly uint arenaPlayer3 = arenaPlayer2 + 0x8;
        public static readonly uint arenaPlayer4 = arenaPlayer3 + 0x8;
        public static readonly uint arenaPlayer5 = arenaPlayer4 + 0x8;
        public static readonly uint characterSlotSelected = 0x6C436C;
        public static readonly uint clientGameUITarget = 0x524BF0;
        public static readonly uint clientObjectManagerGetActivePlayerObject = 0x4038F0;
        public static readonly uint continentName = 0xCE06D0;
        public static readonly uint corpseX = 0xBD0A58;
        public static readonly uint corpseY = corpseX + 0x4;
        public static readonly uint corpseZ = corpseY + 0x4;
        public static readonly uint ctmABase = 0xCA11D8;
        public static readonly uint ctmAction = ctmABase + 0x1C;
        public static readonly uint ctmDistance = ctmABase + 0xC;
        public static readonly uint ctmGUID = ctmABase + 0x20;
        public static readonly uint ctmX = ctmABase + 0x8C;
        public static readonly uint ctmY = ctmABase + 0x90;
        public static readonly uint ctmZ = ctmABase + 0x94;
        public static readonly uint currentClientConnection = 0xC79CE0;
        public static readonly uint currentManagerLocalGUID = 0xC0;
        public static readonly uint currentManagerOffset = 0x2ED0;
        public static readonly uint devicePtr1 = 0xC5DF88;
        public static readonly uint devicePtr2 = 0x397C;
        public static readonly uint dynamicObjectBytes = 0x8;
        public static readonly uint dynamicObjectCaster = 0x6;
        public static readonly uint dynamicObjectCastTime = 0xB;
        public static readonly uint dynamicObjectRadius = 0xA;
        public static readonly uint dynamicObjectSpellID = 0x9;
        public static readonly uint endScene = 0xA8;
        public static readonly uint firstObjectOffset = 0xAC;
        public static readonly uint gameobjectGUIDOffset = 0x30;
        public static readonly uint gameobjectTypeOffset = 0x14;
        public static readonly uint gameState = 0xB6A9E0;
        public static readonly uint isBattlegroundOver = 0xBEA588;
        public static readonly uint isLoading = 0xB6AA30;
        public static readonly uint localComboPoint = 0xBD0845;
        public static readonly uint localLastTarget = 0xBD07B0;
        public static readonly uint localLootWindowOpen = 0xBFA8D0;
        public static readonly uint localMouseoverGUID = 0xBD07B0;
        public static readonly uint localPlayerCharacterState = 0x6DACA4;
        public static readonly uint localPlayerCharacterStateOffset1 = 0xC;
        public static readonly uint localPlayerCharacterStateOffset2 = 0x94;
        public static readonly uint localPlayerCharacterStateOffset3 = 0x90;
        public static readonly uint localPlayerGUID = 0xCA1238;
        public static readonly uint localTargetGUID = 0xBD07B0;
        public static readonly uint luaDoString = 0x819210;
        public static readonly uint luaGetLocalizedText = 0x7225E0;
        public static readonly uint mapID = 0xBD0804;
        public static readonly uint nameBase = 0x1C;
        public static readonly uint nameMask = 0x24;
        public static readonly uint nameStore = 0xC5D938 + 0x8;
        public static readonly uint nameString = 0x20;
        public static readonly uint nextObjectOffset = 0x3C;
        public static readonly uint partyLeader = 0xBD1968;
        public static readonly uint partyPlayer1 = 0xBD1948;
        public static readonly uint partyPlayer2 = partyPlayer1 + 0x8;
        public static readonly uint partyPlayer3 = partyPlayer2 + 0x8;
        public static readonly uint partyPlayer4 = partyPlayer3 + 0x8;
        public static readonly uint petGUID = 0xC234D0;
        public static readonly uint playerBase = 0xCD87A8;
        public static readonly uint playerCorpseX = 0xBD0A58;
        public static readonly uint playerCorpseY = playerCorpseX + 0x4;
        public static readonly uint playerCorpseZ = playerCorpseY + 0x4;
        public static readonly uint playerHealth = 0x19B8;
        public static readonly uint playerIsIngame = 0xBD0792;
        public static readonly uint playerIsLoadingscreen = 0xB6AA38;
        public static readonly uint playerName = 0xC79D18;
        public static readonly uint realmName = 0xC79B9E;
        public static readonly uint sendMovementPacket = 0x7413F0;
        public static readonly uint setFacing = 0x9606E0;
        public static readonly uint staticCastingstate = 0x6F5250;
        public static readonly uint subZoneText = 0xBD0784;
        public static readonly uint tickCount = 0xB499A4;
        public static readonly uint timestamp = 0xB1D618;
        public static readonly uint worldLoaded = 0xBEBA40;
        public static readonly uint wowChat = 0xB75A60;
        public static readonly uint wowChatNextMsg = 0x17C0;
        public static readonly uint zoneID = 0xBD080C;
        public static readonly uint zoneText = 0xBD0788;
    }
}