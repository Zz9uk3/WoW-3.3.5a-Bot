using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AmeisenCore.Objects;
using System.Threading;
using Magic;

namespace AmeisenCore
{
    /// <summary>
    /// Abstract class that contains various static method's
    /// to interact with WoW's memory and the EndScene hook.
    /// </summary>
    public abstract class AmeisenCore
    {
        /// <summary>
        /// Returns the running WoW's in a WoWExe List
        /// containing the logged in playername and Process object.
        /// </summary>
        /// <returns>A list containing all the runnign WoW processes</returns>
        public static List<WoWExe> GetRunningWoWs()
        {
            List<WoWExe> wows = new List<WoWExe>();
            List<Process> processList = new List<Process>(Process.GetProcessesByName("Wow"));

            foreach (Process p in processList)
            {
                Console.WriteLine("Found WoW Process! PID: " + p.Id);

                WoWExe wow = new WoWExe();
                BlackMagic blackmagic = new BlackMagic(p.Id);

                wow.characterName = blackmagic.ReadASCIIString(AmeisenOffsets.WoWOffsets.playerName, 12);
                wow.process = p;
                wows.Add(wow);
            }

            return wows;
        }

        /// <summary>
        /// Move the Player to the given x, y and z coordinates.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="z">z coordinate</param>
        public static void MovePlayerToXYZ(float x, float y, float z)
        {
            if (AmeisenManager.GetInstance().GetMe().posX != x && AmeisenManager.GetInstance().GetMe().posY != y && AmeisenManager.GetInstance().GetMe().posZ != z)
            {
                WriteXYZToMemory(x, y, z, 0x4);
            }
        }

        /// <summary>
        /// Move the player to the given guid npc, object or whatever and iteract with it.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="z">z coordinate</param>
        /// <param name="guid">guid of the entity</param>
        public static void InteractWithGUID(float x, float y, float z, UInt64 guid)
        {
            AmeisenManager.GetInstance().GetBlackMagic().WriteUInt64(AmeisenOffsets.WoWOffsets.ctmGUID, guid);
            WriteXYZToMemory(x, y, z, 0x5);
        }

        /// <summary>
        /// Move the player to this npc or player and attack it.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="z">z coordinate</param>
        /// <param name="guid">guid of the entitiy to attack</param>
        public static void AttackGUID(float x, float y, float z, UInt64 guid)
        {
            AmeisenManager.GetInstance().GetBlackMagic().WriteUInt64(AmeisenOffsets.WoWOffsets.ctmGUID, guid);
            WriteXYZToMemory(x, y, z, 0xA);
        }

        /// <summary>
        /// Move the player to an object and loot it.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="z">z coordinate</param>
        /// <param name="guid">guid of the entity to loot</param>
        public static void LootGUID(float x, float y, float z, UInt64 guid)
        {
            AmeisenManager.GetInstance().GetBlackMagic().WriteUInt64(AmeisenOffsets.WoWOffsets.ctmGUID, guid);
            WriteXYZToMemory(x, y, z, 0x6);
        }

        /// <summary>
        /// Write the coordinates and action to the memory.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="z">z coordinate</param>
        /// <param name="action">0x4 = move
        /// 0x5 = interact
        /// 0x6 = loot
        /// 0xA = attack</param>
        private static void WriteXYZToMemory(float x, float y, float z, int action)
        {
            AmeisenManager.GetInstance().GetBlackMagic().WriteFloat(AmeisenOffsets.WoWOffsets.ctmX, x);
            AmeisenManager.GetInstance().GetBlackMagic().WriteFloat(AmeisenOffsets.WoWOffsets.ctmY, y);
            AmeisenManager.GetInstance().GetBlackMagic().WriteFloat(AmeisenOffsets.WoWOffsets.ctmZ, z);
            AmeisenManager.GetInstance().GetBlackMagic().WriteInt(AmeisenOffsets.WoWOffsets.ctmAction, action);
            AmeisenManager.GetInstance().GetBlackMagic().WriteFloat(AmeisenOffsets.WoWOffsets.ctmDistance, 1.5f);
        }

        public static int GetContainerNumFreeSlots()
        {
            LUADoString("print(\"Hello World\");");
            return 0;
        }

        /// <summary>
        /// Execute the given LUA command inside WoW to
        /// for example switch targets.
        /// 
        /// !!! W.I.P !!!
        /// </summary>
        /// <param name="command">lua command to run</param>
        public static void LUADoString(string command)
        {
            uint argCC = AmeisenManager.GetInstance().GetBlackMagic().AllocateMemory(Encoding.UTF8.GetBytes(command).Length + 1);
            AmeisenManager.GetInstance().GetBlackMagic().WriteBytes(argCC, Encoding.UTF8.GetBytes(command));

            string[] asm = new string[]
            {
                "mov eax, " + argCC,
                "push 0",
                "push eax",
                "push eax",
                "mov eax, " + ((uint)AmeisenOffsets.WoWOffsets.LuaDoString),
                "call eax",
                "add esp, 0xC",
                "retn",
            };

            AmeisenManager.GetInstance().GetAmeisenHook().InjectAndExecute(asm);
            AmeisenManager.GetInstance().GetBlackMagic().FreeMemory(argCC);
        }

        /// <summary>
        /// Get Localized Text for command
        /// 
        /// !!! W.I.P !!!
        /// </summary>
        /// <param name="command">lua command to run</param>
        public static string GetLocalizedText(string command)
        {
            uint argCC = AmeisenManager.GetInstance().GetBlackMagic().AllocateMemory(Encoding.UTF8.GetBytes(command).Length + 1);
            AmeisenManager.GetInstance().GetBlackMagic().WriteBytes(argCC, Encoding.UTF8.GetBytes(command));

            string[] asm = new string[]
            {
            "call " + AmeisenOffsets.WoWOffsets.clientObjectManagerGetActivePlayerObject,
            "mov ecx, eax",
            "push -1",
            "mov edx, " + argCC + "",
            "push edx",
            "call " + AmeisenOffsets.WoWOffsets.frameScriptGetLocalizedText,
            "retn",
            };

            string result = Encoding.UTF8.GetString(AmeisenManager.GetInstance().GetAmeisenHook().InjectAndExecute(asm));
            AmeisenManager.GetInstance().GetBlackMagic().FreeMemory(argCC);

            return result;
        }

        /// <summary>
        /// Run through the WoWObjectManager and find the BaseAdress
        /// corresponding to the given GUID
        /// </summary>
        /// <param name="guid">guid to search for</param>
        /// <returns>BaseAdress of the WoWObject</returns>
        private static uint GetMemLocByGUID(UInt64 guid)
        {
            uint currentObjectManager = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(AmeisenOffsets.WoWOffsets.currentClientConnection);
            currentObjectManager = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(currentObjectManager + AmeisenOffsets.WoWOffsets.currentManagerOffset);

            uint activeObj = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(currentObjectManager + AmeisenOffsets.WoWOffsets.firstObjectOffset);
            uint objType = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(activeObj + AmeisenOffsets.WoWOffsets.gameobjectTypeOffset);
            UInt64 objGUID = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt64(activeObj + AmeisenOffsets.WoWOffsets.gameobjectGUIDOffset);

            int count = 1;

            // loop through the objects until an object is bigger than 7 or lower than 1
            // Object Types:
            // ITEM      = 1
            // CONTAINER = 2
            // UNIT      = 3
            // PLAYER    = 4
            // GAMEOBJ   = 5
            // DYNOBJ    = 6
            // CORPSE    = 7
            while (objType <= 7 && objType > 0)
            {
                objGUID = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt64((uint)(activeObj + AmeisenOffsets.WoWOffsets.gameobjectGUIDOffset));

                if (objGUID == guid)
                    return activeObj;

                activeObj = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(activeObj + AmeisenOffsets.WoWOffsets.nextObjectOffset);
                objType = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(activeObj + AmeisenOffsets.WoWOffsets.gameobjectTypeOffset);
                count++;
            }

            return 0;
        }

        /// <summary>
        /// Get any NPC's name by its BaseAdress
        /// </summary>
        /// <param name="objBase">BaseAdress of the npc to search the name for</param>
        /// <returns>name of the npc</returns>
        private static string GetMobNameFromBase(uint objBase)
        {
            uint objName = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(objBase + 0x964);
            objName = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(objName + 0x05C);

            return AmeisenManager.GetInstance().GetBlackMagic().ReadASCIIString(objName, 12);
        }

        /// <summary>
        /// Get a player's name from its GUID
        /// </summary>
        /// <param name="guid">player's GUID</param>
        /// <returns>name of the player</returns>
        private static string GetPlayerNameFromGuid(UInt64 guid)
        {
            uint playerMask, playerBase, shortGUID, testGUID, offset, current;

            playerMask = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt((AmeisenOffsets.WoWOffsets.nameStore + AmeisenOffsets.WoWOffsets.nameMask));
            playerBase = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt((AmeisenOffsets.WoWOffsets.nameStore + AmeisenOffsets.WoWOffsets.nameBase));

            // Shorten the GUID
            shortGUID = (uint)guid & 0xfffffff;
            offset = 12 * (playerMask & shortGUID);

            current = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(playerBase + offset + 8);
            offset = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(playerBase + offset);

            // Check for empty name
            if ((current & 0x1) == 0x1) { return ""; }

            testGUID = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(current);

            while (testGUID != shortGUID)
            {
                current = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(current + offset + 4);

                // Check for empty name
                if ((current & 0x1) == 0x1) { return ""; }
                testGUID = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(current);
            }

            return AmeisenManager.GetInstance().GetBlackMagic().ReadASCIIString(current + AmeisenOffsets.WoWOffsets.nameString, 12);
        }

        /// <summary>
        /// Get the current state of the bots character including its target
        /// </summary>
        /// <returns>the bots character information</returns>
        public static Me ReadMe()
        {
            Me me = (Me)ReadWoWObjectFromGUID(true, GetPlayerGUID());
            UInt64 targetGUID = GetTargetGUID();


            // If we have a target lets read it
            if (targetGUID != 0)
            {
                // Read all information from memory
                me.target = (Target)ReadWoWObjectFromGUID(false, targetGUID);

                // Calculate the distance
                me.target.distance = Math.Sqrt((me.posX - me.target.posX) * (me.posX - me.target.posX) +
                                               (me.posY - me.target.posY) * (me.posY - me.target.posY) +
                                               (me.posZ - me.target.posZ) * (me.posZ - me.target.posZ));
            }

            return me;
        }

        /// <summary>
        /// Read WoWObject from WoW's memory by its GUID
        /// </summary>
        /// <param name="isMyself">only set to true if you want to read the bots char's target</param>
        /// <param name="guid">guid of the object</param>
        /// <returns>the WoWObject</returns>
        private static WoWObject ReadWoWObjectFromGUID(bool isMyself, UInt64 guid)
        {
            WoWObject wowObject;

            if (isMyself)
                wowObject = new Me();
            else
                wowObject = new Target();

            uint targetBase = GetMemLocByGUID(guid);
            uint targetBaseUnitFields = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt((uint)(targetBase + 0x8));

            wowObject.guid = guid;
            wowObject.objectType = AmeisenManager.GetInstance().GetBlackMagic().ReadInt((uint)(targetBase + 0x14));


            // If it is me get my name the easy way, else get the targets name
            if (isMyself)
                wowObject.name = AmeisenManager.GetInstance().GetBlackMagic().ReadASCIIString(AmeisenOffsets.WoWOffsets.playerName, 12);
            else
            {
                if (wowObject.objectType == 3)
                    wowObject.name = GetMobNameFromBase(targetBase);
                else if (wowObject.objectType == 4)
                    wowObject.name = GetPlayerNameFromGuid(guid);
                else
                    wowObject.name = "Unknown";
            }

            // If it is me, try to get the target and groupmembers
            if (isMyself)
            {
                uint playerbasex = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(AmeisenOffsets.WoWOffsets.playerBase);
                playerbasex = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(playerbasex + 0x34);
                playerbasex = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(playerbasex + 0x24);

                ((Me)wowObject).exp = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(playerbasex + 0x3794);
                ((Me)wowObject).maxExp = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(playerbasex + 0x3798);

                ((Me)wowObject).partymembers = new List<Target>();

                UInt64 leaderGUID = 0;

                try
                {
                    leaderGUID = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt64((AmeisenOffsets.WoWOffsets.partyLeader));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                if (leaderGUID != 0)
                {
                    ((Me)wowObject).partymembers.Add(TryReadPartymember(leaderGUID, AmeisenOffsets.WoWOffsets.partyplayer1));
                    ((Me)wowObject).partymembers.Add(TryReadPartymember(leaderGUID, AmeisenOffsets.WoWOffsets.partyplayer2));
                    ((Me)wowObject).partymembers.Add(TryReadPartymember(leaderGUID, AmeisenOffsets.WoWOffsets.partyplayer3));
                    ((Me)wowObject).partymembers.Add(TryReadPartymember(leaderGUID, AmeisenOffsets.WoWOffsets.partyplayer4));
                }
            }

            wowObject.level = AmeisenManager.GetInstance().GetBlackMagic().ReadInt((uint)targetBaseUnitFields + (0x36 * 4));
            wowObject.health = AmeisenManager.GetInstance().GetBlackMagic().ReadInt((uint)targetBaseUnitFields + (0x18 * 4));
            wowObject.maxHealth = AmeisenManager.GetInstance().GetBlackMagic().ReadInt((uint)targetBaseUnitFields + (0x20 * 4));
            wowObject.energy = AmeisenManager.GetInstance().GetBlackMagic().ReadInt((uint)targetBaseUnitFields + (0x19 * 4));
            wowObject.maxEnergy = AmeisenManager.GetInstance().GetBlackMagic().ReadInt((uint)targetBaseUnitFields + (0x21 * 4));
            wowObject.summonedBy = AmeisenManager.GetInstance().GetBlackMagic().ReadInt((uint)targetBaseUnitFields + (0xE * 4));

            wowObject.targetGUID = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt64((uint)targetBaseUnitFields + (0x12 * 4));
            wowObject.combatReach = AmeisenManager.GetInstance().GetBlackMagic().ReadInt((uint)targetBaseUnitFields + (0x42 * 4));
            wowObject.factionTemplate = AmeisenManager.GetInstance().GetBlackMagic().ReadInt((uint)targetBaseUnitFields + (0x37 * 4));
            wowObject.channelSpell = AmeisenManager.GetInstance().GetBlackMagic().ReadInt((uint)targetBaseUnitFields + (0x16 * 4));

            wowObject.posX = AmeisenManager.GetInstance().GetBlackMagic().ReadFloat((uint)targetBase + 0x798);
            wowObject.posY = AmeisenManager.GetInstance().GetBlackMagic().ReadFloat((uint)targetBase + 0x79C);
            wowObject.posZ = AmeisenManager.GetInstance().GetBlackMagic().ReadFloat((uint)targetBase + 0x7A0);
            wowObject.rotation = AmeisenManager.GetInstance().GetBlackMagic().ReadFloat((uint)targetBase + 0x7A8);

            wowObject.mapID = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(AmeisenOffsets.WoWOffsets.mapID);
            wowObject.zoneID = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(AmeisenOffsets.WoWOffsets.zoneID);

            return wowObject;
        }

        /// <summary>
        /// Try to get a partymember
        /// </summary>
        /// <param name="leaderGUID">guid of the party leader</param>
        /// <param name="offset">offset to read the party member from</param>
        /// <returns>a Target object containing the party member's deatils</returns>
        private static Target TryReadPartymember(UInt64 leaderGUID, uint offset)
        {
            try
            {
                Target t = (Target)ReadWoWObjectFromGUID(false, AmeisenManager.GetInstance().GetBlackMagic().ReadUInt64(offset));
                Me me = AmeisenManager.GetInstance().GetMe();

                if (t.posX != 0 && t.posY != 0 && t.posZ != 0)
                    t.distance = Math.Sqrt((me.posX - t.posX) * (me.posX - t.posX) +
                                           (me.posY - t.posY) * (me.posY - t.posY) +
                                           (me.posZ - t.posZ) * (me.posZ - t.posZ));

                if (t.guid == leaderGUID)
                {
                    t.isPartyLeader = true;
                }
                return t;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return new Target();
        }

        /// <summary>
        /// Get the bot#s char's GUID
        /// </summary>
        /// <returns>the GUID</returns>
        public static UInt64 GetPlayerGUID()
        {
            return AmeisenManager.GetInstance().GetBlackMagic().ReadUInt64(AmeisenOffsets.WoWOffsets.localPlayerGUID);
        }

        /// <summary>
        /// Get the bot's char's target's GUID
        /// </summary>
        /// <returns></returns>
        public static UInt64 GetTargetGUID()
        {
            return AmeisenManager.GetInstance().GetBlackMagic().ReadUInt64(AmeisenOffsets.WoWOffsets.localTargetGUID);
        }

        /// <summary>
        /// Set the target by its GUID
        /// </summary>
        /// <returns></returns>
        public static void TargetGUID(UInt64 guid)
        {
            Me me = AmeisenManager.GetInstance().GetMe();
            InteractWithGUID(me.posX, me.posY, me.posZ, guid);
        }

        /// <summary>
        /// Set the target by its GUID
        /// </summary>
        /// <returns></returns>
        public static void TargetMyself()
        {
            Me me = AmeisenManager.GetInstance().GetMe();
            InteractWithGUID(me.posX, me.posY, me.posZ, GetPlayerGUID());
        }

        /// <summary>
        /// Let the bot jump.
        /// 
        /// This runs Async.
        /// </summary>
        public static void CharacterJump()
        {
            Thread keyExecutorThread = new Thread(new ThreadStart(CharacterJumpAsync));
            keyExecutorThread.Start();
        }

        /// <summary>
        /// Press the spacebar once for 100ms
        /// </summary>
        private static void CharacterJumpAsync()
        {
            /*RemoteWindow window = AmeisenManager.GetInstance().GetMemorySharp().Windows.MainWindow;
            window.Keyboard.Press(Keys.Space, TimeSpan.FromMilliseconds(20));
            Thread.Sleep(100);
            window.Keyboard.Release(Keys.Space);*/

            //TODO: Replace this old crap
        }
    }
}
