namespace AmeisenAI
{
    class AmeisenCombatManager
    {
        private static AmeisenCombatManager i;
        
        private AmeisenCombatManager()
        {
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
