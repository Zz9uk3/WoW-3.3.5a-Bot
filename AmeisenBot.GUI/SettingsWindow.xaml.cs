using AmeisenBotManager;
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
        private BotManager BotManager { get; }

        public SettingsWindow(BotManager botManager)
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
                Directory.CreateDirectory(configDir);

            if (!Directory.Exists(imageDir))
                Directory.CreateDirectory(imageDir);

            string imagePath = $"{AppDomain.CurrentDomain.BaseDirectory}config\\\\img\\\\{Path.GetFileName(fileName)}";

            if (imagePath != fileName)
            {
                if (File.Exists(imagePath))
                    File.Delete(imagePath);
                File.Copy(fileName, imagePath);
            }

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
            BotManager.Settings.healthColor = ((Color)Application.Current.Resources["HealthColor"]).ToString();
            BotManager.Settings.energyColor = ((Color)Application.Current.Resources["EnergyColor"]).ToString();
            BotManager.Settings.expColor = ((Color)Application.Current.Resources["ExpColor"]).ToString();
            BotManager.Settings.targetHealthColor = ((Color)Application.Current.Resources["TargetHealthColor"]).ToString();
            BotManager.Settings.targetEnergyColor = ((Color)Application.Current.Resources["TargetEnergyColor"]).ToString();
            BotManager.Settings.threadsColor = ((Color)Application.Current.Resources["ThreadsColor"]).ToString();
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

            BotManager.SaveSettingsToFile(BotManager.LoadedConfigName);
        }

        private void SelectColor(string resourceColor)
        {
            ColorPickWindow colorpicker = new ColorPickWindow((Color)Application.Current.Resources[resourceColor])
            {
                Topmost = BotManager.Settings.topMost
            };
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

        private void ColorBackground_Click(object sender, RoutedEventArgs e)
        {
            SelectColor("BackgroundColor");
        }

        private void ColorOutline_Click(object sender, RoutedEventArgs e)
        {
            SelectColor("AccentColor");
        }

        private void ColorText_Click(object sender, RoutedEventArgs e)
        {
            SelectColor("TextColor");
        }

        private void ColorHealth_Click(object sender, RoutedEventArgs e)
        {
            SelectColor("HealthColor");
        }

        private void ColorEnergy_Click(object sender, RoutedEventArgs e)
        {
            SelectColor("EnergyColor");
        }

        private void ColorExp_Click(object sender, RoutedEventArgs e)
        {
            SelectColor("EXPColor");
        }

        private void ColorTargetHealth_Click(object sender, RoutedEventArgs e)
        {
            SelectColor("TargetHealthColor");
        }

        private void ColorTargetEnergy_Click(object sender, RoutedEventArgs e)
        {
            SelectColor("TargetEnergyColor");
        }

        private void ColorThreads_Click(object sender, RoutedEventArgs e)
        {
            SelectColor("ThreadsColor");
        }
    }
}