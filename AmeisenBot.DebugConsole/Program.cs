using AmeisenBotMapping.objects;
using AmeisenBotUtilities;
using AmeisenPathLib;
using AmeisenPathLib.objects;
using System;
using System.Collections.Generic;

namespace AmeisenBot.DebugConsole
{
    internal class Program
    {
        /// <summary>
        /// This thing is used for cuntions testing etc. it will get removed some day
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            while (true)
            {
                int count = 0;
                Console.Clear();

                int ticks = Environment.TickCount;
                List<Node> nodeList = FindWayToNode(new Vector3(5, 5, 0), new Vector3(17, 8, 0));
                ticks = Environment.TickCount - ticks;
                Console.WriteLine($"Calculation used {ticks} ticks...");

                foreach (Node n in nodeList)
                {
                    Console.WriteLine($"{count}: {n.Position.X},{n.Position.Y},{n.Position.Z}");
                    count++;
                }

                Console.ReadKey();
            }
        }

        private static List<Node> FindWayToNode(Vector3 initialPosition, Vector3 targetPosition)
        {
            List<MapNode> nodes = new List<MapNode> {
                new MapNode(new Vector3(5,5,0),0,0),
                new MapNode(new Vector3(6,6,0),0,0),
                new MapNode(new Vector3(7,7,0),0,0),
                new MapNode(new Vector3(8,8,0),0,0),
                new MapNode(new Vector3(9,9,0),0,0),
                new MapNode(new Vector3(10,10,0),0,0),
                new MapNode(new Vector3(11,10,0),0,0),
                new MapNode(new Vector3(12,10,0),0,0),
                new MapNode(new Vector3(13,10,0),0,0),
                new MapNode(new Vector3(14,10,0),0,0),
                new MapNode(new Vector3(14,9,0),0,0),
                new MapNode(new Vector3(14,8,0),0,0),
                new MapNode(new Vector3(14,7,0),0,0),
                new MapNode(new Vector3(14,6,0),0,0),
                new MapNode(new Vector3(14,5,0),0,0),
                new MapNode(new Vector3(15,5,0),0,0),
                new MapNode(new Vector3(16,5,0),0,0),
                new MapNode(new Vector3(17,5,0),0,0),
                new MapNode(new Vector3(17,6,0),0,0),
                new MapNode(new Vector3(17,7,0),0,0),
                new MapNode(new Vector3(17,8,0),0,0),
            };

            int distance = (int)Utils.GetDistance(initialPosition, targetPosition);

            int maxX = (int)initialPosition.X + (distance * 2);
            int minX = (int)initialPosition.X - (distance * 2);

            int maxY = (int)initialPosition.Y + (distance * 2);
            int minY = (int)initialPosition.Y - (distance * 2);

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

            /*for (int y = 0; y <= maxY; y++)
            {
                for (int x = 0; x <= maxX; x++)
                {
                    if (map[x, y].IsBlocked)
                    {
                        Console.ResetColor();
                        Console.Write("x ");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("+ ");
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine();*/

            return AmeisenPath.FindPathAStar(map,
                                             new NodePosition((int)initialPosition.X, (int)initialPosition.Y, (int)initialPosition.Z),
                                             new NodePosition((int)targetPosition.X, (int)targetPosition.Y, (int)targetPosition.Z));
        }
    }
}
