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
    class CombatEngine
    {
        private static readonly string combatclassesPath = AppDomain.CurrentDomain.BaseDirectory + "/combatclasses/";
        private static readonly string extension = ".json";

        private int posAt;
        private CombatLogic currentCombatLogic;

        public void ExecuteNextStep()
        {
            if (posAt == currentCombatLogic.combatLogicEntries.Capacity)
                posAt = 0;
            if (ExecuteLogic(currentCombatLogic.combatLogicEntries[posAt]))
            {
                ExecuteAction(currentCombatLogic.combatLogicEntries[posAt]);
                posAt++;
            }
        }

        public void ExecuteAction(CombatLogicEntry entry)
        {
            switch (entry.Action)
            {
                case CombatLogicAction.USE_SPELL:
                    AmeisenAction action;
                    if (entry.IsForMyself)
                        action = new AmeisenAction(AmeisenActionType.USE_SPELL_ON_ME, (string)entry.Parameters);
                    else
                        action = new AmeisenAction(AmeisenActionType.USE_SPELL, (string)entry.Parameters);

                    AmeisenAIManager.GetInstance().AddActionToQueue(ref action);

                    do Thread.Sleep(10);
                    while (!action.IsActionDone());
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

        public bool ExecuteLogic(CombatLogicEntry entry)
        {
            if (AmeisenManager.GetInstance().GetMe().target.distance >= entry.MaxSpellDistance)
                return false;

            foreach (Condition c in entry.Conditions)
                if (!CheckCondition(c))
                    return false;
            return true;
        }

        private bool CheckCondition(Condition condition)
        {
            switch (condition.statement)
            {
                case CombatLogicStatement.GREATER:
                    return CompareGreater((double)condition.conditionValues[0], (double)condition.conditionValues[1]);

                case CombatLogicStatement.GREATER_OR_EQUAL:
                    return CompareGreaterOrEqual((double)condition.conditionValues[0], (double)condition.conditionValues[1]);

                case CombatLogicStatement.EQUAL:
                    return CompareEqual((double)condition.conditionValues[0], (double)condition.conditionValues[1]);

                case CombatLogicStatement.LESS_OR_EQUAL:
                    return CompareLessOrEqual((double)condition.conditionValues[0], (double)condition.conditionValues[1]);

                case CombatLogicStatement.LESS:
                    return CompareLess((double)condition.conditionValues[0], (double)condition.conditionValues[1]);

                case CombatLogicStatement.HAS_BUFF:
                    return AmeisenCore.AmeisenCore.CheckForAura((string)condition.conditionValues[0], false);

                case CombatLogicStatement.HAS_BUFF_MYSELF:
                    return AmeisenCore.AmeisenCore.CheckForAura((string)condition.conditionValues[0], true);

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
        /// Save a combatclass file from ./combatclasses/ folder by its name as a JSON.
        /// </summary>
        /// <param name="filename">Filename without extension</param>
        public void SaveToFile(string filename, CombatLogic combatLogic)
        {
            if (!Directory.Exists(combatclassesPath))
                Directory.CreateDirectory(combatclassesPath);

            // Serialize our object with the help of NewtosoftJSON
            File.WriteAllText(combatclassesPath + filename.ToLower() + extension, Newtonsoft.Json.JsonConvert.SerializeObject(combatLogic));
        }

        /// <summary>
        /// Load a combatclass file from ./combatclasses/ folder by its name. No need to append .json to the end.
        /// </summary>
        /// <param name="filename">Filename without extension</param>
        public void LoadCombatLogicFromFile(string filename)
        {
            if (!Directory.Exists(combatclassesPath))
                Directory.CreateDirectory(combatclassesPath);

            if (File.Exists(combatclassesPath + filename.ToLower() + extension))
            {
                currentCombatLogic = Newtonsoft.Json.JsonConvert.DeserializeObject<CombatLogic>(File.ReadAllText(combatclassesPath + filename.ToLower() + extension));
                currentCombatLogic.combatLogicEntries = currentCombatLogic.combatLogicEntries.OrderBy(p => p.Priority).ToList();
            }
        }
    }
}
