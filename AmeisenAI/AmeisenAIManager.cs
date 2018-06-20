using AmeisenCore;
using AmeisenCore.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AmeisenAI
{
    public enum AmeisenActionType
    {
        FOLLOW_TARGET,
        FOLLOW_GROUPLEADER,

        TARGET_ENTITY,
        ATTACK_TARGET,
        USE_SPELL,
    }

    public class AmeisenAction
    {
        private AmeisenActionType actionType;
        private object actionParams;

        public AmeisenAction(AmeisenActionType actionType, object actionParams)
        {
            this.actionType = actionType;
            this.actionParams = actionParams;
        }

        public AmeisenActionType GetActionType() { return actionType; }
        public object GetActionParams() { return actionParams; }
    }

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

        private void WorkActions(int threadID)
        {
            while (aiActive)
            {
                if (!actionQueue.IsEmpty)
                {
                    busyThreads[threadID-1] = true;
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

                            default:
                                break;
                        }
                    }
                }
                else
                {
                    Thread.Sleep(50);
                    busyThreads[threadID-1] = false;
                }
            }
        }

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

        public void StopAI()
        {
            if (aiActive)
            {
                aiActive = false;
                aiWorkers.Clear();
            }
        }

        public void AddActionToQueue(AmeisenAction action)
        {
            actionQueue.Enqueue(action);
        }

        public List<AmeisenAction> GetQueueItems()
        {
            List<AmeisenAction> actions = new List<AmeisenAction>();
            foreach (AmeisenAction a in actionQueue)
                actions.Add(a);
            return actions;
        }

        public int GetActiveThreadCount()
        {
            return aiWorkers.Count;
        }

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
    }
}
