using AmeisenManager;
using AmeisenUtilities;
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
        public GroupWindow()
        {
            InitializeComponent();
            botViews = new List<BotView>();
        }

        private List<BotView> botViews;
        private DispatcherTimer uiUpdateTimer;

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void UIUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (AmeisenBotManager.Instance.AmeisenClient.IsRegistered)
            {
                botViewPanel.Children.Clear();

                List<NetworkBot> networkBots = AmeisenBotManager.Instance.GetNetworkBots();
                if (networkBots != null)
                    foreach (NetworkBot bot in networkBots)
                    {
                        BotView botView = new BotView();
                        botView.botName.Content = bot.name;
                        botView.botLevel.Content = bot.me.Level;

                        botView.botHealth.Content = bot.me.Health + "/" + bot.me.MaxHealth;
                        botView.botEnergy.Content = bot.me.Energy + "/" + bot.me.MaxEnergy;
                        botView.botExp.Content = bot.me.Exp + "/" + bot.me.MaxExp;

                        botView.botHealthProgressbar.Maximum = bot.me.MaxHealth;
                        botView.botEnergyProgressbar.Maximum = bot.me.MaxEnergy;
                        botView.botExpProgressbar.Maximum = bot.me.MaxExp;
                        botView.botHealthProgressbar.Value = bot.me.Health;
                        botView.botEnergyProgressbar.Value = bot.me.Energy;
                        botView.botExpProgressbar.Value = bot.me.Exp;

                        botView.botImage.Source = Utils.Base64ToBitmapImage(bot.base64Image);
                        botViewPanel.Children.Add(botView);
                    }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            uiUpdateTimer.Stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            uiUpdateTimer = new DispatcherTimer();
            uiUpdateTimer.Tick += new EventHandler(UIUpdateTimer_Tick);
            uiUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            uiUpdateTimer.Start();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch { }
        }
    }
}