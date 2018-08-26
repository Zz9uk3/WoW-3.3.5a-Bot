using AmeisenManager;
using AmeisenUtilities;
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
    public partial class DebugUI : Window
    {
        #region Public Constructors

        public DebugUI()
        {
            InitializeComponent();
            BotManager = AmeisenBotManager.Instance;
        }

        #endregion Public Constructors

        #region Private Properties

        private AmeisenBotManager BotManager { get; }

        #endregion Private Properties

        #region Private Methods

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
            DispatcherTimer uiUpdateTimer = new DispatcherTimer();
            uiUpdateTimer.Tick += new EventHandler(ObjectUpdateTimer_Tick);
            uiUpdateTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            uiUpdateTimer.Start();
        }

        private void DebugUI_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ListboxObjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listboxObjects.SelectedValue != null)
                textboxSelectedItem.Text = ((DataItem)listboxObjects.SelectedValue).Text;
        }

        private void ObjectUpdateTimer_Tick(object sender, EventArgs e)
        {
            listboxObjects.Items.Clear();
            if (BotManager.WoWObjects != null)
                foreach (WoWObject obj in BotManager.WoWObjects)
                {
                    if (obj == null)
                        break;
                    if (obj.GetType() == typeof(WoWObject) && checkboxFilterWOWOBJECT.IsChecked == true)
                        listboxObjects.Items.Add(new DataItem(obj.ToString(), new SolidColorBrush((Color)Application.Current.Resources["WoWObjectColor"])));
                    else if (obj.GetType() == typeof(GameObject) && checkboxFilterGAMEOBJECT.IsChecked == true)
                        listboxObjects.Items.Add(new DataItem(obj.ToString(), new SolidColorBrush((Color)Application.Current.Resources["GameObjectColor"])));
                    else if (obj.GetType() == typeof(DynObject) && checkboxFilterDYNOBJECT.IsChecked == true)
                        listboxObjects.Items.Add(new DataItem(obj.ToString(), new SolidColorBrush((Color)Application.Current.Resources["DynObjectColor"])));
                    else if (obj.GetType() == typeof(Container) && checkboxFilterCONTAINER.IsChecked == true)
                        listboxObjects.Items.Add(new DataItem(obj.ToString(), new SolidColorBrush((Color)Application.Current.Resources["ContainerColor"])));
                    else if (obj.GetType() == typeof(Corpse) && checkboxFilterCORPSE.IsChecked == true)
                        listboxObjects.Items.Add(new DataItem(obj.ToString(), new SolidColorBrush((Color)Application.Current.Resources["CorpseColor"])));
                    else if (obj.GetType() == typeof(Unit) && checkboxFilterUNIT.IsChecked == true)
                        listboxObjects.Items.Add(new DataItem(obj.ToString(), new SolidColorBrush((Color)Application.Current.Resources["UnitColor"])));
                    else if (obj.GetType() == typeof(Player) && checkboxFilterPLAYER.IsChecked == true)
                        listboxObjects.Items.Add(new DataItem(obj.ToString(), new SolidColorBrush((Color)Application.Current.Resources["PlayerColor"])));
                    else if (obj.GetType() == typeof(Me) && checkboxFilterME.IsChecked == true)
                        listboxObjects.Items.Add(new DataItem(obj.ToString(), new SolidColorBrush((Color)Application.Current.Resources["MeColor"])));
                }
        }

        #endregion Private Methods
    }

    internal class DataItem
    {
        #region Public Constructors

        public DataItem(string text, Brush background)
        {
            Text = text;
            Background = background;
        }

        #endregion Public Constructors

        #region Public Properties

        public Brush Background { get; set; }
        public string Text { get; set; }

        #endregion Public Properties
    }
}