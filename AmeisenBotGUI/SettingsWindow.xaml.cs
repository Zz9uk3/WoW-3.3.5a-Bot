using AmeisenBotLib;
using AmeisenCore;
using System.Windows;
using System.Windows.Input;

namespace AmeisenBotGUI
{
    /// <summary>
    /// Interaktionslogik für SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private AmeisenBotManager BotManager { get; }

        public SettingsWindow()
        {
            InitializeComponent();
            BotManager = AmeisenBotManager.GetInstance();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch { }
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            switch (BotManager.Settings.dataRefreshRate)
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

            switch (BotManager.Settings.botMaxThreads)
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
                BotManager.Settings.dataRefreshRate = 1000;
            else if ((bool)radiobuttonRefreshSpeedLow.IsChecked)
                BotManager.Settings.dataRefreshRate = 500;
            else if ((bool)radiobuttonRefreshSpeedMedium.IsChecked)
                BotManager.Settings.dataRefreshRate = 250;
            else if ((bool)radiobuttonRefreshSpeedHigh.IsChecked)
                BotManager.Settings.dataRefreshRate = 100;
            else if ((bool)radiobuttonRefreshSpeedHighest.IsChecked)
                BotManager.Settings.dataRefreshRate = 0;
            else
            {
                //something is wrong...
            }

            if ((bool)radiobuttonIntLowest.IsChecked)
                BotManager.Settings.botMaxThreads = 1;
            else if ((bool)radiobuttonIntLow.IsChecked)
                BotManager.Settings.botMaxThreads = 2;
            else if ((bool)radiobuttonIntMedium.IsChecked)
                BotManager.Settings.botMaxThreads = 3;
            else if ((bool)radiobuttonIntHigh.IsChecked)
                BotManager.Settings.botMaxThreads = 4;
            else if ((bool)radiobuttonIntHighest.IsChecked)
                BotManager.Settings.botMaxThreads = 8;
            else
            {
                //something is wrong...
            }

            BotManager.SaveSettingsToFile(BotManager.GetLoadedConfigName());
        }
    }
}
