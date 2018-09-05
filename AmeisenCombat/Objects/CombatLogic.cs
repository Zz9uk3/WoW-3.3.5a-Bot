using System.Collections.Generic;

namespace AmeisenCombat.Objects
{
    public class CombatLogic
    {
        public List<CombatLogicEntry> combatLogicEntries;

        public CombatLogic()
        {
            combatLogicEntries = new List<CombatLogicEntry>();
        }
    }
}