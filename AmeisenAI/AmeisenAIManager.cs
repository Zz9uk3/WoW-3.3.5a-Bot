using AmeisenData;
using AmeisenLogging;
using AmeisenUtilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace AmeisenAI
{
    /// <summary>
    /// Singleton to manage the bots AI.
    ///
    /// It contains a Queue that can be filled up with AmeisenAction objects. This Queue will be
    /// processed by the threads powering this AI.
    ///
    /// To Start / Stop the AI call:
    /// - StartAI(THREADCOUNT);
    /// - StopAI();
    ///
    /// You can Add Actions like this:
    /// - AddActionToQueue(AMEISENACTIONOBJECT);
    ///
    /// If you wannt to get all remaining actions in the Queue call:
    /// - GetQueueItems();
    ///
    /// Or get Informations about the Threads:
    /// - GetActiveThreadCount();
    /// - GetBusyThreadCount();
    /// </summary>
    public class AmeisenAIManager
    {
        #region Private Fields

        private static readonly object padlock = new object();
        private static AmeisenAIManager instance;
        private ConcurrentQueue<AmeisenAction> actionQueue;
        private bool aiActive;
        private List<Thread> aiWorkers;
        private bool[] busyThreads;

        #endregion Private Fields

        #region Private Constructors

        private AmeisenAIManager()
        {
            IsAllowedToMove = true;
            DoFollow = true;
            IsAllowedToRevive = true;
            actionQueue = new ConcurrentQueue<AmeisenAction>();
            aiWorkers = new List<Thread>();
        }

        #endregion Private Constructors

        #region Public Properties

        /// <summary>
        /// Initialize/Get the instance of our singleton
        /// </summary>
        /// <returns>AmeisenAIManager instance</returns>
        public static AmeisenAIManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new AmeisenAIManager();
                    return instance;
                }
            }
        }

        public bool DoFollow { get; set; }
        public bool IsAllowedToMove { get; set; }
        public bool IsAllowedToRevive { get; set; }

        #endregion Public Properties

        #region Private Properties

        private Me Me
        {
            get { return AmeisenDataHolder.Instance.Me; }
            set { AmeisenDataHolder.Instance.Me = value; }
        }

        private Unit Target
        {
            get { return AmeisenDataHolder.Instance.Target; }
            set { AmeisenDataHolder.Instance.Target = value; }
        }

        #endregion Private Properties

        #region Public Methods

        /// <summary>
        /// Add an action for the bot to do.
        /// </summary>
        /// <param name="action">Action you want the bot to do</param>
        public void AddActionToQueue(ref AmeisenAction action)
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Added action to AI-Queue: " + action.ToString(), this);
            actionQueue.Enqueue(action);
        }

        /// <summary>
        /// Add an action for the bot to do.
        /// </summary>
        /// <param name="action">Action you want the bot to do</param>
        public void AddActionToQueue(AmeisenAction action)
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Added action to AI-Queue: " + action.ToString(), this);
            actionQueue.Enqueue(action);
        }

        /// <summary>
        /// Get the active running threads
        /// </summary>
        /// <returns>active threads</returns>
        public int GetActiveThreadCount()
        {
            AmeisenLogger.Instance.Log(LogLevel.VERBOSE, "Getting active Thread count", this);
            return aiWorkers.Count;
        }

        /// <summary>
        /// Get the busy threads currently working on an action
        /// </summary>
        /// <returns>busy threads</returns>
        public int GetBusyThreadCount()
        {
            AmeisenLogger.Instance.Log(LogLevel.VERBOSE, "Getting busy threads", this);
            int bThreads = 0;

            foreach (bool b in busyThreads)
                if (b)
                    bThreads++;

            return bThreads;
        }

        /// <summary>
        /// Get all the actions currently waiting to get processed.
        /// </summary>
        /// <returns>list of actions in the queue</returns>
        public List<AmeisenAction> GetQueueItems()
        {
            AmeisenLogger.Instance.Log(LogLevel.VERBOSE, "Getting AI-Queue", this);
            List<AmeisenAction> actions = new List<AmeisenAction>();
            foreach (AmeisenAction a in actionQueue)
                actions.Add(a);
            return actions;
        }

        /// <summary>
        /// Call this to start our bots "brain" and get things up and running inside the bot.
        /// </summary>
        /// <param name="threadCount">how many "Brain-Thread's" should our bot get</param>
        public void Start(int threadCount)
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Starting AI", this);
            if (!aiActive)
            {
                busyThreads = new bool[threadCount];

                for (int i = 0; i < threadCount; i++)
                    aiWorkers.Add(new Thread(() => WorkActions(i)));

                foreach (Thread t in aiWorkers)
                    t.Start();

                aiActive = true;
                AmeisenLogger.Instance.Log(LogLevel.DEBUG, "AI running", this);
            }
        }

        /// <summary>
        /// Stop the bots "brain" it won't process actions if its stopped.
        /// </summary>
        public void Stop()
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Stopping AI", this);
            if (aiActive)
            {
                aiActive = false;
                aiWorkers.Clear();
                AmeisenLogger.Instance.Log(LogLevel.DEBUG, "AI stopped", this);
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary> Modify our go-to-position by a small factor to provide "naturality" </summary>
        /// <param name="targetPos">pos you want to go to/param> <param
        /// name="distanceToTarget">distance to keep to the target</param> <returns>modified position</returns>
        private Vector3 CalculatePosToGoTo(Vector3 targetPos, int distanceToTarget)
        {
            Random rnd = new Random();
            float factorX = rnd.Next((distanceToTarget / 4) * -1, distanceToTarget / 2);
            float factorY = rnd.Next((distanceToTarget / 4) * -1, distanceToTarget / 2);
            return new Vector3(targetPos.X + factorX, targetPos.Y + factorY, targetPos.Z);
        }

        private bool CheckIfWeAreStuckIfYesJump(Vector3 initialPosition, Vector3 activePosition)
        {
            if (Utils.GetDistance(initialPosition, activePosition) < 1)
            {
                AmeisenCore.AmeisenCore.CharacterJumpAsync();
                return true;
            }
            // Here comes the Obstacle-Avoid-System in the future
            return false;
        }

        private void FaceTarget(ref AmeisenAction currentAction)
        {
            if (Target == null)
                currentAction.ActionIsDone();  // If there is no target, we can't face anyone...
            else
            {
                AmeisenCore.AmeisenCore.InteractWithGUID(Target.pos, Target.Guid, Interaction.FACETARGET);
                currentAction.ActionIsDone();
            }
        }

        private void GoToCorpseAndRevive(ref AmeisenAction currentAction)
        {
            Vector3 corpsePosition = AmeisenCore.AmeisenCore.GetCorpsePosition();

            AmeisenAction ameisenAction = new AmeisenAction(
                AmeisenActionType.MOVE_NEAR_POSITION,
                new object[] { corpsePosition, 10.0 }
                );
            AddActionToQueue(ref ameisenAction);

            while (!ameisenAction.IsActionDone()) { Thread.Sleep(250); }

            AmeisenCore.AmeisenCore.RetrieveCorpse();
            currentAction.ActionIsDone();
        }

        private void InteractWithTarget(double distance, Interaction action, ref AmeisenAction ameisenAction)
        {
            if (Target == null)
                ameisenAction.ActionIsDone();  // If there is no target, we can't interact with anyone...
            else if (Target.Distance > 3 && Target.Distance > distance)
            {
                Vector3 posToGoTo = CalculatePosToGoTo(Target.pos, (int)distance);
                AmeisenCore.AmeisenCore.InteractWithGUID(posToGoTo, Target.Guid, action);

                ameisenAction.ActionIsDone();
            }
            else if (Target.Distance < 3) // Check If we are standing to near to the current target to trigger the CTM-Action
            {
                Me.Update();
                Vector3 initialPosition = Me.pos;
                Vector3 posToGoToToMakeSureTheInteractionGetsFired = CalculatePosToGoTo(Target.pos, 16);
                AmeisenCore.AmeisenCore.MovePlayerToXYZ(posToGoToToMakeSureTheInteractionGetsFired, Interaction.MOVE);

                // Let the character run
                Thread.Sleep(2000);

                Me.Update();
                Vector3 activePosition = Me.pos;
                // Stuck check, if we haven't moved since the last iteration, jump
                CheckIfWeAreStuckIfYesJump(initialPosition, activePosition);
            }
            else
                ameisenAction.ActionIsDone();
        }

        private void MoveNearPosition(Vector3 position, double distance, ref AmeisenAction ameisenAction, bool shouldStopInRange = false)
        {
            double distanceToPoint = Utils.GetDistance(Me.pos, position);

            if (distanceToPoint > distance)
            {
                Me.Update();
                Vector3 initialPosition = Me.pos;
                Vector3 posToGoTo = CalculatePosToGoTo(position, (int)distance);

                if (IsAllowedToMove)
                    AmeisenCore.AmeisenCore.MovePlayerToXYZ(posToGoTo, Interaction.MOVE);

                // Let the character run to prevent random jumping
                Thread.Sleep(300);

                Me.Update();
                Vector3 activePosition = Me.pos;
                // Stuck check, if we haven't moved since the last iteration, jump
                CheckIfWeAreStuckIfYesJump(initialPosition, activePosition);
            }
            else
            {
                if (shouldStopInRange)
                {
                    Vector3 currentPosition = AmeisenDataHolder.Instance.Me.pos;
                    if (currentPosition.X != 0 && currentPosition.Y != 0 && currentPosition.Z != 0)
                        AmeisenCore.AmeisenCore.MovePlayerToXYZ(currentPosition, Interaction.STOP);
                }

                ameisenAction.ActionIsDone();
            }
        }

        private void MoveToPosition(Vector3 position, double distance, ref AmeisenAction ameisenAction)
        {
            double distanceToPoint = Utils.GetDistance(Me.pos, position);

            if (distanceToPoint > distance * 2)
            {
                Me.Update();
                Vector3 initialPosition = Me.pos;
                Vector3 posToGoTo = CalculatePosToGoTo(position, (int)distance);
                AmeisenCore.AmeisenCore.MovePlayerToXYZ(posToGoTo, Interaction.MOVE);

                // Let the character run to prevent random jumping
                Thread.Sleep(300);

                Me.Update();
                Vector3 activePosition = Me.pos;
                // Stuck check, if we haven't moved since the last iteration, jump
                CheckIfWeAreStuckIfYesJump(initialPosition, activePosition);
            }
            else
                ameisenAction.ActionIsDone();
        }

        /// <summary>
        /// This runs on the Brain-Threads which are constantly processing the queue of actions that
        /// the bot has to do.
        /// </summary>
        /// <param name="threadID">id to identify the thread</param>
        private void WorkActions(int threadID)
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "AI-Thread up: " + threadID, this);
            while (aiActive)
            {
                if (!actionQueue.IsEmpty)
                {
                    busyThreads[threadID - 1] = true;
                    if (actionQueue.TryDequeue(out AmeisenAction currentAction))
                    {
                        AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Processing Action: " + currentAction.ToString(), this);
                        switch (currentAction.GetActionType())
                        {
                            case AmeisenActionType.MOVE_TO_POSITION:
                                if (IsAllowedToMove)
                                    MoveToPosition((Vector3)currentAction.GetActionParams(), 3.0, ref currentAction);
                                else
                                    currentAction.ActionIsDone();
                                break;

                            case AmeisenActionType.MOVE_NEAR_POSITION:
                                if (IsAllowedToMove)
                                    MoveNearPosition((Vector3)((object[])currentAction.GetActionParams())[0], (double)((object[])currentAction.GetActionParams())[1], ref currentAction);
                                else
                                    currentAction.ActionIsDone();
                                break;

                            case AmeisenActionType.FORCE_MOVE_TO_POSITION:
                                MoveToPosition((Vector3)currentAction.GetActionParams(), 3.0, ref currentAction);
                                currentAction.ActionIsDone();
                                break;

                            case AmeisenActionType.FORCE_MOVE_NEAR_TARGET:
                                MoveNearPosition((Vector3)((object[])currentAction.GetActionParams())[0], (double)((object[])currentAction.GetActionParams())[1], ref currentAction, true);
                                break;

                            case AmeisenActionType.FACETARGET:
                                FaceTarget(ref currentAction);
                                break;

                            case AmeisenActionType.INTERACT_TARGET:
                                if (IsAllowedToMove)
                                    InteractWithTarget(3.0, (Interaction)currentAction.GetActionParams(), ref currentAction);
                                else
                                    currentAction.ActionIsDone();
                                break;

                            case AmeisenActionType.TARGET_ENTITY:
                                AmeisenCore.AmeisenCore.TargetGUID((UInt64)currentAction.GetActionParams());
                                currentAction.ActionIsDone();
                                break;

                            case AmeisenActionType.USE_SPELL:
                                WoWSpellInfo spellInfo = AmeisenCore.AmeisenCore.GetSpellInfo((string)currentAction.GetActionParams());
                                AmeisenCore.AmeisenCore.CastSpellByName((string)currentAction.GetActionParams(), false);

                                Thread.Sleep(200);

                                if (Me.CurrentState == UnitState.CASTING)
                                    Thread.Sleep(spellInfo.castTime);
                                currentAction.ActionIsDone();
                                IsAllowedToMove = true;
                                break;

                            case AmeisenActionType.USE_SPELL_ON_ME:
                                AmeisenCore.AmeisenCore.CastSpellByName((string)currentAction.GetActionParams(), true);
                                currentAction.ActionIsDone();
                                break;

                            case AmeisenActionType.GO_TO_CORPSE_AND_REVIVE:
                                GoToCorpseAndRevive(ref currentAction);
                                break;

                            default:
                                currentAction.ActionIsDone();
                                break;
                        }

                        // Reque the unfinished AmeisenAction
                        if (!currentAction.IsActionDone())
                            actionQueue.Enqueue(currentAction);
                    }
                }
                else
                {
                    Thread.Sleep(5);
                    busyThreads[threadID - 1] = false;
                }
            }
        }

        #endregion Private Methods
    }
}