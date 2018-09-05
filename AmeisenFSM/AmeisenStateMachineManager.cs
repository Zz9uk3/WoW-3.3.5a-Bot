using System.Threading;

namespace AmeisenFSM
{
    public class AmeisenStateMachineManager
    {
        public bool Active { get; private set; }
        private Thread MainWorker { get; set; }
        public AmeisenStateMachine StateMachine { get; private set; }

        public AmeisenStateMachineManager()
        {
            MainWorker = new Thread(new ThreadStart(DoWork));
            StateMachine = new AmeisenStateMachine();
        }

        public void Start()
        {
            if (!Active)
            {
                MainWorker.Start();
                Active = true;
            }
        }

        public void Stop()
        {
            if (Active)
            {
                Active = true;
                MainWorker.Join();
            }
        }

        private void DoWork()
        {
            while (Active)
            {
                StateMachine.Update();
                Thread.Sleep(25);
            }
        }

    }
}
