using System;
using System.Net;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using AmeisenBotLib;
using AmeisenLogging;
using AmeisenCore.Objects;
using AmeisenOffsets.Objects;
using AmeisenUtilities;

namespace AmeisenBotGUI
{
    /// <summary>
    /// Interaktionslogik für mainscreenForm.xaml
    /// </summary>
    public partial class MainscreenForm : Window
    {
        private AmeisenBotManager BotManager { get; }
        private DispatcherTimer uiUpdateTimer;

        public MainscreenForm(WoWExe wowExe)
        {
            InitializeComponent();
            BotManager = AmeisenBotManager.GetInstance();

            // Load Settings
            BotManager.LoadSettingsFromFile(wowExe.characterName);
            BotManager.StartBot(wowExe);
        }

        // -- Window state stuff
        // Minimize, Exit, FileDialogs
        #region WindowStuff
        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
            BotManager.StopBot();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
                AmeisenBotManager.GetInstance().LoadCombatCLass(openFileDialog.FileName);
        }
        #endregion

        // -- Window Callbacks
        // Loading, Closing, MouseDown
        #region WindowCallbacks
        private void Mainscreen_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Mainscreen_Loaded(object sender, RoutedEventArgs e)
        {
            AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Loaded MainScreen", this);

            Title = "AmeisenBot - " + BotManager.GetWowExe().characterName + " [" + BotManager.GetWowExe().process.Id + "]";
            UpdateUI();

            uiUpdateTimer = new DispatcherTimer();
            uiUpdateTimer.Tick += new EventHandler(UIUpdateTimer_Tick);
            uiUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, BotManager.Settings.dataRefreshRate);
            uiUpdateTimer.Start();
            AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Started UI-Update-Timer", this);

            checkBoxAssistPartyAttack.IsChecked = BotManager.Settings.behaviourAttack;
            checkBoxAssistPartyTank.IsChecked = BotManager.Settings.behaviourTank;
            checkBoxAssistPartyHeal.IsChecked = BotManager.Settings.behaviourHeal;
            checkBoxFollowMaster.IsChecked = BotManager.Settings.followMaster;
        }

        private void Mainscreen_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BotManager.Settings.behaviourAttack = (bool)checkBoxAssistPartyAttack.IsChecked;
            BotManager.Settings.behaviourTank = (bool)checkBoxAssistPartyTank.IsChecked;
            BotManager.Settings.behaviourHeal = (bool)checkBoxAssistPartyHeal.IsChecked;
            BotManager.Settings.followMaster = (bool)checkBoxFollowMaster.IsChecked;
            BotManager.SaveSettingsToFile(BotManager.GetLoadedConfigName());
        }
        #endregion

        // -- Bot Combat STATES
        // TANK, HEAL, ATTACK, checkboxes
        #region BotCombatStates
        private void CheckBoxAssistPartyTank_Click(object sender, RoutedEventArgs e)
        {
            BotManager.IsSupposedToTank = (bool)checkBoxAssistPartyTank.IsChecked;
        }

        private void CheckBoxAssistPartyHeal_Click(object sender, RoutedEventArgs e)
        {
            BotManager.IsSupposedToAttack = (bool)checkBoxAssistPartyAttack.IsChecked;
        }

        private void CheckBoxAssistPartyAttack_Click(object sender, RoutedEventArgs e)
        {
            BotManager.IsSupposedToAttack = (bool)checkBoxAssistPartyAttack.IsChecked;
        }
        #endregion

        // -- Debug stuff goes here, will be removed in the future
        // Debug stuff, buttons
        #region DebugStuff
        private void ButtonMoveToTarget_Click(object sender, RoutedEventArgs e)
        {
            BotManager.AddActionToAIQueue(new AmeisenAction(AmeisenActionType.MOVE_TO_POSITION, null));
        }

        private void ButtonMoveInteractTarget_Click(object sender, RoutedEventArgs e)
        {
            BotManager.AddActionToAIQueue(new AmeisenAction(AmeisenActionType.INTERACT_TARGET, (AmeisenActionType)comboboxInteraction.SelectedItem));
        }
        #endregion

        // -- External Windows
        // SettingsWindow, DebugUI, CombatClass Editor
        #region ExternalWindows
        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow().ShowDialog();
        }

        private void ButtonExtendedDebugUI_Click(object sender, RoutedEventArgs e)
        {
            new DebugUI().Show();
        }

        private void ButtonCobatClassEditor_Click(object sender, RoutedEventArgs e)
        {
            new CombatClassEditor().Show();
        }
        #endregion

        // -- UI TIMER
        // Update the GUI
        #region UITimer
        private void UIUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (BotManager.IsBotIngame())
                UpdateUI();
        }

        /// <summary>
        /// This thing updates the UI...
        /// Note to myself: "may need to improve this thing in the future..."
        /// </summary>
        private void UpdateUI()
        {
            // TODO: find a better way to update this
            //AmeisenManager.GetInstance().GetObjects();

            labelLoadedCombatClass.Content = "CombatClass: " + Path.GetFileName(BotManager.Settings.combatClassPath);

            if (BotManager.Me != null)
            {
                try
                {
                    BotManager.Me.Update();

                    labelName.Content = BotManager.Me.Name + " lvl." + BotManager.Me.level;
                    //labelCasting.Content = "Casting: " + me.currentState;

                    //labelHP.Content = "HP [" + me.health + "/" + me.maxHealth + "]";
                    progressBarHP.Maximum = BotManager.Me.maxHealth;
                    progressBarHP.Value = BotManager.Me.health;

                    //labelEnergy.Content = "Energy [" + me.energy + "/" + me.maxEnergy + "]";
                    progressBarEnergy.Maximum = BotManager.Me.maxEnergy;
                    progressBarEnergy.Value = BotManager.Me.energy;

                    //labelXP.Content = "XP [" + me.exp + "/" + me.maxExp + "]";
                    progressBarXP.Maximum = BotManager.Me.maxExp;
                    progressBarXP.Value = BotManager.Me.exp;

                    /*labelPosition.Content =
                        "X: " + me.pos.x +
                        "\nY: " + me.pos.y +
                        "\nZ: " + me.pos.z +
                        "\nR: " + me.rotation;*/
                }
                catch (Exception e)
                {
                    AmeisenLogger.GetInstance().Log(LogLevel.ERROR, e.ToString(), this);
                }
                if (BotManager.Me.target != null)
                    try
                    {
                        labelNameTarget.Content = BotManager.Me.target.Name + " lvl." + BotManager.Me.target.level;
                        //labelCastingTarget.Content = "Current state: " + me.target.currentState;

                        //labelHPTarget.Content = "HP [" + me.target.health + "/" + me.target.maxHealth + "]";
                        progressBarHPTarget.Maximum = BotManager.Me.target.maxHealth;
                        progressBarHPTarget.Value = BotManager.Me.target.health;

                        //labelEnergyTarget.Content = "Energy [" + me.target.energy + "/" + me.target.maxEnergy + "]";
                        progressBarEnergyTarget.Maximum = BotManager.Me.target.maxEnergy;
                        progressBarEnergyTarget.Value = BotManager.Me.target.energy;

                        labelTargetDistance.Content = "Distance: " + BotManager.Me.target.Distance + "m";

                        /*labelPositionTarget.Content =
                            "X: " + me.target.pos.x +
                            "\nY: " + me.target.pos.y +
                            "\nZ: " + me.target.pos.z +
                            "\nR: " + me.target.rotation;*/
                    }
                    catch (Exception e)
                    {
                        AmeisenLogger.GetInstance().Log(LogLevel.ERROR, e.ToString(), this);
                    }
            }

            try
            {
                //labelThreadsActive.Content = "⚡ Threads: " + AmeisenAIManager.GetInstance().GetBusyThreadCount() + "/" + AmeisenAIManager.GetInstance().GetActiveThreadCount();
                //progressBarBusyAIThreads.Maximum = AmeisenAIManager.GetInstance().GetActiveThreadCount();
                //progressBarBusyAIThreads.Value = AmeisenAIManager.GetInstance().GetBusyThreadCount();

                //listboxCurrentQueue.Items.Clear();
                //foreach (AmeisenAction a in AmeisenAIManager.GetInstance().GetQueueItems())
                //listboxCurrentQueue.Items.Add(a.GetActionType() + " [" + a.GetActionParams() + "]");
            }
            catch (Exception e)
            {
                AmeisenLogger.GetInstance().Log(LogLevel.ERROR, e.ToString(), this);
            }
        }
        #endregion

    }
}
