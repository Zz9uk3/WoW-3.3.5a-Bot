using AmeisenCore;
using System.Threading;

namespace AmeisenAI
{
    class AmeisenCombatManager
    {
        private CombatEngine combatEngine;
        private readonly Thread mainWorker;

        private bool stop = false;

        private static AmeisenCombatManager i;

        private AmeisenCombatManager()
        {
            mainWorker = new Thread(new ThreadStart(DoWork));
            combatEngine = new CombatEngine();

            string defaultCombatClass = AmeisenSettings.GetInstance().settings.combatClassName;
            if (defaultCombatClass != "none")
                combatEngine.LoadCombatLogicFromFile(defaultCombatClass);
        }

        /// <summary>
        /// Stop the CombatEngine
        /// </summary>
        public void Stop() { stop = true; }

        /// <summary>
        /// Start the CombatEngine
        /// </summary>
        public void Start() { mainWorker.Start(); }

        private void DoWork()
        {
            while (!stop)
            {
                if (AmIAbleToDoTheNextThing())
                {
                    combatEngine.ExecuteNextStep();
                }
                Thread.Sleep(10);
            }
        }

        private bool AmIAbleToDoTheNextThing()
        {
            return true;
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
