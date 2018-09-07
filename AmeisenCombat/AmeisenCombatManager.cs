using AmeisenBotData;
using AmeisenBotUtilities;
using System.Threading;

namespace AmeisenBotCombat
{
    public class AmeisenCombatManager
    {
        private AmeisenCombatEngine CombatEngine { get; set; }
        private AmeisenDataHolder AmeisenDataHolder { get; set; }

        private Me Me
        {
            get { return AmeisenDataHolder.Me; }
            set { AmeisenDataHolder.Me = value; }
        }

        public AmeisenCombatManager(AmeisenDataHolder ameisenDataHolder)
        {
            AmeisenDataHolder = ameisenDataHolder;
            CombatEngine = new AmeisenCombatEngine(AmeisenDataHolder);
            ReloadCombatClass();
        }

        /// <summary>
        /// Reload our CombatClass specified in the AmeisenSettings class
        /// </summary>
        public void ReloadCombatClass()
        {
            string defaultCombatClass = AmeisenDataHolder.Settings.combatClassPath;
            if (defaultCombatClass != "none")
                CombatEngine.CurrentCombatLogic = AmeisenCombatEngine.LoadCombatLogicFromFile(defaultCombatClass);
        }

        public void DoWork()
        {
            CombatEngine.ExecuteNextStep();
        }
    }
}