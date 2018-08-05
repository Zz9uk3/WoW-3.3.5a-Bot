namespace AmeisenAI.Combat
{
    public abstract class CombatStructures
    {
        public enum CombatLogicStatement
        {
            GREATER,
            GREATER_OR_EQUAL,
            EQUAL,
            LESS_OR_EQUAL,
            LESS
        }

        public enum CombatLogicAction
        {
            USE_SPELL,
            USE_AOE_SPELL,
            FLEE
        }
    }
}
