using AmeisenBotManager;
using AmeisenBotUtilities;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace AmeisenBotGUI
{
    /// <summary>
    /// Interaction logic for GroupWindow.xaml
    /// </summary>
    public partial class GroupWindow : Window
    {
        private List<BotView> botViews;
        private DispatcherTimer uiUpdateTimer;
        private BotManager BotManager { get; set; }

        public GroupWindow(BotManager botManager)
        {
            InitializeComponent();
            BotManager = botManager;

            botViews = new List<BotView>();
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

        private void StartUIUpdateTimer()
        {
            uiUpdateTimer = new DispatcherTimer();
            uiUpdateTimer.Tick += new EventHandler(UIUpdateTimer_Tick);
            uiUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            uiUpdateTimer.Start();
        }

        private void UIUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (BotManager.IsRegisteredAtServer)
            {
                botViewPanel.Children.Clear();

                List<NetworkBot> networkBots = BotManager.NetworkBots;
                if (networkBots != null)
                {
                    foreach (NetworkBot bot in networkBots)
                    {
                        BotView botView = new BotView();
                        SendableMe me = bot.GetMe();
                        botView.botName.Content = me.Name;
                        botView.botLevel.Content = me.Level;

                        botView.botHealth.Content = $"{me.Health} / {me.MaxHealth}";
                        botView.botEnergy.Content = $"{me.Energy} / {me.MaxEnergy}";
                        botView.botExp.Content = $"{me.Exp} / {me.MaxExp}";

                        botView.botHealthProgressbar.Maximum = me.MaxHealth;
                        botView.botEnergyProgressbar.Maximum = me.MaxEnergy;
                        botView.botExpProgressbar.Maximum = me.MaxExp;
                        botView.botHealthProgressbar.Value = me.Health;
                        botView.botEnergyProgressbar.Value = me.Energy;
                        botView.botExpProgressbar.Value = me.Exp;

                        if (bot.picture != "")
                        {
                            botView.botImage.Source = Utils.Base64ToBitmapImage(bot.picture);
                        }

                        botViewPanel.Children.Add(botView);
                    }
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            uiUpdateTimer.Stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartUIUpdateTimer();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch { }
        }
    }
}