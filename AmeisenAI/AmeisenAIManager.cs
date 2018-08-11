using AmeisenCore;
using AmeisenCore.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using AmeisenLogging;
using AmeisenUtilities;

namespace AmeisenAI
{
    /// <summary>
    /// Class that stores an action for the bot to perform as soon as possible
    /// </summary>
    public class AmeisenAction
    {
        private readonly AmeisenActionType actionType;
        private readonly object actionParams;
        private bool isDone;

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

        /// <summary>
        /// Return the AmeisenActionType of AmeisenAction
        /// </summary>
        /// <returns></returns>
        public AmeisenActionType GetActionType() { return actionType; }

        /// <summary>
        /// Get the parameters for our AmeisenAction
        /// </summary>
        /// <returns>parameters as object, need to cast that after</returns>
        public object GetActionParams() { return actionParams; }

        /// <summary>
        /// Flag the AmeisenAction as done, this allows it to be removed from the queue
        /// </summary>
        public void ActionIsDone() { isDone = true; }

        /// <summary>
        /// Is the AmeisenAction done
        /// </summary>
        /// <returns>true when its done, false when there is still stuff to do</returns>
        public bool IsActionDone() { return isDone; }
    }

    /// <summary>
    /// Singleton to manage the bots AI.
    /// 
    /// It contains a Queue that can be filled up with AmeisenAction objects.
    /// This Queue will be processed by the threads powering this AI.
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
        private List<Thread> aiWorkers;
        private bool aiActive;
        private bool[] busyThreads;
        private static AmeisenAIManager i;
        private ConcurrentQueue<AmeisenAction> actionQueue;

        private AmeisenAIManager()
        {
            actionQueue = new ConcurrentQueue<AmeisenAction>();
            aiWorkers = new List<Thread>();
        }

        ~AmeisenAIManager()
        {
            GetInstance().StopAI();
        }

        /// <summary>
        /// Initialize/Get the instance of our singleton
        /// </summary>
        /// <returns>AmeisenAIManager instance</returns>
        public static AmeisenAIManager GetInstance()
        {
            if (i == null)
                i = new AmeisenAIManager();
            return i;
        }

        /// <summary>
        /// This runs on the Brain-Threads which are constantly processing
        /// the queue of actions that the bot has to do.
        /// </summary>
        /// <param name="threadID">id to identify the thread</param>
        private void WorkActions(int threadID)
        {
            AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "AI-Thread up: " + threadID, this);
            while (aiActive)
            {
                if (!actionQueue.IsEmpty)
                {
                    busyThreads[threadID - 1] = true;
                    if (actionQueue.TryDequeue(out AmeisenAction currentAction))
                    {
                        AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Processing Action: " + currentAction.ToString(), this);
                        switch (currentAction.GetActionType())
                        {
                            case AmeisenActionType.MOVE_TO_POSITION:
                                MoveToPosition((Vector3)currentAction.GetActionParams(), 3.0, ref currentAction);
                                break;

                            case AmeisenActionType.INTERACT_TARGET:
                                InteractWithTarget(3.0, (Interaction)currentAction.GetActionParams(), ref currentAction);
                                break;
                                
                            case AmeisenActionType.TARGET_ENTITY:
                                AmeisenCore.AmeisenCore.TargetGUID((UInt64)currentAction.GetActionParams());
                                break;

                            case AmeisenActionType.USE_SPELL:
                                AmeisenCore.AmeisenCore.CastSpellByName((string)currentAction.GetActionParams(), false);
                                break;

                            case AmeisenActionType.USE_SPELL_ON_ME:
                                AmeisenCore.AmeisenCore.CastSpellByName((string)currentAction.GetActionParams(), true);
                                break;

                            default:
                                break;
                        }

                        // Reque the unfinished AmeisenAction
                        if (!currentAction.IsActionDone())
                            actionQueue.Enqueue(currentAction);
                    }
                }
                else
                {
                    Thread.Sleep(50);
                    busyThreads[threadID - 1] = false;
                }
            }
        }

        /// <summary>
        /// Call this to start our bots "brain" and get things up and running inside the bot.
        /// </summary>
        /// <param name="threadCount">how many "Brain-Thread's" should our bot get</param>
        public void StartAI(int threadCount)
        {
            AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Starting AI", this);
            if (!aiActive)
            {
                busyThreads = new bool[threadCount];

                for (int i = 0; i < threadCount; i++)
                    aiWorkers.Add(new Thread(() => WorkActions(i)));

                foreach (Thread t in aiWorkers)
                    t.Start();

                aiActive = true;
                AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "AI running", this);
            }
        }

        /// <summary>
        /// Stop the bots "brain" it won't process actions if its stopped.
        /// </summary>
        public void StopAI()
        {
            AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Stopping AI", this);
            if (aiActive)
            {
                aiActive = false;
                aiWorkers.Clear();
                AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "AI stopped", this);
            }
        }

        /// <summary>
        /// Add an action for the bot to do.
        /// </summary>
        /// <param name="action">Action you want the bot to do</param>
        public void AddActionToQueue(ref AmeisenAction action)
        {
            AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Added action to AI-Queue: " + action.ToString(), this);
            actionQueue.Enqueue(action);
        }

        /// <summary>
        /// Add an action for the bot to do.
        /// </summary>
        /// <param name="action">Action you want the bot to do</param>
        public void AddActionToQueue(AmeisenAction action)
        {
            AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Added action to AI-Queue: " + action.ToString(), this);
            actionQueue.Enqueue(action);
        }

        /// <summary>
        /// Get all the actions currently waiting to get processed.
        /// </summary>
        /// <returns>list of actions in the queue</returns>
        public List<AmeisenAction> GetQueueItems()
        {
            AmeisenLogger.GetInstance().Log(LogLevel.VERBOSE, "Getting AI-Queue", this);
            List<AmeisenAction> actions = new List<AmeisenAction>();
            foreach (AmeisenAction a in actionQueue)
                actions.Add(a);
            return actions;
        }

        /// <summary>
        /// Get the active running threads
        /// </summary>
        /// <returns>active threads</returns>
        public int GetActiveThreadCount()
        {
            AmeisenLogger.GetInstance().Log(LogLevel.VERBOSE, "Getting active Thread count", this);
            return aiWorkers.Count;
        }

        /// <summary>
        /// Get the busy threads currently working on an action
        /// </summary>
        /// <returns>busy threads</returns>
        public int GetBusyThreadCount()
        {
            AmeisenLogger.GetInstance().Log(LogLevel.VERBOSE, "Getting busy threads", this);
            int bThreads = 0;

            foreach (bool b in busyThreads)
                if (b)
                    bThreads++;

            return bThreads;
        }

        private double lastDistance;
        private Vector3 lastPosition = new Vector3 { x = float.MaxValue, y = float.MaxValue, z = float.MaxValue };

        private Vector3 CalculatePosToGoTo(Vector3 targetPos, int distanceToTarget)
        {
            Random rnd = new Random();
            float factorX = rnd.Next((distanceToTarget / 4) * -1, distanceToTarget / 2);
            float factorY = rnd.Next((distanceToTarget / 4) * -1, distanceToTarget / 2);
            return new Vector3 { x = targetPos.x + factorX, y = targetPos.y + factorY, z = targetPos.z };
        }

        private void CheckIfWeAreStuckIfYesJump(double activeDistance)
        {
            if (activeDistance * 0.9 >= lastDistance)
                AmeisenCore.AmeisenCore.CharacterJumpAsync();
            // Here comes the Obstacle-Avoid-System in the future
        }

        private void MoveToPosition(Vector3 position, double dist, ref AmeisenAction ameisenAction)
        {
            Me me = AmeisenManager.GetInstance().GetMe();
            double distanceToPoint = Utils.GetDistance(me.pos, position);

            if (distanceToPoint > dist * 2)
            {
                CheckIfWeAreStuckIfYesJump(distanceToPoint); // Stuck check, if we haven't moved since the last iteration, jump

                Vector3 posToGoTo = CalculatePosToGoTo(position, (int)dist);
                AmeisenCore.AmeisenCore.MovePlayerToXYZ(posToGoTo, Interaction.MOVE);

                // Let the character run to prevent random jumping
                Thread.Sleep(500);

                // recalculate
                distanceToPoint = Utils.GetDistance(me.pos, position);
                lastPosition = me.pos;
                lastDistance = distanceToPoint;
            }
            else
                ameisenAction.ActionIsDone();
        }

        private void InteractWithTarget(double dist, Interaction action, ref AmeisenAction ameisenAction)
        {
            Me me = AmeisenManager.GetInstance().GetMe();

            if (me.target == null)
                ameisenAction.ActionIsDone();  // If there is no target, we can't interact with anyone...
            else if (me.target.distance > 3 && me.target.distance > dist)
            {
                Vector3 posToGoTo = CalculatePosToGoTo(me.target.pos, (int)dist);
                AmeisenCore.AmeisenCore.InteractWithGUID(posToGoTo, me.target.guid, action);

                ameisenAction.ActionIsDone();
            }
            else if (me.target.distance < 3) // Check If we are standing to near to the current target to trigger the CTM-Action
            {
                CheckIfWeAreStuckIfYesJump(me.target.distance);

                Vector3 posToGoToToMakeSureTheInteractionGetsFired = CalculatePosToGoTo(me.target.pos, 16);
                AmeisenCore.AmeisenCore.MovePlayerToXYZ(posToGoToToMakeSureTheInteractionGetsFired, Interaction.MOVE);

                // Let the character run
                Thread.Sleep(2000);

                lastPosition = me.pos;
                lastDistance = me.target.distance;
            }
            else
                ameisenAction.ActionIsDone();
        }
    }
}
