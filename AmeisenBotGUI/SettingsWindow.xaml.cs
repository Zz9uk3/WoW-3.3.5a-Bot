using AmeisenManager;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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
            BotManager = AmeisenBotManager.Instance;
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
                LoadBotPicture(openFileDialog.FileName);
            }
        }

        private void ColorMe_Click(object sender, RoutedEventArgs e)
        {
            SelectColor("MeNodeColor");
        }

        private void ColorWalkable_Click(object sender, RoutedEventArgs e)
        {
            SelectColor("WalkableNodeColorLow");
        }

        private void ColorWalkableNodeHigh_Click(object sender, RoutedEventArgs e)
        {
            SelectColor("WalkableNodeColorHigh");
        }

        private void LoadAmeisenServerSettings()
        {
            textboxIP.Text = BotManager.Settings.ameisenServerIP;
            textboxPort.Text = BotManager.Settings.ameisenServerPort.ToString();
            checkboxAutoConnect.IsChecked = BotManager.Settings.serverAutoConnect;
        }

        private void LoadBotPicture(string fileName)
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

            string imagePath = $"{AppDomain.CurrentDomain.BaseDirectory}config\\\\img\\\\{Path.GetFileName(fileName)}";

            if (!File.Exists(imagePath + fileName))
                File.Copy(fileName, imagePath);

            BotManager.Settings.picturePath = imagePath;
            labelSelectedPicture.Content = Path.GetFileName(fileName);
        }

        private void LoadDatabaseSettings()
        {
            textboxDBIP.Text = BotManager.Settings.databaseIP;
            textboxDBPort.Text = BotManager.Settings.databasePort.ToString();
            textboxDBDatabase.Text = BotManager.Settings.databaseName;
            textboxDBUsername.Text = BotManager.Settings.databaseUsername;
            textboxDBPassword.Password = BotManager.Settings.databasePasswort;
            checkboxDBAutoConnect.IsChecked = BotManager.Settings.databaseAutoConnect;
        }

        /// <summary>
        /// Load settings to UI
        /// </summary>
        private void LoadSettings()
        {
            labelSelectedPicture.Content = Path.GetFileName(BotManager.Settings.picturePath);

            LoadAmeisenServerSettings();
            LoadDatabaseSettings();

            // Colors are already loaded
        }

        private void SaveAmeisenServerSettings()
        {
            BotManager.Settings.ameisenServerIP = textboxIP.Text;
            BotManager.Settings.ameisenServerPort = Convert.ToInt32(textboxPort.Text);
            BotManager.Settings.serverAutoConnect = (bool)checkboxAutoConnect.IsChecked;
        }

        private void SaveDatabaseSettings()
        {
            BotManager.Settings.databaseIP = textboxDBIP.Text;
            BotManager.Settings.databasePort = Convert.ToInt32(textboxDBPort.Text);
            BotManager.Settings.databaseName = textboxDBDatabase.Text;
            BotManager.Settings.databaseUsername = textboxDBUsername.Text;
            BotManager.Settings.databasePasswort = textboxDBPassword.Password;
            BotManager.Settings.databaseAutoConnect = (bool)checkboxDBAutoConnect.IsChecked;
        }

        private void SaveMainUISettings()
        {
            BotManager.Settings.accentColor = ((Color)Application.Current.Resources["AccentColor"]).ToString();
            BotManager.Settings.backgroundColor = ((Color)Application.Current.Resources["BackgroundColor"]).ToString();
            BotManager.Settings.textColor = ((Color)Application.Current.Resources["TextColor"]).ToString();
            BotManager.Settings.healthColor = ((Color)Application.Current.Resources["healthColor"]).ToString();
            BotManager.Settings.energyColor = ((Color)Application.Current.Resources["energyColor"]).ToString();
            BotManager.Settings.expColor = ((Color)Application.Current.Resources["expColor"]).ToString();
            BotManager.Settings.targetHealthColor = ((Color)Application.Current.Resources["targetHealthColor"]).ToString();
            BotManager.Settings.targetEnergyColor = ((Color)Application.Current.Resources["targetEnergyColor"]).ToString();
            BotManager.Settings.threadsColor = ((Color)Application.Current.Resources["threadsColor"]).ToString();
        }

        private void SaveMapUISettings()
        {
            BotManager.Settings.walkableNodeColorLow = ((Color)Application.Current.Resources["WalkableNodeColorLow"]).ToString();
            BotManager.Settings.walkableNodeColorHigh = ((Color)Application.Current.Resources["WalkableNodeColorHigh"]).ToString();
            BotManager.Settings.meNodeColor = ((Color)Application.Current.Resources["MeNodeColor"]).ToString();
        }

        /// <summary>
        /// Save settings from UI
        /// </summary>
        private void SaveSettings()
        {
            SaveAmeisenServerSettings();
            SaveDatabaseSettings();
            SaveMapUISettings();
            SaveMainUISettings();

            BotManager.SaveSettingsToFile(BotManager.GetLoadedConfigName());
        }

        private void SelectColor(string resourceColor)
        {
            ColorPickWindow colorpicker = new ColorPickWindow();
            colorpicker.ShowDialog();
            if (colorpicker.ApplyColor)
                Application.Current.Resources[resourceColor] = colorpicker.ActiveColor;
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

            SaveSettings();
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

            LoadSettings();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch { }
        }
    }
}