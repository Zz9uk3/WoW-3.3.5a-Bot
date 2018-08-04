using AmeisenAI;
using AmeisenCore;
using AmeisenCore.Objects;
using AmeisenLogging;
using AmeisenUtilities;
using System;
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
            }
        }

        private void ButtonMoveToTarget_Click(object sender, RoutedEventArgs e)
        {
            AmeisenAIManager.GetInstance().AddActionToQueue(new AmeisenAction(AmeisenActionType.MOVE_TO_TARGET, null));
        }

        private void ButtonMoveInteractTarget_Click(object sender, RoutedEventArgs e)
        {
            AmeisenAIManager.GetInstance().AddActionToQueue(new AmeisenAction(AmeisenActionType.INTERACT_TARGET, (AmeisenActionType)comboboxInteraction.SelectedItem));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
            AmeisenAIManager.GetInstance().StopAI();
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

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            //AmeisenAIManager.GetInstance().AddActionToQueue(new AmeisenAction(AmeisenActionType.TARGET_MYSELF, null));
            AmeisenCore.AmeisenCore.CharacterJumpAsync();
        }

        private void UIUpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateUI();

            if (checkBoxFollowGroupLeader.IsChecked == true)
                AmeisenAIManager.GetInstance().AddActionToQueue(new AmeisenAction(AmeisenActionType.MOVE_TO_GROUPLEADER, 8.0));
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

            try
            {
                labelName.Content = me.name + " - lvl." + me.level + " - ";

                labelHP.Content = "HP [" + me.health + "/" + me.maxHealth + "]";
                progressBarHP.Maximum = me.maxHealth;
                progressBarHP.Value = me.health;

                labelEnergy.Content = "Energy [" + me.energy + "/" + me.maxEnergy + "]";
                progressBarEnergy.Maximum = me.maxEnergy;
                progressBarEnergy.Value = me.energy;

                labelXP.Content = "XP [" + me.exp + "/" + me.maxExp + "]";
                progressBarXP.Maximum = me.maxExp;
                progressBarXP.Value = me.exp;

                labelPosition.Content =
                    "X: " + me.pos.x +
                    "\nY: " + me.pos.y +
                    "\nZ: " + me.pos.z +
                    "\nR: " + me.rotation;

                StringBuilder sb = new StringBuilder();

                foreach (Player t in me.partymembers)
                    sb.Append(t.ToShortString() + ",\n");

                labelDebugInfo.Content =
                    "- DebugInfo -\nTargetGUID: " + me.target.guid +
                    "\nFactionTemplate: " + me.factionTemplate +
                    "\nMapID: " + me.mapID +
                    "\nZoneID: " + me.zoneID +
                    "\nPartyMembers { " + sb + "\n}";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            try
            {
                labelNameTarget.Content = me.target.name + " - lvl." + me.target.level + " - ";

                labelHPTarget.Content = "HP [" + me.target.health + "/" + me.target.maxHealth + "]";
                progressBarHPTarget.Maximum = me.target.maxHealth;
                progressBarHPTarget.Value = me.target.health;

                labelEnergyTarget.Content = "Energy [" + me.target.energy + "/" + me.target.maxEnergy + "]";
                progressBarEnergyTarget.Maximum = me.target.maxEnergy;
                progressBarEnergyTarget.Value = me.target.energy;

                labelDistance.Content = "Distance: " + me.target.distance + "m";

                labelPositionTarget.Content =
                    "X: " + me.target.pos.x +
                    "\nY: " + me.target.pos.y +
                    "\nZ: " + me.target.pos.z +
                    "\nR: " + me.target.rotation;

                labelDebugInfoTarget.Content =
                    "- DebugInfo -\nTargetGUID:" + me.target.target.guid +
                    "\nFactionTemplate:" + me.target.factionTemplate;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            try
            {
                labelThreadsActive.Content = "Threads: " + AmeisenAIManager.GetInstance().GetBusyThreadCount() + "/" + AmeisenAIManager.GetInstance().GetActiveThreadCount();
                progressBarBusyAIThreads.Maximum = AmeisenAIManager.GetInstance().GetActiveThreadCount();
                progressBarBusyAIThreads.Value = AmeisenAIManager.GetInstance().GetBusyThreadCount();
                listboxCurrentQueue.Items.Clear();
                foreach (AmeisenAction a in AmeisenAIManager.GetInstance().GetQueueItems())
                    listboxCurrentQueue.Items.Add(a.GetActionType() + " [" + a.GetActionParams() + "]");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
