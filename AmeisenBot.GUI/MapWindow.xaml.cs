using AmeisenBotDB;
using AmeisenBotManager;
using AmeisenBotMapping;
using AmeisenBotMapping.objects;
using AmeisenBotUtilities;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AmeisenBotGUI
{
    /// <summary>
    /// Interaktionslogik für Map.xaml
    /// </summary>
    public partial class MapWindow : Window
    {
        private Map currentMap;
        private DispatcherTimer dbUpdateTimer;
        private int newX;
        private int newY;
        private DispatcherTimer uiUpdateTimer;
        private BotManager BotManager { get; set; }
        private AmeisenDBManager AmeisenDBManager { get; set; }

        public MapWindow(BotManager botManager, AmeisenDBManager ameisenDBManager)
        {
            InitializeComponent();
            BotManager = botManager;
            AmeisenDBManager = ameisenDBManager;
            Topmost = BotManager.Settings.topMost;
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

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void DBUpdateTimer_Tick(object sender, EventArgs e)
        {
            LoadMap();
        }

        private void DrawMap(Map map)
        {
            mapCanvas.Children.Clear();
            Vector3 myPos = BotManager.Me.pos;
            Vector3 myCanvasMiddle = new Vector3
            {
                X = Width / 2,
                Y = Height / 2
            };

            foreach (MapNode node in map.Nodes)
            {
                Vector3 tempPos = NodePosToCanvasPos(new Vector3(node.X, node.Y, node.Z), myPos);
                newX = (int)(myCanvasMiddle.X + tempPos.X);
                newY = (int)(myCanvasMiddle.Y + tempPos.Y);

                Color nodeColor = Utils.InterpolateColors(new Color[]{
                            (Color)Application.Current.Resources["WalkableNodeColorLow"],
                            (Color)Application.Current.Resources["WalkableNodeColorHigh"],
                            }, tempPos.Z / 100.0);

                DrawRectangle(newX - 2, newY - 2, 4, 4, nodeColor, mapCanvas);
            }

            foreach (ulong guid in BotManager.Me.PartymemberGuids)
            {
                if (guid == 0)
                    continue;

                string unitName = "";
                Vector3 position = new Vector3(0, 0, 0);
                foreach (WowObject p in BotManager.ActiveWoWObjects)
                    if (p.Guid == guid)
                    {
                        p.Update();
                        position = p.pos;
                        unitName = p.Name;
                    }

                if (Utils.GetDistance(BotManager.Me.pos, position) < 200)
                {
                    Vector3 tempPos = NodePosToCanvasPos(new Vector3(position.X, position.Y, position.Z), myPos);
                    newX = (int)(myCanvasMiddle.X + tempPos.X);
                    newY = (int)(myCanvasMiddle.Y + tempPos.Y);
                    DrawRectangle(newX - 2, newY - 2, 4, 4, Colors.Orange, mapCanvas);
                    DrawText(newX + 6, newY - 8, unitName, Colors.White, mapCanvas);
                }
            }

            DrawRectangle((int)myCanvasMiddle.X, (int)myCanvasMiddle.Y, 4, 4,
                (Color)Application.Current.Resources["MeNodeColor"],
                mapCanvas);
            DrawText((int)myCanvasMiddle.X + 6, (int)myCanvasMiddle.Y - 6, "Me", Colors.White, mapCanvas);
        }

        private void LoadMap()
        {
            Vector3 myPos = BotManager.Me.pos;
            List<MapNode> nodelist = AmeisenDBManager.GetNodes(
                BotManager.ZoneID,
                BotManager.MapID,
                (int)(myPos.X + ((Width / 2) - 20)), // Get the max drawing point x
                (int)(myPos.X - ((Width / 2) + 20)), // Get the min drawing point x
                (int)(myPos.Y + ((Height / 2) - 20)), // Get the max drawing point y
                (int)(myPos.Y - ((Height / 2) + 20)) // Get the min drawing point y
                );

            if (nodelist.Count > 0)
                currentMap = new Map(nodelist);
        }

        private Vector3 NodePosToCanvasPos(Vector3 canvasPos, Vector3 myPos)
        {
            canvasPos.X -= myPos.X;
            canvasPos.Y -= myPos.Y;
            return canvasPos;
        }

        private void StartDatabaseUpdateTimer()
        {
            dbUpdateTimer = new DispatcherTimer();
            dbUpdateTimer.Tick += new EventHandler(DBUpdateTimer_Tick);
            dbUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, BotManager.Settings.dataRefreshRate * 10);
            dbUpdateTimer.Start();
        }

        private void StartUIUpdateTimer()
        {
            uiUpdateTimer = new DispatcherTimer();
            uiUpdateTimer.Tick += new EventHandler(UIUpdateTimer_Tick);
            uiUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 2000);
            uiUpdateTimer.Start();
        }

        private void UIUpdateTimer_Tick(object sender, EventArgs e)
        {
            DrawMap(currentMap);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            uiUpdateTimer.Stop();
            dbUpdateTimer.Stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            currentMap = new Map(new List<MapNode>());
            // Refresh UI, my position, odes in current map
            StartUIUpdateTimer();
            // refresh the nodes from our database, this will be called each 10th time we update our
            // UI to save database performance
            StartDatabaseUpdateTimer();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try { DragMove(); } catch { }
        }
    }
}