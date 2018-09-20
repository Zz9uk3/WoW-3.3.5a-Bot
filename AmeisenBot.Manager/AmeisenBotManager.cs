using AmeisenBotCore;
using AmeisenBotData;
using AmeisenBotDB;
using AmeisenBotFSM;
using AmeisenBotFSM.Enums;
using AmeisenBotLogger;
using AmeisenBotUtilities;
using AmeisenBotUtilities.Objects;
using AmeisenCombatEngine.Interfaces;
using AmeisenMovement;
using AmeisenMovement.Formations;
using Magic;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;

namespace AmeisenBotManager
{
    /// <summary>
    /// This Singleton provides an Interface to the bot at a single point
    /// </summary>
    public class BotManager
    {
        private readonly string sqlConnectionString =
                "server={0};port={1};database={2};uid={3};password={4};";

        public AmeisenDBManager AmeisenDBManager { get; private set; }
        public List<WowObject> ActiveWoWObjects { get { return AmeisenDataHolder.ActiveWoWObjects; } }

        public bool IsAllowedToAssistParty
        {
            get { return AmeisenDataHolder.IsAllowedToAssistParty; }
            set { AmeisenDataHolder.IsAllowedToAssistParty = value; }
        }

        public bool IsAllowedToDoRandomEmotes
        {
            get { return AmeisenDataHolder.IsAllowedToDoRandomEmotes; }
            set { AmeisenDataHolder.IsAllowedToDoRandomEmotes = value; }
        }

        public bool IsAllowedToAttack
        {
            get { return AmeisenDataHolder.IsAllowedToAttack; }
            set { AmeisenDataHolder.IsAllowedToAttack = value; }
        }

        public bool IsAllowedToDoOwnStuff
        {
            get { return AmeisenDataHolder.IsAllowedToDoOwnStuff; }
            set { AmeisenDataHolder.IsAllowedToDoOwnStuff = value; }
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

        public bool IsBlackmagicAttached { get; private set; }
        public bool IsEndsceneHooked { get; private set; }
        public Me Me { get { return AmeisenDataHolder.Me; } }
        public List<WowExe> RunningWows { get { return AmeisenCore.GetRunningWows(); } }
        public Settings Settings { get { return AmeisenSettings.Settings; } }
        public Unit Target { get { return AmeisenDataHolder.Target; } }
        public WowExe WowExe { get; private set; }
        public Process WowProcess { get; private set; }
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
                   && !AmeisenCore.CheckLoadingScreen(); // TODO: implement this
            }
        }

        public bool IsRegisteredAtServer { get { return AmeisenClient.IsRegistered; } }
        public object CurrentFSMState { get { return AmeisenStateMachineManager.StateMachine.GetCurrentState(); } }
        private AmeisenDataHolder AmeisenDataHolder { get; set; }
        private AmeisenClient AmeisenClient { get; set; }
        private AmeisenHook AmeisenHook { get; set; }
        private AmeisenObjectManager AmeisenObjectManager { get; set; }
        private AmeisenSettings AmeisenSettings { get; set; }
        private AmeisenStateMachineManager AmeisenStateMachineManager { get; set; }
        private AmeisenMovementEngine AmeisenMovementEngine { get; set; }
        private BlackMagic Blackmagic { get; set; }

        /// <summary>
        /// Create a new AmeisenBotManager to manage the bot's functionality
        /// </summary>
        public BotManager()
        {
            IsBlackmagicAttached = false;
            IsEndsceneHooked = false;

            AmeisenDataHolder = new AmeisenDataHolder();
            AmeisenSettings = new AmeisenSettings(AmeisenDataHolder);
            AmeisenClient = new AmeisenClient(AmeisenDataHolder);
            AmeisenDBManager = new AmeisenDBManager();
        }

        /// <summary>
        /// Load a given CombatClass *.cs file into the CombatManager by compiling it at runtime
        /// </summary>
        /// <param name="fileName">*.cs CombatClass file</param>
        public void LoadCombatClassFromFile(string fileName)
        {
            AmeisenSettings.Settings.combatClassPath = fileName;
            AmeisenSettings.SaveToFile(AmeisenSettings.loadedconfName);
            CompileAndLoadCombatClass(fileName);
        }

        /// <summary>
        /// Loads the Settings from a given file
        /// </summary>
        /// <param name="filename">file to load the Settings from</param>
        public void LoadSettingsFromFile(string filename)
        {
            AmeisenSettings.LoadFromFile(filename);
        }

        /// <summary>
        /// Save the current Settings to the given file
        /// </summary>
        /// <param name="filename">file to save the Settings to</param>
        public void SaveSettingsToFile(string filename)
        {
            AmeisenSettings.SaveToFile(filename);
        }

        /// <summary>
        /// Starts the bots mechanisms, hooks, ...
        /// </summary>
        /// <param name="wowExe">WowExe to start the bot on</param>
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
            IsBlackmagicAttached = Blackmagic.IsProcessOpen;
            // TODO: make this non static
            AmeisenCore.BlackMagic = Blackmagic;

            // Hook EndScene
            AmeisenHook = new AmeisenHook(Blackmagic);
            IsEndsceneHooked = AmeisenHook.isHooked;
            // TODO: make this non static
            AmeisenCore.AmeisenHook = AmeisenHook;

            // Start our object updates
            AmeisenObjectManager = new AmeisenObjectManager(AmeisenDataHolder, AmeisenDBManager);
            AmeisenObjectManager.Start();

            // Load the combatclass
            IAmeisenCombatClass combatClass = CompileAndLoadCombatClass(AmeisenSettings.Settings.combatClassPath);

            // Init our MovementEngine to hposition ourself according to our formation
            AmeisenMovementEngine = new AmeisenMovementEngine(new DefaultFormation());

            // Start the StateMachine
            AmeisenStateMachineManager = new AmeisenStateMachineManager(
                AmeisenDataHolder,
                AmeisenDBManager,
                AmeisenMovementEngine,
                combatClass);
            // Deafult Idle state
            AmeisenStateMachineManager.StateMachine.PushAction(BotState.Idle);
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

        /// <summary>
        /// Stops the bots mechanisms, hooks, ...
        /// </summary>
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
        }

        /// <summary>
        /// Add a RememberedUnit to the RememberedUnits Database to remember its position and UnitTraits
        /// </summary>
        /// <param name="rememberedUnit">Unit that you want to remember</param>
        public void RememberUnit(RememberedUnit rememberedUnit)
        {
            AmeisenDBManager.RememberUnit(rememberedUnit);
        }

        /// <summary>
        /// Check if we remember a Unit by its Name, ZoneID and MapID
        /// </summary>
        /// <param name="name">name of the npc</param>
        /// <param name="zoneID">zoneid of the npc</param>
        /// <param name="mapID">mapid of the npc</param>
        /// <returns>RememberedUnit with if we remember it, its UnitTraits and position</returns>
        public RememberedUnit CheckForRememberedUnit(string name, int zoneID, int mapID)
        {
            return AmeisenDBManager.CheckForRememberedUnit(name, zoneID, mapID);
        }

        /// <summary>
        /// Compile a CombatClass *.cs file and return its Instance
        /// </summary>
        /// <param name="combatclassPath">*.cs CombatClass file</param>
        /// <returns>Instance of the built Class, if its null somethings gone wrong</returns>
        private IAmeisenCombatClass CompileAndLoadCombatClass(string combatclassPath)
        {
            if (File.Exists(combatclassPath))
            {
                try
                {
                    return CompileCombatClass(combatclassPath);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Compilation error", MessageBoxButton.OK, MessageBoxImage.Error);
                    AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"Error while compiling CombatClass: {Path.GetFileName(combatclassPath)}", this);
                    AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"{e.Message}", this);
                }
            }

            return null;
        }

        /// <summary>
        /// Compile a combatclass *.cs file at runtime and load it into the bot
        /// </summary>
        /// <param name="combatclassPath">path to the *.cs file</param>
        /// <returns></returns>
        private IAmeisenCombatClass CompileCombatClass(string combatclassPath)
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"Compiling CombatClass: {Path.GetFileName(combatclassPath)}", this);

            CompilerParameters parameters = new CompilerParameters();
            // Include dependencies
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("./lib/AmeisenBot.Combat.dll");
            parameters.ReferencedAssemblies.Add("./lib/AmeisenBot.Utilities.dll");
            parameters.ReferencedAssemblies.Add("./lib/AmeisenBot.Logger.dll");
            parameters.ReferencedAssemblies.Add("./lib/AmeisenBot.Data.dll");
            parameters.GenerateInMemory = true; // generate no file
            parameters.GenerateExecutable = false; // to output a *.dll not a *.exe

            // compile it
            CompilerResults results = new CSharpCodeProvider().CompileAssemblyFromSource(parameters, File.ReadAllText(combatclassPath));

            if (results.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();

                foreach (CompilerError error in results.Errors)
                {
                    sb.AppendLine($"Error ({error.ErrorNumber}): {error.ErrorText}");
                }

                throw new InvalidOperationException(sb.ToString());
            }

            // Create Instance of CombatClass
            IAmeisenCombatClass result = (IAmeisenCombatClass)results.CompiledAssembly.CreateInstance("AmeisenBotCombat.CombatClass");

            AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"Successfully compiled CombatClass: {Path.GetFileName(combatclassPath)}", this);
            return result;
        }
    }
}