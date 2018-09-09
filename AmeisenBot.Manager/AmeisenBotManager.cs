using AmeisenBotCore;
using AmeisenBotData;
using AmeisenBotDB;
using AmeisenBotFSM;
using AmeisenBotFSM.Enums;
using AmeisenBotLogger;
using AmeisenBotUtilities;
using Magic;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace AmeisenBotManager
{
    /// <summary>
    /// This Singleton provides an Interface to the bot at a single point
    /// </summary>
    public class BotManager
    {
        private readonly string sqlConnectionString =
                "server={0};port={1};database={2};uid={3};password={4};";

        private AmeisenDataHolder AmeisenDataHolder { get; set; }
        private AmeisenClient AmeisenClient { get; set; }
        public AmeisenDBManager AmeisenDBManager { get; private set; }
        private AmeisenHook AmeisenHook { get; set; }
        private AmeisenObjectManager AmeisenObjectManager { get; set; }
        private AmeisenSettings AmeisenSettings { get; set; }
        private AmeisenStateMachineManager AmeisenStateMachineManager { get; set; }
        private BlackMagic Blackmagic { get; set; }
        public List<WowObject> ActiveWoWObjects { get { return AmeisenDataHolder.ActiveWoWObjects; } }

        public bool IsAllowedToAssistParty
        {
            get { return AmeisenDataHolder.IsAllowedToAssistParty; }
            set { AmeisenDataHolder.IsAllowedToAssistParty = value; }
        }

        public bool IsAllowedToAttack
        {
            get { return AmeisenDataHolder.IsAllowedToAttack; }
            set { AmeisenDataHolder.IsAllowedToAttack = value; }
        }

        public bool IsAllowedToBuff
        {
            get { return AmeisenDataHolder.IsAllowedToBuff; }
            set { AmeisenDataHolder.IsAllowedToBuff = value; }
        }

        public bool IsAllowedToFollowParty
        {
            get { return AmeisenDataHolder.IsAllowedToFollowParty; }
            set { AmeisenDataHolder.IsAllowedToFollowParty = value; }
        }

        public bool IsAllowedToHeal
        {
            get { return AmeisenDataHolder.IsAllowedToHeal; }
            set { AmeisenDataHolder.IsAllowedToHeal = value; }
        }

        public bool IsAllowedToTank
        {
            get { return AmeisenDataHolder.IsAllowedToTank; }
            set { AmeisenDataHolder.IsAllowedToTank = value; }
        }

        public bool IsAllowedToReleaseSpirit
        {
            get { return AmeisenDataHolder.IsAllowedToReleaseSpirit; }
            set { AmeisenDataHolder.IsAllowedToReleaseSpirit = value; }
        }

        public bool IsAllowedToRevive
        {
            get { return AmeisenDataHolder.IsAllowedToRevive; }
            set { AmeisenDataHolder.IsAllowedToRevive = value; }
        }

        public bool IsAttached { get; private set; }
        public bool IsHooked { get; private set; }
        public Me Me { get { return AmeisenDataHolder.Me; } }
        public List<WowExe> RunningWoWs { get { return AmeisenCore.GetRunningWoWs(); } }
        public Settings Settings { get { return AmeisenSettings.Settings; } }
        public Unit Target { get { return AmeisenDataHolder.Target; } }
        public WowExe WowExe { get; private set; }
        public List<WowObject> WoWObjects { get { return AmeisenObjectManager.GetObjects(); } }
        public Process WowProcess { get; private set; }

        public BotManager()
        {
            IsAttached = false;
            IsHooked = false;

            AmeisenDataHolder = new AmeisenDataHolder();
            AmeisenSettings = new AmeisenSettings(AmeisenDataHolder);
            AmeisenClient = new AmeisenClient(AmeisenDataHolder);
            AmeisenDBManager = new AmeisenDBManager();
        }

        public int MapID { get { return AmeisenCore.GetMapID(); } }
        public int ZoneID { get { return AmeisenCore.GetZoneID(); } }
        public string LoadedConfigName { get { return AmeisenSettings.loadedconfName; } }

        public List<NetworkBot> NetworkBots
        {
            get
            {
                if (AmeisenClient.IsRegistered)
                {
                    return AmeisenClient.BotList;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool IsIngame
        {
            get
            {
                return AmeisenCore.CheckWorldLoaded()
                   && !AmeisenCore.CheckLoadingScreen();
            }
        }

        public bool IsRegisteredAtServer { get { return AmeisenClient.IsRegistered; } }
        public object CurrentFSMState { get { return AmeisenStateMachineManager.StateMachine.GetCurrentState(); } }

        public void LoadCombatClass(string fileName)
        {
            AmeisenSettings.Settings.combatClassPath = fileName;
            AmeisenSettings.SaveToFile(AmeisenSettings.loadedconfName);

            //TODO: replace AmeisenCombatManager.ReloadCombatClass();
        }

        public void LoadSettingsFromFile(string filename)
        {
            AmeisenSettings.LoadFromFile(filename);
        }

        public void SaveSettingsToFile(string filename)
        {
            AmeisenSettings.SaveToFile(filename);
        }

        public void StartBot(WowExe wowExe)
        {
            WowExe = wowExe;

            // Load Settings
            AmeisenSettings.LoadFromFile(wowExe.characterName);

            // Connect to DB
            if (AmeisenSettings.Settings.databaseAutoConnect)
            {
                AmeisenDBManager.ConnectToMySQL(
                string.Format(sqlConnectionString,
                    AmeisenSettings.Settings.databaseIP,
                    AmeisenSettings.Settings.databasePort,
                    AmeisenSettings.Settings.databaseName,
                    AmeisenSettings.Settings.databaseUsername,
                    AmeisenSettings.Settings.databasePasswort)
                );
            }

            // Attach to Proccess
            Blackmagic = new BlackMagic(wowExe.process.Id);
            IsAttached = Blackmagic.IsProcessOpen;
            // TODO: make this better
            AmeisenCore.BlackMagic = Blackmagic;

            // Hook EndScene
            AmeisenHook = new AmeisenHook();
            IsHooked = AmeisenHook.isHooked;
            AmeisenCore.AmeisenHook = AmeisenHook;

            // Start our object updates
            AmeisenObjectManager = new AmeisenObjectManager(AmeisenDataHolder, AmeisenDBManager);
            AmeisenObjectManager.Start();

            // Start the StateMachine
            AmeisenStateMachineManager = new AmeisenStateMachineManager(AmeisenDataHolder);
            AmeisenStateMachineManager.StateMachine.PushAction(BotState.Idle);
            AmeisenStateMachineManager.StateMachine.PushAction(BotState.Follow);
            AmeisenStateMachineManager.Start();

            // Connect to Server
            if (Settings.serverAutoConnect)
            {
                AmeisenClient.Register(
                    Me,
                    IPAddress.Parse(AmeisenSettings.Settings.ameisenServerIP),
                    AmeisenSettings.Settings.ameisenServerPort);
            }
        }

        public void StopBot()
        {
            // Disconnect from Server
            if (AmeisenClient.IsRegistered)
            {
                AmeisenClient.Unregister();
            }

            // Stop object updates
            AmeisenObjectManager.Stop();

            // Stop the statemachine
            AmeisenStateMachineManager.Stop();

            // Unhook the EndScene
            AmeisenHook.DisposeHooking();

            // Detach BlackMagic, causing weird crash right now...
            //Blackmagic.Close();

            // Stop logging
            AmeisenLogger.Instance.StopLogging();

            //Close SQL Connection
            AmeisenDBManager.Disconnect();
        }
    }
}