using AmeisenCoreUtils;
using AmeisenData;
using AmeisenLogging;
using AmeisenUtilities;
using System.Collections.Generic;
using System.Threading;

namespace AmeisenAI.Follow
{
    public class AmeisenFollowManager
    {
        /// <summary>
        /// Initialize/Get the instance of our singleton
        /// </summary>
        /// <returns>AmeisenFollowManager instance</returns>
        public static AmeisenFollowManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new AmeisenFollowManager();
                    return instance;
                }
            }
        }

        /// <summary>
        /// Register a unit to be followed
        /// </summary>
        /// <param name="unit">unit object to follow</param>
        public void AddPlayerToFollow(Unit unit)
        {
            followUnitList.Add(unit);
        }

        public void OnArrivedAtUnit()
        {
            AmeisenLogger.Instance.Log(LogLevel.VERBOSE, "Arrived at Units Position", this);
            AmeisenDataHolder.Instance.IsMoving = false;
            arrivedAtUnit = true;
        }

        /// <summary>
        /// Unregister all units to be no longer followed
        /// </summary>
        public void RemoveAllPlayersToFollow()
        {
            followUnitList.Clear();
        }

        /// <summary>
        /// Unregister a single unit to be no longer followed
        /// </summary>
        /// <param name="unit"></param>
        public void RemovePlayerToFollow(Unit unit)
        {
            followUnitList.Remove(unit);
        }

        /// <summary>
        /// Start the follow engine
        /// </summary>
        public void Start()
        {
            mainWorker.Start();
        }

        /// <summary>
        /// Stop the follow engine
        /// </summary>
        public void Stop()
        {
            stop = true; mainWorker.Join();
        }

        private static readonly object padlock = new object();
        private static AmeisenFollowManager instance;
        private readonly Thread mainWorker;
        private bool arrivedAtUnit;
        private List<Unit> followUnitList = new List<Unit>();
        private bool stop = false;

        private AmeisenFollowManager()
        {
            arrivedAtUnit = true;
            mainWorker = new Thread(new ThreadStart(DoWork));
        }

        private Me Me
        {
            get { return AmeisenDataHolder.Instance.Me; }
            set { AmeisenDataHolder.Instance.Me = value; }
        }

        private bool AmIDeadOrGhost()
        {
            if (Me?.Health == 0)
            {
                AmeisenLogger.Instance.Log(LogLevel.DEBUG, "I'm Dead, need to Release Spirit", this);
                AmeisenDataHolder.Instance.IsDead = true;

                ReleaseSpiritCheck();
                GhostReviveCheck();

                Thread.Sleep(2000);
                return true;
            }
            return false;
        }

        private void CheckForUnitsToFollow()
        {
            if (followUnitList.Count > 0)
            {
                Unit activeUnit = GetActiveUnit();

                if (activeUnit != null)
                {
                    Me.Update();
                    if (!Me.InCombat
                        && arrivedAtUnit)
                        FollowUnit(activeUnit);
                }
            }
        }

        private void DoWork()
        {
            while (!stop)
            {
                if (AmIDeadOrGhost())
                    continue;

                if (Me != null)
                    if (Me.IsCasting || Me.InCombat)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                CheckForUnitsToFollow();

                Thread.Sleep(200);
            }
        }

        private void FollowUnit(Unit activeUnit)
        {
            Me.Update();
            activeUnit.Update();
            if (Utils.GetDistance(Me.pos, activeUnit.pos) > AmeisenSettings.Instance.Settings.followDistance)
            {
                arrivedAtUnit = false;
                AmeisenLogger.Instance.Log(LogLevel.VERBOSE, $"Following Unit: {activeUnit.Name}", this);
                AmeisenAction ameisenAction = new AmeisenAction(
                                    AmeisenActionType.MOVE_NEAR_POSITION,
                                    new object[] {
                                    activeUnit.pos,
                                    AmeisenSettings.Instance.Settings.followDistance }, // Follow distance
                                    OnArrivedAtUnit
                                   );

                AmeisenAIManager.Instance.AddActionToQueue(ref ameisenAction);

                // Slow follow, falling behind the master quite fast
                while (!ameisenAction.IsDone) { Thread.Sleep(1); }
            }
        }

        private Unit GetActiveUnit()
        {
            foreach (Unit u in followUnitList)
            {
                if (u == null)
                    continue;
                u.Update();
                return u;
            }
            return null;
        }

        private void GhostReviveCheck()
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Checking Ghost", this);
            if (AmeisenCore.IsGhost(LuaUnit.player))
            {
                AmeisenLogger.Instance.Log(LogLevel.DEBUG, "I'm a Ghost", this);

                AmeisenAction ameisenAction = new AmeisenAction(
                AmeisenActionType.GO_TO_CORPSE_AND_REVIVE, null, OnReviveSuccessful);

                AmeisenAIManager.Instance.AddActionToQueue(ref ameisenAction);
            }
        }

        private void OnReviveSuccessful()
        {
            AmeisenDataHolder.Instance.IsDead = false;
        }

        private void ReleaseSpiritCheck()
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Checking IsDead", this);
            if (AmeisenCore.IsDead(LuaUnit.player) && AmeisenAIManager.Instance.IsAllowedToRevive)
            {
                AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Releasing Spirit", this);
                AmeisenCore.ReleaseSpirit();
            }
        }
    }
}