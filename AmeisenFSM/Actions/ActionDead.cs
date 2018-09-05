using AmeisenCoreUtils;
using AmeisenData;
using AmeisenFSM.Interfaces;
using AmeisenUtilities;
using System.Collections.Generic;
using static AmeisenFSM.Objects.Delegates;

namespace AmeisenFSM.Actions
{
    public class ActionDead : IAction
    {
        public Start StartAction { get { return Start; } }
        public DoThings StartDoThings { get { return DoThings; } }
        public Exit StartExit { get { return Stop; } }

        private Unit ActiveUnit { get; set; }

        private List<Unit> ActiveUnits { get; set; }

        private Me Me
        {
            get { return AmeisenDataHolder.Instance.Me; }
            set { AmeisenDataHolder.Instance.Me = value; }
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

        private void GoToCorpseAndRevive()
        {
            Vector3 corpsePosition = AmeisenCoreUtils.AmeisenCore.GetCorpsePosition();

            if (corpsePosition.X != 0 && corpsePosition.Y != 0 && corpsePosition.Z != 0)
                MoveNearCorpseAndRevive(corpsePosition);

            //if (Me.Health <= 1)
            //We are alive
        }

        private void MoveNearCorpseAndRevive(Vector3 corpsePosition)
        {
            // Move to corpse Revive
            AmeisenCore.RetrieveCorpse();
        }
    }
}