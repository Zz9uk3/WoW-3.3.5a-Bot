using AmeisenFSM.Actions;
using AmeisenFSM.Enums;
using AmeisenFSM.Interfaces;
using AmeisenLogging;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AmeisenFSM
{
    /// <summary>
    /// This is a Stack based FSM that will manage our bots actions
    ///
    /// You can Push a BotState and Pop a BotState from the stack. This allows you to manage the bots
    /// executed logic. (sidenote: duplicate BotStates wont be added to the Stack)
    ///
    /// There is a Bot enum, wich an IAction gets mapped to using a Dictionary. This IAction has to
    /// implement 3 methods:
    ///
    /// Start:      will be called on first call only
    /// DoThings:   will be called everytime Update(); is called
    /// Exit:       will be called after the last call only
    ///
    /// In this 3 methods you can implement logic or so...
    ///
    /// And remember to call Update(); on a frequent basis, else this thing isnt going to do anything.
    /// </summary>
    public class AmeisenStateMachine
    {
        private Dictionary<BotState, IAction> StateActionMap { get; set; }

        private Stack<BotState> StateStack { get; set; }

        public AmeisenStateMachine()
        {
            StateStack = new Stack<BotState>();
            StateActionMap = new Dictionary<BotState, IAction>
            {
                { BotState.Idle, new ActionIdle() },
                { BotState.Follow, new ActionFollow() },
                { BotState.Moving, new ActionMoving() },
                { BotState.Combat, new ActionCombat() },
                { BotState.Dead, new ActionDead() }
            };
        }

        /// <summary>
        /// Returns our current BotState to see what the bot is doing right now
        /// </summary>
        /// <returns>current BotState</returns>
        public BotState GetCurrentState()
        {
            return StateStack.Count > 0 ?
                StateStack.Peek() : BotState.None;
        }

        /// <summary>
        /// Pop the state Stack of the bot, calls the Start() of new State and the Stop() of current State.
        /// </summary>
        /// <param name="botState">the state you want to change to</param>
        public BotState PopAction([CallerMemberName]string functionName = "")
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"FSM Pop called by: {functionName}", this);
            GetCurrentStateAction(GetCurrentState())?.StartExit.Invoke();
            BotState tmpState = StateStack.Pop();
            GetCurrentStateAction(GetCurrentState())?.StartAction.Invoke();
            return tmpState;
        }

        /// <summary>
        /// Push something onto the state Stack of the bot, this calls the Stop() of current State
        /// and the Start() of new State.
        /// </summary>
        /// <param name="botState">the state you want to change to</param>
        public void PushAction(BotState botState, [CallerMemberName]string functionName = "")
        {
            if (GetCurrentState() != botState)
            {
                AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"FSM Push [{botState}] called by: {functionName}", this);
                GetCurrentStateAction(GetCurrentState())?.StartExit.Invoke();
                StateStack.Push(botState);
                GetCurrentStateAction(GetCurrentState())?.StartAction.Invoke();
            }
        }

        /// <summary>
        /// Call this to Update the Statemachine and execute actions
        /// </summary>
        public void Update()
        {
            GetCurrentStateAction(GetCurrentState())?.StartDoThings.Invoke();
        }

        /// <summary>
        /// Map the BotState to an IAction containing Start(), DoThings() and Stop()
        /// </summary>
        /// <param name="state">state to map</param>
        /// <returns>IAction</returns>
        private IAction GetCurrentStateAction(BotState state)
        {
            if (state == BotState.None)
                return null;
            return StateActionMap[state];
        }
    }
}