using AmeisenBotCombat.SampleClasses;
using AmeisenBotData;
using AmeisenBotFSM.Interfaces;
using AmeisenBotUtilities;
using AmeisenCombatEngine.Interfaces;
using static AmeisenBotFSM.Objects.Delegates;

namespace AmeisenBotFSM.Actions
{
    internal class ActionCombat : IAction
    {
        public Start StartAction { get { return Start; } }
        public DoThings StartDoThings { get { return DoThings; } }
        public Exit StartExit { get { return Stop; } }
        private AmeisenDataHolder AmeisenDataHolder { get; set; }
        private IAmeisenCombatClass CombatClass { get; set; }

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

        public ActionCombat(AmeisenDataHolder ameisenDataHolder, IAmeisenCombatClass combatClass)
        {
            AmeisenDataHolder = ameisenDataHolder;

            // TODO: Add Clas/Spec recognition
            if (combatClass == null)
            {
                CombatClass = new CCAutoAttackOnly
                {
                    AmeisenDataHolder = ameisenDataHolder
                };
            }
            else
            {
                CombatClass = combatClass;
                CombatClass.AmeisenDataHolder = ameisenDataHolder;
            }
        }

        public void DoThings()
        {
            try
            {
                if (CombatClass != null)
                {
                    if (AmeisenDataHolder.IsAllowedToAttack)
                    {
                        CombatClass.HandleAttacking();
                    }

                    if (AmeisenDataHolder.IsAllowedToTank)
                    {
                        CombatClass.HandleTanking();
                    }

                    if (AmeisenDataHolder.IsAllowedToHeal)
                    {
                        CombatClass.HandleHealing();
                    }
                }
            }
            catch { }
        }

        public void Start()
        {
            CombatClass?.Init();
        }

        public void Stop()
        {
            CombatClass?.Exit();
        }
    }
}