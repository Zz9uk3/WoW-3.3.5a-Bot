using AmeisenCore;
using System.Threading;

namespace AmeisenAI
{
    public class AmeisenCombatManager
    {
        private CombatEngine combatEngine;
        private readonly Thread mainWorker;

        private bool stop = false;

        private static AmeisenCombatManager i;

        private AmeisenCombatManager()
        {
            mainWorker = new Thread(new ThreadStart(DoWork));
            combatEngine = new CombatEngine();

            ReloadCombatClass();
        }

        /// <summary>
        /// Stop the CombatEngine
        /// </summary>
        public void Stop() {
            stop = true;
            mainWorker.Abort();
        }

        /// <summary>
        /// Start the CombatEngine
        /// </summary>
        public void Start() { if(mainWorker.ThreadState == ThreadState.Unstarted) mainWorker.Start(); }

        private void DoWork()
        {
            while (!stop)
            {
                combatEngine.ExecuteNextStep();
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Reload our CombatClass specified in the AmeisenSettings class
        /// </summary>
        public void ReloadCombatClass()
        {
            string defaultCombatClass = AmeisenSettings.GetInstance().settings.combatClassPath;
            if (defaultCombatClass != "none")
                combatEngine.currentCombatLogic = CombatEngine.LoadCombatLogicFromFile(defaultCombatClass);
        }

        /// <summary>
        /// Initialize/Get the instance of our singleton
        /// </summary>
        /// <returns>AmeisenAIManager instance</returns>
        public static AmeisenCombatManager GetInstance()
        {
            if (i == null)
                i = new AmeisenCombatManager();
            return i;
        }
    }
}
