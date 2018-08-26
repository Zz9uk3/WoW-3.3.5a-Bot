namespace AmeisenUtilities
{
    /// <summary>
    /// Class that stores an action for the bot to perform as soon as possible
    /// </summary>
    public class AmeisenAction
    {
        #region Private Fields

        private readonly object actionParams;
        private readonly AmeisenActionType actionType;
        private bool isDone;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Class to describe an action for the Brain-Threads to process
        /// </summary>
        /// <param name="actionType">what the bot should do</param>
        /// <param name="actionParams">parameters for the action</param>
        public AmeisenAction(AmeisenActionType actionType, object actionParams)
        {
            this.actionType = actionType;
            this.actionParams = actionParams;
            isDone = false;
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Flag the AmeisenAction as done, this allows it to be removed from the queue
        /// </summary>
        public void ActionIsDone() { isDone = true; }

        /// <summary>
        /// Get the parameters for our AmeisenAction
        /// </summary>
        /// <returns>parameters as object, need to cast that after</returns>
        public object GetActionParams() { return actionParams; }

        /// <summary>
        /// Return the AmeisenActionType of AmeisenAction
        /// </summary>
        /// <returns></returns>
        public AmeisenActionType GetActionType() { return actionType; }

        /// <summary>
        /// Is the AmeisenAction done
        /// </summary>
        /// <returns>true when its done, false when there is still stuff to do</returns>
        public bool IsActionDone() { return isDone; }

        #endregion Public Methods
    }
}