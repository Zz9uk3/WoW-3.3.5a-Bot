using AmeisenData;
using AmeisenLogging;
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
    ///   - get the state by isAttached boolean
    /// - AmeisenHook: instance that is hooked to WoW.exe's EndScene
    ///   - get the state by isHooked boolean
    /// - Me: all character information
    /// </summary>
    public class AmeisenObjectManager
    {
        private static AmeisenObjectManager instance;
        private static readonly object padlock = new object();

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
        private List<WoWObject> ActiveWoWObjects
        {
            get { return AmeisenDataHolder.Instance.ActiveWoWObjects; }
            set { AmeisenDataHolder.Instance.ActiveWoWObjects = value; }
        }

        private DateTime timestampObjects;

        private System.Timers.Timer objectUpdateTimer;
        private Thread objectUpdateThread;

        #region Singleton stuff
        private AmeisenObjectManager()
        {
            RefreshObjects();
        }
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
        #endregion

        private void ObjectUpdateTimer(object source, ElapsedEventArgs e)
        {
            RefreshObjects();
        }

        /// <summary>
        /// Starts the ObjectUpdates
        /// </summary>
        public void Start()
        {
            objectUpdateTimer = new System.Timers.Timer(2000);
            objectUpdateTimer.Elapsed += ObjectUpdateTimer;
            objectUpdateThread = new Thread(new ThreadStart(() => { objectUpdateTimer.Start(); }));
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

        /// <summary>
        /// Refresh our bot's stats, you can get the stats by calling Me().
        ///
        /// This runs Async.
        /// </summary>
        public void RefreshMeAsync()
        {
            AmeisenLogger.Instance.Log(LogLevel.VERBOSE, "Refresh Me Async", this);
            new Thread(new ThreadStart(RefreshMe)).Start();
        }

        /// <summary>
        /// Get our WoWObjects in the memory
        /// </summary>
        /// <returns>WoWObjects in the memory</returns>
        public List<WoWObject> GetObjects()
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

        /// <summary>
        /// Refresh the Me Object
        /// </summary>
        public void RefreshMe()
        {
            Me = AmeisenCore.AmeisenCore.ReadMe(Me.BaseAddress);
        }

        /// <summary>
        /// Return a Player by the given GUID
        /// </summary>
        /// <param name="guid">guid of the player you want to get</param>
        /// <returns>Player that you want to get</returns>
        public WoWObject GetWoWObjectFromGUID(UInt64 guid)
        {
            foreach (WoWObject p in ActiveWoWObjects)
                    if (p.Guid == guid)
                        return p;

            return null;
        }

        private void RefreshObjects()
        {
            ActiveWoWObjects = AmeisenCore.AmeisenCore.GetAllWoWObjects();

            foreach (WoWObject m in ActiveWoWObjects)
                if (m.GetType() == typeof(Me))
                {
                    Me = (Me)m;
                    break;
                }

            foreach (WoWObject t in ActiveWoWObjects)
                if (t != null && Me != null)
                    if (t.Guid == Me.TargetGUID)
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
                            t.Distance = Utils.GetDistance(Me.pos, t.pos);
                            Target = (Me)t;
                            break;
                        }
            // Best place for this :^)
            AntiAFK();
        }

        private void AntiAFK() { AmeisenCore.AmeisenCore.AntiAFK(); }
    }
}