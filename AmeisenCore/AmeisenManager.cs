using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using AmeisenCore.Objects;
using AmeisenLogging;
using AmeisenUtilities;
using Magic;

namespace AmeisenCore
{
    /// <summary>
    /// Singleton class to hold important things like
    /// - Proccess: WoW.exe we're attached to
    /// - BlackMagic: instance that is attached to WoW.exe
    ///   - get the state by isAttached boolean
    /// - AmeisenHook: instance that is hookes to WoW.exe's EndScene
    ///   - get the state by isHooked boolean
    /// - Me: all character information
    /// </summary>
    public class AmeisenManager
    {
        private static AmeisenManager i;

        private bool isAttached, isHooked;

        private Process wowProcess;
        private BlackMagic blackmagic;
        private AmeisenHook ameisenHook;

        private Me me;

        private List<WoWObject> activeWoWObjects;
        // To determine if we need to refresh some things
        private DateTime timestampObjects;

        private AmeisenManager()
        {
            isAttached = false;
            isHooked = false;
        }

        public static AmeisenManager GetInstance()
        {
            if (i == null)
                i = new AmeisenManager();
            return i;
        }

        /// <summary>
        /// Attach the manager to the given WoW Process to be able to read and write memory etc.
        /// </summary>
        /// <param name="p">wow process object</param>
        public void AttachManager(Process p)
        {
            wowProcess = p;

            // Attach to Proccess
            blackmagic = new BlackMagic(p.Id);
            isAttached = blackmagic.IsProcessOpen;

            // Hook EndScene LMAO
            // TODO: Fix this piece of garbage
            // ameisenHook = new AmeisenHook();
            // isHooked = ameisenHook.isHooked;
        }

        /// <summary>
        /// Get current MemorySharp instance
        /// </summary>
        /// <returns>memorysharp</returns>
        public BlackMagic GetBlackMagic()
        {
            if (isAttached)
                return blackmagic;
            else
                throw new Exception("Manager is not attached to any WoW...");
        }

        /// <summary>
        /// Get current AmeisenHook instance
        /// </summary>
        /// <returns>memorysharp</returns>
        public AmeisenHook GetAmeisenHook()
        {
            if (isHooked)
                return ameisenHook;
            else
                throw new Exception("Manager is not hooked to any WoW's EndScene...");
        }

        /// <summary>
        /// Get our char's stats, group members, target...
        /// </summary>
        /// <returns>char's stats, group members, target</returns>
        public Me GetMe()
        {
            AmeisenLogger.GetInstance().Log(LogLevel.VERBOSE, "Getting Me", this);
            if (isAttached)
                return me;
            else
                throw new Exception("Manager is not attached to any WoW...");
        }

        /// <summary>
        /// Refresh our bot's stats, you can get the stats by calling GetMe().
        /// 
        /// This runs Async.
        /// </summary>
        public void RefreshMeAsync()
        {
            AmeisenLogger.GetInstance().Log(LogLevel.VERBOSE, "Refresh Me Async", this);
            if (isAttached)
            {
                new Thread(new ThreadStart(RefreshMe)).Start();
            }
            else
                throw new Exception("Manager is not attached to any WoW...");
        }

        /// <summary>
        /// Get our WoWObjects in the memory
        /// </summary>
        /// <returns>WoWObjects in the memory</returns>
        public List<WoWObject> GetObjects()
        {
            AmeisenLogger.GetInstance().Log(LogLevel.VERBOSE, "Getting Objects", this);
            if (isAttached)
            {
                bool needToRefresh = (DateTime.Now - timestampObjects).TotalSeconds > 5;

                if (activeWoWObjects == null)
                    RefreshObjects();
                if (needToRefresh)
                    RefreshObjectsAsync();
                return activeWoWObjects;
            }
            else
                throw new Exception("Manager is not attached to any WoW...");
        }

        /// <summary>
        /// Refresh our bot's objectlist, you can get the stats by calling GetObjects().
        /// 
        /// This runs Async.
        /// </summary>
        private void RefreshObjectsAsync()
        {
            AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Refreshing Objects Async", this);
            timestampObjects = DateTime.Now;

            if (isAttached)
            {
                new Thread(new ThreadStart(RefreshObjects)).Start();
            }
            else
                throw new Exception("Manager is not attached to any WoW...");
        }

        /// <summary>
        /// Get our WoW-Process
        /// </summary>
        public Process GetProcess()
        {
            return wowProcess;
        }

        public void RefreshMe() { me = AmeisenCore.ReadMe(); }

        private void RefreshObjects()
        {
            activeWoWObjects = AmeisenCore.RefreshAllWoWObjects();
        }
    }
}
