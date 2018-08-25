using AmeisenData;
using System.Threading;

namespace AmeisenAI
{
    public class AmeisenCombatManager
    {
        private static AmeisenCombatManager instance;
        private static readonly object padlock = new object();

        private CombatEngine combatEngine;
        private readonly Thread mainWorker;

        private bool stop = false;

        private AmeisenCombatManager()
        {
            mainWorker = new Thread(new ThreadStart(DoWork));
            combatEngine = new CombatEngine();

            ReloadCombatClass();
        }

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
        /// Stop the CombatEngine
        /// </summary>
        public void Stop()
        {
            stop = true;
            mainWorker.Abort();
        }

        /// <summary>
        /// Start the CombatEngine
        /// </summary>
        public void Start() { if (mainWorker.ThreadState == ThreadState.Unstarted) mainWorker.Start(); }

        private void DoWork()
        {
            while (!stop)
            {
                combatEngine.ExecuteNextStep();
                Thread.Sleep(1000);
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
    }
}