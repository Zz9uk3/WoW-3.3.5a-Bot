using AmeisenAI;
using AmeisenCore;
using AmeisenData;
using AmeisenLogging;
using AmeisenUtilities;
using Magic;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace AmeisenManager
{
    /// <summary>
    /// This Singleton provides an Interface to the bot at a single point
    /// </summary>
    public class AmeisenBotManager
    {
        private static AmeisenBotManager instance;
        private static readonly object padlock = new object();

        public BlackMagic Blackmagic { get; private set; }
        public AmeisenSettings AmeisenSettings { get; private set; }
        public AmeisenCombatManager AmeisenCombatManager { get; private set; }
        public AmeisenAIManager AmeisenAIManager { get; private set; }
        public AmeisenObjectManager AmeisenObjectManager { get; private set; }
        public AmeisenClient AmeisenClient { get; private set; }
        public AmeisenHook AmeisenHook { get; private set; }

        public Me Me { get { return AmeisenDataHolder.Instance.Me; } }
        public Unit Target { get { return AmeisenDataHolder.Instance.Target; } }
        public List<WoWObject> ActiveWoWObjects { get { return AmeisenDataHolder.Instance.ActiveWoWObjects; } }

        public WoWExe WowExe { get; private set; }
        public Process WowProcess { get; private set; }

        public bool IsAttached { get; private set; }
        public bool IsHooked { get; private set; }

        public bool IsAllowedToMove
        {
            get { return AmeisenAIManager.IsAllowedToMove; }
            set { AmeisenAIManager.IsAllowedToMove = value; }
        }

        public bool IsSupposedToAttack { get; set; }
        public bool IsSupposedToTank { get; set; }
        public bool IsSupposedToHeal { get; set; }

        private AmeisenBotManager()
        {
            IsAttached = false;
            IsHooked = false;

            AmeisenSettings = AmeisenSettings.Instance;
            AmeisenClient = AmeisenClient.Instance;
        }

        public static AmeisenBotManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new AmeisenBotManager();
                    return instance;
                }
            }
        }

        public void StartBot(WoWExe wowExe)
        {
            WowExe = wowExe;

            // Load Settings
            AmeisenSettings.LoadFromFile(wowExe.characterName);

            // Attach to Proccess
            Blackmagic = new BlackMagic(wowExe.process.Id);
            IsAttached = Blackmagic.IsProcessOpen;
            // TODO: make this better
            AmeisenCore.AmeisenCore.Blackmagic = Blackmagic;

            // Hook EndScene
            AmeisenHook = AmeisenHook.Instance;
            IsHooked = AmeisenHook.isHooked;

            // Start our object updates
            AmeisenObjectManager = AmeisenObjectManager.Instance;
            AmeisenObjectManager.StartObjectUpdates();

            // Start the AI
            AmeisenAIManager = AmeisenAIManager.Instance;
            AmeisenAIManager.StartAI(AmeisenSettings.Settings.botMaxThreads);

            // Start the combatmanager
            AmeisenCombatManager = AmeisenCombatManager.Instance;
            AmeisenCombatManager.Start();

            // TODO vvvv
            //AmeisenCore.AmeisenCore.AntiAFK();

            // Connect to Server
            // TODO: Move into settings
            AmeisenClient.Register(Me, IPAddress.Parse("127.0.0.1"));
        }

        public void NoAction() { }

        public void StopBot()
        {
            // Disconnect from Server
            AmeisenClient.Unregister();

            // Stop object updates
            AmeisenObjectManager.StopObjectUpdates();

            // Stop the combatmanager
            AmeisenCombatManager.Stop();

            // Stop the AI
            AmeisenAIManager.StopAI();

            // Unhook the EndScene
            AmeisenHook.DisposeHooking();

            // Detach BlackMagic
            // Blackmagic.Close();

            // Stop logging
            AmeisenLogger.Instance.StopLogging();
        }

        public List<WoWExe> RunningWoWs { get { return AmeisenCore.AmeisenCore.GetRunningWoWs(); } }

        public void LoadCombatClass(string fileName)
        {
            AmeisenSettings.Settings.combatClassPath = fileName;
            AmeisenSettings.SaveToFile(AmeisenSettings.loadedconfName);

            AmeisenCombatManager.ReloadCombatClass();
        }

        public bool IsBotIngame()
        {
            return AmeisenCore.AmeisenCore.CheckWorldLoaded()
               && !AmeisenCore.AmeisenCore.CheckLoadingScreen();
        }

        public bool FollowGroup
        {
            get { return FollowGroup; }
            set { FollowGroup = value; /*Add Follow code here*/ }
        }

        public List<Bot> GetNetworkBots() { if (AmeisenClient.IsRegistered) return AmeisenClient.BotList; else return null; }

        #region Objects
        public WoWExe GetWowExe() { return WowExe; }
        public List<WoWObject> WoWObjects { get { return AmeisenObjectManager.GetObjects(); } }
        #endregion

        #region AI
        public void AddActionToAIQueue(AmeisenAction ameisenAction) { AmeisenAIManager.AddActionToQueue(ameisenAction); }
        public void AddActionToAIQueue(ref AmeisenAction ameisenAction) { AmeisenAIManager.AddActionToQueue(ref ameisenAction); }
        #endregion

        #region Settings
        public Settings Settings { get { return AmeisenSettings.Settings; } }

        public void SaveSettingsToFile(string filename) { AmeisenSettings.SaveToFile(filename); }
        public void LoadSettingsFromFile(string filename) { AmeisenSettings.LoadFromFile(filename); }
        public string GetLoadedConfigName() { return AmeisenSettings.loadedconfName; }
        #endregion
    }
}
