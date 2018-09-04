using AmeisenAI;
using AmeisenAI.Combat;
using AmeisenAI.Follow;
using AmeisenCoreUtils;
using AmeisenData;
using AmeisenDB;
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
        public static AmeisenBotManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new AmeisenBotManager();
                    }

                    return instance;
                }
            }
        }

        public List<WowObject> ActiveWoWObjects { get { return AmeisenDataHolder.Instance.ActiveWoWObjects; } }
        public AmeisenAIManager AmeisenAIManager { get; private set; }
        public AmeisenClient AmeisenClient { get; private set; }
        public AmeisenCombatManager AmeisenCombatManager { get; private set; }
        public AmeisenDBManager AmeisenDBManager { get; private set; }
        public AmeisenFollowManager AmeisenFollowManager { get; private set; }
        public AmeisenHook AmeisenHook { get; private set; }
        public AmeisenObjectManager AmeisenObjectManager { get; private set; }
        public AmeisenSettings AmeisenSettings { get; private set; }
        public BlackMagic Blackmagic { get; private set; }

        public bool FollowGroup
        {
            get
            {
                return followGroup;
            }
            set
            {
                followGroup = value;

                if (value == true)
                {
                    foreach (ulong guid in Me.PartymemberGuids)
                    {
                        AmeisenFollowManager.AddPlayerToFollow((Unit)AmeisenObjectManager.GetWoWObjectFromGUID(guid));
                    }
                }
                else
                {
                    AmeisenFollowManager.RemoveAllPlayersToFollow();
                }
            }
        }

        public bool IsAllowedToAttack { get { return AmeisenDataHolder.Instance.IsAllowedToAttack; } set { AmeisenDataHolder.Instance.IsAllowedToAttack = value; } }

        public bool IsAllowedToBuff { get { return AmeisenDataHolder.Instance.IsAllowedToBuff; } set { AmeisenDataHolder.Instance.IsAllowedToBuff = value; } }

        public bool IsAllowedToHeal { get { return AmeisenDataHolder.Instance.IsAllowedToHeal; } set { AmeisenDataHolder.Instance.IsAllowedToHeal = value; } }

        public bool IsAllowedToAssistParty { get { return AmeisenDataHolder.Instance.IsAllowedToAssistParty; } set { AmeisenDataHolder.Instance.IsAllowedToAssistParty = value; } }

        public bool IsAllowedToMove
        {
            get { return AmeisenAIManager.IsAllowedToMove; }
            set { AmeisenAIManager.IsAllowedToMove = value; }
        }

        public bool IsAllowedToTank { get { return AmeisenDataHolder.Instance.IsAllowedToTank; } set { AmeisenDataHolder.Instance.IsAllowedToTank = value; } }
        public bool IsAttached { get; private set; }
        public bool IsHooked { get; private set; }
        public Me Me { get { return AmeisenDataHolder.Instance.Me; } }
        public List<WowExe> RunningWoWs { get { return AmeisenCore.GetRunningWoWs(); } }
        public Settings Settings { get { return AmeisenSettings.Settings; } }
        public Unit Target { get { return AmeisenDataHolder.Instance.Target; } }
        public WowExe WowExe { get; private set; }
        public List<WowObject> WoWObjects { get { return AmeisenObjectManager.GetObjects(); } }
        public Process WowProcess { get; private set; }

        public static int GetMapID()
        {
            return AmeisenCore.GetMapID();
        }

        public static int GetZoneID()
        {
            return AmeisenCore.GetZoneID();
        }

        public void AddActionToAIQueue(AmeisenAction ameisenAction)
        {
            AmeisenAIManager.AddActionToQueue(ameisenAction);
        }

        public void AddActionToAIQueue(ref AmeisenAction ameisenAction)
        {
            AmeisenAIManager.AddActionToQueue(ref ameisenAction);
        }

        public string GetLoadedConfigName()
        {
            return AmeisenSettings.loadedconfName;
        }

        public List<NetworkBot> GetNetworkBots()
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

        public WowExe GetWowExe()
        {
            return WowExe;
        }

        public bool IsBotIngame()
        {
            return AmeisenCore.CheckWorldLoaded()
               && !AmeisenCore.CheckLoadingScreen();
        }

        public void LoadCombatClass(string fileName)
        {
            AmeisenSettings.Settings.combatClassPath = fileName;
            AmeisenSettings.SaveToFile(AmeisenSettings.loadedconfName);

            AmeisenCombatManager.ReloadCombatClass();
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

            // Stop the combatmanager
            AmeisenCombatManager.Stop();

            // Stop the Follow Engine
            AmeisenFollowManager.Stop();

            // Stop the AI
            AmeisenAIManager.Stop();

            // Unhook the EndScene
            AmeisenHook.DisposeHooking();

            // Detach BlackMagic Blackmagic.Close();

            // Stop logging
            AmeisenLogger.Instance.StopLogging();

            //Close SQL Connection
            AmeisenDBManager.Instance.Disconnect();
        }

        private static readonly object padlock = new object();
        private static AmeisenBotManager instance;

        private readonly string sqlConnectionString =
                "server={0};port={1};database={2};uid={3};password={4};";

        private bool followGroup;

        private AmeisenBotManager()
        {
            IsAttached = false;
            IsHooked = false;

            AmeisenSettings = AmeisenSettings.Instance;
            AmeisenClient = AmeisenClient.Instance;
            AmeisenDBManager = AmeisenDBManager.Instance;
        }
    }
}