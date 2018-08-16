using AmeisenBotLib;
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
using System.Threading;
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
        private AmeisenBotManager BotManager { get; }
        private bool loginAutoIsPossible = false;

        private readonly string configPath = AppDomain.CurrentDomain.BaseDirectory + "/credentials/";
        private readonly string extension = ".json";
        private readonly string autoLoginExe = AppDomain.CurrentDomain.BaseDirectory + "/WoW-LoginAutomator.exe";

        public MainWindow()
        {
            InitializeComponent();
            BotManager = AmeisenBotManager.GetInstance();

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
                    
                    // Apply our colors defined in the config file
                    Application.Current.Resources["AccentColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.accentColor);
                    Application.Current.Resources["BackgroundColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.backgroundColor);
                    Application.Current.Resources["TextColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.fontColor);

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
        }

        private void SearchForWoW(string selectByCharname = "")
        {
            AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Searching for WoW's", this);

            comboBoxWoWs.Items.Clear();
            List<WoWExe> wowList = BotManager.RunningWoWs;

            foreach (WoWExe w in wowList)
                comboBoxWoWs.Items.Add(w);

            if (selectByCharname != "")
            {
                foreach (WoWExe i in comboBoxWoWs.Items)
                    if (i.characterName == selectByCharname)
                        comboBoxWoWs.SelectedItem = i;
            }
            else if (comboBoxWoWs.Items.Count > 0)
            {
                if ((WoWExe)comboBoxWoWs.SelectedItem == null
                || ((WoWExe)comboBoxWoWs.SelectedItem).characterName == ""
                || ((WoWExe)comboBoxWoWs.SelectedItem).process == null)
                    comboBoxWoWs.SelectedItem = comboBoxWoWs.Items[0];
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

                    textboxCharactername.Text = credentials.charname;
                    textboxUsername.Text = credentials.username;
                    textboxPassword.Password = credentials.password;
                    textboxCharSlot.Text = credentials.charSlot.ToString();
                }
            }
        }

        private void ButtonGoAuto_Click(object sender, RoutedEventArgs e)
        {
            Credentials credentials;

            if (loginAutoIsPossible && ((WoWExe)comboBoxWoWs.SelectedItem).characterName == "")
            {
                if (!Directory.Exists(configPath))
                    Directory.CreateDirectory(configPath);

                string path = configPath + textboxCharactername.Text.ToLower() + extension;

                credentials.charname = textboxCharactername.Text;
                credentials.username = textboxUsername.Text;
                credentials.password = textboxPassword.Password;
                credentials.charSlot = Convert.ToInt32(textboxCharSlot.Text);

                if (checkboxSave.IsChecked == true)
                    File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(credentials));

                string charname = textboxCharactername.Text;
                Process p = Process.Start(autoLoginExe, ((WoWExe)comboBoxWoWs.SelectedItem).process.Id.ToString() + " " + credentials.charSlot + " " + credentials.username + " " + credentials.password);
                StartAutoLogin(p, charname);
                buttonGo.IsEnabled = false;
            }
        }

        private void StartAutoLogin(Process p, string charname)
        {
            p.WaitForExit();
            ((WoWExe)comboBoxWoWs.SelectedItem).characterName = charname;
            ButtonGo_Click(this, null);
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

            comboBoxAccounts.Items.Clear();

            foreach (string f in Directory.GetFiles(configPath))
                if (f.Length > 0)
                    comboBoxAccounts.Items.Add(Path.GetFileNameWithoutExtension(f));
        }
    }
}
