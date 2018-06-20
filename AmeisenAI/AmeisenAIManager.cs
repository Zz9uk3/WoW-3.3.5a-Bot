using AmeisenCore;
using AmeisenCore.Objects;
using System;
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
        private Thread aiWorker;
        private bool aiActive;
        private static AmeisenAIManager i;
        private Queue<AmeisenAction> actionQueue;

        private AmeisenAIManager()
        {
            aiWorker = new Thread(new ThreadStart(WorkActions));
            actionQueue = new Queue<AmeisenAction>();
        }

        public static AmeisenAIManager GetInstance()
        {
            if (i == null)
                i = new AmeisenAIManager();
            return i;
        }

        private void WorkActions()
        {
            while (aiActive)
            {
                if (actionQueue.Count > 0)
                {
                    AmeisenAction currentAction = actionQueue.Dequeue();
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
                else
                    Thread.Sleep(50);
            }
        }

        public void StartAI()
        {
            if (!aiActive)
            {
                aiWorker.Start();
                aiActive = true;
            }
        }

        public void StopAI()
        {
            if (aiActive)
            {
                aiActive = false;
                aiWorker = new Thread(new ThreadStart(WorkActions));
            }
        }

        public void AddActionToQueue(AmeisenAction action)
        {
            actionQueue.Enqueue(action);
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
                        lastDistance = groupleader.distance;
                    }
                }
            }
        }
    }
}
