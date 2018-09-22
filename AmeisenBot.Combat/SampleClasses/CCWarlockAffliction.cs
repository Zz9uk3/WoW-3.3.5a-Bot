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
                    CombatUtils.FaceTarget(unitToAttack);
                    CombatUtils.AttackTarget();
                }

                DoAttackRoutine();
            }
        }

        private void DoAttackRoutine()
        {
            List<string> targetAuras = CombatUtils.GetAuras(LuaUnit.target);

            Me?.Update();
            // Restore Mana
            if (Me.EnergyPercentage < 30 && Me.HealthPercentage > 50)
            {
                CombatUtils.CastSpellByName("Life Tap", true);
            }

            Target?.Update();
            // DoT's to apply
            if (!targetAuras.Contains("Curse of Agony"))
            {
                CombatUtils.CastSpellByName("Curse of Agony", false);
            }
            if (!targetAuras.Contains("Corruption"))
            {
                CombatUtils.CastSpellByName("Corruption", false);
            }
            if (!targetAuras.Contains("Unstable Affliction"))
            {
                CombatUtils.CastSpellByName("Unstable Affliction", false);
            }
            if (!targetAuras.Contains("Haunt"))
            {
                CombatUtils.CastSpellByName("Haunt", false);
            }

            Target?.Update();
            // Active-Damage Spell
            if (Target?.HealthPercentage < 25)
            {
                CombatUtils.CastSpellByName("Drain Soul", false);
            }
            else
            {
                CombatUtils.CastSpellByName("Shadow Bolt", false);
            }
        }

        public void HandleBuffs()
        {
            List<string> myAuras = CombatUtils.GetAuras(LuaUnit.player);

            if (!myAuras.Contains("Demon Armor"))
            {
                CombatUtils.CastSpellByName("Demon Armor", true);
            }
            if (!myAuras.Contains("Blood Pact"))
            {
                CombatUtils.CastSpellByName("Summon Imp", true);
            }
        }

        public void HandleHealing()
        {
        }

        public void HandleTanking()
        {
        }
    }
}
