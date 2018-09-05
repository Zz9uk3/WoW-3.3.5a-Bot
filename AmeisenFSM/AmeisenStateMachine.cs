using AmeisenFSM.Actions;
using AmeisenFSM.Enums;
using AmeisenFSM.Interfaces;
using AmeisenFSM.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                StateStack.Peek() : BotState.Error;
        }

        public void PushAction(BotState botState)
        {
            if (GetCurrentState() != botState)
                StateStack.Push(botState);
        }

        public BotState PopAction()
        {
            return StateStack.Pop();
        }

        private IAction GetCurrentStateAction(BotState state)
        {
            return StateActionMap[state];
        }
    }
}
