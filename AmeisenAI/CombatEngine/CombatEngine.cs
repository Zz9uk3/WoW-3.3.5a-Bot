using AmeisenAI.Combat;
using AmeisenData;
using AmeisenUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace AmeisenAI
{
    public class CombatEngine
    {
        public CombatLogic currentCombatLogic;

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
        /// Work on our List of things to do in combat
        /// </summary>
        public void ExecuteNextStep()
        {
            AssistParty();

            if (currentCombatLogic != null)
                if (currentCombatLogic.combatLogicEntries.Count > 0)
                {
                    if (posAt == currentCombatLogic.combatLogicEntries.Count)
                        posAt = 0;
                    if (ExecuteLogic(currentCombatLogic.combatLogicEntries[posAt]))
                        ExecuteAction(currentCombatLogic.combatLogicEntries[posAt]);
                    posAt++;
                }
        }

        private static readonly string combatclassesPath = AppDomain.CurrentDomain.BaseDirectory + "/combatclasses/";

        private int posAt;

        private List<WoWObject> ActiveWoWObjects
        {
            get { return AmeisenDataHolder.Instance.ActiveWoWObjects; }
        }

        private Me Me
        {
            get { return AmeisenDataHolder.Instance.Me; }
            set { AmeisenDataHolder.Instance.Me = value; }
        }

        private Unit Target
        {
            get { return AmeisenDataHolder.Instance.Target; }
            set { AmeisenDataHolder.Instance.Target = value; }
        }

        private void AssistParty()
        {
            int i = 1;
            foreach (UInt64 guid in Me.PartymemberGUIDs)
            {
                foreach (WoWObject o in ActiveWoWObjects)
                    if (o.Guid == guid)
                        if (((Unit)o).InCombat)
                        {
                            AmeisenCoreUtils.AmeisenCore.RunSlashCommand("/assist party" + i);
                            AmeisenCoreUtils.AmeisenCore.AttackTarget();
                            AmeisenAIManager.Instance.DoFollow = false;
                        }
                i++;
            }
        }

        private bool CheckCondition(Condition condition)
        {
            double value1 = GetFirstValue(condition);
            double value2 = GetSecondValue(condition);

            return CheckCondition(value1, value2, condition);
        }

        private bool CheckCondition(double value1, double value2, Condition condition)
        {
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
                    return AmeisenCoreUtils.AmeisenCore.GetAuraInfo(
                        (string)condition.customValue, 
                        LuaUnit.target).duration > 0;

                case CombatLogicStatement.NOT_HAS_BUFF:
                    return AmeisenCoreUtils.AmeisenCore.GetAuraInfo(
                        (string)condition.customValue, 
                        LuaUnit.target).duration <= 0;

                case CombatLogicStatement.HAS_BUFF_MYSELF:
                    return AmeisenCoreUtils.AmeisenCore.GetAuraInfo(
                        (string)condition.customValue, 
                        LuaUnit.player).duration > 0;

                case CombatLogicStatement.NOT_HAS_BUFF_MYSELF:
                    return AmeisenCoreUtils.AmeisenCore.GetAuraInfo(
                        (string)condition.customValue, 
                        LuaUnit.player).duration <= 0;
            }
            return false;
        }

        private double GetSecondValue(Condition condition)
        {
            if (!condition.customSecondValue)
                switch (condition.conditionValues[1])
                {
                    case CombatLogicValues.MYSELF_HP:
                        return Me.Health;

                    case CombatLogicValues.MYSELF_ENERGY:
                        return Me.Energy;

                    case CombatLogicValues.TARGET_HP:
                        return Target.Health;
                }
            else if (condition.customValue.GetType() == typeof(double))
                return (double)condition.customValue;
            return -1;
        }

        private double GetFirstValue(Condition condition)
        {
            switch (condition.conditionValues[0])
            {
                case CombatLogicValues.MYSELF_HP:
                    return Me.Health;

                case CombatLogicValues.MYSELF_ENERGY:
                    return Me.Energy;

                case CombatLogicValues.TARGET_HP:
                    return Target.Health;
            }
            return -1;
        }

        private bool CompareEqual(double a, double b)
        {
            return a == b ? true : false;
        }

        private bool CompareGreater(double a, double b)
        {
            return a > b ? true : false;
        }

        private bool CompareGreaterOrEqual(double a, double b)
        {
            return a >= b ? true : false;
        }

        private bool CompareLess(double a, double b)
        {
            return a < b ? true : false;
        }

        private bool CompareLessOrEqual(double a, double b)
        {
            return a <= b ? true : false;
        }

        /// <summary>
        /// Executes the action specified in the CombatLogicEntry object
        /// </summary>
        /// <param name="entry">CombatLogicEntry entry that contains an action to execute</param>
        private void ExecuteAction(CombatLogicEntry entry)
        {
            switch (entry.Action)
            {
                case CombatLogicAction.USE_SPELL:
                    if (Target != null)
                    {
                        AmeisenAIManager.Instance.DoFollow = false;
                        Me.Update();
                        Target.Update();

                        CheckFacingTarget();
                        CheckOnCooldown(entry);
                    }
                    break;

                case CombatLogicAction.USE_AOE_SPELL:
                    break;

                case CombatLogicAction.SHAPESHIFT:
                    AmeisenCoreUtils.AmeisenCore.CastShapeshift((int)entry.Parameters);
                    break;

                case CombatLogicAction.FLEE:
                    break;

                default:
                    break;
            }
        }

        private void CheckOnCooldown(CombatLogicEntry entry)
        {
            if (!AmeisenCoreUtils.AmeisenCore.IsOnCooldown((string)entry.Parameters))
            {
                AmeisenAction action;
                if (entry.IsForMyself)
                    action = new AmeisenAction(AmeisenActionType.USE_SPELL_ON_ME, (string)entry.Parameters);
                else
                    action = new AmeisenAction(AmeisenActionType.USE_SPELL, (string)entry.Parameters);

                AmeisenAIManager.Instance.AddActionToQueue(ref action);

                do Thread.Sleep(5);
                while (!action.IsActionDone());
            }
        }

        private void CheckFacingTarget()
        {
            if (!Utils.IsFacing(Me.pos, Me.Rotation, Target.pos))
            {
                AmeisenAction action = new AmeisenAction(
                    AmeisenActionType.FACETARGET,
                    InteractionType.FACETARGET
                    );
                AmeisenAIManager.Instance.AddActionToQueue(ref action);
            }
        }

        /// <summary>
        /// Check if we are able to do whatever the given condition is
        /// </summary>
        /// <param name="entry">CombatLogicEntry containing the Conditions and parameters</param>
        /// <returns>true if we are able to, false if not</returns>
        private bool ExecuteLogic(CombatLogicEntry entry)
        {
            if (Me == null)
                return false;

            if (CheckCombatStuff(entry))
                return false;

            bool isMeeleeSpell = entry.MaxSpellDistance < 3.2 ? true : false;

            if (!entry.IsBuff && !entry.IsForMyself)
            {
                if (Me.TargetGUID == 0)
                    return false;

                if (Target != null)
                    CheckTargetDistance(entry, isMeeleeSpell);
            }

            foreach (Condition c in entry.Conditions)
                if (!CheckCondition(c))
                    return false;

            return true;
        }

        private void CheckTargetDistance(CombatLogicEntry entry, bool isMeeleeSpell)
        {
            if (Target.Distance > entry.MaxSpellDistance)
            {
                AmeisenAction action;
                if (isMeeleeSpell)
                    action = new AmeisenAction(AmeisenActionType.INTERACT_TARGET, InteractionType.ATTACKPOS);
                else
                {
                    object[] parameters = new object[2] { Target.pos, entry.MaxSpellDistance * 0.9 }; // 10% Offset
                    action = new AmeisenAction(AmeisenActionType.FORCE_MOVE_NEAR_TARGET, parameters);
                }

                AmeisenAIManager.Instance.AddActionToQueue(ref action);

                do Thread.Sleep(100);
                while (!action.IsActionDone());
            }
        }

        private bool CheckCombatStuff(CombatLogicEntry entry)
        {
            if (entry.CombatOnly)
            {
                if (!AmeisenCoreUtils.AmeisenCore.GetCombatState(LuaUnit.player))
                {
                    AmeisenAIManager.Instance.DoFollow = true;
                    if (!IsPartyInCombat())
                        return true;
                }
                else
                {
                    AmeisenAIManager.Instance.DoFollow = false;
                }
            }
            return false;
        }

        private bool IsPartyInCombat()
        {
            try
            {
                foreach (UInt64 guid in Me.PartymemberGUIDs)
                    foreach (WoWObject o in ActiveWoWObjects)
                        if (o.Guid == guid)
                            if (((Unit)o).InCombat)
                                return true;
            }
            catch { }
            return false;
        }
    }
}