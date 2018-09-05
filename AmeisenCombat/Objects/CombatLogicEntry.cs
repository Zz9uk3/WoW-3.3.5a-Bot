using AmeisenUtilities;
using System.Collections.Generic;
using System.Text;

namespace AmeisenCombat.Objects
{
    public class CombatLogicEntry
    {
        public CombatLogicAction Action { get; set; }

        public CombatActionType ActionType { get; set; }

        public bool CanMoveDuringCast { get; set; }

        public bool CombatOnly { get; set; }

        public List<Condition> Conditions { get; set; }

        public bool IsBuff { get; set; }

        public bool IsBuffForParty { get; set; }

        public bool IsForMyself { get; set; }

        public float MaxSpellDistance { get; set; }

        public object Parameters { get; set; }

        public int Priority { get; set; }

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
            sb.Append($"[{Priority}");
            sb.Append($"] {Action.ToString()}");
            sb.Append($" - {Parameters.ToString()}");

            return sb.ToString();
        }
    }

    public class Condition
    {
        public LuaUnit[] conditionLuaUnits;
        public CombatLogicValues[] conditionValues;
        public bool customSecondValue;
        public object customValue;
        public CombatLogicStatement statement;

        public Condition()
        {
            statement = CombatLogicStatement.EQUAL;
            conditionValues = new CombatLogicValues[2] { 0, 0 };
            conditionLuaUnits = new LuaUnit[2] { 0, 0 };
            customSecondValue = false;
            customValue = 0.0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(conditionLuaUnits[0].ToString());
            sb.Append($" {conditionValues[0].ToString()}");
            sb.Append($" ? {statement.ToString()}");
            if (customSecondValue)
            {
                sb.Append($" ? {conditionLuaUnits[1].ToString()}");
                sb.Append($" {conditionValues[1].ToString()}");
            }
            else
            {
                sb.Append($" ? {customValue}");
            }

            return sb.ToString(); ;
        }
    }
}