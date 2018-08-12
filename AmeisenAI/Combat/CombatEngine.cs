using AmeisenAI.Combat;
using AmeisenCore;
using AmeisenUtilities;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using static AmeisenAI.Combat.CombatStructures;

namespace AmeisenAI
{
    public class CombatEngine
    {
        private static readonly string combatclassesPath = AppDomain.CurrentDomain.BaseDirectory + "/combatclasses/";

        private int posAt;
        public CombatLogic currentCombatLogic;

        /// <summary>
        /// Work on our List of things to do in combat
        /// </summary>
        public void ExecuteNextStep()
        {
            if (currentCombatLogic.combatLogicEntries.Count > 0)
            {
                if (posAt == currentCombatLogic.combatLogicEntries.Count)
                    posAt = 0;
                if (ExecuteLogic(currentCombatLogic.combatLogicEntries[posAt]))
                    ExecuteAction(currentCombatLogic.combatLogicEntries[posAt]);
                posAt++;
            }
        }

        /// <summary>
        /// Executes the action specified in the CombatLogicEntry object
        /// </summary>
        /// <param name="entry">CombatLogicEntry entry that contains an action to execute</param>
        public void ExecuteAction(CombatLogicEntry entry)
        {
            switch (entry.Action)
            {
                case CombatLogicAction.USE_SPELL:
                    if (!AmeisenCore.AmeisenCore.IsOnCooldown((string)entry.Parameters))
                    {
                        AmeisenAction action;
                        if (entry.IsForMyself)
                            action = new AmeisenAction(AmeisenActionType.USE_SPELL_ON_ME, (string)entry.Parameters);
                        else
                            action = new AmeisenAction(AmeisenActionType.USE_SPELL, (string)entry.Parameters);

                        AmeisenCore.AmeisenCore.MovePlayerToXYZ(AmeisenManager.GetInstance().GetMe().pos, Interaction.FACETARGET);

                        AmeisenAIManager.GetInstance().AddActionToQueue(ref action);

                        do Thread.Sleep(100);
                        while (!action.IsActionDone());
                    }
                    break;

                case CombatLogicAction.USE_AOE_SPELL:
                    break;

                case CombatLogicAction.SHAPESHIFT:
                    AmeisenCore.AmeisenCore.CastShapeshift((int)entry.Parameters);
                    break;

                case CombatLogicAction.FLEE:
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Check if we are able to do whatever the given condition is
        /// </summary>
        /// <param name="entry">CombatLogicEntry containing the Conditions and parameters</param>
        /// <returns>true if we are able to, false if not</returns>
        public bool ExecuteLogic(CombatLogicEntry entry)
        {
            if (entry.CombatOnly)
                if (!AmeisenCore.AmeisenCore.GetCombatState(true) || !AmeisenCore.AmeisenCore.GetCombatState(true))
                    return false;

            bool isMeeleeSpell = entry.MaxSpellDistance < 3.2 ? true : false;

            if (!entry.IsBuff && !entry.IsForMyself)
            {
                if (AmeisenManager.GetInstance().GetMe().target == null)
                    return false;

                if (AmeisenManager.GetInstance().GetMe().target.distance > entry.MaxSpellDistance)
                {
                    AmeisenAction action;
                    if (isMeeleeSpell)
                        action = new AmeisenAction(AmeisenActionType.INTERACT_TARGET, Interaction.ATTACKPOS);
                    else
                    {
                        object[] parameters = new object[2] { AmeisenManager.GetInstance().GetMe().target.pos, entry.MaxSpellDistance * 0.9 };
                        action = new AmeisenAction(AmeisenActionType.FORCE_MOVE_NEAR_TARGET, parameters); // 10% Offset
                    }

                    AmeisenAIManager.GetInstance().AddActionToQueue(ref action);

                    do Thread.Sleep(100);
                    while (!action.IsActionDone());
                }
            }

            foreach (Condition c in entry.Conditions)
                if (!CheckCondition(c))
                    return false;

            return true;
        }

        private bool CheckCondition(Condition condition)
        {
            double value1 = 0.0;
            double value2 = 0.0;

            switch (condition.conditionValues[0])
            {
                case CombatLogicValues.MYSELF_HP:
                    value1 = AmeisenManager.GetInstance().GetMe().health;
                    break;

                case CombatLogicValues.MYSELF_ENERGY:
                    value1 = AmeisenManager.GetInstance().GetMe().energy;
                    break;

                case CombatLogicValues.TARGET_HP:
                    value1 = AmeisenManager.GetInstance().GetMe().target.health;
                    break;

                default:
                    break;
            }

            if (!condition.customSecondValue)
                switch (condition.conditionValues[1])
                {
                    case CombatLogicValues.MYSELF_HP:
                        value2 = AmeisenManager.GetInstance().GetMe().health;
                        break;

                    case CombatLogicValues.MYSELF_ENERGY:
                        value2 = AmeisenManager.GetInstance().GetMe().energy;
                        break;

                    case CombatLogicValues.TARGET_HP:
                        value2 = AmeisenManager.GetInstance().GetMe().target.health;
                        break;

                    default:
                        break;
                }
            else if (condition.customValue.GetType() == typeof(double))
                value2 = (double)condition.customValue;

            switch (condition.statement)
            {
                case CombatLogicStatement.GREATER:
                    return CompareGreater(value1, value2);

                case CombatLogicStatement.GREATER_OR_EQUAL:
                    return CompareGreaterOrEqual(value1, value2);

                case CombatLogicStatement.EQUAL:
                    return CompareEqual(value1, value2);

                case CombatLogicStatement.LESS_OR_EQUAL:
                    return CompareLessOrEqual(value1, value2);

                case CombatLogicStatement.LESS:
                    return CompareLess(value1, value2);

                case CombatLogicStatement.HAS_BUFF:
                    return AmeisenCore.AmeisenCore.GetAuraInfo((string)condition.customValue, false).duration > 0;

                case CombatLogicStatement.NOT_HAS_BUFF:
                    return AmeisenCore.AmeisenCore.GetAuraInfo((string)condition.customValue, false).duration == -1;

                case CombatLogicStatement.HAS_BUFF_MYSELF:
                    return AmeisenCore.AmeisenCore.GetAuraInfo((string)condition.customValue, false).duration > 0;

                case CombatLogicStatement.NOT_HAS_BUFF_MYSELF:
                    return AmeisenCore.AmeisenCore.GetAuraInfo((string)condition.customValue, true).duration == -1;

                default:
                    return false;
            }
        }

        private bool CompareGreater(double a, double b) { return a > b ? true : false; }
        private bool CompareGreaterOrEqual(double a, double b) { return a >= b ? true : false; }
        private bool CompareEqual(double a, double b) { return a == b ? true : false; }
        private bool CompareLessOrEqual(double a, double b) { return a <= b ? true : false; }
        private bool CompareLess(double a, double b) { return a < b ? true : false; }

        /// <summary>
        /// Save a combatclass file to the given filepath.
        /// </summary>
        /// <param name="filepath">Filename without extension</param>
        public static void SaveToFile(string filepath, CombatLogic combatLogic)
        {
            if (!Directory.Exists(combatclassesPath))
                Directory.CreateDirectory(combatclassesPath);

            // Serialize our object with the help of NewtosoftJSON
            File.WriteAllText(filepath, Newtonsoft.Json.JsonConvert.SerializeObject(combatLogic));
        }

        /// <summary>
        /// Load a combatclass file.
        /// </summary>
        /// <param name="filepath">Filepath</param>
        public static CombatLogic LoadCombatLogicFromFile(string filepath)
        {
            CombatLogic currentCombatLogic;

            if (!Directory.Exists(combatclassesPath))
                Directory.CreateDirectory(combatclassesPath);

            if (File.Exists(filepath))
            {
                currentCombatLogic = Newtonsoft.Json.JsonConvert.DeserializeObject<CombatLogic>(File.ReadAllText(filepath));
                currentCombatLogic.combatLogicEntries = currentCombatLogic.combatLogicEntries.OrderBy(p => p.Priority).ToList();
                return currentCombatLogic;
            }
            return null;
        }
    }
}
