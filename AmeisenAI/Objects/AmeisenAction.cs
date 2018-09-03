using System.Text;

namespace AmeisenAI
{
    /// <summary>
    /// Class that stores an action for the bot to perform as soon as possible
    /// </summary>
    public class AmeisenAction
    {
        /// <summary>
        /// Class to describe an action for the Brain-Threads to process
        /// </summary>
        /// <param name="actionType">what the bot should do</param>
        /// <param name="actionParams">parameters for the action</param>
        public AmeisenAction(AmeisenActionType actionType, object actionParams, AmeisenActionCallback callback)
        {
            ActionType = actionType;
            ActionParams = actionParams;
            IsDone = false;
            Callback = callback;
        }

        public delegate void AmeisenActionCallback();

        public object ActionParams { get; private set; }
        public AmeisenActionType ActionType { get; private set; }
        public AmeisenActionCallback Callback { get; private set; }

        public bool IsDone { get; private set; }

        public void ActionIsDone()
        {
            IsDone = true;
            Callback?.Invoke();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Type: " + ActionType + ", ");
            if (ActionParams != null)
                sb.Append("ActionParams: " + ActionParams.ToString() + ", ");
            sb.Append("IsDone: " + IsDone.ToString());
            if (Callback != null)
                sb.Append("Callback: " + Callback.ToString());
            return sb.ToString();
        }
    }
}