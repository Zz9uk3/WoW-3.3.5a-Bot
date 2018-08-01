using AmeisenCore;
using AmeisenCore.Objects;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AmeisenBotGUI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // May used in the future, this isn't working ATM
        [DllImport("kernel32")]
        static extern bool AllocConsole();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadingForm_Loaded(object sender, RoutedEventArgs e)
        {
            SearchForWoW();
        }

        private void LoadingForm_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            SearchForWoW();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            if (((WoWExe)comboBoxWoWs.SelectedItem) != null)
            {
                // Attach to WoW
                AmeisenManager.GetInstance().AttachManager(((WoWExe)comboBoxWoWs.SelectedItem).process);
                // Load the config for specific charactername
                // May need to add another factor like the REALMNAME to it to make it unique...
                AmeisenSettings.GetInstance().LoadFromFile(((WoWExe)comboBoxWoWs.SelectedItem).characterName);

                // Apply our colors defined in the config file
                Application.Current.Resources["AccentColor"] = (Color)ColorConverter.ConvertFromString(AmeisenSettings.GetInstance().settings.accentColor);
                Application.Current.Resources["BackgroundColor"] = (Color)ColorConverter.ConvertFromString(AmeisenSettings.GetInstance().settings.backgroundColor);
                Application.Current.Resources["TextColor"] = (Color)ColorConverter.ConvertFromString(AmeisenSettings.GetInstance().settings.fontColor);

                // Show the Mainscreen
                new mainscreenForm((WoWExe)comboBoxWoWs.SelectedItem).Show();
                Close();
            }
            else
            {
#if DEBUG
                WoWExe debugExe = new WoWExe();
                debugExe.characterName = "DEBUG";
                debugExe.process = null;

                new mainscreenForm(debugExe).Show();
                Close();
#endif
            }
        }

        private void SearchForWoW()
        {
            comboBoxWoWs.Items.Clear();
            List<WoWExe> wowList = AmeisenCore.AmeisenCore.GetRunningWoWs();

            foreach (WoWExe w in wowList)
            {
                comboBoxWoWs.Items.Add(w);
                comboBoxWoWs.SelectedItem = w;
            }
        }
    }
}
