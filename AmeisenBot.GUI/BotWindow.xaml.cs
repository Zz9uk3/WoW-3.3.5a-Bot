using AmeisenBotLogger;
using AmeisenBotManager;
using AmeisenBotUtilities;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AmeisenBotGUI
{
    /// <summary>
    /// Interaktionslogik für mainscreenForm.xaml 💕
    /// </summary>
    public partial class BotWindow : Window
    {
        private string lastImgPath;
        private DispatcherTimer uiUpdateTimer;
        private BotManager BotManager { get; }

        public BotWindow(WowExe wowExe, BotManager botManager)
        {
            InitializeComponent();
            BotManager = botManager;

            // Load Settings
            BotManager.LoadSettingsFromFile(wowExe.characterName);
            ApplyConfigColors();
            BotManager.StartBot(wowExe);
        }

        private void ApplyConfigColors()
        {
            Application.Current.Resources["AccentColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.accentColor);
            Application.Current.Resources["BackgroundColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.backgroundColor);
            Application.Current.Resources["TextColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.textColor);

            Application.Current.Resources["MeNodeColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.meNodeColor);
            Application.Current.Resources["WalkableNodeColorLow"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.walkableNodeColorLow);
            Application.Current.Resources["WalkableNodeColorHigh"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.walkableNodeColorHigh);

            Application.Current.Resources["HealthColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.healthColor);
            Application.Current.Resources["EnergyColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.energyColor);
            Application.Current.Resources["ExpColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.expColor);
            Application.Current.Resources["TargetHealthColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.targetHealthColor);
            Application.Current.Resources["TargetEnergyColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.targetEnergyColor);
            Application.Current.Resources["ThreadsColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.threadsColor);
        }

        private void ButtonCobatClassEditor_Click(object sender, RoutedEventArgs e)
        {
            // Going to be reworked
            //new CombatClassWindow(BotManager).Show();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
            BotManager.StopBot();
        }

        private void ButtonExtendedDebugUI_Click(object sender, RoutedEventArgs e)
        {
            new DebugWindow(BotManager).Show();
        }

        private void ButtonGroup_Click(object sender, RoutedEventArgs e)
        {
            new GroupWindow(BotManager).Show();
        }

        private void ButtonMap_Click(object sender, RoutedEventArgs e)
        {
            new MapWindow(BotManager, BotManager.AmeisenDBManager).Show();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                AddExtension = true,
                RestoreDirectory = true,
                Filter = "CombatClass *.cs|*.cs"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                BotManager.LoadCombatClassFromFile(openFileDialog.FileName);
            }
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow(BotManager).ShowDialog();
        }

        private void CheckBoxAssistGroup_Click(object sender, RoutedEventArgs e)
        {
            BotManager.IsAllowedToAssistParty = (bool)checkBoxAssistGroup.IsChecked;
        }

        private void CheckBoxAssistPartyAttack_Click(object sender, RoutedEventArgs e)
        {
            BotManager.IsAllowedToAttack = (bool)checkBoxAssistPartyAttack.IsChecked;
        }

        private void CheckBoxAssistPartyBuff_Click(object sender, RoutedEventArgs e)
        {
            BotManager.IsAllowedToBuff = (bool)checkBoxAssistPartyBuff.IsChecked;
        }

        private void CheckBoxAssistPartyHeal_Click(object sender, RoutedEventArgs e)
        {
            BotManager.IsAllowedToHeal = (bool)checkBoxAssistPartyAttack.IsChecked;
        }

        private void CheckBoxAssistPartyTank_Click(object sender, RoutedEventArgs e)
        {
            BotManager.IsAllowedToTank = (bool)checkBoxAssistPartyTank.IsChecked;
        }

        private void CheckBoxFollowMaster_Click(object sender, RoutedEventArgs e)
        {
            BotManager.IsAllowedToFollowParty = (bool)checkBoxFollowParty.IsChecked;
        }

        private void CheckBoxTopMost_Click(object sender, RoutedEventArgs e)
        {
            SetTopMost();
        }

        private void LoadViewSettings()
        {
            checkBoxAssistPartyAttack.IsChecked = BotManager.Settings.behaviourAttack;
            BotManager.IsAllowedToAttack = BotManager.Settings.behaviourAttack;

            checkBoxAssistPartyTank.IsChecked = BotManager.Settings.behaviourTank;
            BotManager.IsAllowedToTank = BotManager.Settings.behaviourTank;

            checkBoxAssistPartyHeal.IsChecked = BotManager.Settings.behaviourHeal;
            BotManager.IsAllowedToHeal = BotManager.Settings.behaviourHeal;

            checkBoxAssistPartyBuff.IsChecked = BotManager.Settings.behaviourBuff;
            BotManager.IsAllowedToBuff = BotManager.Settings.behaviourBuff;

            checkBoxAssistGroup.IsChecked = BotManager.Settings.assistParty;
            BotManager.IsAllowedToAssistParty = BotManager.Settings.assistParty;

            checkBoxFollowParty.IsChecked = BotManager.Settings.followMaster;
            BotManager.IsAllowedToFollowParty = BotManager.Settings.followMaster;

            checkBoxReleaseSpirit.IsChecked = BotManager.Settings.releaseSpirit;
            BotManager.IsAllowedToReleaseSpirit = BotManager.Settings.releaseSpirit;

            checkBoxRevive.IsChecked = BotManager.Settings.revive;
            BotManager.IsAllowedToRevive = BotManager.Settings.revive;

            sliderDistance.Value = BotManager.Settings.followDistance;

            checkBoxTopMost.IsChecked = BotManager.Settings.topMost;
            Topmost = BotManager.Settings.topMost;
        }

        private void Mainscreen_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveViewSettings();
        }

        private void Mainscreen_Loaded(object sender, RoutedEventArgs e)
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Loaded MainScreen", this);

            Title = $"AmeisenBot - {BotManager.WowExe.characterName} [{BotManager.WowExe.process.Id}]";

            UpdateUI();
            StartUIUpdateTime();

            LoadViewSettings();
        }

        private void Mainscreen_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch { }
        }

        private void SaveViewSettings()
        {
            BotManager.Settings.behaviourAttack = (bool)checkBoxAssistPartyAttack.IsChecked;
            BotManager.Settings.behaviourTank = (bool)checkBoxAssistPartyTank.IsChecked;
            BotManager.Settings.behaviourHeal = (bool)checkBoxAssistPartyHeal.IsChecked;
            BotManager.Settings.behaviourBuff = (bool)checkBoxAssistPartyBuff.IsChecked;
            BotManager.Settings.followMaster = (bool)checkBoxFollowParty.IsChecked;
            BotManager.Settings.releaseSpirit = (bool)checkBoxReleaseSpirit.IsChecked;
            BotManager.Settings.revive = (bool)checkBoxRevive.IsChecked;
            BotManager.SaveSettingsToFile(BotManager.LoadedConfigName);
        }

        private void SetTopMost()
        {
            Topmost = (bool)checkBoxTopMost.IsChecked;
            BotManager.Settings.topMost = (bool)checkBoxTopMost.IsChecked;
        }

        private void SliderDistance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                labelDistance.Content = $"Follow Distance: {Math.Round(sliderDistance.Value, 2)}m";
                BotManager.Settings.followDistance = Math.Round(sliderDistance.Value, 2);
            }
            catch { }
        }

        private void StartUIUpdateTime()
        {
            uiUpdateTimer = new DispatcherTimer();
            uiUpdateTimer.Tick += new EventHandler(UIUpdateTimer_Tick);
            uiUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, BotManager.Settings.dataRefreshRate);
            uiUpdateTimer.Start();
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Started UI-Update-Timer", this);
        }

        private void UIUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (BotManager.IsIngame)
            {
                UpdateUI();
            }
        }

        private void UpdateAIView()
        {
        }

        private void UpdateMyViews()
        {
            try
            {
                if (BotManager.Settings.picturePath != lastImgPath)
                    if (BotManager.Settings.picturePath.Length > 0)
                    {
                        botPicture.Source = new BitmapImage(new Uri(BotManager.Settings.picturePath));
                        lastImgPath = BotManager.Settings.picturePath;
                    }
            }
            catch { AmeisenLogger.Instance.Log(LogLevel.ERROR, "Failed to load picture...", this); }

            labelName.Content = BotManager.Me.Name + " lvl." + BotManager.Me.Level;

            labelHP.Content = $"Health {BotManager.Me.Health} / {BotManager.Me.MaxHealth}";
            progressBarHP.Maximum = BotManager.Me.MaxHealth;
            progressBarHP.Value = BotManager.Me.Health;

            labelEnergy.Content = $"Energy {BotManager.Me.Energy} / {BotManager.Me.MaxEnergy}";
            progressBarEnergy.Maximum = BotManager.Me.MaxEnergy;
            progressBarEnergy.Value = BotManager.Me.Energy;

            labelExp.Content = $"Exp {BotManager.Me.Exp} / {BotManager.Me.MaxExp}";
            progressBarXP.Maximum = BotManager.Me.MaxExp;
            progressBarXP.Value = BotManager.Me.Exp;
        }

        private void UpdateTargetViews()
        {
            labelNameTarget.Content = $"{BotManager.Target.Name} lvl.{BotManager.Target.Level}";

            labelTargetHP.Content = $"Health {BotManager.Target.Health} / {BotManager.Target.MaxHealth}";
            progressBarHPTarget.Maximum = BotManager.Target.MaxHealth;
            progressBarHPTarget.Value = BotManager.Target.Health;

            labelTargetEnergy.Content = $"Energy {BotManager.Target.Energy} / {BotManager.Target.MaxEnergy}";
            progressBarEnergyTarget.Maximum = BotManager.Target.MaxEnergy;
            progressBarEnergyTarget.Value = BotManager.Target.Energy;

            labelTargetDistance.Content = $"Distance: {Math.Round(BotManager.Target.Distance, 2)}m";
        }

        private void UpdateFSMViews()
        {
            labelFSMState.Content = $"FSM state: {BotManager.CurrentFSMState}";
        }

        /// <summary>
        /// This thing updates the UI... Note to myself: "may need to improve this thing in the future..."
        /// </summary>
        private void UpdateUI()
        {
            // TODO: find a better way to update this
            //AmeisenManager.Instance.GetObjects();

            labelLoadedCombatClass.Content = $"CombatClass: {Path.GetFileName(BotManager.Settings.combatClassPath)}";

            if (BotManager.Me != null)
            {
                try
                {
                    UpdateFSMViews();
                    UpdateMyViews();

                    if (BotManager.Target != null)
                    {
                        UpdateTargetViews();
                    }
                }
                catch (Exception e)
                {
                    AmeisenLogger.Instance.Log(LogLevel.ERROR, e.ToString(), this);
                }
            }

            UpdateAIView();
        }

        private void CheckBoxReleaseSpirit_Click(object sender, RoutedEventArgs e)
        {
            BotManager.IsAllowedToReleaseSpirit = (bool)checkBoxReleaseSpirit.IsChecked;
        }

        private void CheckBoxReleaseSpirit_Copy_Click(object sender, RoutedEventArgs e)
        {
            BotManager.IsAllowedToRevive = (bool)checkBoxRevive.IsChecked;
        }
    }
}