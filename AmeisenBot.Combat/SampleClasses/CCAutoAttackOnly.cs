using AmeisenBotData;
using AmeisenBotLogger;
using AmeisenBotUtilities;
using AmeisenCombatEngine.Interfaces;
using System.Collections.Generic;

namespace AmeisenBotCombat.SampleClasses
{
    public class CCAutoAttackOnly : IAmeisenCombatClass
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

        public void HandleBuffs()
        {

        }

        public void HandleHealing()
        {
        }

        public void HandleTanking()
        {
        }

        private void DoAttackRoutine()
        {
        }
    }
}