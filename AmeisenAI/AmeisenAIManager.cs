using AmeisenCore;
using AmeisenCore.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace AmeisenAI
{
    /// <summary>
    /// Action types
    /// </summary>
    public enum AmeisenActionType
    {
        FOLLOW_TARGET,
        FOLLOW_GROUPLEADER,
        LOOT_TARGET,
        TARGET_ENTITY,
        TARGET_MYSELF,
        ATTACK_TARGET,
        USE_SPELL,
        INTERACT_TARGET,
    }
    
    /// <summary>
    /// Class that stores an action for the bot to perform as soon as possible
    /// </summary>
    public class AmeisenAction
    {
        private AmeisenActionType actionType;
        private object actionParams;

        /// <summary>
        /// Class to describe an action for the Barin-Threads to process
        /// </summary>
        /// <param name="actionType">what the bot should do</param>
        /// <param name="actionParams">parameters for the action</param>
        public AmeisenAction(AmeisenActionType actionType, object actionParams)
        {
            this.actionType = actionType;
            this.actionParams = actionParams;
        }

        public AmeisenActionType GetActionType() { return actionType; }
        public object GetActionParams() { return actionParams; }
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
            while (aiActive)
            {
                if (!actionQueue.IsEmpty)
                {
                    busyThreads[threadID - 1] = true;
                    if (actionQueue.TryDequeue(out AmeisenAction currentAction))
                    {
                        switch (currentAction.GetActionType())
                        {
                            case AmeisenActionType.FOLLOW_TARGET:
                                FollowTarget((double)currentAction.GetActionParams());
                                break;

                            case AmeisenActionType.FOLLOW_GROUPLEADER:
                                FollowGroupLeader((double)currentAction.GetActionParams());
                                break;

                            case AmeisenActionType.INTERACT_TARGET:
                                InteractWithTarget();
                                break;

                            case AmeisenActionType.ATTACK_TARGET:
                                AttackTarget();
                                break;

                            case AmeisenActionType.LOOT_TARGET:
                                LootTarget();
                                break;

                            case AmeisenActionType.TARGET_MYSELF:
                                AmeisenCore.AmeisenCore.LUADoString("/target player");
                                break;

                            case AmeisenActionType.TARGET_ENTITY:
                                AmeisenCore.AmeisenCore.LUADoString("/target " + (string)currentAction.GetActionParams());
                                break;

                            case AmeisenActionType.USE_SPELL:
                                AmeisenCore.AmeisenCore.LUADoString("/cast " + (string)currentAction.GetActionParams());
                                break;

                            default:
                                break;
                        }
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
            if (!aiActive)
            {
                busyThreads = new bool[threadCount];

                for (int i = 0; i < threadCount; i++)
                    aiWorkers.Add(new Thread(() => WorkActions(i)));

                foreach (Thread t in aiWorkers)
                    t.Start();

                aiActive = true;
            }
        }

        /// <summary>
        /// Stop the bots "brain" it won't process actions if its stopped.
        /// </summary>
        public void StopAI()
        {
            if (aiActive)
            {
                aiActive = false;
                aiWorkers.Clear();
            }
        }

        /// <summary>
        /// Add an action for the bot to do.
        /// </summary>
        /// <param name="action">Action you want the bot to do</param>
        public void AddActionToQueue(AmeisenAction action)
        {
            actionQueue.Enqueue(action);
        }

        /// <summary>
        /// Get all the actions currently waiting to get processed.
        /// </summary>
        /// <returns>list of actions in the queue</returns>
        public List<AmeisenAction> GetQueueItems()
        {
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
            return aiWorkers.Count;
        }

        /// <summary>
        /// Get the busy threads currently working on an action
        /// </summary>
        /// <returns>busy threads</returns>
        public int GetBusyThreadCount()
        {
            int bThreads = 0;

            foreach (bool b in busyThreads)
                if (b)
                    bThreads++;

            return bThreads;
        }

        private double lastDistance;
        private float[] lastPosition = new float[3];

        private void FollowTarget(double dist)
        {
            Me me = AmeisenManager.GetInstance().GetMe();

            if (me.target != null)
            {
                Random rnd = new Random();
                float factorX = rnd.Next((int)dist / 4, (int)dist / 2);
                float factorY = rnd.Next((int)dist / 4, (int)dist / 2);

                if (me.target.distance > dist)
                {
                    if (me.target.distance >= lastDistance)
                        AmeisenCore.AmeisenCore.CharacterJump();

                    AmeisenCore.AmeisenCore.MovePlayerToXYZ(me.target.posX + factorX, me.target.posY + factorY, me.target.posZ);

                    lastPosition[0] = me.posX;
                    lastPosition[1] = me.posY;
                    lastPosition[2] = me.posZ;
                    lastDistance = me.target.distance;
                }
            }
        }

        private void FollowGroupLeader(double dist)
        {
            Me me = AmeisenManager.GetInstance().GetMe();

            if (me.partymembers != null)
            {
                Target groupleader = null;

                foreach (Target t in me.partymembers)
                    if (t.isPartyLeader)
                        groupleader = t;

                if (groupleader != null)
                {
                    Random rnd = new Random();
                    float factorX = rnd.Next((int)dist / 4, (int)dist / 2);
                    float factorY = rnd.Next((int)dist / 4, (int)dist / 2);

                    if (groupleader.distance > dist)
                    {
                        if (groupleader.distance >= lastDistance)
                            AmeisenCore.AmeisenCore.CharacterJump();

                        AmeisenCore.AmeisenCore.MovePlayerToXYZ(groupleader.posX + factorX, groupleader.posY + factorY, groupleader.posZ);

                        lastPosition[0] = me.posX;
                        lastPosition[1] = me.posY;
                        lastPosition[2] = me.posZ;
                        lastDistance = groupleader.distance;
                    }
                }
            }
        }

        private void InteractWithTarget()
        {
            Me me = AmeisenManager.GetInstance().GetMe();

            int dist = 8;

            if (me.target != null)
            {
                Random rnd = new Random();

                if (me.target.distance < 3)
                {
                    float xOffset = rnd.Next(8, 12);
                    float yOffset = rnd.Next(8, 12);

                    AmeisenCore.AmeisenCore.MovePlayerToXYZ(me.target.posX + xOffset, me.target.posY + yOffset, me.target.posZ);
                }

                Thread.Sleep(3000);

                float factorX = rnd.Next((int)dist / 4, (int)dist / 2);
                float factorY = rnd.Next((int)dist / 4, (int)dist / 2);

                AmeisenCore.AmeisenCore.InteractWithGUID(me.target.posX + factorX, me.target.posY + factorY, me.target.posZ, me.target.guid);
            }
        }

        private void AttackTarget()
        {
            Me me = AmeisenManager.GetInstance().GetMe();

            int dist = 8;

            if (me.target != null)
            {
                Random rnd = new Random();

                if (me.target.distance < 3)
                {
                    float xOffset = rnd.Next(8, 12);
                    float yOffset = rnd.Next(8, 12);

                    AmeisenCore.AmeisenCore.MovePlayerToXYZ(me.target.posX + xOffset, me.target.posY + yOffset, me.target.posZ);
                }

                Thread.Sleep(3000);

                float factorX = rnd.Next((int)dist / 4, (int)dist / 2);
                float factorY = rnd.Next((int)dist / 4, (int)dist / 2);

                AmeisenCore.AmeisenCore.AttackGUID(me.target.posX + factorX, me.target.posY + factorY, me.target.posZ, me.target.guid);
            }
        }

        private void LootTarget()
        {
            Me me = AmeisenManager.GetInstance().GetMe();

            int dist = 8;

            if (me.target != null)
            {
                Random rnd = new Random();

                if (me.target.distance < 3)
                {
                    float xOffset = rnd.Next(8, 12);
                    float yOffset = rnd.Next(8, 12);

                    AmeisenCore.AmeisenCore.MovePlayerToXYZ(me.target.posX + xOffset, me.target.posY + yOffset, me.target.posZ);
                }

                Thread.Sleep(3000);

                float factorX = rnd.Next((int)dist / 4, (int)dist / 2);
                float factorY = rnd.Next((int)dist / 4, (int)dist / 2);

                AmeisenCore.AmeisenCore.LootGUID(me.target.posX + factorX, me.target.posY + factorY, me.target.posZ, me.target.guid);
            }
        }
    }
}
