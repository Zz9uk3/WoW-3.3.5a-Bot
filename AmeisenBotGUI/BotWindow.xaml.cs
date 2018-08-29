using AmeisenLogging;
using AmeisenManager;
using AmeisenUtilities;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace AmeisenBotGUI
{
    /// <summary>
    /// Interaktionslogik für mainscreenForm.xaml
    /// </summary>
    public partial class BotWindow : Window
    {
        #region Private Fields

        private DispatcherTimer uiUpdateTimer;

        #endregion Private Fields

        #region Public Constructors

        public BotWindow(WoWExe wowExe)
        {
            InitializeComponent();
            BotManager = AmeisenBotManager.Instance;

            // Load Settings
            BotManager.LoadSettingsFromFile(wowExe.characterName);
            BotManager.StartBot(wowExe);
        }

        #endregion Public Constructors

        #region Private Properties

        private AmeisenBotManager BotManager { get; }

        #endregion Private Properties

        // -- Window state stuff Minimize, Exit, FileDialogs

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
            {
                AmeisenBotManager.Instance.LoadCombatClass(openFileDialog.FileName);
            }
        }

        #endregion WindowStuff

        // -- Window Callbacks Loading, Closing, MouseDown

        #region WindowCallbacks

        private void Mainscreen_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BotManager.Settings.behaviourAttack = (bool)checkBoxAssistPartyAttack.IsChecked;
            BotManager.Settings.behaviourTank = (bool)checkBoxAssistPartyTank.IsChecked;
            BotManager.Settings.behaviourHeal = (bool)checkBoxAssistPartyHeal.IsChecked;
            BotManager.Settings.followMaster = (bool)checkBoxFollowMaster.IsChecked;
            BotManager.SaveSettingsToFile(BotManager.GetLoadedConfigName());
        }

        private void Mainscreen_Loaded(object sender, RoutedEventArgs e)
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Loaded MainScreen", this);

            Title = "AmeisenBot - " + BotManager.GetWowExe().characterName + " [" + BotManager.GetWowExe().process.Id + "]";
            UpdateUI();

            uiUpdateTimer = new DispatcherTimer();
            uiUpdateTimer.Tick += new EventHandler(UIUpdateTimer_Tick);
            uiUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, BotManager.Settings.dataRefreshRate);
            uiUpdateTimer.Start();
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Started UI-Update-Timer", this);

            checkBoxAssistPartyAttack.IsChecked = BotManager.Settings.behaviourAttack;
            checkBoxAssistPartyTank.IsChecked = BotManager.Settings.behaviourTank;
            checkBoxAssistPartyHeal.IsChecked = BotManager.Settings.behaviourHeal;

            checkBoxFollowMaster.IsChecked = BotManager.Settings.followMaster;
            AmeisenBotManager.Instance.FollowGroup = BotManager.Settings.followMaster;

            comboboxInteraction.Items.Add(Interaction.FACETARGET);
            comboboxInteraction.Items.Add(Interaction.FACEDESTINATION);
            comboboxInteraction.Items.Add(Interaction.STOP);
            comboboxInteraction.Items.Add(Interaction.MOVE);
            comboboxInteraction.Items.Add(Interaction.INTERACT);
            comboboxInteraction.Items.Add(Interaction.LOOT);
            comboboxInteraction.Items.Add(Interaction.INTERACTOBJECT);
            comboboxInteraction.Items.Add(Interaction.FACEOTHER);
            comboboxInteraction.Items.Add(Interaction.SKIN);
            comboboxInteraction.Items.Add(Interaction.ATTACK);
            comboboxInteraction.Items.Add(Interaction.ATTACKPOS);
            comboboxInteraction.Items.Add(Interaction.ATTACKGUID);
            comboboxInteraction.Items.Add(Interaction.WALKANDROTATE);
        }

        private void Mainscreen_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch { }
        }

        #endregion WindowCallbacks

        // -- Bot Combat STATES TANK, HEAL, ATTACK, checkboxes

        #region BotCombatStates

        private void CheckBoxAssistPartyAttack_Click(object sender, RoutedEventArgs e)
        {
            BotManager.IsSupposedToAttack = (bool)checkBoxAssistPartyAttack.IsChecked;
        }

        private void CheckBoxAssistPartyHeal_Click(object sender, RoutedEventArgs e)
        {
            BotManager.IsSupposedToAttack = (bool)checkBoxAssistPartyAttack.IsChecked;
        }

        private void CheckBoxAssistPartyTank_Click(object sender, RoutedEventArgs e)
        {
            BotManager.IsSupposedToTank = (bool)checkBoxAssistPartyTank.IsChecked;
        }

        #endregion BotCombatStates

        // -- Debug stuff goes here, will be removed in the future Debug stuff, buttons

        #region DebugStuff

        private void ButtonMoveInteractTarget_Click(object sender, RoutedEventArgs e)
        {
            BotManager.AddActionToAIQueue(new AmeisenAction(AmeisenActionType.INTERACT_TARGET, (AmeisenActionType)comboboxInteraction.SelectedItem));
        }

        private void ButtonMoveToTarget_Click(object sender, RoutedEventArgs e)
        {
            BotManager.AddActionToAIQueue(new AmeisenAction(AmeisenActionType.MOVE_TO_POSITION, null));
        }

        #endregion DebugStuff

        // -- External Windows SettingsWindow, DebugUI, CombatClass Editor

        #region ExternalWindows

        private void ButtonCobatClassEditor_Click(object sender, RoutedEventArgs e)
        {
            new CombatClassWindow().Show();
        }

        private void ButtonExtendedDebugUI_Click(object sender, RoutedEventArgs e)
        {
            new DebugWindow().Show();
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow().ShowDialog();
        }

        #endregion ExternalWindows

        // -- UI Stuff Update the GUI

        #region UITimer

        private void UIUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (BotManager.IsBotIngame())
            {
                UpdateUI();
            }
        }

        /// <summary>
        /// This thing updates the UI... Note to myself: "may need to improve this thing in the future..."
        /// </summary>
        private void UpdateUI()
        {
            // TODO: find a better way to update this
            //AmeisenManager.Instance.GetObjects();

            labelLoadedCombatClass.Content = "CombatClass: " + Path.GetFileName(BotManager.Settings.combatClassPath);

            if (BotManager.Me != null)
            {
                try
                {
                    labelName.Content = BotManager.Me.Name + " lvl." + BotManager.Me.Level;
                    //labelCasting.Content = "Casting: " + me.currentState;

                    //labelHP.Content = "HP [" + me.health + "/" + me.maxHealth + "]";
                    progressBarHP.Maximum = BotManager.Me.MaxHealth;
                    progressBarHP.Value = BotManager.Me.Health;

                    //labelEnergy.Content = "Energy [" + me.energy + "/" + me.maxEnergy + "]";
                    progressBarEnergy.Maximum = BotManager.Me.MaxEnergy;
                    progressBarEnergy.Value = BotManager.Me.Energy;

                    //labelXP.Content = "XP [" + me.exp + "/" + me.maxExp + "]";
                    progressBarXP.Maximum = BotManager.Me.MaxExp;
                    progressBarXP.Value = BotManager.Me.Exp;

                    /*labelPosition.Content =
                        "X: " + me.pos.x +
                        "\nY: " + me.pos.y +
                        "\nZ: " + me.pos.z +
                        "\nR: " + me.rotation;*/
                }
                catch (Exception e)
                {
                    AmeisenLogger.Instance.Log(LogLevel.ERROR, e.ToString(), this);
                }
                if (BotManager.Target != null)
                {
                    try
                    {
                        labelNameTarget.Content = BotManager.Target.Name + " lvl." + BotManager.Target.Level;
                        //labelCastingTarget.Content = "Current state: " + me.target.currentState;

                        //labelHPTarget.Content = "HP [" + me.target.health + "/" + me.target.maxHealth + "]";
                        progressBarHPTarget.Maximum = BotManager.Target.MaxHealth;
                        progressBarHPTarget.Value = BotManager.Target.Health;

                        //labelEnergyTarget.Content = "Energy [" + me.target.energy + "/" + me.target.maxEnergy + "]";
                        progressBarEnergyTarget.Maximum = BotManager.Target.MaxEnergy;
                        progressBarEnergyTarget.Value = BotManager.Target.Energy;

                        labelTargetDistance.Content = "Distance: " + BotManager.Target.Distance + "m";

                        /*labelPositionTarget.Content =
                            "X: " + me.target.pos.x +
                            "\nY: " + me.target.pos.y +
                            "\nZ: " + me.target.pos.z +
                            "\nR: " + me.target.rotation;*/
                    }
                    catch (Exception e)
                    {
                        AmeisenLogger.Instance.Log(LogLevel.ERROR, e.ToString(), this);
                    }
                }
            }

            try
            {
                labelThreadsActive.Content = "⚡ Threads: " + BotManager.AmeisenAIManager.GetBusyThreadCount() + 
                                             "/" + BotManager.AmeisenAIManager.GetActiveThreadCount();
                progressBarBusyAIThreads.Maximum = BotManager.AmeisenAIManager.GetActiveThreadCount();
                progressBarBusyAIThreads.Value = BotManager.AmeisenAIManager.GetBusyThreadCount();
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log(LogLevel.ERROR, e.ToString(), this);
            }
        }

        #endregion UITimer

        #region Private Methods

        private void CheckBoxFollowMaster_Click(object sender, RoutedEventArgs e)
        {
            AmeisenBotManager.Instance.FollowGroup = (bool)checkBoxFollowMaster.IsChecked;
        }

        #endregion Private Methods

        private void ButtonFaceTarget_Click(object sender, RoutedEventArgs e)
        {
            BotManager.FaceTarget();
        }

        private void ButtonMap_Click(object sender, RoutedEventArgs e)
        {
            new MapWindow().Show();
        }

        private void ButtonGroup_Click(object sender, RoutedEventArgs e)
        {
            new GroupWindow().Show();
        }
    }
}