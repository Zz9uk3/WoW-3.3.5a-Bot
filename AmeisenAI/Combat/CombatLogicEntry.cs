using static AmeisenAI.Combat.CombatStructures;

namespace AmeisenAI.Combat
{
    public class CombatLogicEntry
    {
        public int priority { get; }
        public CombatLogicAction action { get; }
        public CombatLogicStatement statement { get; }
        public double conditionA { get; }
        public double conditionB { get; }
        public bool combatOnly { get; }
        public bool isBuff { get; }
        public bool isBuffForParty { get; }
        public bool canMoveDuringCast { get; }
        public float spellDistance { get; }

        public CombatLogicEntry(
            int priority,
            CombatLogicAction action,
            CombatLogicStatement statement,
            double conditionA,
            double conditionB,
            bool combatOnly,
            bool isBuff,
            bool isBuffForParty,
            bool canMoveDuringCast,
            float spellDistance)
        {
            this.priority = priority;
            this.action = action;
            this.statement = statement;
            this.conditionA = conditionA;
            this.conditionB = conditionB;
            this.combatOnly = combatOnly;
            this.isBuff = isBuff;
            this.isBuffForParty = isBuffForParty;
            this.canMoveDuringCast = canMoveDuringCast;
            this.spellDistance = spellDistance;
        }
    }
}
