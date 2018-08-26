using AmeisenUtilities;
using System.Collections.Generic;
using System.Threading;

namespace AmeisenAI
{
    public class AmeisenFollowManager
    {
        #region Private Fields

        private static readonly object padlock = new object();
        private static AmeisenFollowManager instance;
        private readonly Thread mainWorker;
        private List<Unit> followUnitList = new List<Unit>();
        private bool stop = false;

        #endregion Private Fields

        #region Singleton stuff

        private AmeisenFollowManager()
        {
            mainWorker = new Thread(new ThreadStart(DoWork));
        }

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

        #endregion Singleton stuff

        #region Public Methods

        public void AddPlayerToFollow(Unit unit)
        {
            followUnitList.Add(unit);
        }

        public void RemoveAllPlayersToFollow()
        {
            followUnitList.Clear();
        }

        public void RemovePlayerToFollow(Unit unit)
        {
            followUnitList.Remove(unit);
        }

        public void Start()
        {
            mainWorker.Start();
        }

        public void Stop()
        {
            stop = true; mainWorker.Join();
        }

        #endregion Public Methods

        #region Private Methods

        private void DoWork()
        {
            while (!stop)
            {
                if (!AmeisenAIManager.Instance.DoFollow)
                {
                    Thread.Sleep(25);
                    continue;
                }

                if (followUnitList.Count > 0)
                {
                    Unit activeUnit = null;

                    foreach (Unit u in followUnitList)
                    {
                        if (u == null)
                            continue;
                        u.Update();
                        activeUnit = u;
                        break;
                    }

                    if (activeUnit != null && AmeisenAIManager.Instance.DoFollow)
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
                }

                Thread.Sleep(50);
            }
        }

        #endregion Private Methods
    }
}