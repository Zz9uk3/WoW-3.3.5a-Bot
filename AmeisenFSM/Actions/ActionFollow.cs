using AmeisenFSM.Interfaces;
using static AmeisenFSM.Objects.Delegates;

namespace AmeisenFSM.Actions
{
    public class ActionFollow : IAction
    {
        public Start StartAction { get { return Start; } }
        public DoThings StartDoThings { get { return DoThings; } }
        public Exit StartExit { get { return Stop; } }

        // Idle Start code here
        public static void Start() { }

        // Idle DoThings code here
        public static void DoThings() { }

        // Idle Stop code here
        public static void Stop() { }
    }
}
