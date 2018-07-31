using AmeisenCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AmeisenBotGUI
{
    /// <summary>
    /// Interaktionslogik für SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            switch (AmeisenSettings.GetInstance().settings.dataRefreshRate)
            {
                case 1000:
                    radiobuttonRefreshSpeedLowest.IsChecked = true;
                    break;

                case 500:
                    radiobuttonRefreshSpeedLow.IsChecked = true;
                    break;

                case 250:
                    radiobuttonRefreshSpeedMedium.IsChecked = true;
                    break;

                case 100:
                    radiobuttonRefreshSpeedHigh.IsChecked = true;
                    break;

                case 0:
                    radiobuttonRefreshSpeedHighest.IsChecked = true;
                    break;

                default:
                    break;
            }

            switch (AmeisenSettings.GetInstance().settings.botMaxThreads)
            {
                case 1:
                    radiobuttonIntLowest.IsChecked = true;
                    break;

                case 2:
                    radiobuttonIntLow.IsChecked = true;
                    break;

                case 3:
                    radiobuttonIntMedium.IsChecked = true;
                    break;

                case 4:
                    radiobuttonIntHigh.IsChecked = true;
                    break;

                case 8:
                    radiobuttonIntHighest.IsChecked = true;
                    break;

                default:
                    break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ((bool)radiobuttonRefreshSpeedLowest.IsChecked)
                AmeisenSettings.GetInstance().settings.dataRefreshRate = 1000;
            else if ((bool)radiobuttonRefreshSpeedLow.IsChecked)
                AmeisenSettings.GetInstance().settings.dataRefreshRate = 500;
            else if ((bool)radiobuttonRefreshSpeedMedium.IsChecked)
                AmeisenSettings.GetInstance().settings.dataRefreshRate = 250;
            else if ((bool)radiobuttonRefreshSpeedHigh.IsChecked)
                AmeisenSettings.GetInstance().settings.dataRefreshRate = 100;
            else if ((bool)radiobuttonRefreshSpeedHighest.IsChecked)
                AmeisenSettings.GetInstance().settings.dataRefreshRate = 0;
            else
            {
                //something is wrong...
            }

            if ((bool)radiobuttonIntLowest.IsChecked)
                AmeisenSettings.GetInstance().settings.botMaxThreads = 1;
            else if ((bool)radiobuttonIntLow.IsChecked)
                AmeisenSettings.GetInstance().settings.botMaxThreads = 2;
            else if ((bool)radiobuttonIntMedium.IsChecked)
                AmeisenSettings.GetInstance().settings.botMaxThreads = 3;
            else if ((bool)radiobuttonIntHigh.IsChecked)
                AmeisenSettings.GetInstance().settings.botMaxThreads = 4;
            else if ((bool)radiobuttonIntHighest.IsChecked)
                AmeisenSettings.GetInstance().settings.botMaxThreads = 8;
            else
            {
                //something is wrong...
            }

            AmeisenSettings.GetInstance().SaveToFile(AmeisenSettings.GetInstance().loadedconfName);
        }
    }
}
