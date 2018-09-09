using AmeisenBotCore;
using AmeisenBotData;
using AmeisenBotDB;
using AmeisenBotFSM.Enums;
using AmeisenBotLogger;
using AmeisenBotUtilities;
using System.Threading;

namespace AmeisenBotFSM
{
    public class AmeisenStateMachineManager
    {
        public bool Active { get; private set; }
        public bool PushedCombat { get; private set; }
        public AmeisenStateMachine StateMachine { get; private set; }
        private AmeisenDataHolder AmeisenDataHolder { get; set; }
        private AmeisenDBManager AmeisenDBManager { get; set; }
        private Thread MainWorker { get; set; }

        private Me Me
        {
            get { return AmeisenDataHolder.Me; }
            set { AmeisenDataHolder.Me = value; }
        }

        private Thread StateWatcherWorker { get; set; }

        public AmeisenStateMachineManager(AmeisenDataHolder ameisenDataHolder, AmeisenDBManager ameisenDBManager)
        {
            Active = false;
            AmeisenDataHolder = ameisenDataHolder;
            AmeisenDBManager = ameisenDBManager;
            MainWorker = new Thread(new ThreadStart(DoWork));
            StateWatcherWorker = new Thread(new ThreadStart(WatchForStateChanges));
            StateMachine = new AmeisenStateMachine(ameisenDataHolder, ameisenDBManager);
        }

        /// <summary>
        /// Fire up the FSM
        /// </summary>
        public void Start()
        {
            if (!Active)
            {
                Active = true;
                MainWorker.Start();
                StateWatcherWorker.Start();
            }
        }

        /// <summary>
        /// Shutdown the FSM
        /// </summary>
        public void Stop()
        {
            if (Active)
            {
                Active = false;
                MainWorker.Join();
                StateWatcherWorker.Join();
            }
        }

        /// <summary>
        /// Update the Statemachine, let it do its work
        /// </summary>
        private void DoWork()
        {
            while (Active)
            {
                // Do the Actions
                StateMachine.Update();
                Thread.Sleep(AmeisenDataHolder.Settings.stateMachineUpdateMillis);
            }
        }

        /// <summary>
        /// Change the state of out FSM
        /// </summary>
        private void WatchForStateChanges()
        {
            while (Active)
            {
                // Am I in combat
                InCombatCheck();

                // Do I need to release my spirit
                ReleaseSpiritCheck();

                // Am I dead?
                DeadCheck();

                AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"FSM: {StateMachine.GetCurrentState()}", this);

                Thread.Sleep(AmeisenDataHolder.Settings.stateMachineStateUpdateMillis);
            }
        }

        private void InCombatCheck()
        {
            if (Me != null)
                if (Me.InCombat || (PartymembersInCombat() && AmeisenDataHolder.IsAllowedToAssistParty))
                    StateMachine.PushAction(BotState.Combat);
                else if (StateMachine.GetCurrentState() == BotState.Combat)
                    StateMachine.PopAction();
        }

        private void ReleaseSpiritCheck()
        {
            if (AmeisenDataHolder.IsAllowedToReleaseSpirit)
                if (Me.Health == 0)
                {
                    AmeisenCore.ReleaseSpirit();
                    Thread.Sleep(2000);
                }
        }

        private void DeadCheck()
        {
            if (AmeisenDataHolder.IsAllowedToRevive)
                if (Me.IsDead)
                    StateMachine.PushAction(BotState.Dead);
                else if (StateMachine.GetCurrentState() == BotState.Dead)
                    StateMachine.PopAction();
        }

        /// <summary>
        /// Check if any of our partymembers are in combat
        /// </summary>
        /// <returns>returns yes if one or more member/s is/are in combat</returns>
        private bool PartymembersInCombat()
        {
            try
            {
                foreach (ulong guid in Me.PartymemberGuids)
                    foreach (WowObject obj in AmeisenDataHolder.ActiveWoWObjects)
                        if (guid == obj.Guid)
                        {
                            if (((Unit)obj).InCombat)
                                return true;
                            break;
                        }
            }
            catch { }
            return false;
        }
    }
}