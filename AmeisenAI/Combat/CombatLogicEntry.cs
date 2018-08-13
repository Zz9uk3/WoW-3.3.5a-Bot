using AmeisenUtilities;
using System.Collections.Generic;
using System.Text;
using static AmeisenAI.Combat.CombatStructures;

namespace AmeisenAI.Combat
{
    public class Condition
    {
        public CombatLogicStatement statement;
        public CombatLogicValues[] conditionValues;
        public bool customSecondValue;
        public object customValue;

        public Condition()
        {
            statement = CombatLogicStatement.EQUAL;
            conditionValues = new CombatLogicValues[2] { 0, 0 };
            customSecondValue = false;
            customValue = 0.0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(conditionValues[0].ToString());
            sb.Append(" - " + statement.ToString() + " - ");
            sb.Append(conditionValues[1].ToString());

            return sb.ToString(); ;
        }
    }

    public class CombatLogicEntry
    {
        public int Priority { get; set; }
        public CombatLogicAction Action { get; set; }
        public List<Condition> Conditions { get; set; }
        public bool CombatOnly { get; set; }
        public bool IsBuff { get; set; }
        public bool IsBuffForParty { get; set; }
        public bool CanMoveDuringCast { get; set; }
        public float MaxSpellDistance { get; set; }
        public object Parameters { get; set; }
        public bool IsForMyself { get; set; }
        public CombatActionType ActionType { get; set; }

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
            bool isForMyself,
            CombatActionType actionType)
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
            ActionType = actionType;
        }

        public CombatLogicEntry()
        {
            Action = CombatLogicAction.USE_SPELL;
            ActionType = CombatActionType.ATTACK;
            Conditions = new List<Condition>();
            Parameters = "";
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[" + Priority);
            sb.Append("] " + Action.ToString());
            sb.Append(" - " + Parameters.ToString());

            return sb.ToString();
        }
    }
}
