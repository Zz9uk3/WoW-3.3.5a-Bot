using AmeisenData;
using AmeisenUtilities;
using System.Threading;

namespace AmeisenAI
{
    public class AmeisenCombatManager
    {
        #region Private Fields

        private static readonly object padlock = new object();
        private static AmeisenCombatManager instance;
        private readonly Thread mainWorker;
        private CombatEngine combatEngine;
        private bool stop = false;

        #endregion Private Fields

        #region Private Constructors

        private AmeisenCombatManager()
        {
            mainWorker = new Thread(new ThreadStart(DoWork));
            combatEngine = new CombatEngine();

            ReloadCombatClass();
        }

        #endregion Private Constructors

        #region Public Properties

        /// <summary>
        /// Initialize/Get the instance of our singleton
        /// </summary>
        /// <returns>AmeisenAIManager instance</returns>
        public static AmeisenCombatManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new AmeisenCombatManager();
                    return instance;
                }
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Reload our CombatClass specified in the AmeisenSettings class
        /// </summary>
        public void ReloadCombatClass()
        {
            string defaultCombatClass = AmeisenSettings.Instance.Settings.combatClassPath;
            if (defaultCombatClass != "none")
                combatEngine.currentCombatLogic = CombatEngine.LoadCombatLogicFromFile(defaultCombatClass);
        }

        /// <summary>
        /// Start the CombatEngine
        /// </summary>
        public void Start() { if (mainWorker.ThreadState == ThreadState.Unstarted) mainWorker.Start(); }

        /// <summary>
        /// Stop the CombatEngine
        /// </summary>
        public void Stop()
        {
            stop = true;
            mainWorker.Abort();
        }

        #endregion Public Methods

        #region Private Properties

        private Me Me
        {
            get { return AmeisenDataHolder.Instance.Me; }
            set { AmeisenDataHolder.Instance.Me = value; }
        }

        #endregion Private Properties

        #region Private Methods

        private void DoWork()
        {
            while (!stop)
            {
                if (AmeisenCore.AmeisenCore.IsGhost(LUAUnit.player))
                {
                    if (Me.NeedToRevive)
                        if (AmeisenAIManager.Instance.IsAllowedToRevive)
                            AmeisenCore.AmeisenCore.Revive();

                    Thread.Sleep(500);
                    continue;
                }

                combatEngine.ExecuteNextStep();
                Thread.Sleep(500);
            }
        }

        #endregion Private Methods
    }
}