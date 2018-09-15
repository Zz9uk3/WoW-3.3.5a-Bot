using AmeisenBotCore;
using AmeisenBotData;
using AmeisenBotDB;
using AmeisenBotFSM.Interfaces;
using AmeisenBotLogger;
using AmeisenBotMapping.objects;
using AmeisenBotUtilities;
using AmeisenPathLib;
using AmeisenPathLib.objects;
using System;
using System.Collections.Generic;
using System.Threading;
using static AmeisenBotFSM.Objects.Delegates;

namespace AmeisenBotFSM.Actions
{
    public class ActionMoving : IAction
    {
        public virtual Start StartAction { get { return Start; } }
        public virtual DoThings StartDoThings { get { return DoThings; } }
        public virtual Exit StartExit { get { return Stop; } }
        public Queue<Vector3> WaypointQueue { get; set; }
        public bool PathCalculated { get; set; }
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

        private static List<Node> GetNeighbours(Node[,] map, NodePosition currentPosition)
        {
            List<Node> neighbours = new List<Node>();

            int xStart = currentPosition.X > 0 ? currentPosition.X - 1 : currentPosition.X;
            int xEnd = currentPosition.X < map.GetLength(0) - 1 ? currentPosition.X + 1 : currentPosition.X;

            int yStart = currentPosition.Y > 0 ? currentPosition.Y - 1 : currentPosition.Y;
            int yEnd = currentPosition.Y < map.GetLength(1) - 1 ? currentPosition.Y + 1 : currentPosition.Y;

            for (int x = xStart; x <= xEnd; x++)
                for (int y = yStart; y <= yEnd; y++)
                    neighbours.Add(map[x, y]);

            return neighbours;
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
            double movedSinceLastTick = Utils.GetDistance(initialPosition, activePosition);

            // we are possibly stuck at a fence or something alike
            if (movedSinceLastTick != 0 && movedSinceLastTick < 1000)
                if (movedSinceLastTick < AmeisenDataHolder.Settings.MovementJumpThreshold)
                {
                    AmeisenCore.CharacterJumpAsync();
                    AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"Jumping: {movedSinceLastTick}", this);
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

                if (!PathCalculated && Utils.GetDistance(initialPosition, targetPosition) > AmeisenDataHolder.Settings.PathfindingUsageThreshold)
                {

                    WaypointQueue.Dequeue();
                    List<Node> path = FindWayToNode(initialPosition, targetPosition);

                    if (path != null)
                    {
                        PathCalculated = true;
                        AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Found path: " + path.ToString(), this);
                        foreach (Node node in path)
                        {
                            WaypointQueue.Enqueue(new Vector3(node.Position.X, node.Position.Y, node.Position.Z));
                        }
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
                else
                {
                    CheckIfWeAreStuckIfYesJump(targetPosition, LastPosition);
                    AmeisenCore.MovePlayerToXYZ(targetPosition, InteractionType.MOVE);
                    LastPosition = WaypointQueue.Dequeue();
                    Thread.Sleep(100);
                }
            }
            else { PathCalculated = false; }
        }

        private List<Node> SimplifyPath(List<Node> path)
        {
            if (path == null)
                return null;

            List<Node> simplePath = new List<Node>();
            double oldX = 0.0;
            double oldY = 0.0;

            for (int x = 1; x < path.Count; x++)
            {
                double newX = path[x - 1].Position.X - path[x].Position.X;
                double newY = path[x - 1].Position.Y - path[x].Position.Y;

                if (newX != oldX && newY != oldY)
                    simplePath.Add(path[x]);

                oldX = newX;
                oldY = newY;
            }
            return simplePath;
        }

        private List<Node> FindWayToNode(Vector3 initialPosition, Vector3 targetPosition)
        {
            Console.WriteLine("Finding Path...");

            int offsetX = 0;
            int offsetY = 0;

            int distance = (int)Utils.GetDistance(initialPosition, targetPosition);
            int maxX = targetPosition.X >= initialPosition.X ? (int)targetPosition.X : (int)initialPosition.X;
            int minX = initialPosition.X <= targetPosition.X ? (int)initialPosition.X : (int)targetPosition.X;

            int maxY = targetPosition.Y >= initialPosition.Y ? (int)targetPosition.Y : (int)initialPosition.Y;
            int minY = initialPosition.Y <= targetPosition.Y ? (int)initialPosition.Y : (int)targetPosition.Y;

            AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"Trying to find path from {initialPosition.X},{initialPosition.Y},{initialPosition.Z} to: {targetPosition.X},{targetPosition.Y},{targetPosition.Z} Distance: {distance}", this);

            offsetX = minX * -1;
            minX += offsetX;
            maxX += offsetX;

            offsetY = minY * -1;
            minY += offsetY;
            maxY += offsetY;

            List<MapNode> nodes = AmeisenDBManager.GetNodes(
                Me.ZoneID,
                Me.MapID,
                maxX - offsetX,
                minX - offsetX,
                maxY - offsetY,
                minY - offsetY);

            if (nodes.Count < 1)
                return null;

            Node[,] map = new Node[maxX + 1, maxY + 1];

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    map[x, y] = new Node(new NodePosition(x, y, 0), true);
                }
            }

            foreach (MapNode node in nodes)
            {
                map[node.X + offsetX, node.Y + offsetY] = new Node(new NodePosition(node.X + offsetX, node.Y + offsetY, node.Z), false);
            }

            map = ThinkenPathsOnMap(map, maxX, maxY);

            List<Node> path = AmeisenPath.FindPathAStar(map,
                                             new NodePosition((int)initialPosition.X + offsetX, (int)initialPosition.Y + offsetY, (int)initialPosition.Z),
                                             new NodePosition((int)targetPosition.X + offsetX, (int)targetPosition.Y + offsetY, (int)targetPosition.Z));

            if (path == null)
                return null;

            List<Node> rebasedPath = new List<Node>();
            foreach (Node node in path)
                rebasedPath.Add(
                    new Node(
                        new NodePosition(
                            node.Position.X - offsetX,
                            node.Position.Y - offsetY,
                            node.Position.Z),
                        false));

            PathCalculated = true;

            return rebasedPath;
        }

        private Node[,] ThinkenPathsOnMap(Node[,] map, int maxX, int maxY)
        {
            Node[,] newMap = new Node[map.GetLength(0), map.GetLength(1)];

            for (int y = 0; y <= maxY; y++)
            {
                for (int x = 0; x <= maxX; x++)
                {
                    if (!map[x, y].IsBlocked)
                        foreach (Node node in GetNeighbours(map, new NodePosition(x, y)))
                        {
                            newMap[node.Position.X, node.Position.Y] = new Node(
                                new NodePosition(
                                    map[node.Position.X, node.Position.Y].Position.X,
                                    map[node.Position.X, node.Position.Y].Position.Y,
                                    map[node.Position.X, node.Position.Y].Position.Z),
                                    false);
                        }
                    else
                        newMap[x, y] = map[x, y];
                }
            }

            return newMap;
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