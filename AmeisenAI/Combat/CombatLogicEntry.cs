using System.Collections.Generic;
using static AmeisenAI.Combat.CombatStructures;

namespace AmeisenAI.Combat
{
    public struct Condition
    {
        public CombatLogicStatement statement;
        public object[] conditionValues;
    }

    public class CombatLogicEntry
    {
        public int Priority { get; }
        public CombatLogicAction Action { get; }
        public List<Condition> Conditions { get; }
        public bool CombatOnly { get; }
        public bool IsBuff { get; }
        public bool IsBuffForParty { get; }
        public bool CanMoveDuringCast { get; }
        public float MaxSpellDistance { get; }
        public object Parameters { get; }
        public bool IsForMyself { get; }

        public CombatLogicEntry(
            int priority,
            CombatLogicAction action,
            List<Condition> conditions,
            bool combatOnly,
            bool isBuff,
            bool isBuffForParty,
            bool canMoveDuringCast,
            float spellDistance,
            object parameters,
            bool isForMyself)
        {
            Priority = priority;
            Action = action;
            Conditions = conditions;
            CombatOnly = combatOnly;
            IsBuff = isBuff;
            IsBuffForParty = isBuffForParty;
            CanMoveDuringCast = canMoveDuringCast;
            MaxSpellDistance = spellDistance;
            Parameters = parameters;
            IsForMyself = isForMyself;
        }
    }
}
