using AmeisenCoreUtils;
using AmeisenData;
using AmeisenLogging;
using AmeisenUtilities;
using System.Collections.Generic;
using System.Threading;

namespace AmeisenAI
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
        private List<Unit> followUnitList = new List<Unit>();
        private bool stop = false;

        private AmeisenFollowManager()
        {
            mainWorker = new Thread(new ThreadStart(DoWork));
        }

        private Me Me
        {
            get { return AmeisenDataHolder.Instance.Me; }
            set { AmeisenDataHolder.Instance.Me = value; }
        }

        private bool AmIDeadOrGhost()
        {
            if (Me.Health == 0)
            {
                AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Need to revive myself", this);

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

                if (activeUnit != null && AmeisenAIManager.Instance.DoFollow)
                    MoveToCorpse(activeUnit);
            }
        }

        private void DoWork()
        {
            while (!stop)
            {
                if (AmIDeadOrGhost())
                    continue;

                CheckForUnitsToFollow();

                Thread.Sleep(50);
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
            if (AmeisenCore.IsGhost(LuaUnit.player))
            {
                AmeisenLogger.Instance.Log(LogLevel.DEBUG, "I'm a ghost", this);

                AmeisenAction ameisenAction = new AmeisenAction(
                AmeisenActionType.GO_TO_CORPSE_AND_REVIVE, null);

                AmeisenAIManager.Instance.AddActionToQueue(ref ameisenAction);

                while (!ameisenAction.IsActionDone())
                    Thread.Sleep(50);
            }
        }

        private void MoveToCorpse(Unit activeUnit)
        {
            AmeisenAction ameisenAction = new AmeisenAction(
                               AmeisenActionType.MOVE_NEAR_POSITION,
                               new object[] {
                                activeUnit.pos,
                                4.0 } // Follow distance
                               );

            AmeisenAIManager.Instance.AddActionToQueue(ref ameisenAction);

            while (!ameisenAction.IsActionDone())
                Thread.Sleep(50);
        }

        private void ReleaseSpiritCheck()
        {
            if (AmeisenCore.IsDead(LuaUnit.player) && AmeisenAIManager.Instance.IsAllowedToRevive)
            {
                AmeisenCore.ReleaseSpirit();
            }
        }
    }
}