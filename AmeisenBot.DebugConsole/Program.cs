using AmeisenBotMapping.objects;
using AmeisenBotUtilities;
using AmeisenPathLib;
using AmeisenPathLib.objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AmeisenBot.DebugConsole
{
    internal class Program
    {
        private static int posX, posY;
        private static List<MapNode> nodes;

        /// <summary>
        /// This thing is used for cuntions testing etc. it will get removed some day
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            Console.Title = "AmeisenBot - DebugConsole";
            int nodeCount;

            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"NODECOUNT: ");
                Console.ForegroundColor = ConsoleColor.Red;
                try
                {
                    nodeCount = Convert.ToInt32(Console.ReadLine());
                }
                catch { nodeCount = 100; }

                posX = 0;
                posY = 0;
                int count = 0;

                Random rnd = new Random();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\nCreating Path with {nodeCount} Nodes...");

                nodes = new List<MapNode> { new MapNode(new Vector3(posX, posY, 0), 0, 0) };

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                for (int i = 0; i < nodeCount; i++)
                {
                    posX += rnd.Next(0, 2);
                    posY += rnd.Next(0, 2);

                    nodes.Add(new MapNode(new Vector3(posX, posY, 0), 0, 0));
                }
                stopwatch.Stop();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nPath Creation used {stopwatch.ElapsedMilliseconds}ms...");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Calculating Path from [{0},{0},{0}] to [{posX},{posY},{0}]...");

                stopwatch = new Stopwatch();
                stopwatch.Start();

                List<Node> nodeList = FindWayToNode(new Vector3(0, 0, 0), new Vector3(posX, posY, 0));

                stopwatch.Stop();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nCalculation used {stopwatch.ElapsedMilliseconds}ms...");

                if (nodeList == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No path found...");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Path found...");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    foreach (Node n in nodeList)
                    {
                        //Console.WriteLine($"{count}: {n.Position.X},{n.Position.Y},{n.Position.Z}");
                        count++;
                    }
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Press key to continue...");
                Console.ReadKey();
            }
        }

        private static List<Node> FindWayToNode(Vector3 initialPosition, Vector3 targetPosition)
        {
            int distance = (int)Utils.GetDistance(initialPosition, targetPosition);

            int maxX = (int)targetPosition.X;
            int minX = (int)initialPosition.X;

            int maxY = (int)targetPosition.Y;
            int minY = (int)initialPosition.Y;

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Distance {distance}, MaxX {maxX}, MinX {minX}, MaxY {maxY}, MinY {minY}");

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
                        Console.Write("x");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("+");
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
