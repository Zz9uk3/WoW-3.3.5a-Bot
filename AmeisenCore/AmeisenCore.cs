using AmeisenLogging;
using AmeisenUtilities;
using Magic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

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

        public static BlackMagic Blackmagic { get; set; }

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
                AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Found WoW Process! PID: " + p.Id, "AmeisenCore.AmeisenCore");

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
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Moving to: X [" + pos.x + "] Y [" + pos.y + "] Z [" + pos.z + "]", "AmeisenCore.AmeisenCore");
            //if (AmeisenManager.Instance.Me().pos.x != pos.x && AmeisenManager.Instance.Me().pos.y != pos.y && AmeisenManager.Instance.Me().pos.z != pos.z)
            //{
            WriteXYZToMemory(pos, action);
            //}
        }

        /// <summary>
        /// Move the player to the given guid npc, object or whatever and iteract with it.
        /// </summary>
        /// <param name="pos">Vector3 containing the X,y & Z coordinates</param>
        /// <param name="guid">guid of the entity</param>
        /// <param name="action">CTM Interaction to perform</param>
        public static void InteractWithGUID(Vector3 pos, UInt64 guid, Interaction action)
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Interacting: X [" + pos.x + "] Y [" + pos.y + "] Z [" + pos.z + "] GUID [" + guid + "]", "AmeisenCore.AmeisenCore");
            Blackmagic.WriteUInt64(WoWOffsets.ctmGUID, guid);
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

            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Writing: X [" + pos.x + "] Y [" + pos.y + "] Z [" + pos.z + "] Action [" + action + "] Distance [" + distance + "]", "AmeisenCore.AmeisenCore");
            Blackmagic.WriteFloat(WoWOffsets.ctmX, (float)pos.x);
            Blackmagic.WriteFloat(WoWOffsets.ctmY, (float)pos.y);
            Blackmagic.WriteFloat(WoWOffsets.ctmZ, (float)pos.z);
            Blackmagic.WriteInt(WoWOffsets.ctmAction, (int)action);
            Blackmagic.WriteFloat(WoWOffsets.ctmDistance, distance);
        }

        /// <summary>
        /// Execute the given LUA command inside WoW's MainThread
        /// </summary>
        /// <param name="command">lua command to run</param>
        public static void LUADoString(string command)
        {
            AmeisenLogger.Instance.Log(LogLevel.VERBOSE, "Doing string: Command [" + command + "]", "AmeisenCore.AmeisenCore");
            uint argCC = Blackmagic.AllocateMemory(Encoding.UTF8.GetBytes(command).Length + 1);
            Blackmagic.WriteBytes(argCC, Encoding.UTF8.GetBytes(command));

            string[] asm = new string[]
            {
                "MOV EAX, " + (argCC),
                "PUSH 0",
                "PUSH EAX",
                "PUSH EAX",
                "CALL " + (WoWOffsets.luaDoString),
                "ADD ESP, 0xC",
                "RETN",
            };

            byte[] returnBytes = AmeisenHook.Instance.InjectAndExecute(asm);
            string result = Encoding.UTF8.GetString(returnBytes);

            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Command returned: Command [" + command + "] ReturnValue: " + result + " ReturnBytes: " + returnBytes.ToString(), "AmeisenCore.AmeisenCore");
            Blackmagic.FreeMemory(argCC);
        }

        /// <summary>
        /// Get Localized Text for command
        /// </summary>
        /// <param name="command">lua command to run</param>
        public static string GetLocalizedText(string variable)
        {
            AmeisenLogger.Instance.Log(LogLevel.VERBOSE, "Getting text: Variable [" + variable + "]", "AmeisenCore.AmeisenCore");
            uint argCC = Blackmagic.AllocateMemory(Encoding.UTF8.GetBytes(variable).Length + 1);
            Blackmagic.WriteBytes(argCC, Encoding.UTF8.GetBytes(variable));

            string[] asm = new string[]
            {
                "CALL " + (WoWOffsets.clientObjectManagerGetActivePlayerObject),
                "MOV ECX, EAX",
                "PUSH -1",
                "MOV EDX, " + (argCC),
                "PUSH EDX",
                "CALL " + (WoWOffsets.luaGetLocalizedText),
                "RETN",
            };

            string result = Encoding.UTF8.GetString(AmeisenHook.Instance.InjectAndExecute(asm));
            Blackmagic.FreeMemory(argCC);

            return result;
        }

        /// <summary>
        /// Run the given slash-commando
        /// </summary>
        /// <param name="slashCommand">Example: /target player</param>
        public static void RunSlashCommand(string slashCommand)
        {
            LUADoString("DEFAULT_CHAT_FRAME.editBox:SetText(\"" + slashCommand + "\") ChatEdit_SendText(DEFAULT_CHAT_FRAME.editBox, 0)");
        }

        /// <summary>
        /// Target a GUID
        /// </summary>
        /// <param name="guid">guid to target</param>
        public static void TargetGUID(UInt64 guid)
        {
            AmeisenLogger.Instance.Log(LogLevel.VERBOSE, "TargetGUID: " + guid, "AmeisenCore.AmeisenCore");

            byte[] guidBytes = BitConverter.GetBytes(guid);

            string[] asm = new string[]
            {
                "PUSH " + BitConverter.ToInt32(guidBytes, 4),
                "PUSH " + BitConverter.ToInt32(guidBytes, 0),
                "CALL " + (WoWOffsets.clientGameUITarget),
                "RETN",
            };

            AmeisenHook.Instance.InjectAndExecute(asm);
        }

        /// <summary>
        /// Run through the WoWObjectManager and find the BaseAdress
        /// corresponding to the given GUID
        /// </summary>
        /// <param name="guid">guid to search for</param>
        /// <returns>BaseAdress of the WoWObject</returns>
        public static uint GetMemLocByGUID(UInt64 guid, List<WoWObject> woWObjects)
        {
            AmeisenLogger.Instance.Log(LogLevel.VERBOSE, "Reading: GUID [" + guid + "]", "AmeisenCore.AmeisenCore");

            if (woWObjects != null)
                foreach (WoWObject obj in woWObjects)
                    if (obj != null)
                        if (obj.Guid == guid)
                            return obj.BaseAddress;

            return 0;
        }

        public static List<WoWObject> RefreshAllWoWObjects()
        {
            List<WoWObject> objects = new List<WoWObject>();

            uint currentObjectManager = Blackmagic.ReadUInt(WoWOffsets.currentClientConnection);
            currentObjectManager = Blackmagic.ReadUInt(currentObjectManager + WoWOffsets.currentManagerOffset);

            uint activeObj = Blackmagic.ReadUInt(currentObjectManager + WoWOffsets.firstObjectOffset);
            uint objType = Blackmagic.ReadUInt(activeObj + WoWOffsets.gameobjectTypeOffset);

            UInt64 myGUID = GetPlayerGUID();

            // loop through the objects until an object is bigger than 7 or lower than 1, that means we got all objects
            while (objType <= 7 && objType > 0)
            {
                WoWObject wowObject = ReadWoWObjectFromWoW(activeObj, (WoWObjectType)objType);

                wowObject.Update();

                objects.Add(wowObject);

                activeObj = Blackmagic.ReadUInt(activeObj + WoWOffsets.nextObjectOffset);
                objType = Blackmagic.ReadUInt(activeObj + WoWOffsets.gameobjectTypeOffset);
            }

            return objects;
        }

        /// <summary>
        /// Get the current state of the bots character including its target
        /// </summary>
        /// <returns>the bots character information</returns>
        public static Me ReadMe(uint address)
        {
            return (Me)ReadWoWObjectFromWoW(address, WoWObjectType.PLAYER, true);
        }

        /// <summary>
        /// Read WoWObject from WoW's memory by its GUID/BaseAddress
        /// </summary>
        /// <param name="guid">guid of the object</param>
        /// <param name="baseAddress">baseAddress of the object</param>
        /// <returns>the WoWObject</returns>
        public static WoWObject ReadWoWObjectFromWoW(uint baseAddress, WoWObjectType woWObjectType, bool isMe = false)
        {
            AmeisenLogger.Instance.Log(LogLevel.VERBOSE, "Reading: baseAddress [" + baseAddress + "]", "AmeisenCore.AmeisenCore");

            if (baseAddress == 0)
                return null;

            switch (woWObjectType)
            {
                case WoWObjectType.CONTAINER:
                    return new Container(baseAddress, Blackmagic);

                case WoWObjectType.ITEM:
                    return new Item(baseAddress, Blackmagic);

                case WoWObjectType.GAMEOBJECT:
                    return new GameObject(baseAddress, Blackmagic);

                case WoWObjectType.DYNOBJECT:
                    return new DynObject(baseAddress, Blackmagic);

                case WoWObjectType.CORPSE:
                    return new Corpse(baseAddress, Blackmagic);

                case WoWObjectType.PLAYER:
                    Player obj = new Player(baseAddress, Blackmagic);

                    if (obj.Guid == GetPlayerGUID())
                    {
                        Me meObj = new Me(baseAddress, Blackmagic);

                        UInt64 leaderGUID = Blackmagic.ReadUInt64((WoWOffsets.partyLeader));
                        if (leaderGUID != 0)
                        {
                            meObj.partymembers.Add((Unit)ReadWoWObjectFromWoW(WoWOffsets.partyPlayer1, WoWObjectType.UNIT));
                            meObj.partymembers.Add((Unit)ReadWoWObjectFromWoW(WoWOffsets.partyPlayer2, WoWObjectType.UNIT));
                            meObj.partymembers.Add((Unit)ReadWoWObjectFromWoW(WoWOffsets.partyPlayer3, WoWObjectType.UNIT));
                            meObj.partymembers.Add((Unit)ReadWoWObjectFromWoW(WoWOffsets.partyPlayer4, WoWObjectType.UNIT));

                            foreach (Unit u in meObj.partymembers)
                                if (u != null)
                                    if (u.Guid == leaderGUID)
                                    {
                                        meObj.partyLeader = u;
                                        meObj.partyLeader.Distance = Utils.GetDistance(meObj.pos, meObj.partyLeader.pos);
                                    }
                        }
                        return meObj;
                    }

                    return obj;

                case WoWObjectType.UNIT:
                    return new Unit(baseAddress, Blackmagic);

                default:
                    break;
            }
            return null;
        }

        /// <summary>
        /// Get the bot's char's GUID
        /// </summary>
        /// <returns>the GUID</returns>
        public static UInt64 GetPlayerGUID()
        {
            return Blackmagic.ReadUInt64(WoWOffsets.localPlayerGUID);
        }

        /// <summary>
        /// Get the bot's char's target's GUID
        /// </summary>
        /// <returns>guid</returns>
        public static UInt64 GetTargetGUID()
        {
            return Blackmagic.ReadUInt64(WoWOffsets.localTargetGUID);
        }

        /// <summary>
        /// AntiAFK
        /// </summary>
        public static void AntiAFK()
        {
            Blackmagic.WriteInt(WoWOffsets.tickCount, Environment.TickCount);
        }

        /// <summary>
        /// Check if the player's world is loaded
        /// </summary>
        /// <returns>true if yes, false if no</returns>
        public static bool CheckWorldLoaded()
        {
            return Blackmagic.ReadInt(WoWOffsets.worldLoaded) == 1;
        }

        /// <summary>
        /// Check if the player's world is in a loadingscreen
        /// </summary>
        /// <returns>true if yes, false if no</returns>
        public static bool CheckLoadingScreen()
        {
            return false;
        }

        /// <summary>
        /// Cast a spell by its name
        /// </summary>
        public static void CastSpellByName(string spellname, bool onMyself)
        {
            if (onMyself)
                LUADoString("CastSpellByName(\"" + spellname + "\");");
            else
                LUADoString("CastSpellByName(\"" + spellname + "\", true);");
        }

        /// <summary>
        /// Check for Auras/Buffs
        /// </summary>
        /// <returns>true if target has that aura, false if not</returns>
        public static WoWAuraInfo GetAuraInfo(string auraname, bool onMyself)
        {
            WoWAuraInfo info = new WoWAuraInfo();

            if (onMyself)
                LUADoString("name, rank, icon, count, debuffType, duration, expirationTime, unitCaster, canStealOrPurge, nameplateShowPersonal, spellId = UnitAura(\"player\", \"" + auraname + "\");");
            else
                LUADoString("name, rank, icon, count, debuffType, duration, expirationTime, unitCaster, canStealOrPurge, nameplateShowPersonal, spellId = UnitAura(\"target\", \"" + auraname + "\");");

            try { info.name = GetLocalizedText("name"); } catch { info.name = ""; }
            try { info.stacks = int.Parse(GetLocalizedText("count")); } catch { info.stacks = -1; }
            try { info.duration = int.Parse(GetLocalizedText("duration")); } catch { info.duration = -1; }

            return info;
        }

        /// <summary>
        /// Switch shapeshift forms, use for example "WoWDruid.ShapeshiftForms.Bear"
        /// </summary>
        public static void CastShapeshift(int index)
        {
            LUADoString("CastShapeshiftForm(\"" + index + "\");");
        }

        /// <summary>
        /// Check if the spell is on cooldown
        /// </summary>
        /// <param name="spell">spellname</param>
        /// <returns>true if it is on cooldown, false if not</returns>
        public static bool IsOnCooldown(string spell)
        {
            LUADoString("start, duration, enabled = GetSpellCooldown(\"" + spell + "\");");
            Thread.Sleep(100);
            try { return int.Parse(GetLocalizedText("duration")) > 0; } catch { return true; }
        }

        /// <summary>
        /// Check if the spell is on cooldown
        /// </summary>
        /// <param name="spell">spellname</param>
        /// <returns>true if it is on cooldown, false if not</returns>
        public static WoWSpellInfo GetSpellInfo(string spell)
        {
            WoWSpellInfo info = new WoWSpellInfo();

            LUADoString("name, rank, icon, cost, minRange, maxRange, castTime, powerType = GetSpellInfo(\"" + spell + "\");");
            Thread.Sleep(100);

            info.name = spell; //try { info.name = GetLocalizedText("name"); } catch { info.castTime = -1; }
            try { info.castTime = int.Parse(GetLocalizedText("castTime")); } catch { info.castTime = -1; }
            try { info.cost = int.Parse(GetLocalizedText("cost")); } catch { info.cost = -1; }

            return info;
        }

        /// <summary>
        /// Returns the current combat state
        /// </summary>
        /// <param name="onMyself">check my owm state</param>
        /// <returns>true if unit is in combat, false if not</returns>
        public static bool GetCombatState(bool onMyself)
        {
            bool isInCombat;

            if (onMyself)
                LUADoString("affectingCombat = UnitAffectingCombat(\"player\");");
            else
                LUADoString("affectingCombat = UnitAffectingCombat(\"target\");");

            try { if (int.Parse(GetLocalizedText("affectingCombat")) == 1) isInCombat = true; else isInCombat = false; } catch { isInCombat = false; }
            return isInCombat;
        }

        /// <summary>
        /// Returns wether the Unit is Friendly or not
        /// </summary>
        /// <returns>true if unit is friendly, false if not</returns>
        public static bool IsTargetFriendly()
        {
            bool isFriendly;
            LUADoString("isFriendly  = UnitAffectingCombat(\"player\", \"target\");");

            try { if (int.Parse(GetLocalizedText("isFriendly")) == 1) isFriendly = true; else isFriendly = false; } catch { isFriendly = false; }
            return isFriendly;
        }

        /// <summary>
        /// Let the bot jump by pressing the spacebar once for 20-40ms
        /// 
        /// This runs Async.
        /// </summary>
        public static void CharacterJumpAsync()
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Jumping", "AmeisenCore.AmeisenCore");
            new Thread(CharacterJump).Start();
        }

        private static void CharacterJump()
        {
            const uint KEYDOWN = 0x100;
            const uint KEYUP = 0x101;

            IntPtr windowHandle = Blackmagic.WindowHandle;

            // 0x20 = Spacebar (VK_SPACE)
            SendMessage(windowHandle, KEYDOWN, new IntPtr(0x20), new IntPtr(0));
            Thread.Sleep(new Random().Next(20, 40)); // make it look more human-like :^)
            SendMessage(windowHandle, KEYUP, new IntPtr(0x20), new IntPtr(0));
        }
    }
}
