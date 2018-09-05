using AmeisenLogging;
using AmeisenManager;
using AmeisenUtilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AmeisenBotGUI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class SelectWindow : Window
    {
        #region Private Fields

        private readonly string autoLoginExe = AppDomain.CurrentDomain.BaseDirectory + "/WoWLoginAutomator.exe";
        private readonly string configPath = AppDomain.CurrentDomain.BaseDirectory + "/credentials/";
        private readonly string extension = ".json";
        private bool autologinIsPossible = false;

        #endregion Private Fields

        #region Public Constructors

        public SelectWindow()
        {
            InitializeComponent();
            BotManager = AmeisenBotManager.Instance;

            if (File.Exists(autoLoginExe))
            {
                GetAllAcoounts();
                loadingForm.Height = 150;
                autologinIsPossible = true;
            }
            else
                loadingForm.Height = 58;
        }

        #endregion Public Constructors

        #region Private Properties

        private AmeisenBotManager BotManager { get; }

        #endregion Private Properties

        #region Private Methods

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
            AmeisenLogger.Instance.StopLogging();
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            if (((WowExe)comboBoxWoWs.SelectedItem) != null)
            {
                if (((WowExe)comboBoxWoWs.SelectedItem).characterName == "")
                    MessageBox.Show("Please login first!", "Warning");
                else
                {
                    AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Selected WoW: " + ((WowExe)comboBoxWoWs.SelectedItem).ToString(), this);

                    // Apply our colors defined in the config file
                    Application.Current.Resources["AccentColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.accentColor);
                    Application.Current.Resources["BackgroundColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.backgroundColor);
                    Application.Current.Resources["TextColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.textColor);

                    Application.Current.Resources["WalkableNodeColorLow"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.walkableNodeColorLow);
                    Application.Current.Resources["WalkableNodeColorHigh"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.walkableNodeColorHigh);
                    Application.Current.Resources["MeNodeColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.meNodeColor);

                    // Show the Mainscreen
                    new BotWindow((WowExe)comboBoxWoWs.SelectedItem).Show();
                    Close();
                }
            }
        }

        private void ButtonGoAuto_Click(object sender, RoutedEventArgs e)
        {
            Credentials credentials;

            if (autologinIsPossible && ((WowExe)comboBoxWoWs.SelectedItem).characterName == "")
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

                WoWLoginAutomator.LoginAutomator.DoLogin(((WowExe)comboBoxWoWs.SelectedItem).process.Id, credentials.charSlot, credentials.username, credentials.password);

                ((WowExe)comboBoxWoWs.SelectedItem).characterName = charname;
                ButtonGo_Click(this, null);
            }
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            SearchForWoW();
        }

        private void ComboBoxAccounts_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (comboBoxAccounts.SelectedItem != null)
                LoadAccount(comboBoxAccounts.SelectedValue.ToString().ToLower());
        }

        private void ComboBoxWoWs_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (comboBoxWoWs.SelectedItem != null)
                LoadAccount(((WowExe)comboBoxWoWs.SelectedItem).characterName.ToLower());
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

        private void LoadAccount(string accountName)
        {
            if (autologinIsPossible)
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

        private void LoadingForm_Loaded(object sender, RoutedEventArgs e)
        {
            AmeisenLogger.Instance.SetActiveLogLevel(LogLevel.DEBUG);
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Loaded MainWindow", this);
            SearchForWoW();
        }

        private void LoadingForm_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch { }
        }

        private void OnlyNumberTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void SearchForWoW(string selectByCharname = "")
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Searching for WoW's", this);

            comboBoxWoWs.Items.Clear();
            List<WowExe> wowList = BotManager.RunningWoWs;

            foreach (WowExe w in wowList)
                comboBoxWoWs.Items.Add(w);

            if (selectByCharname != "")
            {
                foreach (WowExe i in comboBoxWoWs.Items)
                    if (i.characterName == selectByCharname)
                        comboBoxWoWs.SelectedItem = i;
            }
            else if (comboBoxWoWs.Items.Count > 0)
            {
                if ((WowExe)comboBoxWoWs.SelectedItem == null
                || ((WowExe)comboBoxWoWs.SelectedItem).characterName == ""
                || ((WowExe)comboBoxWoWs.SelectedItem).process == null)
                    comboBoxWoWs.SelectedItem = comboBoxWoWs.Items[0];
            }
        }

        #endregion Private Methods
    }
}