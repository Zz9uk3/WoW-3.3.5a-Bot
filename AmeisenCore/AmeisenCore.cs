using Binarysharp.MemoryManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AmeisenCore.Objects;
using Binarysharp.MemoryManagement.Native;
using System.Threading;
using Binarysharp.MemoryManagement.Windows;
using Binarysharp.MemoryManagement.Memory;

namespace AmeisenCore
{
    public class AmeisenCore
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
                MemorySharp memorySharp = new MemorySharp(p);

                wow.characterName = memorySharp.Modules.MainModule.ReadString(AmeisenOffsets.WoWOffsets.playerName, Encoding.ASCII, 12); ;
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
            AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Write(AmeisenOffsets.WoWOffsets.ctmGUID, guid);
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
            AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Write(AmeisenOffsets.WoWOffsets.ctmGUID, guid);
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
            AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Write(AmeisenOffsets.WoWOffsets.ctmGUID, guid);
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
            AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Write(AmeisenOffsets.WoWOffsets.ctmX, x);
            AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Write(AmeisenOffsets.WoWOffsets.ctmY, y);
            AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Write(AmeisenOffsets.WoWOffsets.ctmZ, z);
            AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Write(AmeisenOffsets.WoWOffsets.ctmAction, action);
            AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Write(AmeisenOffsets.WoWOffsets.ctmDistance, 1.5f);
        }

        public static int GetContainerNumFreeSlots()
        {
            LUADoString("freeslots = GetContainerNumFreeSlots(0) + GetContainerNumFreeSlots(1) + GetContainerNumFreeSlots(2) + GetContainerNumFreeSlots(3) + GetContainerNumFreeSlots(4)");
            return Convert.ToInt32(GetLocalizedText("freeslots"));
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
            RemoteAllocation argCC = AmeisenManager.GetInstance().GetMemorySharp().Memory.Allocate(Encoding.UTF8.GetBytes(command).Length + 1);
            AmeisenManager.GetInstance().GetMemorySharp().Write<byte>(argCC.BaseAddress, Encoding.UTF8.GetBytes(command));

            string[] asm = new string[]
            {
                "mov eax, " + argCC.BaseAddress,
                "push 0",
                "push eax",
                "push eax",
                "mov eax, " + AmeisenOffsets.WoWOffsets.LuaDoString,
                "call eax",
                "add esp, 0xC",
                "retn",
            };

            AmeisenManager.GetInstance().GetAmeisenHook().InjectAndExecute(asm);

            argCC.Release();
        }

        /// <summary>
        /// Get Localized Text for command
        /// 
        /// !!! W.I.P !!!
        /// </summary>
        /// <param name="command">lua command to run</param>
        public static string GetLocalizedText(string command)
        {
            RemoteAllocation argCC = AmeisenManager.GetInstance().GetMemorySharp().Memory.Allocate(Encoding.UTF8.GetBytes(command).Length + 1);            
            AmeisenManager.GetInstance().GetMemorySharp().Write<byte>(argCC.BaseAddress, Encoding.UTF8.GetBytes(command));

            string[] asm = new string[]
            {
            "call " + AmeisenOffsets.WoWOffsets.clientObjectManagerGetActivePlayerObject,
            "mov ecx, eax",
            "push -1",
            "mov edx, " + argCC.BaseAddress + "",
            "push edx",
            "call " + AmeisenOffsets.WoWOffsets.frameScriptGetLocalizedText,
            "retn",
            };
            
            return Encoding.ASCII.GetString(AmeisenManager.GetInstance().GetAmeisenHook().InjectAndExecute(asm)); ;
        }

        /// <summary>
        /// Run through the WoWObjectManager and find the BaseAdress
        /// corresponding to the given GUID
        /// </summary>
        /// <param name="guid">guid to search for</param>
        /// <returns>BaseAdress of the WoWObject</returns>
        private static int GetMemLocByGUID(UInt64 guid)
        {
            int currentObjectManager = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>(AmeisenOffsets.WoWOffsets.currentClientConnection);
            currentObjectManager = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((currentObjectManager + AmeisenOffsets.WoWOffsets.currentManagerOffset) - 0x400000);

            int activeObj = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((currentObjectManager + AmeisenOffsets.WoWOffsets.firstObjectOffset) - 0x400000);
            int objType = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((activeObj + AmeisenOffsets.WoWOffsets.gameobjectTypeOffset) - 0x400000);
            UInt64 objGUID = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<UInt64>((activeObj + AmeisenOffsets.WoWOffsets.gameobjectGUIDOffset) - 0x400000);

            int count = 1;

            while (objType <= 7 && objType > 0)
            {
                objGUID = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<UInt64>((activeObj + AmeisenOffsets.WoWOffsets.gameobjectGUIDOffset) - 0x400000);

                if (objGUID == guid)
                    return activeObj;

                activeObj = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((activeObj + AmeisenOffsets.WoWOffsets.nextObjectOffset) - 0x400000);
                objType = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((activeObj + AmeisenOffsets.WoWOffsets.gameobjectTypeOffset) - 0x400000);
                count++;
            }

            return 0;
        }

        /// <summary>
        /// Get any NPC's name by its BaseAdress
        /// </summary>
        /// <param name="objBase">BaseAdress of the npc to search the name for</param>
        /// <returns>name of the npc</returns>
        private static string GetMobNameFromBase(int objBase)
        {
            int objName = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((objBase + 0x964) - 0x400000);
            objName = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((objName + 0x05C) - 0x400000);

            return AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.ReadString((objName) - 0x400000, Encoding.ASCII);
        }

        /// <summary>
        /// Get a player's name from its GUID
        /// </summary>
        /// <param name="guid">player's GUID</param>
        /// <returns>name of the player</returns>
        private static string GetPlayerNameFromGuid(UInt64 guid)
        {
            int mask, base_, shortGUID, testGUID, offset, current;

            mask = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((AmeisenOffsets.WoWOffsets.nameStore + AmeisenOffsets.WoWOffsets.nameMask) - 0x400000);
            base_ = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((AmeisenOffsets.WoWOffsets.nameStore + AmeisenOffsets.WoWOffsets.nameBase) - 0x400000);

            shortGUID = (int)guid & 0xfffffff;
            offset = 12 * (mask & shortGUID);

            current = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((base_ + offset + 8) - 0x400000);
            offset = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((base_ + offset) - 0x400000);

            if ((current & 0x1) == 0x1) { return ""; }

            testGUID = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((current) - 0x400000);

            while (testGUID != shortGUID)
            {
                current = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((current + offset + 4) - 0x400000);

                if ((current & 0x1) == 0x1) { return ""; }
                testGUID = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((current) - 0x400000);
            }

            return AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.ReadString((current + AmeisenOffsets.WoWOffsets.nameString) - 0x400000, Encoding.ASCII);
        }

        /// <summary>
        /// Get the current state of the bots character including its target
        /// </summary>
        /// <returns>the bots character information</returns>
        public static Me GetMe()
        {
            Me me = (Me)ReadWoWObjectFromGUID(true, GetPlayerGUID());

            UInt64 targetGUID = GetTargetGUID();
            if (targetGUID != 0)
            {
                me.target = (Target)ReadWoWObjectFromGUID(false, targetGUID);

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

            int targetBase = GetMemLocByGUID(guid);
            int targetBaseUnitFields = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((targetBase + 0x8) - 0x400000);

            wowObject.guid = guid;
            wowObject.objectType = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((targetBase + 0x14) - 0x400000);

            if (isMyself)
                wowObject.name = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.ReadString(AmeisenOffsets.WoWOffsets.playerName, Encoding.ASCII, 12);
            else
            {
                if (wowObject.objectType == 3)
                    wowObject.name = GetMobNameFromBase(targetBase);
                else if (wowObject.objectType == 4)
                    wowObject.name = GetPlayerNameFromGuid(guid);
                else
                    wowObject.name = "Unknown";
            }

            if (isMyself)
            {
                int playerbasex = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>(AmeisenOffsets.WoWOffsets.playerBase);
                playerbasex = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((playerbasex + 0x34) - 0x400000);
                playerbasex = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((playerbasex + 0x24) - 0x400000);

                ((Me)wowObject).exp = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((playerbasex + 0x3794) - 0x400000);
                ((Me)wowObject).maxExp = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((playerbasex + 0x3798) - 0x400000);

                ((Me)wowObject).partymembers = new List<Target>();

                UInt64 leaderGUID = 0;

                try
                {
                    leaderGUID = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<UInt64>((AmeisenOffsets.WoWOffsets.partyLeader) - 0x400000);
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

            wowObject.level = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((targetBaseUnitFields + (0x36 * 4)) - 0x400000);
            wowObject.health = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((targetBaseUnitFields + (0x18 * 4)) - 0x400000);
            wowObject.maxHealth = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((targetBaseUnitFields + (0x20 * 4)) - 0x400000);
            wowObject.energy = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((targetBaseUnitFields + (0x19 * 4)) - 0x400000);
            wowObject.maxEnergy = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((targetBaseUnitFields + (0x21 * 4)) - 0x400000);
            wowObject.summonedBy = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((targetBaseUnitFields + (0xE * 4)) - 0x400000);

            wowObject.targetGUID = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<UInt64>((targetBaseUnitFields + (0x12 * 4)) - 0x400000);
            wowObject.combatReach = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((targetBaseUnitFields + (0x42 * 4)) - 0x400000);
            wowObject.factionTemplate = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((targetBaseUnitFields + (0x37 * 4)) - 0x400000);
            wowObject.channelSpell = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>((targetBaseUnitFields + (0x16 * 4)) - 0x400000);

            wowObject.posX = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<float>((targetBase + 0x798) - 0x400000);
            wowObject.posY = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<float>((targetBase + 0x79C) - 0x400000);
            wowObject.posZ = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<float>((targetBase + 0x7A0) - 0x400000);
            wowObject.rotation = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<float>((targetBase + 0x7A8) - 0x400000);

            wowObject.mapID = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>(AmeisenOffsets.WoWOffsets.mapID);
            wowObject.zoneID = AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<int>(AmeisenOffsets.WoWOffsets.zoneID);

            return wowObject;
        }

        /// <summary>
        /// Try to get a partymember
        /// </summary>
        /// <param name="leaderGUID">guid of the party leader</param>
        /// <param name="offset">offset to read the party member from</param>
        /// <returns>a Target object containing the party member's deatils</returns>
        private static Target TryReadPartymember(UInt64 leaderGUID, int offset)
        {
            try
            {
                Target t = (Target)ReadWoWObjectFromGUID(false, AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<UInt64>((offset) - 0x400000));
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
            return AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<UInt64>(AmeisenOffsets.WoWOffsets.localPlayerGUID);
        }

        /// <summary>
        /// Get the bot's char's target's GUID
        /// </summary>
        /// <returns></returns>
        public static UInt64 GetTargetGUID()
        {
            return AmeisenManager.GetInstance().GetMemorySharp().Modules.MainModule.Read<UInt64>(AmeisenOffsets.WoWOffsets.localTargetGUID);
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
            RemoteWindow window = AmeisenManager.GetInstance().GetMemorySharp().Windows.MainWindow;
            window.Keyboard.Press(Keys.Space, TimeSpan.FromMilliseconds(20));
            Thread.Sleep(100);
            window.Keyboard.Release(Keys.Space);
        }
    }
}
