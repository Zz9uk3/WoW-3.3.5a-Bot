using AmeisenAI.Combat;
using AmeisenCore;
using System;
using System.IO;
using System.Linq;
using static AmeisenAI.Combat.CombatStructures;

namespace AmeisenAI
{
    class CombatEngine
    {
        private static string combatclassesPath = AppDomain.CurrentDomain.BaseDirectory + "/combatclasses/";
        private static string extension = ".json";

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
            switch (entry.action)
            {
                case CombatLogicAction.USE_SPELL:
                    break;

                case CombatLogicAction.USE_AOE_SPELL:
                    break;

                case CombatLogicAction.FLEE:
                    break;

                default:
                    break;
            }
        }

        private bool ExecuteLogic(CombatLogicEntry entry)
        {
            /* Need Hook/LUADoString for this
             * 
             * if (entry.combatOnly && AmeisenCore.AmeisenCore.GetCombatState())
             *     return false;
             * 
             * if(entry.isBuffForParty)
             *     if(CheckForBuff(AmeisenManager.GetInstance().GetMe().buffs, entry.spellDistance))
             * else
             *     if(CheckForBuff(AmeisenManager.GetInstance().GetMe().buffs, entry.spellDistance))
             *         return false;
             */

            if (AmeisenManager.GetInstance().GetMe().target.distance > entry.spellDistance)
                return false;

            switch (entry.statement)
            {
                case CombatLogicStatement.GREATER:
                    return CompareGreater(entry.conditionA, entry.conditionB);

                case CombatLogicStatement.GREATER_OR_EQUAL:
                    return CompareGreaterOrEqual(entry.conditionA, entry.conditionB);

                case CombatLogicStatement.EQUAL:
                    return CompareEqual(entry.conditionA, entry.conditionB);

                case CombatLogicStatement.LESS_OR_EQUAL:
                    return CompareLessOrEqual(entry.conditionA, entry.conditionB);

                case CombatLogicStatement.LESS:
                    return CompareLess(entry.conditionA, entry.conditionB);

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
                currentCombatLogic.combatLogicEntries = currentCombatLogic.combatLogicEntries.OrderBy(p => p.priority).ToList();
            }
        }
    }
}
