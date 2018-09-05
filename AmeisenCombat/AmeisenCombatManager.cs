using AmeisenData;
using AmeisenUtilities;
using System.Threading;

namespace AmeisenCombat
{
    public class AmeisenCombatManager
    {
        private static readonly object padlock = new object();

        private static AmeisenCombatManager instance;

        private readonly Thread mainWorker;

        private AmeisenCombatEngine combatEngine;

        private bool stop = false;

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

        private Me Me
        {
            get { return AmeisenDataHolder.Instance.Me; }
            set { AmeisenDataHolder.Instance.Me = value; }
        }

        private AmeisenCombatManager()
        {
            mainWorker = new Thread(new ThreadStart(DoWork));
            combatEngine = new AmeisenCombatEngine();

            ReloadCombatClass();
        }

        /// <summary>
        /// Reload our CombatClass specified in the AmeisenSettings class
        /// </summary>
        public void ReloadCombatClass()
        {
            string defaultCombatClass = AmeisenSettings.Instance.Settings.combatClassPath;
            if (defaultCombatClass != "none")
                combatEngine.CurrentCombatLogic = AmeisenCombatEngine.LoadCombatLogicFromFile(defaultCombatClass);
        }

        /// <summary>
        /// Start the CombatEngine
        /// </summary>
        public void Start() { mainWorker.Start(); }

        /// <summary>
        /// Stop the CombatEngine
        /// </summary>
        public void Stop()
        {
            stop = true;
            mainWorker.Abort();
        }

        private void DoWork()
        {
            while (!stop)
            {
                combatEngine.ExecuteNextStep();
                Thread.Sleep(50);
            }
        }
    }
}