using AmeisenData;
using AmeisenUtilities;
using System.Threading;

namespace AmeisenAI
{
    public class AmeisenCombatManager
    {
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

        private static readonly object padlock = new object();
        private static AmeisenCombatManager instance;
        private readonly Thread mainWorker;
        private CombatEngine combatEngine;
        private bool stop = false;

        private AmeisenCombatManager()
        {
            mainWorker = new Thread(new ThreadStart(DoWork));
            combatEngine = new CombatEngine();

            ReloadCombatClass();
        }

        private Me Me
        {
            get { return AmeisenDataHolder.Instance.Me; }
            set { AmeisenDataHolder.Instance.Me = value; }
        }

        private void DoWork()
        {
            while (!stop)
            {
                if (Me.Health <= 1)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                combatEngine.ExecuteNextStep();
                Thread.Sleep(500);
            }
        }
    }
}