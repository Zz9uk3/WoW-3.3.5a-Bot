using AmeisenData;
using AmeisenFSM.Enums;
using AmeisenUtilities;
using System.Threading;

namespace AmeisenFSM
{
    public class AmeisenStateMachineManager
    {
        public bool Active { get; private set; }
        public bool PushedCombat { get; private set; }
        public AmeisenStateMachine StateMachine { get; private set; }

        private Thread MainWorker { get; set; }

        private Me Me
        {
            get { return AmeisenDataHolder.Instance.Me; }
            set { AmeisenDataHolder.Instance.Me = value; }
        }

        private Thread StateWatcherWorker { get; set; }

        public AmeisenStateMachineManager()
        {
            Active = false;
            MainWorker = new Thread(new ThreadStart(DoWork));
            StateWatcherWorker = new Thread(new ThreadStart(WatchForStateChanges));
            StateMachine = new AmeisenStateMachine();
        }

        public void Start()
        {
            if (!Active)
            {
                Active = true;
                MainWorker.Start();
                StateWatcherWorker.Start();
            }
        }

        public void Stop()
        {
            if (Active)
            {
                Active = false;
                MainWorker.Join();
                StateWatcherWorker.Join();
            }
        }

        private void DoWork()
        {
            while (Active)
            {
                StateMachine.Update();
                Thread.Sleep(10);
            }
        }

        private void WatchForStateChanges()
        {
            while (Active)
            {
                if (Me.InCombat)
                    StateMachine.PushAction(BotState.Combat);
                else if (StateMachine.GetCurrentState() == BotState.Combat)
                    StateMachine.PopAction();

                Thread.Sleep(250);
            }
        }
    }
}