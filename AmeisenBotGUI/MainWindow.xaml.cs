using AmeisenCore;
using AmeisenCore.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AmeisenBotGUI
{
    /// <summary>
    /// UInt646464eraktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        [DllImport("kernel32")]
        static extern bool AllocConsole();

        public MainWindow()
        {
            InitializeComponent();
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

        private void buttonGo_Click(object sender, RoutedEventArgs e)
        {
            AmeisenManager.GetInstance().AttachManager(((WoWExe)comboBoxWoWs.SelectedItem).process);

            new mainscreenForm((WoWExe)comboBoxWoWs.SelectedItem).Show();
            Close();
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            SearchForWoW();
        }

        private void loadingForm_Loaded(object sender, RoutedEventArgs e)
        {
            SearchForWoW();
        }

        private void loadingForm_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
