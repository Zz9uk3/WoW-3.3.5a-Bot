using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Threading;
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
    /// - AmeisenHook: instance that is hooked to WoW.exe's EndScene
    ///   - get the state by isHooked boolean
    /// - Me: all character information
    /// </summary>
    public class AmeisenManager
    {
        private static AmeisenManager i;

        private bool isAttached;
        private bool isHooked;

        private Process wowProcess;
        private BlackMagic blackmagic;
        private AmeisenHook ameisenHook;

        public Me Me { get; set; }
        public Unit Target { get; set; }

        private List<WoWObject> activeWoWObjects;
        // To determine if we need to refresh some things
        private DateTime timestampObjects;

        private bool isAllowedToMove;

        public bool IsSupposedToAttack { get; set; }
        public bool IsSupposedToTank { get; set; }
        public bool IsSupposedToHeal { get; set; }

        private System.Timers.Timer objectUpdateTimer;
        private Thread objectUpdateThread;

        private AmeisenManager()
        {
            isAttached = false;
            isHooked = false;
            isAllowedToMove = true;
        }

        private void ObjectUpdateTimer(object source, ElapsedEventArgs e)
        {
            RefreshObjects();
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
            ameisenHook = new AmeisenHook();
            isHooked = ameisenHook.isHooked;

            // Read all objects to prevent crashes
            RefreshObjects();
        }

        /// <summary>
        /// Starts the ObjectUpdates
        /// </summary>
        public void StartObjectUpdates()
        {
            // Update our ObjectList AmeisenSettings.GetInstance().Settings.dataRefreshRate
            objectUpdateTimer = new System.Timers.Timer(AmeisenSettings.GetInstance().Settings.dataRefreshRate);
            objectUpdateTimer.Elapsed += ObjectUpdateTimer;
            objectUpdateThread = new Thread(new ThreadStart(() => { objectUpdateTimer.Start(); }));
            objectUpdateThread.Start();
        }

        /// <summary>
        /// Stops the ObjectUpdates
        /// </summary>
        public void StopObjectUpdates()
        {
            objectUpdateTimer.Stop();
            objectUpdateThread.Join();
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
        /// Refresh our bot's stats, you can get the stats by calling Me().
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
                //bool needToRefresh = (DateTime.Now - timestampObjects).TotalSeconds > 5;

                if (activeWoWObjects == null)
                    RefreshObjectsAsync();

                // need to do this only for specific objects, saving cpu usage
                //if (needToRefresh)
                //RefreshObjectsAsync();
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

        public void RefreshMe() { Me = AmeisenCore.ReadMe(); }

        private void RefreshObjects()
        {
            activeWoWObjects = AmeisenCore.RefreshAllWoWObjects();

            foreach (WoWObject m in activeWoWObjects)
                if (m.GetType() == typeof(Me))
                {
                    Me = (Me)m;
                    break;
                }

            foreach (WoWObject t in activeWoWObjects)
                if (t.Guid == Me.targetGUID)
                    if (t.GetType() == typeof(Player))
                    {
                        t.Distance = Utils.GetDistance(Me.pos, t.pos);
                        Target = (Player)t;
                        break;
                    }
                    else if (t.GetType() == typeof(Unit))
                    {
                        t.Distance = Utils.GetDistance(Me.pos, t.pos);
                        Target = (Unit)t;
                        break;
                    }
        }

        /// <summary>
        /// Lock bot movement
        /// </summary>
        public void LockMovement() { isAllowedToMove = false; }

        /// <summary>
        /// Unlock bot movement
        /// </summary>
        public void UnlockMovement() { isAllowedToMove = true; }

        /// <summary>
        /// Is the bot allowed to move right now?
        /// </summary>
        public bool IsAllowedToMove() { return isAllowedToMove; }
    }
}
