using AmeisenAI;
using AmeisenCore;
using AmeisenCore.Objects;
using AmeisenLogging;
using AmeisenUtilities;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace AmeisenBotGUI
{
    /// <summary>
    /// Interaktionslogik für mainscreenForm.xaml
    /// </summary>
    public partial class MainscreenForm : Window
    {
        private WoWExe wowExe;
        private DispatcherTimer uiUpdateTimer;

        private bool uiMode;

        public MainscreenForm(WoWExe wowExe)
        {
            InitializeComponent();

            if (wowExe.characterName == "DEBUG")
                uiMode = true;
            else
                this.wowExe = wowExe;
        }

        private void Mainscreen_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Mainscreen_Loaded(object sender, RoutedEventArgs e)
        {
            AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Loaded MainScreen", this);

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
            comboboxInteraction.SelectedIndex = 0;

            if (uiMode)
                AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "UI-MODE active", this);

            if (!uiMode)
            {
                // Fire up the AI
                AmeisenAIManager.GetInstance().StartAI(AmeisenSettings.GetInstance().settings.botMaxThreads);

                Title = "AmeisenBot - " + wowExe.characterName + " [" + wowExe.process.Id + "]";
                UpdateUI();

                uiUpdateTimer = new DispatcherTimer();
                uiUpdateTimer.Tick += new EventHandler(UIUpdateTimer_Tick);
                uiUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, AmeisenSettings.GetInstance().settings.dataRefreshRate);
                uiUpdateTimer.Start();
                AmeisenLogger.GetInstance().Log(LogLevel.DEBUG, "Started UI-Update-Timer", this);

                checkBoxAssistPartyAttack.IsChecked = AmeisenSettings.GetInstance().settings.behaviourAttack;
                checkBoxAssistPartyTank.IsChecked = AmeisenSettings.GetInstance().settings.behaviourTank;
                checkBoxAssistPartyHeal.IsChecked = AmeisenSettings.GetInstance().settings.behaviourHeal;
                checkBoxFollowMaster.IsChecked = AmeisenSettings.GetInstance().settings.followMaster;
            }
        }

        private void ButtonMoveToTarget_Click(object sender, RoutedEventArgs e)
        {
            AmeisenAIManager.GetInstance().AddActionToQueue(new AmeisenAction(AmeisenActionType.MOVE_TO_POSITION, null));
        }

        private void ButtonMoveInteractTarget_Click(object sender, RoutedEventArgs e)
        {
            AmeisenAIManager.GetInstance().AddActionToQueue(new AmeisenAction(AmeisenActionType.INTERACT_TARGET, (AmeisenActionType)comboboxInteraction.SelectedItem));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
            AmeisenManager.GetInstance().GetAmeisenHook().DisposeHooking();
            AmeisenAIManager.GetInstance().StopAI();
            AmeisenCombatManager.GetInstance().Stop();
            AmeisenLogger.GetInstance().StopLogging();
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow(uiMode).ShowDialog();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ButtonExtendedDebugUI_Click(object sender, RoutedEventArgs e)
        {
            new DebugUI().Show();
        }

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            AmeisenCore.AmeisenCore.LUADoString(debugInput.Text);
        }

        private void ButtonTargetMyself_Click(object sender, RoutedEventArgs e)
        {
            AmeisenCore.AmeisenCore.TargetGUID(AmeisenManager.GetInstance().GetMe().guid);
        }

        private void ButtonTargetGUID_Click(object sender, RoutedEventArgs e)
        {
            AmeisenCore.AmeisenCore.TargetGUID(Convert.ToUInt64(debugInputGUID.Text));
        }

        private void UIUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (AmeisenCore.AmeisenCore.CheckWorldLoaded()
                && !AmeisenCore.AmeisenCore.CheckLoadingScreen())
            {

                AmeisenCore.AmeisenCore.AntiAFK();
                UpdateUI();

                if (checkBoxFollowMaster.IsChecked == true
                    && AmeisenManager.GetInstance().GetMe().partyLeader != null
                    && AmeisenManager.GetInstance().GetMe().currentState != UnitState.ATTACKING
                    && AmeisenManager.GetInstance().GetMe().currentState != UnitState.AUTOHIT
                    && AmeisenManager.GetInstance().GetMe().currentState != UnitState.CASTING)
                {
                    bool addAction = true;

                    foreach (AmeisenAction a in AmeisenAIManager.GetInstance().GetQueueItems())
                        if (a.GetActionType() == AmeisenActionType.MOVE_TO_POSITION)
                        {
                            addAction = false;
                            break;
                        }

                    if (addAction)
                        AmeisenAIManager.GetInstance().AddActionToQueue(new AmeisenAction(AmeisenActionType.MOVE_TO_POSITION, AmeisenManager.GetInstance().GetMe().partyLeader.pos));
                }
            }
        }

        /// <summary>
        /// This thing updates the UI...
        /// Note to myself: "may need to improve this thing in the future..."
        /// </summary>
        private void UpdateUI()
        {
            AmeisenManager.GetInstance().RefreshMeAsync();
            Me me = AmeisenManager.GetInstance().GetMe();

            // TODO: find a better way to update this
            AmeisenManager.GetInstance().GetObjects();

            labelLoadedCombatClass.Content = "CombatClass: " + Path.GetFileName(AmeisenSettings.GetInstance().settings.combatClassPath);

            if (me != null)
            {
                try
                {
                    labelName.Content = me.name + " lvl." + me.level;
                    //labelCasting.Content = "Casting: " + me.currentState;

                    //labelHP.Content = "HP [" + me.health + "/" + me.maxHealth + "]";
                    progressBarHP.Maximum = me.maxHealth;
                    progressBarHP.Value = me.health;

                    //labelEnergy.Content = "Energy [" + me.energy + "/" + me.maxEnergy + "]";
                    progressBarEnergy.Maximum = me.maxEnergy;
                    progressBarEnergy.Value = me.energy;

                    //labelXP.Content = "XP [" + me.exp + "/" + me.maxExp + "]";
                    progressBarXP.Maximum = me.maxExp;
                    progressBarXP.Value = me.exp;

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
                if (me.target != null)
                    try
                    {
                        labelNameTarget.Content = me.target.name + " lvl." + me.target.level;
                        //labelCastingTarget.Content = "Current state: " + me.target.currentState;

                        //labelHPTarget.Content = "HP [" + me.target.health + "/" + me.target.maxHealth + "]";
                        progressBarHPTarget.Maximum = me.target.maxHealth;
                        progressBarHPTarget.Value = me.target.health;

                        //labelEnergyTarget.Content = "Energy [" + me.target.energy + "/" + me.target.maxEnergy + "]";
                        progressBarEnergyTarget.Maximum = me.target.maxEnergy;
                        progressBarEnergyTarget.Value = me.target.energy;

                        labelTargetDistance.Content = "Distance: " + me.target.distance + "m";

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
                labelThreadsActive.Content = "⚡ Threads: " + AmeisenAIManager.GetInstance().GetBusyThreadCount() + "/" + AmeisenAIManager.GetInstance().GetActiveThreadCount();
                progressBarBusyAIThreads.Maximum = AmeisenAIManager.GetInstance().GetActiveThreadCount();
                progressBarBusyAIThreads.Value = AmeisenAIManager.GetInstance().GetBusyThreadCount();
                //listboxCurrentQueue.Items.Clear();
                //foreach (AmeisenAction a in AmeisenAIManager.GetInstance().GetQueueItems())
                //listboxCurrentQueue.Items.Add(a.GetActionType() + " [" + a.GetActionParams() + "]");
            }
            catch (Exception e)
            {
                AmeisenLogger.GetInstance().Log(LogLevel.ERROR, e.ToString(), this);
            }
        }

        private void ButtonCobatClassEditor_Click(object sender, RoutedEventArgs e)
        {
            new CombatClassEditor().Show();
        }

        private void ButtonTest2_Click(object sender, RoutedEventArgs e)
        {
            AmeisenCore.AmeisenCore.LUADoString("start, duration, enabled = GetSpellCooldown(\"Every Man for Himself\");");
            labelDebug.Content = AmeisenCore.AmeisenCore.GetLocalizedText("duration");
        }

        private void Mainscreen_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AmeisenSettings.GetInstance().settings.behaviourAttack = (bool)checkBoxAssistPartyAttack.IsChecked;
            AmeisenSettings.GetInstance().settings.behaviourTank = (bool)checkBoxAssistPartyTank.IsChecked;
            AmeisenSettings.GetInstance().settings.behaviourHeal = (bool)checkBoxAssistPartyHeal.IsChecked;
            AmeisenSettings.GetInstance().settings.followMaster = (bool)checkBoxFollowMaster.IsChecked;
            AmeisenSettings.GetInstance().SaveToFile(AmeisenSettings.GetInstance().loadedconfName);
        }

        private void ButtonTestX_Click(object sender, RoutedEventArgs e)
        {
            AmeisenCombatManager.GetInstance().Start();
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                AmeisenSettings.GetInstance().settings.combatClassPath = openFileDialog.FileName;
                AmeisenSettings.GetInstance().SaveToFile(AmeisenSettings.GetInstance().loadedconfName);

                AmeisenCombatManager.GetInstance().ReloadCombatClass();
            }
        }
    }
}
