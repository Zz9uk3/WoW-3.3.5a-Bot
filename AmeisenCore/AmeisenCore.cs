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
    /// Abstract class that contains various static method's to interact with WoW's memory and the
    /// EndScene hook.
    /// </summary>
    public abstract class AmeisenCore
    {
        #region Public Properties

        public static BlackMagic Blackmagic { get; set; }

        #endregion Public Properties

        #region DllImports

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        #endregion DllImports

        #region Getables

        /// <summary>
        /// Reads all WoWObject out of WoW's ObjectManager
        /// </summary>
        /// <returns>all WoWObjects in WoW Manager</returns>
        public static List<WoWObject> GetAllWoWObjects()
        {
            List<WoWObject> objects = new List<WoWObject>();

            uint currentObjectManager = Blackmagic.ReadUInt(WoWOffsets.currentClientConnection);
            currentObjectManager = Blackmagic.ReadUInt(currentObjectManager + WoWOffsets.currentManagerOffset);

            uint activeObj = Blackmagic.ReadUInt(currentObjectManager + WoWOffsets.firstObjectOffset);
            uint objType = Blackmagic.ReadUInt(activeObj + WoWOffsets.gameobjectTypeOffset);

            UInt64 myGUID = ReadPlayerGUID();

            // loop through the objects until an object is bigger than 7 or lower than 1, that means
            // we got all objects
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
        /// Returns the running WoW's in a WoWExe List containing the logged in playername and
        /// Process object.
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

        #endregion Getables

        #region Movement Interaction stuff

        /// <summary> Move the player to the given guid npc, object or whatever and iteract with it.
        /// </summary> <param name="pos">Vector3 containing the X,y & Z coordinates</param> <param
        /// name="guid">guid of the entity</param> <param name="action">CTM Interaction to perform</param>
        public static void InteractWithGUID(Vector3 pos, UInt64 guid, Interaction action)
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Interacting: X [" + pos.x + "] Y [" + pos.y + "] Z [" + pos.z + "] GUID [" + guid + "]", "AmeisenCore.AmeisenCore");
            Blackmagic.WriteUInt64(WoWOffsets.ctmGUID, guid);
            MovePlayerToXYZ(pos, action);
        }

        /// <summary> Move the Player to the given x, y and z coordinates. </summary> <param
        /// name="pos">Vector3 containing the X,y & Z coordinates</param> <param name="action">CTM
        /// Interaction to perform</param>
        public static void MovePlayerToXYZ(Vector3 pos, Interaction action)
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Moving to: X [" + pos.x + "] Y [" + pos.y + "] Z [" + pos.z + "]", "AmeisenCore.AmeisenCore");
            //if (AmeisenManager.Instance.Me().pos.x != pos.x && AmeisenManager.Instance.Me().pos.y != pos.y && AmeisenManager.Instance.Me().pos.z != pos.z)
            //{
            WriteXYZToMemory(pos, action);
            //}
        }

        /// <summary> Write the coordinates and action to the memory. </summary> <param
        /// name="pos">Vector3 containing the X,y & Z coordinates</param> <param name="action">CTM
        /// Interaction to perform</param>
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

        #endregion Movement Interaction stuff

        #region Hook stuff

        /// <summary>
        /// Returns the current combat state
        /// </summary>
        /// <param name="onMyself">check my owm state</param>
        /// <returns>true if unit is in combat, false if not</returns>
        public static bool GetCombatState(LUAUnit luaUnit)
        {
            bool isInCombat = false;
            try { if (int.Parse(GetLocalizedText("affectingCombat = UnitAffectingCombat(\"" + luaUnit.ToString() + "\");", "affectingCombat")) == 1) isInCombat = true; else isInCombat = false; } catch { isInCombat = false; }
            return isInCombat;
        }

        /// <summary>
        /// Get Localized Text for command
        /// </summary>
        /// <param name="command">lua command to run</param>
        public static string GetLocalizedText(string command, string variable)
        {
            uint argCCCommand = Blackmagic.AllocateMemory(Encoding.UTF8.GetBytes(command).Length + 1);
            Blackmagic.WriteBytes(argCCCommand, Encoding.UTF8.GetBytes(command));

            string[] asmDoString = new string[]
            {
                "MOV EAX, " + (argCCCommand),
                "PUSH 0",
                "PUSH EAX",
                "PUSH EAX",
                "CALL " + (WoWOffsets.luaDoString),
                "ADD ESP, 0xC",
                "RETN",
            };

            uint argCC = Blackmagic.AllocateMemory(Encoding.UTF8.GetBytes(variable).Length + 1);
            Blackmagic.WriteBytes(argCC, Encoding.UTF8.GetBytes(variable));

            string[] asmLocalText = new string[]
            {
                "CALL " + (WoWOffsets.clientObjectManagerGetActivePlayerObject),
                "MOV ECX, EAX",
                "PUSH -1",
                "MOV EDX, " + (argCC),
                "PUSH EDX",
                "CALL " + (WoWOffsets.luaGetLocalizedText),
                "RETN",
            };

            HookJob hookJobLocaltext = new HookJob(asmLocalText, true);
            ReturnHookJob hookJobDoString = new ReturnHookJob(asmDoString, false, hookJobLocaltext);

            AmeisenHook.Instance.AddHookJob(ref hookJobDoString);

            while (!hookJobDoString.IsFinished || !hookJobDoString.IsFinished) { Thread.Sleep(1); }

            string result = Encoding.UTF8.GetString((byte[])hookJobDoString.ReturnValue);
            Blackmagic.FreeMemory(argCC);
            Blackmagic.FreeMemory(argCCCommand);

            return result;
        }

        /// <summary>
        /// Run through the WoWObjectManager and find the BaseAdress corresponding to the given GUID
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

            HookJob hookJob = new HookJob(asm, false);
            AmeisenHook.Instance.AddHookJob(ref hookJob);

            while (!hookJob.IsFinished) { Thread.Sleep(1); }
            /*byte[] returnBytes = (byte[])hookJob.ReturnValue;

            string result = "";
            if (returnBytes != null)
                result = Encoding.UTF8.GetString(returnBytes);*/

            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Command returned: Command [" + command + "]", "AmeisenCore.AmeisenCore");
            Blackmagic.FreeMemory(argCC);
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

            HookJob hookJob = new HookJob(asm, false);
            AmeisenHook.Instance.AddHookJob(ref hookJob);
        }

        #endregion Hook stuff

        #region Reads

        /// <summary>
        /// Get the current state of the bots character including its target
        /// </summary>
        /// <returns>the bots character information</returns>
        public static Me ReadMe(uint address)
        {
            return (Me)ReadWoWObjectFromWoW(address, WoWObjectType.PLAYER, true);
        }

        /// <summary>
        /// Get the bot's char's GUID
        /// </summary>
        /// <returns>the GUID</returns>
        public static UInt64 ReadPlayerGUID()
        {
            return Blackmagic.ReadUInt64(WoWOffsets.localPlayerGUID);
        }

        /// <summary>
        /// Get the bot's char's target's GUID
        /// </summary>
        /// <returns>guid</returns>
        public static UInt64 ReadTargetGUID()
        {
            return Blackmagic.ReadUInt64(WoWOffsets.localTargetGUID);
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

                    if (obj.Guid == ReadPlayerGUID())
                        return new Me(baseAddress, Blackmagic);

                    return obj;

                case WoWObjectType.UNIT:
                    return new Unit(baseAddress, Blackmagic);

                default:
                    break;
            }
            return null;
        }

        #endregion Reads

        #region Misc

        /// <summary>
        /// AntiAFK
        /// </summary>
        public static void AntiAFK()
        {
            Blackmagic.WriteInt(WoWOffsets.tickCount, Environment.TickCount);
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
        /// Check if the player's world is loaded
        /// </summary>
        /// <returns>true if yes, false if no</returns>
        public static bool CheckWorldLoaded()
        {
            return Blackmagic.ReadInt(WoWOffsets.worldLoaded) == 1;
        }

        #endregion Misc

        #region LUA stuff

        /// <summary>
        /// Attack our target
        /// </summary>
        public static void AttackTarget() { LUADoString("AttackTarget();"); }

        /// <summary>
        /// Switch shapeshift forms, use for example "WoWDruid.ShapeshiftForms.Bear"
        /// </summary>
        public static void CastShapeshift(int index)
        {
            LUADoString("CastShapeshiftForm(\"" + index + "\");");
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
        public static WoWAuraInfo GetAuraInfo(string auraname, LUAUnit luaUnit)
        {
            WoWAuraInfo info = new WoWAuraInfo();

            string cmd = "name, rank, icon, count, debuffType, duration, expirationTime, unitCaster, canStealOrPurge, nameplateShowPersonal, spellId = UnitAura(\"" + luaUnit.ToString() + "\", \"" + auraname + "\");";

            try { info.name = GetLocalizedText(cmd, "name"); } catch { info.name = ""; }
            try { info.stacks = int.Parse(GetLocalizedText(cmd, "count")); } catch { info.stacks = -1; }
            try { info.duration = int.Parse(GetLocalizedText(cmd, "duration")); } catch { info.duration = -1; }

            return info;
        }

        /// <summary>
        /// Check if the spell is on cooldown
        /// </summary>
        /// <param name="spell">spellname</param>
        /// <returns>true if it is on cooldown, false if not</returns>
        public static WoWSpellInfo GetSpellInfo(string spell)
        {
            WoWSpellInfo info = new WoWSpellInfo();

            string cmd = "name, rank, icon, cost, minRange, maxRange, castTime, powerType = GetSpellInfo(\"" + spell + "\");";

            info.name = spell; //try { info.name = GetLocalizedText("name"); } catch { info.castTime = -1; }
            try { info.castTime = int.Parse(GetLocalizedText(cmd, "castTime")); } catch { info.castTime = -1; }
            try { info.cost = int.Parse(GetLocalizedText(cmd, "cost")); } catch { info.cost = -1; }

            return info;
        }

        /// <summary>
        /// Check if the spell is on cooldown
        /// </summary>
        /// <param name="spell">spellname</param>
        /// <returns>true if it is on cooldown, false if not</returns>
        public static bool IsOnCooldown(string spell)
        {
            try { return int.Parse(GetLocalizedText("start, duration, enabled = GetSpellCooldown(\"" + spell + "\");", "duration")) > 0; } catch { return true; }
        }

        /// <summary>
        /// Returns wether the Unit is Friendly or not
        /// </summary>
        /// <returns>true if unit is friendly, false if not</returns>
        public static bool IsTargetFriendly()
        {
            bool isFriendly;
            try { if (int.Parse(GetLocalizedText("isFriendly  = UnitAffectingCombat(\"player\", \"target\");", "isFriendly")) == 1) isFriendly = true; else isFriendly = false; } catch { isFriendly = false; }
            return isFriendly;
        }

        #endregion LUA stuff

        #region KeyInteractions

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

        #endregion KeyInteractions
    }
}