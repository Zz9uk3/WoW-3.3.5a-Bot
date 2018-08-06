using AmeisenCore;
using AmeisenCore.Objects;
using AmeisenLogging;
using AmeisenUtilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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
        private bool loginAutoIsPossible = false;

        private readonly string configPath = AppDomain.CurrentDomain.BaseDirectory + "/credentials/";
        private readonly string extension = ".json";
        private readonly string autoLoginExe = AppDomain.CurrentDomain.BaseDirectory + "/WoW-LoginAutomator.exe";

        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists(autoLoginExe))
            {
                GetAllAcoounts();
                loadingForm.Height = 150;
                loginAutoIsPossible = true;
            }
            else
                loadingForm.Height = 58;
        }

        private void LoadingForm_Loaded(object sender, RoutedEventArgs e)
        {
            AmeisenLogger.GetInstance().SetActiveLogLevel(LogLevel.DEBUG);
            AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Loaded MainWindow", this);
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
            AmeisenLogger.GetInstance().StopLogging();
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            if (((WoWExe)comboBoxWoWs.SelectedItem) != null)
            {
                if (((WoWExe)comboBoxWoWs.SelectedItem).characterName == "")
                    MessageBox.Show("Please login first!", "Warning");
                else
                {
                    AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Selected WoW: " + ((WoWExe)comboBoxWoWs.SelectedItem).ToString(), this);

                    // Attach to WoW
                    AmeisenManager.GetInstance().AttachManager(((WoWExe)comboBoxWoWs.SelectedItem).process);
                    // Load the config for specific charactername
                    // May need to add another factor like the REALMNAME to it to make it unique...
                    AmeisenSettings.GetInstance().LoadFromFile(((WoWExe)comboBoxWoWs.SelectedItem).characterName);

                    // Apply our colors defined in the config file
                    Application.Current.Resources["AccentColor"] = (Color)ColorConverter.ConvertFromString(AmeisenSettings.GetInstance().settings.accentColor);
                    Application.Current.Resources["BackgroundColor"] = (Color)ColorConverter.ConvertFromString(AmeisenSettings.GetInstance().settings.backgroundColor);
                    Application.Current.Resources["TextColor"] = (Color)ColorConverter.ConvertFromString(AmeisenSettings.GetInstance().settings.fontColor);

                    AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Loaded colors ["
                        + Application.Current.Resources["AccentColor"] + "]["
                        + Application.Current.Resources["BackgroundColor"] + "]["
                        + Application.Current.Resources["TextColor"] + "]"
                        , this);

                    // Show the Mainscreen
                    new MainscreenForm((WoWExe)comboBoxWoWs.SelectedItem).Show();
                    Close();
                }
            }
            else
            {
#if DEBUG
                WoWExe debugExe = new WoWExe
                {
                    characterName = "DEBUG",
                    process = null
                };

                new MainscreenForm(debugExe).Show();
                Close();
#endif
            }
        }

        private void SearchForWoW()
        {
            AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Searching for WoW's", this);

            comboBoxWoWs.Items.Clear();
            List<WoWExe> wowList = AmeisenCore.AmeisenCore.GetRunningWoWs();

            foreach (WoWExe w in wowList)
            {
                comboBoxWoWs.Items.Add(w);
                comboBoxWoWs.SelectedItem = w;
            }
        }

        private void ComboBoxWoWs_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (comboBoxWoWs.SelectedItem != null)
                LoadAccount(((WoWExe)comboBoxWoWs.SelectedItem).characterName.ToLower());
        }

        private void ComboBoxAccounts_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (comboBoxAccounts.SelectedItem != null)
                LoadAccount(comboBoxAccounts.SelectedValue.ToString().ToLower());
        }

        private void LoadAccount(string accountName)
        {
            if (loginAutoIsPossible)
            {
                if (!Directory.Exists(configPath))
                    Directory.CreateDirectory(configPath);

                string path = configPath + accountName + extension;
                Credentials credentials;

                textboxCharactername.Text = accountName;

                if (File.Exists(path))
                {
                    credentials = Newtonsoft.Json.JsonConvert.DeserializeObject<Credentials>(File.ReadAllText(path));

                    textboxUsername.Text = credentials.username;
                    textboxPassword.Password = credentials.password;
                    textboxCharSlot.Text = credentials.charSlot.ToString();
                }
            }
        }

        private void ButtonGoAuto_Click(object sender, RoutedEventArgs e)
        {
            Credentials credentials;

            if (loginAutoIsPossible)
            {
                if (!Directory.Exists(configPath))
                    Directory.CreateDirectory(configPath);

                string path = configPath + textboxCharactername.Text.ToLower() + extension;

                credentials.username = textboxUsername.Text;
                credentials.password = textboxPassword.Password;
                credentials.charSlot = Convert.ToInt32(textboxCharSlot.Text);

                if (checkboxSave.IsChecked == true)
                    File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(credentials));

                Process p = Process.Start(autoLoginExe, ((WoWExe)comboBoxWoWs.SelectedItem).process.Id.ToString() + " " + credentials.charSlot + " " + credentials.username + " " + credentials.password);
                p.WaitForExit();
            }
        }

        private void OnlyNumberTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void GetAllAcoounts()
        {
            if (!Directory.Exists(configPath))
                Directory.CreateDirectory(configPath);

            foreach (string f in Directory.GetFiles(configPath))
                comboBoxAccounts.Items.Add(Path.GetFileNameWithoutExtension(f));
        }
    }
}
