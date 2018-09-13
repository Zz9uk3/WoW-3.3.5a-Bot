using AmeisenBotCore;
using AmeisenBotData;
using AmeisenBotDB;
using AmeisenBotFSM.Interfaces;
using AmeisenBotMapping.objects;
using AmeisenBotUtilities;
using AmeisenPathLib;
using AmeisenPathLib.objects;
using System;
using System.Collections.Generic;
using static AmeisenBotFSM.Objects.Delegates;

namespace AmeisenBotFSM.Actions
{
    public class ActionMoving : IAction
    {
        public virtual Start StartAction { get { return Start; } }
        public virtual DoThings StartDoThings { get { return DoThings; } }
        public virtual Exit StartExit { get { return Stop; } }
        public Queue<Vector3> WaypointQueue { get; set; }
        private AmeisenDataHolder AmeisenDataHolder { get; set; }
        private AmeisenDBManager AmeisenDBManager { get; set; }
        private Vector3 LastPosition { get; set; }

        private Me Me
        {
            get { return AmeisenDataHolder.Me; }
            set { AmeisenDataHolder.Me = value; }
        }

        public ActionMoving(AmeisenDataHolder ameisenDataHolder, AmeisenDBManager ameisenDBManager)
        {
            AmeisenDataHolder = ameisenDataHolder;
            AmeisenDBManager = ameisenDBManager;
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
            LastPosition = new Vector3(int.MaxValue, int.MaxValue, int.MaxValue);
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
            // we are possibly stuck at a fence or something alike
            if (Utils.GetDistance(initialPosition, activePosition) < AmeisenDataHolder.Settings.MovementJumpThreshold)
            {
                AmeisenCore.CharacterJumpAsync();
                return true;
            }
            return false;
        }

        private void MoveToNode()
        {
            if (WaypointQueue.Count > 0)
            {
                Me.Update();
                Vector3 initialPosition = Me.pos;
                Vector3 targetPosition = WaypointQueue.Peek();

                if (Utils.GetDistance(initialPosition, targetPosition) > AmeisenDataHolder.Settings.PathfindingUsageThreshold)
                {
                    WaypointQueue.Dequeue();
                    foreach (Node node in FindWayToNode(initialPosition, targetPosition))
                    {
                        WaypointQueue.Enqueue(new Vector3(node.Position.X, node.Position.Y, node.Position.Z));
                    }
                }
                else
                {
                    CheckIfWeAreStuckIfYesJump(targetPosition, LastPosition);
                    AmeisenCore.MovePlayerToXYZ(targetPosition, InteractionType.MOVE);
                    LastPosition = WaypointQueue.Dequeue();
                }
            }
        }

        private List<Node> FindWayToNode(Vector3 initialPosition, Vector3 targetPosition)
        {
            int distance = (int)Utils.GetDistance(initialPosition, targetPosition);
            int maxX = (int)initialPosition.X + (distance * AmeisenDataHolder.Settings.PathfindingSearchRadius);
            int minX = (int)initialPosition.X - (distance * AmeisenDataHolder.Settings.PathfindingSearchRadius);
            int maxY = (int)initialPosition.Y + (distance * AmeisenDataHolder.Settings.PathfindingSearchRadius);
            int minY = (int)initialPosition.Y - (distance * AmeisenDataHolder.Settings.PathfindingSearchRadius);

            List<MapNode> nodes = AmeisenDBManager.GetNodes(Me.ZoneID, Me.MapID, maxX, minX, maxY, minY);
            
            Node[,] map = new Node[maxX + 1, maxY + 1];

            for (int x = 0; x <= maxX; x++)
            {
                for (int y = 0; y <= maxY; y++)
                {
                    map[x, y] = new Node(new NodePosition(x, y, 0), true);
                }
            }

            foreach (MapNode node in nodes)
            {
                map[node.X, node.Y] = new Node(new NodePosition(node.X, node.Y, node.Z), false);
            }

            return AmeisenPath.FindPathAStar(map,
                                             new NodePosition((int)initialPosition.X, (int)initialPosition.Y, (int)initialPosition.Z),
                                             new NodePosition((int)targetPosition.X, (int)targetPosition.Y, (int)targetPosition.Z));
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