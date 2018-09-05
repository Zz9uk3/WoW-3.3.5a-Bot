using AmeisenCoreUtils;
using AmeisenData;
using AmeisenDB;
using AmeisenLogging;
using AmeisenMapping.objects;
using AmeisenUtilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;

namespace AmeisenManager
{
    /// <summary>
    /// Singleton class to hold important things like
    /// - Proccess: WoW.exe we're attached to
    /// - BlackMagic: instance that is attached to WoW.exe
    /// - get the state by isAttached boolean
    /// - AmeisenHook: instance that is hooked to WoW.exe's EndScene
    /// - get the state by isHooked boolean
    /// - Me: all character information
    /// </summary>
    public class AmeisenObjectManager
    {
        private static readonly object padlock = new object();

        private static AmeisenObjectManager instance;

        private Thread objectUpdateThread;

        private System.Timers.Timer objectUpdateTimer;

        private DateTime timestampObjects;

        public static AmeisenObjectManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new AmeisenObjectManager();
                    return instance;
                }
            }
        }

        private List<WowObject> ActiveWoWObjects
        {
            get { return AmeisenDataHolder.Instance.ActiveWoWObjects; }
            set { AmeisenDataHolder.Instance.ActiveWoWObjects = value; }
        }

        private Me Me
        {
            get { return AmeisenDataHolder.Instance.Me; }
            set { AmeisenDataHolder.Instance.Me = value; }
        }

        private Unit Target
        {
            get { return AmeisenDataHolder.Instance.Target; }
            set { AmeisenDataHolder.Instance.Target = value; }
        }

        private AmeisenObjectManager()
        {
            RefreshObjects();
        }

        /// <summary>
        /// Get our WoWObjects in the memory
        /// </summary>
        /// <returns>WoWObjects in the memory</returns>
        public List<WowObject> GetObjects()
        {
            AmeisenLogger.Instance.Log(LogLevel.VERBOSE, "Getting Objects", this);

            if (ActiveWoWObjects == null)
                RefreshObjectsAsync();

            // need to do this only for specific objects, saving cpu usage
            //if (needToRefresh)
            //RefreshObjectsAsync();
            return ActiveWoWObjects;
        }

        /// <summary>
        /// Return a Player by the given GUID
        /// </summary>
        /// <param name="guid">guid of the player you want to get</param>
        /// <returns>Player that you want to get</returns>
        public WowObject GetWoWObjectFromGUID(ulong guid)
        {
            foreach (WowObject p in ActiveWoWObjects)
                if (p.Guid == guid)
                    return p;

            return null;
        }

        /// <summary>
        /// Starts the ObjectUpdates
        /// </summary>
        public void Start()
        {
            ActiveWoWObjects = AmeisenCore.GetAllWoWObjects();

            foreach (WowObject t in ActiveWoWObjects)
                if (t.GetType() == typeof(Me))
                {
                    Me = (Me)t;
                    break;
                }

            objectUpdateTimer = new System.Timers.Timer(AmeisenSettings.Instance.Settings.dataRefreshRate);
            objectUpdateTimer.Elapsed += ObjectUpdateTimer;
            objectUpdateThread = new Thread(new ThreadStart(StartTimer));
            objectUpdateThread.Start();
        }

        /// <summary>
        /// Stops the ObjectUpdates
        /// </summary>
        public void Stop()
        {
            objectUpdateTimer.Stop();
            objectUpdateThread.Join();
        }

        private void AntiAFK()
        {
            AmeisenCore.AntiAFK();
        }

        private void ObjectUpdateTimer(object source, ElapsedEventArgs e)
        {
            RefreshObjects();
        }

        private void RefreshObjects()
        {
            ActiveWoWObjects = AmeisenCore.GetAllWoWObjects();

            foreach (WowObject t in ActiveWoWObjects)
            {
                if (t.GetType() == typeof(Me))
                    Me = (Me)t;
                if (Me != null && t.Guid == Me.TargetGuid)
                {
                    t.Update();
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
                    else if (t.GetType() == typeof(Me))
                    {
                        Target = (Me)t;
                        break;
                    }
                }
            }

            // Best place for this :^)
            AntiAFK();
            if (Me != null)
                new Thread(new ThreadStart(() => UpdateNodeInDB(Me))).Start();
        }

        /// <summary>
        /// Refresh our bot's objectlist, you can get the stats by calling GetObjects().
        ///
        /// This runs Async.
        /// </summary>
        private void RefreshObjectsAsync()
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Refreshing Objects Async", this);
            timestampObjects = DateTime.Now;

            new Thread(new ThreadStart(RefreshObjects)).Start();
        }

        private void StartTimer()
        {
            objectUpdateTimer.Start();
        }

        private void UpdateNodeInDB(Me me)
        {
            Vector3 activeNode = new Vector3((int)me.pos.X, (int)me.pos.Y, (int)me.pos.Z);
            //if (activeNode.X != lastNode.X && activeNode.Y != lastNode.Y && activeNode.Z != lastNode.Z)
            //{
            int zoneID = AmeisenCoreUtils.AmeisenCore.GetZoneID();
            int mapID = AmeisenCoreUtils.AmeisenCore.GetMapID();
            AmeisenDBManager.Instance.UpdateOrAddNode(new MapNode(activeNode, zoneID, mapID));
            //}
            //lastNode = activeNode;
        }
    }
}