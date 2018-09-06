using AmeisenCombat.Objects;
using AmeisenCoreUtils;
using AmeisenData;
using AmeisenLogging;
using AmeisenUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace AmeisenCombat
{
    public class AmeisenCombatEngine
    {
        private static readonly string combatclassesPath = AppDomain.CurrentDomain.BaseDirectory + "/combatclasses/";

        private int posAt;

        public bool AttackShit { get; private set; }

        public CombatLogic CurrentCombatLogic { get; set; }

        public List<ulong> GuidsToKill { get; set; }

        public List<ulong> GuidsWithPotentialLoot { get; set; }

        private List<WowObject> ActiveWoWObjects
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

        public AmeisenCombatEngine()
        {
            GuidsToKill = new List<ulong>();
            GuidsWithPotentialLoot = new List<ulong>();
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
            // Check if we have any instructions to follow
            if (CurrentCombatLogic != null && CurrentCombatLogic.combatLogicEntries.Count > 0)
            {
                // Get a target from our "To-Be-Killed"-List or check for Partymembers in Combat and
                // Assist them

                //if (Target == null || Target.Guid == 0)
                SelectTarget();

                // Remove all dead targets from list and Add them to the LootList
                RemoveDeadTargetsFromListAndCheckForLoot();

                // If we are at the end of instructions, go to the beginning
                if (posAt == CurrentCombatLogic.combatLogicEntries.Count)
                    posAt = 0;

                // Check the Logic and if everything is alright, fire the Action
                if (ExecuteLogic(CurrentCombatLogic.combatLogicEntries[posAt]))
                {
                    ExecuteAction(CurrentCombatLogic.combatLogicEntries[posAt]);
                    posAt = 0;
                }
                else
                {
                    posAt++;
                }

                if (!Me.InCombat)
                {
                    // Loot the Guid's from guidsWithPotentialLoot List
                    LootGuidsThatAreMine();
                }
            }
        }

        private void AssistPartyMembers()
        {
            if (Me?.TargetGuid == 0)
            {
                // Sometimes crashing because the List is being updated from elsewhere
                // TODO: need to fix that using a lock or so
                try
                {
                    int i = 1;
                    foreach (ulong guid in Me.PartymemberGuids)
                    {
                        Unit activeUnit = (Unit)GetUnitFromListByGuid(guid);
                        activeUnit.Update();

                        if (activeUnit.InCombat)
                        {
                            ulong partymemberTargetGuid = activeUnit.TargetGuid;
                            AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"Partymember Guid: {partymemberTargetGuid}", this);

                            AmeisenCore.LuaDoString($"AssistUnit(\"party{i}\");");
                            Me.Update();
                            if (!GuidsToKill.Contains(Me.TargetGuid))

                                GuidsToKill.Add(Me.TargetGuid);
                            //AmeisenCore.LuaDoString("AttackTarget();");

                            Target.Update();
                            // Start combat if we aren't already InCombat
                            if (!Me.InCombat)
                            {
                                Thread.Sleep(100);
                                AttackShit = true;
                                //AmeisenCore.LuaDoString("AttackTarget();");
                            }
                        }
                        i++;
                        break;
                    }
                }
                catch { }
            }
        }

        private bool CheckCondition(Condition condition)
        {
            return CheckCondition(
                GetValue(condition, 0),
                GetValue(condition, 1),
                condition
                );
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
                    return AmeisenCore.GetAuras(
                        condition.conditionLuaUnits[0]).Contains(((string)condition.customValue).ToLower());

                case CombatLogicStatement.NOT_HAS_BUFF:
                    return !AmeisenCore.GetAuras(
                        condition.conditionLuaUnits[0]).Contains(((string)condition.customValue).ToLower());
            }
            return false;
        }

        private void CheckOnCooldownAndUseSpell(CombatLogicEntry entry)
        {
            if (!AmeisenCore.IsOnCooldown((string)entry.Parameters))
            {
                SpellInfo spellInfo = AmeisenCore.GetSpellInfo((string)entry.Parameters);
                AmeisenCore.CastSpellByName((string)entry.Parameters, entry.IsForMyself);

                Thread.Sleep(spellInfo.castTime);
            }
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
                    if (entry.IsForMyself)
                    {
                        CheckOnCooldownAndUseSpell(entry);
                    }
                    else if (Target != null)
                    {
                        Me.Update();
                        Target.Update();

                        if (!Utils.IsFacing(Me.pos, Me.Rotation, Target.pos))
                            FaceTarget();

                        CheckOnCooldownAndUseSpell(entry);
                    }
                    break;

                case CombatLogicAction.USE_AOE_SPELL:
                    break;

                case CombatLogicAction.SHAPESHIFT:
                    //AmeisenCore.CastShapeshift((int)entry.Parameters);
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
        private bool ExecuteLogic(CombatLogicEntry entry)
        {
            // If me is null, do nothing, we got a bigger problem going on
            if (Me == null)
                return false;
            Me.Update();

            // Check that we are allowed to perform that action
            if (entry.ActionType == CombatActionType.ATTACK
                && !AmeisenDataHolder.Instance.IsAllowedToAttack)
                return false;
            if (entry.ActionType == CombatActionType.TANK
                && !AmeisenDataHolder.Instance.IsAllowedToTank)
                return false;
            if (entry.ActionType == CombatActionType.HEAL
                && !AmeisenDataHolder.Instance.IsAllowedToHeal)
                return false;
            if (entry.ActionType == CombatActionType.BUFF
                && !AmeisenDataHolder.Instance.IsAllowedToBuff)
                return false;

            // Make sure we are not dead or casting
            if (Me.Health < 1 || Me.IsCasting)
                return false;

            // Check if we are in spell range only if its not for me
            if (!entry.IsForMyself)
            {
                // Check if there is a target to cats the spell on if its not for me
                if (Target == null || Me.TargetGuid == 0)
                    return false;

                // Target is ready to be killed >:)
                if (!GuidsToKill.Contains(Target.Guid))
                    GuidsToKill.Add(Target.Guid);

                // Determine spell type based on range
                bool isMeleeSpell = entry.MaxSpellDistance < 3.0 ? true : false;

                // Finally check if we are in range
                while (true)
                {
                    //Move in range
                    MoveIntoRange(entry, isMeleeSpell);
                    if (IsInRange(entry))
                    {
                        AmeisenCore.MovePlayerToXYZ(Target.pos, InteractionType.STOP);
                        break;
                    }
                    Thread.Sleep(100);
                }

                if (!Utils.IsFacing(Me.pos, Me.Rotation, Target.pos))
                    FaceTarget();
            }


            // Check the Value-Conditions, Health, Energy, ...
            foreach (Condition c in entry.Conditions)
                if (!CheckCondition(c))
                    return false;

            return true;
        }

        private void FaceTarget()
        {
            AmeisenCore.MovePlayerToXYZ(Target.pos, InteractionType.FACETARGET);
        }

        private WowObject GetUnitFromListByGuid(ulong guid)
        {
            foreach (WowObject o in ActiveWoWObjects)
                if (o.Guid == guid)
                    return o;
            return null;
        }

        private double GetValue(Condition condition, int id)
        {
            if (condition.conditionLuaUnits[id] == LuaUnit.target)
                if (Target == null)
                    return -1;

            if (Me != null)
                Me.Update();
            if (Target != null)
                Target.Update();
            
            switch (condition.conditionValues[id])
            {
                case CombatLogicValues.HP:
                    if (condition.conditionLuaUnits[id] == LuaUnit.player)
                        return Me.Health;
                    else if (condition.conditionLuaUnits[id] == LuaUnit.target)
                        return Target.Health;
                    break;

                case CombatLogicValues.ENERGY:
                    if (condition.conditionLuaUnits[id] == LuaUnit.player)
                        return Me.Energy;
                    else if (condition.conditionLuaUnits[id] == LuaUnit.target)
                        return Target.Energy;
                    break;
            }

            if (condition.customValue.GetType() == typeof(int)
                || condition.customValue.GetType() == typeof(double))
                return (double)condition.customValue;

            return -1;
        }

        private bool IsInRange(CombatLogicEntry entry)
        {
            Me.Update();
            Target.Update();
            Target.Distance = Utils.GetDistance(Me.pos, Target.pos);

            if (entry.MaxSpellDistance > 0)
                if (Target.Distance <= entry.MaxSpellDistance)
                    return true;
            return false;
        }

        private bool IsPartyInCombat()
        {
            // Sometimes crashing because the List is being updated from elsewhere
            // TODO: need to fix that using a lock or so
            try
            {
                foreach (ulong guid in Me.PartymemberGuids)
                {
                    Unit activeUnit = (Unit)GetUnitFromListByGuid(guid);
                    activeUnit.Update();
                    if (activeUnit.InCombat)
                        return true;
                }
            }
            catch { }
            return false;
        }

        private void LootGuidsThatAreMine()
        {
            try
            {
                foreach (ulong guid in GuidsWithPotentialLoot)
                {
                    Unit activeUnit = (Unit)GetUnitFromListByGuid(guid);
                    activeUnit.Update();

                    if (activeUnit != null)
                    {
                        bool isLootable = activeUnit.IsLootable;
                        AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"{activeUnit.Name} [{activeUnit.Guid}] IsLootable: {isLootable}", this);

                        if (isLootable)
                        {
                            // Loot the target
                        }
                    }
                }
            }
            catch { }
        }

        // TODO: need to move this into a CombatMovementManager or something like this
        private void MoveIntoRange(CombatLogicEntry entry, bool isMeleeSpell)
        {
            Target.Update();
            AmeisenCore.MovePlayerToXYZ(Target.pos, InteractionType.MOVE);
        }

        private void RemoveDeadTargetsFromListAndCheckForLoot()
        {
            // Sometimes crashing because the List is being updated from elsewhere
            // TODO: need to fix that using a lock or so
            try
            {
                foreach (ulong guid in GuidsToKill)
                {
                    Unit activeUnit = (Unit)GetUnitFromListByGuid(guid);
                    activeUnit.Update();

                    if (activeUnit != null)
                    {
                        if (activeUnit.Health == 0 || guid == 0)
                        {
                            GuidsWithPotentialLoot.Add(guid);
                            GuidsToKill.Remove(guid);
                        }
                    }
                    break;
                }
            }
            catch { }
        }

        private void SelectTarget()
        {
            // Assist our Partymembers to notice new Guid's to be killed
            //if (AmeisenDataHolder.Instance.IsAllowedToAssistParty)
            AssistPartyMembers();

            if (Target != null)
                Target.Update();
            // If we still have no Target, select one from our list
            if (Target == null || Target.Guid == 0)
            {
                // Target a target from our "To-Be-Killed"-List
                SelectTargetFromList();
            }
        }

        private void SelectTargetFromList()
        {
            // TODO: fix this junk
            //AmeisenCore.TargetGUID(GuidsToKill.FirstOrDefault());
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"Target Guid: {GuidsToKill.FirstOrDefault()}", this);
        }
    }
}