using System.Collections.Generic;

namespace AmeisenBotCombat.Objects
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