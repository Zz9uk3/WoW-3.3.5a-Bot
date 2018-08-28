using AmeisenMapping.objects;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AmeisenMapping
{
    public class Map
    {
        public List<MapNode> Nodes { get; private set; }
        public List<MapPath> Paths { get; private set; }

        public Map(MapNode initialNode)
        {
            Nodes = new List<MapNode> { initialNode };
            Paths = new List<MapPath>();
        }

        public void AddOrUpdateNode(MapNode node)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                MapNode n = Nodes[i];
                if (n.X == node.X && n.Y == node.Y)
                {
                    // Update Node
                    Nodes[i] = node;
                    return;
                }
            }
            // Node is not present
            Nodes.Add(node);
        }

        public static void DrawMap(Map map, Canvas canvas)
        {
            // Needs to be reworked
            /*canvas.Children.Clear();

            int count = 0;
            foreach (MapNode mapNode in map.Nodes)
            {
                foreach (MapPath linkedPath in mapNode.LinkedNodes)
                {
                    MapNode linkedMapNode = linkedPath.Node;

                    Color pathColor = Interpolate(new Color[] { Colors.IndianRed, Colors.Yellow, Colors.LimeGreen }, ((double)linkedPath.PathQuality) / 100.0);
                    DrawLine(linkedMapNode.X + 12, linkedMapNode.Y + 12, mapNode.X + 12, mapNode.Y + 12, 4, pathColor, canvas);
                }

                DrawRectangle(mapNode.X - 4, mapNode.Y, 24, 24, Colors.LightGray, canvas);
                DrawText(mapNode.X, mapNode.Y, count.ToString(), Colors.Black, canvas);
                count++;
            }*/
        }

        private static void DrawText(double x, double y, string text, Color color, Canvas canvas)
        {
            TextBlock textBlock = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(color)
            };
            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            canvas.Children.Add(textBlock);
        }

        private static void DrawRectangle(int x, int y, int width, int height, Color color, Canvas canvas)
        {
            Rectangle rect = new Rectangle
            {
                Width = width,
                Height = height
            };

            rect.Fill = new SolidColorBrush(color);

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);

            canvas.Children.Add(rect);
        }

        private static void DrawLine(int startX, int startY, int endX, int endY, int thickness, Color color, Canvas canvas)
        {
            Line line = new Line
            {
                Stroke = new SolidColorBrush(color),
                StrokeThickness = thickness,
                X1 = startX,
                X2 = endX,
                Y1 = startY,
                Y2 = endY,
            };

            canvas.Children.Add(line);
        }

        public static Color Interpolate(Color[] colors, double factor)
        {
            double r = 0.0, g = 0.0, b = 0.0;
            double total = 0.0;
            double step = 1.0 / (double)(colors.Length - 1);
            double mu = 0.0;
            double sigma_2 = 0.035;

            foreach (Color color in colors)
            {
                total += Math.Exp(-(factor - mu) * (factor - mu) / (2.0 * sigma_2)) / Math.Sqrt(2.0 * Math.PI * sigma_2);
                mu += step;
            }

            mu = 0.0;
            foreach (Color color in colors)
            {
                double percent = Math.Exp(-(factor - mu) * (factor - mu) / (2.0 * sigma_2)) / Math.Sqrt(2.0 * Math.PI * sigma_2);
                mu += step;

                r += color.R * percent / total;
                g += color.G * percent / total;
                b += color.B * percent / total;
            }

            return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
        }
    }
}