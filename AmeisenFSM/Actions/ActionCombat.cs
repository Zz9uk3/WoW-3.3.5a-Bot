using AmeisenFSM.Interfaces;
using static AmeisenFSM.Objects.Delegates;

namespace AmeisenFSM.Actions
{
    class ActionCombat : IAction
    {
        public Start StartAction { get { return Start; } }
        public DoThings StartDoThings { get { return DoThings; } }
        public Exit StartExit { get { return Stop; } }

        // Idle Start code here
        public void Start() { }

        // Idle DoThings code here
        public void DoThings() { }

        // Idle Stop code here
        public void Stop() { }
    }
}
