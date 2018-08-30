using AmeisenAI;
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
        #region Private Fields

        private static readonly object padlock = new object();
        private static AmeisenBotManager instance;

        private readonly string sqlConnectionString =
                "server=localhost;" +
                "database=ameisenbot;" +
                "uid=ameisenbot;" +
                "password=AmeisenPassword;";

        private bool followGroup;

        #endregion Private Fields

        #region Private Constructors

        private AmeisenBotManager()
        {
            IsAttached = false;
            IsHooked = false;

            AmeisenSettings = AmeisenSettings.Instance;
            AmeisenClient = AmeisenClient.Instance;
            AmeisenDBManager = AmeisenDBManager.Instance;
        }

        #endregion Private Constructors

        #region Public Properties

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

        public List<WoWObject> ActiveWoWObjects { get { return AmeisenDataHolder.Instance.ActiveWoWObjects; } }
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
            get { return followGroup; }
            set
            {
                followGroup = value;

                if (value == true)
                {
                    foreach (ulong guid in Me.PartymemberGUIDs)
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

        public bool IsAllowedToMove
        {
            get { return AmeisenAIManager.IsAllowedToMove; }
            set { AmeisenAIManager.IsAllowedToMove = value; }
        }

        public bool IsAttached { get; private set; }
        public bool IsHooked { get; private set; }
        public bool IsSupposedToAttack { get; set; }
        public bool IsSupposedToHeal { get; set; }
        public bool IsSupposedToTank { get; set; }
        public Me Me { get { return AmeisenDataHolder.Instance.Me; } }
        public List<WoWExe> RunningWoWs { get { return AmeisenCoreUtils.AmeisenCore.GetRunningWoWs(); } }
        public Settings Settings { get { return AmeisenSettings.Settings; } }
        public Unit Target { get { return AmeisenDataHolder.Instance.Target; } }
        public WoWExe WowExe { get; private set; }
        public List<WoWObject> WoWObjects { get { return AmeisenObjectManager.GetObjects(); } }
        public Process WowProcess { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public static int GetMapID()
        {
            return AmeisenCoreUtils.AmeisenCore.GetMapID();
        }

        public static int GetZoneID()
        {
            return AmeisenCoreUtils.AmeisenCore.GetZoneID();
        }

        public void AddActionToAIQueue(AmeisenAction ameisenAction)
        {
            AmeisenAIManager.AddActionToQueue(ameisenAction);
        }

        public void AddActionToAIQueue(ref AmeisenAction ameisenAction)
        {
            AmeisenAIManager.AddActionToQueue(ref ameisenAction);
        }

        public void FaceTarget()
        {
            AmeisenCoreUtils.AmeisenCore.MovePlayerToXYZ(Target.pos, Interaction.ATTACK);
            AmeisenCoreUtils.AmeisenCore.MovePlayerToXYZ(Target.pos, Interaction.STOP);
        }

        public string GetLoadedConfigName()
        {
            return AmeisenSettings.loadedconfName;
        }

        public List<Bot> GetNetworkBots()
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

        public WoWExe GetWowExe()
        {
            return WowExe;
        }

        public bool IsBotIngame()
        {
            return AmeisenCoreUtils.AmeisenCore.CheckWorldLoaded()
               && !AmeisenCoreUtils.AmeisenCore.CheckLoadingScreen();
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

        public void StartBot(WoWExe wowExe)
        {
            WowExe = wowExe;

            // Load Settings
            AmeisenSettings.LoadFromFile(wowExe.characterName);

            // Connect to DB
            AmeisenDBManager.ConnectToMySQL(sqlConnectionString);

            // Attach to Proccess
            Blackmagic = new BlackMagic(wowExe.process.Id);
            IsAttached = Blackmagic.IsProcessOpen;
            // TODO: make this better
            AmeisenCoreUtils.AmeisenCore.BlackMagic = Blackmagic;

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
            if (Settings.autoConnect)
            {
                AmeisenClient.Register(Me, IPAddress.Parse("127.0.0.1"));
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

        #endregion Public Methods
    }
}