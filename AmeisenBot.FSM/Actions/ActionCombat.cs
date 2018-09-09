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
        private AmeisenDataHolder AmeisenDataHolder { get; set; }

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

        public ActionCombat(AmeisenDataHolder ameisenDataHolder)
        {
            AmeisenDataHolder = ameisenDataHolder;
        }

        public void DoThings()
        {
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