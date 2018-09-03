using AmeisenLogging;
using AmeisenManager;
using AmeisenUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using WoWLoginAutomator;

namespace AmeisenBotGUI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class SelectWindow : Window
    {
        public SelectWindow()
        {
            InitializeComponent();

            // Get our BotManager to interact with our bot
            BotManager = AmeisenBotManager.Instance;

            // currently disabled
            //CheckForAutologin();
            GetAllAccounts();
        }

        private void CheckForAutologin()
        {
            if (File.Exists(autoLoginExe))
            {
                GetAllAccounts();
                loadingForm.Height = 150;
                autologinIsPossible = true;
            }
            else
                loadingForm.Height = 58;
        }

        private readonly string autoLoginExe = AppDomain.CurrentDomain.BaseDirectory + "/WoWLoginAutomator.exe";
        private readonly string configPath = AppDomain.CurrentDomain.BaseDirectory + "/credentials/";
        private readonly string extension = ".json";
        private bool autologinIsPossible = true;
        private AmeisenBotManager BotManager { get; }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
            AmeisenLogger.Instance.StopLogging();
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            DoLogin();
        }

        private void DoLogin()
        {
            WowExe selectedExe = ((WowExe)comboBoxWoWs.SelectedItem);

            if (selectedExe?.characterName == "")
                MessageBox.Show("Please login first!", "Warning");
            else
            {
                AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"Selected WoW: {((WowExe)comboBoxWoWs.SelectedItem).ToString()}", this);

                // Show the Mainscreen
                new BotWindow((WowExe)comboBoxWoWs?.SelectedItem).Show();
                Close();
            }
        }

        private void ButtonGoAuto_Click(object sender, RoutedEventArgs e)
        {
            DoAutoLogin();
        }

        private void DoAutoLogin()
        {
            WowExe selectedExe = ((WowExe)comboBoxWoWs.SelectedItem);
            Credentials credentials;

            if (autologinIsPossible
                && (selectedExe.characterName == ""))
            {
                if (!Directory.Exists(configPath))
                    Directory.CreateDirectory(configPath);

                string path = $"{configPath}{textboxCharactername.Text.ToLower()}{extension}";

                credentials.charname = textboxCharactername.Text;
                credentials.username = textboxUsername.Text;
                credentials.password = textboxPassword.Password;
                credentials.charSlot = Convert.ToInt32(textboxCharSlot.Text);

                if (checkboxSave.IsChecked == true || credentials.charname.Length > 0)
                    File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(credentials));

                string charname = textboxCharactername.Text;

                // Do the autologin
                LoginAutomator.DoLogin(
                    selectedExe.process.Id,
                    credentials.charSlot,
                    credentials.username,
                    credentials.password);

                selectedExe.characterName = charname;
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

        /// <summary>
        /// Add all available account-configs to the combobox
        /// </summary>
        private void GetAllAccounts()
        {
            if (!Directory.Exists(configPath))
                Directory.CreateDirectory(configPath);

            comboBoxAccounts.Items.Clear();

            foreach (string f in Directory.GetFiles(configPath))
                if (f.Length > 0)
                {
                    AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"Adding Account: {Utils.FirstCharToUpper(f)} [{f.Length}]", this);
                    comboBoxAccounts.Items.Add(Utils.FirstCharToUpper(Path.GetFileNameWithoutExtension(f)));
                }
            comboBoxAccounts.SelectedIndex = 0;
        }

        /// <summary>
        /// Load account data from a config file in the /config folder
        /// </summary>
        /// <param name="accountName">name of the account to load</param>
        private void LoadAccount(string accountName)
        {
            if (autologinIsPossible)
            {
                if (!Directory.Exists(configPath))
                    Directory.CreateDirectory(configPath);

                string path = configPath + accountName + extension;
                Credentials credentials;

                textboxCharactername.Text = Utils.FirstCharToUpper(accountName);

                credentials = LoadCredentialsFromFile(path);
            }
        }

        /// <summary>
        /// Load Credentials from file
        /// </summary>
        /// <param name="path">path to credentials file</param>
        /// <returns>loaded Credentials</returns>
        private Credentials LoadCredentialsFromFile(string path)
        {
            Credentials credentials = new Credentials();
            if (File.Exists(path))
            {
                credentials = Newtonsoft.Json.JsonConvert.DeserializeObject<Credentials>(File.ReadAllText(path));

                textboxCharactername.Text = Utils.FirstCharToUpper(credentials.charname);
                textboxUsername.Text = credentials.username;
                textboxPassword.Password = credentials.password;
                textboxCharSlot.Text = credentials.charSlot.ToString();
            }
            return credentials;
        }

        private void LoadingForm_Loaded(object sender, RoutedEventArgs e)
        {
            // Set active logging level
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

        /// <summary>
        /// Regex to only allow numeric input in given Textbox
        /// </summary>
        private void OnlyNumberTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        /// <summary>
        /// Search for active WoW instances, add them to the combobox and
        /// select a char by its name if given, else select the first entry
        /// </summary>
        /// <param name="selectByCharname">set, if you want to select an entry by its charactername</param>
        private void SearchForWoW(string selectByCharname = "")
        {
            WowExe selectedExe = ((WowExe)comboBoxWoWs.SelectedItem);
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Searching for WoW's", this);

            // Update combobox
            comboBoxWoWs.Items.Clear();
            List<WowExe> wowList = BotManager.RunningWoWs;
            foreach (WowExe w in wowList)
                comboBoxWoWs.Items.Add(w);

            // if we should select a char, go for it
            // if not, select first
            if (selectByCharname != "")
            {
                foreach (WowExe i in comboBoxWoWs.Items)
                    if (i.characterName == selectByCharname)
                        comboBoxWoWs.SelectedItem = i;
            }
            else if (comboBoxWoWs.Items.Count > 0)
            {
                // If there is nothing selected, select the first object
                if (selectedExe == null
                || selectedExe.characterName == ""
                || selectedExe.process == null)
                    comboBoxWoWs.SelectedItem = comboBoxWoWs.Items[0];
            }
        }
    }
}