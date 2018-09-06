using AmeisenCoreUtils;
using AmeisenData;
using AmeisenFSM.Interfaces;
using AmeisenUtilities;
using System;
using System.Collections.Generic;
using static AmeisenFSM.Objects.Delegates;

namespace AmeisenFSM.Actions
{
    public class ActionMoving : IAction
    {
        public virtual Start StartAction { get { return Start; } }
        public virtual DoThings StartDoThings { get { return DoThings; } }
        public virtual Exit StartExit { get { return Stop; } }
        public Queue<Vector3> WaypointQueue { get; set; }

        private Me Me
        {
            get { return AmeisenDataHolder.Instance.Me; }
            set { AmeisenDataHolder.Instance.Me = value; }
        }

        public virtual void DoThings()
        {
            if (WaypointQueue.Count > 0)
            {
                MoveToNode();
            }
        }

        public virtual void Start()
        {
            WaypointQueue = new Queue<Vector3>();
        }

        public virtual void Stop()
        {
            WaypointQueue.Clear();
        }

        /// <summary>
        /// Very basic Obstacle avoidance.
        ///
        /// Need to change this to a better waypoint system that uses our MapNode Database...
        /// </summary>
        /// <param name="initialPosition">initial position</param>
        /// <param name="activePosition">position now</param>
        /// <returns>if we havent moved 0.5m in the 2 vectors, jump and return true</returns>
        private bool CheckIfWeAreStuckIfYesJump(Vector3 initialPosition, Vector3 activePosition)
        {
            // we are possibly stuck at a fence or so
            if (Utils.GetDistance(initialPosition, activePosition) < 0.5)
            {
                AmeisenCore.CharacterJumpAsync();
                return true;
            }
            // Here comes the Obstacle-Avoid-System/Pathfinding-System in the future
            return false;
        }

        private void MoveToNode()
        {
            if (WaypointQueue.Count > 0){
                Me.Update();
                Vector3 initialPosition = Me.pos;
                Vector3 targetPosition = WaypointQueue.Peek();

                // Check if we are close enough to the node to dequeue it, otherwise move to it
                if (Utils.GetDistance(initialPosition, targetPosition)
                    > AmeisenSettings.Instance.Settings.followDistance)
                {
                    //Vector3 posToGoTo = RebaseVector(targetPosition, 0);
                    AmeisenCore.MovePlayerToXYZ(targetPosition, InteractionType.MOVE);
                }
                else
                {
                    WaypointQueue.Dequeue();
                }
            }
        }

        /// <summary> Modify our go-to-position by a small factor to provide "naturality" </summary>
        /// <param name="targetPos">pos you want to go to/param> <param name="offset">distance to
        /// keep to the target</param> <returns>modified position</returns>
        private Vector3 RebaseVector(Vector3 targetPos, int offset)
        {
            Random rnd = new Random();
            float factorX = rnd.Next((offset / 4) * -1, offset / 2);
            float factorY = rnd.Next((offset / 4) * -1, offset / 2);
            return new Vector3(targetPos.X + factorX, targetPos.Y + factorY, targetPos.Z);
        }
    }
}