using AmeisenData;
using AmeisenUtilities;
using System.Threading;

namespace AmeisenCombat
{
    public class AmeisenCombatManager
    {
        private AmeisenCombatEngine combatEngine;

        private Me Me
        {
            get { return AmeisenDataHolder.Instance.Me; }
            set { AmeisenDataHolder.Instance.Me = value; }
        }

        public AmeisenCombatManager()
        {
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

        public void DoWork()
        {
            combatEngine.ExecuteNextStep();
        }
    }
}