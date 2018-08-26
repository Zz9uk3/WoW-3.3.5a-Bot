using System.Collections.Generic;

namespace AmeisenAI.Combat
{
    public class CombatLogic
    {
        #region Public Fields

        public List<CombatLogicEntry> combatLogicEntries;

        #endregion Public Fields

        #region Public Constructors

        public CombatLogic()
        {
            combatLogicEntries = new List<CombatLogicEntry>();
        }

        #endregion Public Constructors
    }
}