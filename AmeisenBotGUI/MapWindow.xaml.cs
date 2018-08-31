using AmeisenDB;
using AmeisenManager;
using AmeisenMapping;
using AmeisenMapping.objects;
using AmeisenUtilities;
using System;
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
        private DispatcherTimer uiUpdateTimer;
        private DispatcherTimer mapUpdateTimer;
        #region Public Constructors

        public MapWindow()
        {
            InitializeComponent();
        }

        #endregion Public Constructors

        #region Public Methods

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

        #endregion Public Methods

        #region Private Methods

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

        private void DrawMap(Map map)
        {
            mapCanvas.Children.Clear();
            Vector3 myPos = AmeisenBotManager.Instance.Me.pos;
            Vector3 myCanvasMiddle = new Vector3
            {
                X = Width / 2,
                Y = Height / 2
            };

            foreach (MapNode node in map.Nodes)
            {
                Vector3 tempPos = NodePosToCanvasPos(new Vector3(node.X, node.Y, node.Z), myPos);
                int newX = (int)(myCanvasMiddle.X + tempPos.X);
                int newY = (int)(myCanvasMiddle.Y + tempPos.Y);

                if (!(newX >= Width - 40)
                    && !(newX <= (Width - 40) * -1)
                    && !(newY >= Height - 40)
                    && !(newY <= (Height - 40) * -1))
                {
                    DrawRectangle(newX - 2, newY - 2, 4, 4, 
                    (Color)ColorConverter.ConvertFromString((string)Application.Current.Resources["WalkableNodeColor"]),
                    mapCanvas);
                }
            }

            DrawRectangle((int)myCanvasMiddle.X, (int)myCanvasMiddle.Y, 4, 4,
                (Color)ColorConverter.ConvertFromString((string)Application.Current.Resources["MeNodeColor"]),
                mapCanvas);
        }

        private Map LoadMap()
        {
            return new Map(
                       AmeisenDBManager.Instance.GetNodes(
                           AmeisenBotManager.GetZoneID(),
                           AmeisenBotManager.GetMapID()
                           )
                       );
        }

        private Vector3 NodePosToCanvasPos(Vector3 canvasPos, Vector3 myPos)
        {
            canvasPos.X -= myPos.X;
            canvasPos.Y -= myPos.Y;
            return canvasPos;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            currentMap = LoadMap();

            uiUpdateTimer = new DispatcherTimer();
            uiUpdateTimer.Tick += new EventHandler(UIUpdateTimer_Tick);
            uiUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, AmeisenBotManager.Instance.Settings.dataRefreshRate);
            uiUpdateTimer.Start();

            mapUpdateTimer = new DispatcherTimer();
            mapUpdateTimer.Tick += new EventHandler(MapUpdateTimer_Tick);
            mapUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, AmeisenBotManager.Instance.Settings.dataRefreshRate * 10);
            mapUpdateTimer.Start();
        }

        private void UIUpdateTimer_Tick(object sender, EventArgs e)
        {
            DrawMap(currentMap);
        }

        private void MapUpdateTimer_Tick(object sender, EventArgs e)
        {
            currentMap = LoadMap();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try { DragMove(); } catch { }
        }

        #endregion Private Methods

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            uiUpdateTimer.Stop();
            mapUpdateTimer.Stop();
        }
    }
}