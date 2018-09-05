using AmeisenFSM.Actions;
using AmeisenFSM.Enums;
using AmeisenFSM.Interfaces;
using System.Collections.Generic;

namespace AmeisenFSM
{
    public class AmeisenStateMachine
    {
        private Stack<BotState> StateStack { get; set; }
        private Dictionary<BotState, IAction> StateActionMap { get; set; }

        public AmeisenStateMachine()
        {
            StateStack = new Stack<BotState>();
            StateActionMap = new Dictionary<BotState, IAction>
            {
                { BotState.Idle, new ActionIdle() },
                { BotState.Follow, new ActionFollow() },
                { BotState.Moving, new ActionMoving() },
                { BotState.Combat, new ActionCombat() }
            };
        }

        public void Update()
        {
            GetCurrentStateAction(GetCurrentState())?.StartDoThings.Invoke();
        }

        public BotState GetCurrentState()
        {
            return StateStack.Count > 0 ?
                StateStack.Peek() : BotState.None;
        }

        public void PushAction(BotState botState)
        {
            if (GetCurrentState() != botState)
            {
                GetCurrentStateAction(GetCurrentState())?.StartExit.Invoke();
                StateStack.Push(botState);
                GetCurrentStateAction(GetCurrentState())?.StartAction.Invoke();
            }
        }

        public BotState PopAction()
        {
            GetCurrentStateAction(GetCurrentState())?.StartExit.Invoke();
            BotState tmpState = StateStack.Pop();
            GetCurrentStateAction(GetCurrentState())?.StartAction.Invoke();
            return tmpState;
        }

        private IAction GetCurrentStateAction(BotState state)
        {
            if (state == BotState.None)
                return null;
            return StateActionMap[state];
        }
    }
}
