using AmeisenAI;
using AmeisenCore;
using AmeisenData;
using AmeisenLogging;
using AmeisenUtilities;
using Magic;
using System;
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

        #region Instances
        public BlackMagic Blackmagic { get; private set; }
        public AmeisenSettings AmeisenSettings { get; private set; }
        public AmeisenCombatManager AmeisenCombatManager { get; private set; }
        public AmeisenAIManager AmeisenAIManager { get; private set; }
        public AmeisenObjectManager AmeisenObjectManager { get; private set; }
        public AmeisenClient AmeisenClient { get; private set; }
        public AmeisenHook AmeisenHook { get; private set; }
        public AmeisenFollowManager AmeisenFollowManager { get; private set; }
        #endregion

        #region Fields
        public Me Me { get { return AmeisenDataHolder.Instance.Me; } }
        public Unit Target { get { return AmeisenDataHolder.Instance.Target; } }
        public List<WoWObject> ActiveWoWObjects { get { return AmeisenDataHolder.Instance.ActiveWoWObjects; } }

        public WoWExe WowExe { get; private set; }
        public Process WowProcess { get; private set; }
        #endregion

        #region StateFields
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
        #endregion

        #region SingletonStuff
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
        #endregion

        #region BotActions
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
            AmeisenObjectManager.Start();

            // Start the AI
            AmeisenAIManager = AmeisenAIManager.Instance;
            AmeisenAIManager.Start(AmeisenSettings.Settings.botMaxThreads);

            // Start the combatmanager
            AmeisenCombatManager = AmeisenCombatManager.Instance;
            AmeisenCombatManager.Start();

            // Start Follow Engine
            AmeisenFollowManager = AmeisenFollowManager.Instance;
            AmeisenFollowManager.Start();

            // Connect to Server
            // TODO: Move into settings
            AmeisenClient.Register(Me, IPAddress.Parse("127.0.0.1"));
        }

        public void StopBot()
        {
            // Disconnect from Server
            AmeisenClient.Unregister();

            // Stop object updates
            AmeisenObjectManager.Stop();

            // Stop the combatmanager
            AmeisenCombatManager.Stop();

            // Stop the Follow Engine
            AmeisenFollowManager.Stop();

            // Stop the AI
            AmeisenAIManager.Stop();

            // Unhook the EndScene
            AmeisenHook.DisposeHooking();

            // Detach BlackMagic
            // Blackmagic.Close();

            // Stop logging
            AmeisenLogger.Instance.StopLogging();
        }

        public void LoadCombatClass(string fileName)
        {
            AmeisenSettings.Settings.combatClassPath = fileName;
            AmeisenSettings.SaveToFile(AmeisenSettings.loadedconfName);

            AmeisenCombatManager.ReloadCombatClass();
        }

        private bool followGroup;
        public bool FollowGroup
        {
            get { return followGroup; }
            set
            {
                followGroup = value;

                if (value == true)
                    foreach (UInt64 guid in Me.PartymemberGUIDs)
                        AmeisenFollowManager.AddPlayerToFollow((Unit)AmeisenObjectManager.GetWoWObjectFromGUID(guid));
                else
                    AmeisenFollowManager.RemoveAllPlayersToFollow();
            }
        }
        #endregion

        #region Retriveables
        public List<WoWExe> RunningWoWs { get { return AmeisenCore.AmeisenCore.GetRunningWoWs(); } }

        public bool IsBotIngame()
        {
            return AmeisenCore.AmeisenCore.CheckWorldLoaded()
               && !AmeisenCore.AmeisenCore.CheckLoadingScreen();
        }

        public List<Bot> GetNetworkBots()
        {
            if (AmeisenClient.IsRegistered) return AmeisenClient.BotList; else return null;
        }
        #endregion

        #region Objects

        public WoWExe GetWowExe()
        {
            return WowExe;
        }

        public List<WoWObject> WoWObjects { get { return AmeisenObjectManager.GetObjects(); } }

        #endregion Objects

        #region AI

        public void AddActionToAIQueue(AmeisenAction ameisenAction)
        {
            AmeisenAIManager.AddActionToQueue(ameisenAction);
        }

        public void AddActionToAIQueue(ref AmeisenAction ameisenAction)
        {
            AmeisenAIManager.AddActionToQueue(ref ameisenAction);
        }

        #endregion AI

        #region Settings

        public Settings Settings { get { return AmeisenSettings.Settings; } }

        public void SaveSettingsToFile(string filename)
        {
            AmeisenSettings.SaveToFile(filename);
        }

        public void LoadSettingsFromFile(string filename)
        {
            AmeisenSettings.LoadFromFile(filename);
        }

        public string GetLoadedConfigName()
        {
            return AmeisenSettings.loadedconfName;
        }

        #endregion Settings
    }
}