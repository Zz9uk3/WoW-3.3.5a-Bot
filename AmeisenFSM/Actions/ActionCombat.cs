using AmeisenBotCombat;
using AmeisenBotCore;
using AmeisenBotData;
using AmeisenBotFSM.Interfaces;
using AmeisenBotUtilities;
using static AmeisenBotFSM.Objects.Delegates;

namespace AmeisenBotFSM.Actions
{
    internal class ActionCombat : IAction
    {
        public Start StartAction { get { return Start; } }
        public DoThings StartDoThings { get { return DoThings; } }
        public Exit StartExit { get { return Stop; } }
        private AmeisenCombatManager combatManager;

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

        public ActionCombat() { combatManager = new AmeisenCombatManager(); }

        public void DoThings()
        {
            combatManager.DoWork();
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        private void CastSpellByName(string name, bool onMyself)
        {
            AmeisenCore.CastSpellByName(name, onMyself);
        }

        private void FaceTarget()
        {
            if (Target != null)
            {
                Target.Update();
                AmeisenCore.InteractWithGUID(
                    Target.pos,
                    Target.Guid,
                    InteractionType.FACETARGET
                    );
            }
        }

        private SpellInfo GetSpellInfo(string name, bool onMyself)
        {
            return AmeisenCore.GetSpellInfo(name);
        }
    }
}