using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AmeisenCore.Objects;
using System.Threading;
using Magic;
using AmeisenLogging;
using System.Runtime.InteropServices;
using System.Windows.Input;
using AmeisenUtilities;

namespace AmeisenCore
{
    /// <summary>
    /// Abstract class that contains various static method's
    /// to interact with WoW's memory and the EndScene hook.
    /// </summary>
    public abstract class AmeisenCore
    {
        // - Imports for the SendMessage Windows interactions
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        // - Imports for the SendMessage Windows interactions

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
                AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Found WoW Process! PID: " + p.Id, "AmeisenCore.AmeisenCore");

                WoWExe wow = new WoWExe();
                BlackMagic blackmagic = new BlackMagic(p.Id);

                wow.characterName = blackmagic.ReadASCIIString(WoWOffsets.playerName, 12);
                wow.process = p;
                wows.Add(wow);
            }

            return wows;
        }

        /// <summary>
        /// Move the Player to the given x, y and z coordinates.
        /// </summary>
        /// <param name="pos">Vector3 containing the X,y & Z coordinates</param>
        /// <param name="action">CTM Interaction to perform</param>
        public static void MovePlayerToXYZ(Vector3 pos, Interaction action)
        {
            AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Moving to: X [" + pos.x + "] Y [" + pos.y + "] Z [" + pos.z + "]", "AmeisenCore.AmeisenCore");
            if (AmeisenManager.GetInstance().GetMe().pos.x != pos.x && AmeisenManager.GetInstance().GetMe().pos.y != pos.y && AmeisenManager.GetInstance().GetMe().pos.z != pos.z)
            {
                WriteXYZToMemory(pos, action);
            }
        }

        /// <summary>
        /// Move the player to the given guid npc, object or whatever and iteract with it.
        /// </summary>
        /// <param name="pos">Vector3 containing the X,y & Z coordinates</param>
        /// <param name="guid">guid of the entity</param>
        /// <param name="action">CTM Interaction to perform</param>
        public static void InteractWithGUID(Vector3 pos, UInt64 guid, Interaction action)
        {
            AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Interacting: X [" + pos.x + "] Y [" + pos.y + "] Z [" + pos.z + "] GUID [" + guid + "]", "AmeisenCore.AmeisenCore");
            AmeisenManager.GetInstance().GetBlackMagic().WriteUInt64(WoWOffsets.ctmGUID, guid);
            MovePlayerToXYZ(pos, action);
        }

        /// <summary>
        /// Write the coordinates and action to the memory.
        /// </summary>
        /// <param name="pos">Vector3 containing the X,y & Z coordinates</param>
        /// <param name="action">CTM Interaction to perform</param>
        private static void WriteXYZToMemory(Vector3 pos, Interaction action)
        {
            const float distance = 1.5f;

            AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Writing: X [" + pos.x + "] Y [" + pos.y + "] Z [" + pos.z + "] Action [" + action + "] Distance [" + distance + "]", "AmeisenCore.AmeisenCore");
            AmeisenManager.GetInstance().GetBlackMagic().WriteFloat(WoWOffsets.ctmX, pos.x);
            AmeisenManager.GetInstance().GetBlackMagic().WriteFloat(WoWOffsets.ctmY, pos.y);
            AmeisenManager.GetInstance().GetBlackMagic().WriteFloat(WoWOffsets.ctmZ, pos.z);
            AmeisenManager.GetInstance().GetBlackMagic().WriteInt(WoWOffsets.ctmAction, (int)action);
            AmeisenManager.GetInstance().GetBlackMagic().WriteFloat(WoWOffsets.ctmDistance, distance);
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
            AmeisenLogger.GetInstance().Log(LogLevel.VERBOSE, "Doing string: Command [" + command + "]", "AmeisenCore.AmeisenCore");
            uint argCC = AmeisenManager.GetInstance().GetBlackMagic().AllocateMemory(Encoding.UTF8.GetBytes(command).Length + 1);
            AmeisenManager.GetInstance().GetBlackMagic().WriteBytes(argCC, Encoding.UTF8.GetBytes(command));

            string[] asm = new string[]
            {
                "mov eax, " + argCC,
                "push 0",
                "push eax",
                "push eax",
                "mov eax, " + ((uint)AmeisenUtilities.WoWOffsets.LuaDoString),
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
            AmeisenLogger.GetInstance().Log(LogLevel.VERBOSE, "Getting text: Command [" + command + "]", "AmeisenCore.AmeisenCore");
            uint argCC = AmeisenManager.GetInstance().GetBlackMagic().AllocateMemory(Encoding.UTF8.GetBytes(command).Length + 1);
            AmeisenManager.GetInstance().GetBlackMagic().WriteBytes(argCC, Encoding.UTF8.GetBytes(command));

            string[] asm = new string[]
            {
            "call " + WoWOffsets.clientObjectManagerGetActivePlayerObject,
            "mov ecx, eax",
            "push -1",
            "mov edx, " + argCC + "",
            "push edx",
            "call " + WoWOffsets.frameScriptGetLocalizedText,
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
            AmeisenLogger.GetInstance().Log(LogLevel.VERBOSE, "Reading: GUID [" + guid + "]", "AmeisenCore.AmeisenCore");
            
            foreach (WoWObject o in AmeisenManager.GetInstance().GetObjects())
                if (o.guid == guid)
                    return o.memoryLocation;

            return 0;
        }

        public static List<WoWObject> RefreshAllWoWObjects()
        {
            AmeisenLogger.GetInstance().Log(LogLevel.VERBOSE, "Refreshing WoWObjects", "AmeisenCore.AmeisenCore");
            List<WoWObject> objects = new List<WoWObject>();

            uint currentObjectManager = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(WoWOffsets.currentClientConnection);
            currentObjectManager = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(currentObjectManager + WoWOffsets.currentManagerOffset);

            uint activeObj = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(currentObjectManager + WoWOffsets.firstObjectOffset);
            uint objType = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(activeObj + WoWOffsets.gameobjectTypeOffset);
            UInt64 objGUID = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt64(activeObj + WoWOffsets.gameobjectGUIDOffset);

            // loop through the objects until an object is bigger than 7 or lower than 1, that means we got all objects
            while (objType <= 7 && objType > 0)
            {
                objGUID = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt64((activeObj + WoWOffsets.gameobjectGUIDOffset));

                WoWObject wowObject = ReadWoWObjectFromGUID(false, objGUID, activeObj);
                wowObject.memoryLocation = activeObj;

                objects.Add(wowObject);

                activeObj = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(activeObj + WoWOffsets.nextObjectOffset);
                objType = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(activeObj + WoWOffsets.gameobjectTypeOffset);
            }
            return objects;
        }

        /// <summary>
        /// Get any NPC's name by its BaseAdress
        /// </summary>
        /// <param name="objBase">BaseAdress of the npc to search the name for</param>
        /// <returns>name of the npc</returns>
        private static string GetMobNameFromBase(uint objBase)
        {
            AmeisenLogger.GetInstance().Log(LogLevel.VERBOSE, "Reading: ObjectBase [" + objBase + "]", "AmeisenCore.AmeisenCore");
            uint objName = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(objBase + 0x964);
            objName = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(objName + 0x05C);

            return AmeisenManager.GetInstance().GetBlackMagic().ReadASCIIString(objName, 24);
        }

        /// <summary>
        /// Get a player's name from its GUID
        /// </summary>
        /// <param name="guid">player's GUID</param>
        /// <returns>name of the player</returns>
        private static string GetPlayerNameFromGuid(UInt64 guid)
        {
            AmeisenLogger.GetInstance().Log(LogLevel.VERBOSE, "Reading: GUID [" + guid + "]", "AmeisenCore.AmeisenCore");
            uint playerMask, playerBase, shortGUID, testGUID, offset, current;

            playerMask = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt((WoWOffsets.nameStore + WoWOffsets.nameMask));
            playerBase = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt((WoWOffsets.nameStore + WoWOffsets.nameBase));

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

            return AmeisenManager.GetInstance().GetBlackMagic().ReadASCIIString(current + WoWOffsets.nameString, 12);
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
                me.target.distance = Math.Sqrt((me.pos.x - me.target.pos.x) * (me.pos.x - me.target.pos.x) +
                                               (me.pos.y - me.target.pos.y) * (me.pos.y - me.target.pos.y) +
                                               (me.pos.z - me.target.pos.z) * (me.pos.z - me.target.pos.z));
            }

            return me;
        }

        /// <summary>
        /// Read WoWObject from WoW's memory by its GUID
        /// </summary>
        /// <param name="isMyself">only set to true if you want to read the bots char's target</param>
        /// <param name="guid">guid of the object</param>
        /// <returns>the WoWObject</returns>
        private static WoWObject ReadWoWObjectFromGUID(bool isMyself, UInt64 guid, uint baseaddress = 0)
        {
            AmeisenLogger.GetInstance().Log(LogLevel.VERBOSE, "Reading: isMyself [" + isMyself + "] GUID [" + guid + "]", "AmeisenCore.AmeisenCore");
            WoWObject wowObject;

            if (isMyself)
                wowObject = new Me();
            else
                wowObject = new Target();

            uint targetBase = baseaddress;

            if (baseaddress == 0)
                targetBase = GetMemLocByGUID(guid);

            uint targetBaseUnitFields = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt((targetBase + 0x8));

            wowObject.guid = guid;
            wowObject.objectType = (WoWObjectType)AmeisenManager.GetInstance().GetBlackMagic().ReadInt((targetBase + 0x14));


            // If it is me get my name the easy way, else get the targets name
            if (isMyself)
                wowObject.name = AmeisenManager.GetInstance().GetBlackMagic().ReadASCIIString(WoWOffsets.playerName, 12);
            else
            {
                if (wowObject.objectType == WoWObjectType.UNIT)
                    wowObject.name = GetMobNameFromBase(targetBase);
                else if (wowObject.objectType == WoWObjectType.PLAYER)
                    wowObject.name = GetPlayerNameFromGuid(guid);
                else
                    wowObject.name = "Unknown";
            }

            // If it is me, try to get the target and groupmembers
            if (isMyself)
            {
                uint playerbasex = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(WoWOffsets.playerBase);
                playerbasex = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(playerbasex + 0x34);
                playerbasex = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(playerbasex + 0x24);

                ((Me)wowObject).exp = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(playerbasex + 0x3794);
                ((Me)wowObject).maxExp = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(playerbasex + 0x3798);

                ((Me)wowObject).partymembers = new List<Target>();

                UInt64 leaderGUID = 0;

                try
                {
                    leaderGUID = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt64((WoWOffsets.partyLeader));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                if (leaderGUID != 0)
                {
                    ((Me)wowObject).partymembers.Add(TryReadPartymember(leaderGUID, WoWOffsets.partyplayer1));
                    ((Me)wowObject).partymembers.Add(TryReadPartymember(leaderGUID, WoWOffsets.partyplayer2));
                    ((Me)wowObject).partymembers.Add(TryReadPartymember(leaderGUID, WoWOffsets.partyplayer3));
                    ((Me)wowObject).partymembers.Add(TryReadPartymember(leaderGUID, WoWOffsets.partyplayer4));
                }
            }

            wowObject.level = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(targetBaseUnitFields + (0x36 * 4));
            wowObject.health = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(targetBaseUnitFields + (0x18 * 4));
            wowObject.maxHealth = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(targetBaseUnitFields + (0x20 * 4));
            wowObject.energy = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(targetBaseUnitFields + (0x19 * 4));
            wowObject.maxEnergy = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(targetBaseUnitFields + (0x21 * 4));
            wowObject.summonedBy = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(targetBaseUnitFields + (0xE * 4));

            wowObject.targetGUID = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt64(targetBaseUnitFields + (0x12 * 4));
            wowObject.combatReach = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(targetBaseUnitFields + (0x42 * 4));
            wowObject.factionTemplate = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(targetBaseUnitFields + (0x37 * 4));
            wowObject.channelSpell = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(targetBaseUnitFields + (0x16 * 4));

            wowObject.pos.x = AmeisenManager.GetInstance().GetBlackMagic().ReadFloat(targetBase + 0x798);
            wowObject.pos.y = AmeisenManager.GetInstance().GetBlackMagic().ReadFloat(targetBase + 0x79C);
            wowObject.pos.z = AmeisenManager.GetInstance().GetBlackMagic().ReadFloat(targetBase + 0x7A0);
            wowObject.rotation = AmeisenManager.GetInstance().GetBlackMagic().ReadFloat(targetBase + 0x7A8);

            wowObject.mapID = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(WoWOffsets.mapID);
            wowObject.zoneID = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(WoWOffsets.zoneID);

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
            AmeisenLogger.GetInstance().Log(LogLevel.VERBOSE, "Reading: GUID [" + leaderGUID + "] Offset [" + offset + "]", "AmeisenCore.AmeisenCore");
            try
            {
                Target t = (Target)ReadWoWObjectFromGUID(false, AmeisenManager.GetInstance().GetBlackMagic().ReadUInt64(offset));
                Me me = AmeisenManager.GetInstance().GetMe();

                if (t.pos.x != 0 && t.pos.y != 0 && t.pos.z != 0)
                    t.distance = Math.Sqrt((me.pos.x - t.pos.x) * (me.pos.x - t.pos.x) +
                                           (me.pos.y - t.pos.y) * (me.pos.y - t.pos.y) +
                                           (me.pos.z - t.pos.z) * (me.pos.z - t.pos.z));

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
            return AmeisenManager.GetInstance().GetBlackMagic().ReadUInt64(WoWOffsets.localPlayerGUID);
        }

        /// <summary>
        /// Get the bot's char's target's GUID
        /// </summary>
        /// <returns></returns>
        public static UInt64 GetTargetGUID()
        {
            return AmeisenManager.GetInstance().GetBlackMagic().ReadUInt64(WoWOffsets.localTargetGUID);
        }

        /// <summary>
        /// Set the target by its GUID
        /// </summary>
        /// <returns></returns>
        public static void TargetGUID(UInt64 guid)
        {
            //target it...
        }

        /// <summary>
        /// Set the target by its GUID
        /// </summary>
        /// <returns></returns>
        public static void TargetMyself()
        {
            Me me = AmeisenManager.GetInstance().GetMe();
            TargetGUID(GetPlayerGUID());
        }

        /// <summary>
        /// Let the bot jump by pressing the spacebar once for 20-40ms
        /// 
        /// This runs Async.
        /// </summary>
        public static void CharacterJumpAsync()
        {
            AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Jumping", "AmeisenCore.AmeisenCore");
            new Thread(CharacterJump).Start();
        }

        private static void CharacterJump()
        {
            const uint KEYDOWN = 0x100;
            const uint KEYUP = 0x101;

            IntPtr windowHandle = AmeisenManager.GetInstance().GetProcess().MainWindowHandle;

            // 0x20 = Spacebar (VK_SPACE)
            SendMessage(windowHandle, KEYDOWN, new IntPtr(0x20), new IntPtr(0));
            Thread.Sleep(new Random().Next(20, 40)); // make it look more human-like :^)
            SendMessage(windowHandle, KEYUP, new IntPtr(0x20), new IntPtr(0));
        }
    }
}
