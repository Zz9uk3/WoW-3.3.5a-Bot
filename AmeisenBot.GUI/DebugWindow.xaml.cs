using AmeisenBotManager;
using AmeisenBotUtilities;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace AmeisenBotGUI
{
    /// <summary>
    /// Interaktionslogik für DebugUI.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        private BotManager BotManager { get; }

        public DebugWindow(BotManager botManager)
        {
            InitializeComponent();
            BotManager = botManager;
            Topmost = BotManager.Settings.topMost;
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void DebugUI_Loaded(object sender, RoutedEventArgs e)
        {
            StartUIUpdatTimer();
        }

        private void DebugUI_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ListboxObjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listboxObjects.SelectedValue != null)
            {
                textboxSelectedItem.Text = ((DataItem)listboxObjects.SelectedValue).Text;
            }
        }

        private void ObjectUpdateTimer_Tick(object sender, EventArgs e)
        {
            listboxObjects.Items.Clear();
            if (BotManager.ActiveWoWObjects != null)
            {
                foreach (WowObject obj in BotManager.ActiveWoWObjects)
                {
                    if (obj == null)
                    {
                        break;
                    }

                    if (obj.GetType() == typeof(WowObject) && checkboxFilterWOWOBJECT.IsChecked == true)
                    {
                        listboxObjects.Items.Add(new DataItem(obj.ToString(), new SolidColorBrush((Color)Application.Current.Resources["WoWObjectColor"])));
                    }
                    else if (obj.GetType() == typeof(GameObject) && checkboxFilterGAMEOBJECT.IsChecked == true)
                    {
                        listboxObjects.Items.Add(new DataItem(obj.ToString(), new SolidColorBrush((Color)Application.Current.Resources["GameObjectColor"])));
                    }
                    else if (obj.GetType() == typeof(DynObject) && checkboxFilterDYNOBJECT.IsChecked == true)
                    {
                        listboxObjects.Items.Add(new DataItem(obj.ToString(), new SolidColorBrush((Color)Application.Current.Resources["DynObjectColor"])));
                    }
                    else if (obj.GetType() == typeof(Container) && checkboxFilterCONTAINER.IsChecked == true)
                    {
                        listboxObjects.Items.Add(new DataItem(obj.ToString(), new SolidColorBrush((Color)Application.Current.Resources["ContainerColor"])));
                    }
                    else if (obj.GetType() == typeof(Corpse) && checkboxFilterCORPSE.IsChecked == true)
                    {
                        listboxObjects.Items.Add(new DataItem(obj.ToString(), new SolidColorBrush((Color)Application.Current.Resources["CorpseColor"])));
                    }
                    else if (obj.GetType() == typeof(Unit) && checkboxFilterUNIT.IsChecked == true)
                    {
                        listboxObjects.Items.Add(new DataItem(obj.ToString(), new SolidColorBrush((Color)Application.Current.Resources["UnitColor"])));
                    }
                    else if (obj.GetType() == typeof(Player) && checkboxFilterPLAYER.IsChecked == true)
                    {
                        listboxObjects.Items.Add(new DataItem(obj.ToString(), new SolidColorBrush((Color)Application.Current.Resources["PlayerColor"])));
                    }
                    else if (obj.GetType() == typeof(Me) && checkboxFilterME.IsChecked == true)
                    {
                        listboxObjects.Items.Add(new DataItem(obj.ToString(), new SolidColorBrush((Color)Application.Current.Resources["MeColor"])));
                    }
                }
            }
        }

        private void StartUIUpdatTimer()
        {
            DispatcherTimer uiUpdateTimer = new DispatcherTimer();
            uiUpdateTimer.Tick += new EventHandler(ObjectUpdateTimer_Tick);
            uiUpdateTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            uiUpdateTimer.Start();
        }
    }

    /// <summary>
    /// Display class for the listView elements. Used to color them corresponding to their WoWObject type.
    /// </summary>
    internal class DataItem
    {
        public Brush Background { get; set; }

        public string Text { get; set; }

        public DataItem(string text, Brush background)
        {
            Text = text;
            Background = background;
        }
    }
}