using AmeisenBotData;
using AmeisenBotLogger;
using AmeisenBotUtilities;
using AmeisenCombatEngine.Interfaces;
using System.Collections.Generic;

namespace AmeisenBotCombat.SampleClasses
{
    public class CCWarlockAffliction : IAmeisenCombatClass
    {
        public AmeisenDataHolder AmeisenDataHolder { get; set; }

        private Me Me
        {
            get { return AmeisenDataHolder.Me; }
            set { AmeisenDataHolder.Me = value; }
        }

        private Unit Target
        {
            get { return AmeisenDataHolder.Target; }
            set { AmeisenDataHolder.Target = value; }
        }

        public void Init()
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "CombatClass: In combat now", this);
        }

        public void Exit()
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "CombatClass: Out of combat now", this);
        }

        public void HandleAttacking()
        {
            if (Me != null)
            {
                Me.Update();
            }
            if (Target != null)
            {
                Target.Update();
            }

            Unit unitToAttack = Target;

            // Get a target
            if (Me.TargetGuid == 0)
            {
                unitToAttack = CombatUtils.AssistParty(Me, AmeisenDataHolder.ActiveWoWObjects);
            }

            if (unitToAttack != null)
            {
                // Start autoattack
                if (!Me.InCombat)
                {
                    CombatUtils.FaceTarget(Me, unitToAttack);
                    CombatUtils.AttackTarget();
                }

                DoAttackRoutine();
            }
        }

        public void HandleBuffs()
        {
            List<string> myAuras = CombatUtils.GetAuras(LuaUnit.player);

            if (!myAuras.Contains("demon armor"))
            {
                CombatUtils.CastSpellByName(Me,Target,"Demon Armor", true);
            }
            if (!myAuras.Contains("blood pact"))
            {
                CombatUtils.CastSpellByName(Me, Target, "Summon Imp", true);
            }
        }

        public void HandleHealing()
        {
        }

        public void HandleTanking()
        {
        }

        private void DoAttackRoutine()
        {
            List<string> targetAuras = CombatUtils.GetAuras(LuaUnit.target);

            Me?.Update();
            // Restore Mana
            if (Me.EnergyPercentage < 30 && Me.HealthPercentage > 50)
            {
                CombatUtils.CastSpellByName(Me, Target, "Life Tap", true);
                return;
            }

            Target?.Update();
            // DoT's to apply
            if (!targetAuras.Contains("curse of agony"))
            {
                CombatUtils.CastSpellByName(Me, Target, "Curse of Agony", false);
                return;
            }
            if (!targetAuras.Contains("corruption"))
            {
                CombatUtils.CastSpellByName(Me, Target, "Corruption", false);
                return;
            }
            if (!targetAuras.Contains("unstable affliction"))
            {
                CombatUtils.CastSpellByName(Me, Target, "Unstable Affliction", false);
                return;
            }
            if (!targetAuras.Contains("haunt"))
            {
                CombatUtils.CastSpellByName(Me, Target, "Haunt", false);
                return;
            }

            Target?.Update();
            // Active-Damage Spell
            if (Target?.HealthPercentage < 25)
            {
                CombatUtils.CastSpellByName(Me, Target, "Drain Soul", false);
                return;
            }
            else
            {
                CombatUtils.CastSpellByName(Me, Target, "Shadow Bolt", false);
                return;
            }
        }
    }
}