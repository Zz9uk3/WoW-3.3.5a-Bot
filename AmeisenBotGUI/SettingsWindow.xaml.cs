using AmeisenManager;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace AmeisenBotGUI
{
    /// <summary>
    /// Interaktionslogik für SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        #region Public Constructors

        public SettingsWindow()
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

        private void ButtonSelectPicture_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                AddExtension = true,
                RestoreDirectory = true,
                Filter = "Images|*.png;*.jpg;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string configDir = AppDomain.CurrentDomain.BaseDirectory + "config/";
                string imageDir = AppDomain.CurrentDomain.BaseDirectory + "config/img/";

                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }

                if (!Directory.Exists(imageDir))
                {
                    Directory.CreateDirectory(imageDir);
                }

                string imagePath = AppDomain.CurrentDomain.BaseDirectory + "config/img/" + openFileDialog.SafeFileName;

                File.Move(openFileDialog.FileName, imagePath);

                BotManager.Settings.picturePath = imagePath;
                labelSelectedPicture.Content = openFileDialog.SafeFileName;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ((bool)radiobuttonRefreshSpeedLowest.IsChecked)
            {
                BotManager.Settings.dataRefreshRate = 1000;
            }
            else if ((bool)radiobuttonRefreshSpeedLow.IsChecked)
            {
                BotManager.Settings.dataRefreshRate = 500;
            }
            else if ((bool)radiobuttonRefreshSpeedMedium.IsChecked)
            {
                BotManager.Settings.dataRefreshRate = 250;
            }
            else if ((bool)radiobuttonRefreshSpeedHigh.IsChecked)
            {
                BotManager.Settings.dataRefreshRate = 100;
            }
            else if ((bool)radiobuttonRefreshSpeedHighest.IsChecked)
            {
                BotManager.Settings.dataRefreshRate = 0;
            }
            else
            {
                //something is wrong...
            }

            if ((bool)radiobuttonIntLowest.IsChecked)
            {
                BotManager.Settings.botMaxThreads = 1;
            }
            else if ((bool)radiobuttonIntLow.IsChecked)
            {
                BotManager.Settings.botMaxThreads = 2;
            }
            else if ((bool)radiobuttonIntMedium.IsChecked)
            {
                BotManager.Settings.botMaxThreads = 3;
            }
            else if ((bool)radiobuttonIntHigh.IsChecked)
            {
                BotManager.Settings.botMaxThreads = 4;
            }
            else if ((bool)radiobuttonIntHighest.IsChecked)
            {
                BotManager.Settings.botMaxThreads = 8;
            }
            else
            {
                //something is wrong...
            }

            BotManager.Settings.serverIP = textboxIP.Text;
            BotManager.Settings.serverPort = Convert.ToInt32(textboxPort.Text);
            BotManager.Settings.autoConnect = (bool)checkboxAutoConnect.IsChecked;

            BotManager.SaveSettingsToFile(BotManager.GetLoadedConfigName());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            labelSelectedPicture.Content = Path.GetFileName(BotManager.Settings.picturePath);

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

            textboxIP.Text = BotManager.Settings.serverIP;
            textboxPort.Text = BotManager.Settings.serverPort.ToString();
            checkboxAutoConnect.IsChecked = (bool)BotManager.Settings.autoConnect;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch { }
        }

        #endregion Private Methods
    }
}