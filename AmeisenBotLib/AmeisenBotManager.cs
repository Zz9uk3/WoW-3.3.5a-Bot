using AmeisenAI;
using AmeisenCore;
using AmeisenCore.Objects;
using AmeisenLogging;
using AmeisenOffsets.Objects;
using AmeisenUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotLib
{
    public class AmeisenBotManager
    {
        private static AmeisenBotManager i;

        private AmeisenSettings ameisenSettings;
        private AmeisenCombatManager ameisenCombatManager;
        private AmeisenAIManager ameisenAIManager;
        private AmeisenManager ameisenManager;
        private AmeisenClient ameisenClient;

        private WoWExe wowExe;

        private AmeisenBotManager()
        {
            ameisenSettings = AmeisenSettings.GetInstance();
            ameisenClient = AmeisenClient.GetInstance();
        }

        public static AmeisenBotManager GetInstance()
        {
            if (i == null)
                i = new AmeisenBotManager();
            return i;
        }

        public void StartBot(WoWExe wowExe)
        {
            this.wowExe = wowExe;

            ameisenManager = AmeisenManager.GetInstance();
            // Attach the shit
            ameisenManager.AttachManager(wowExe.process);

            ameisenCombatManager = AmeisenCombatManager.GetInstance();
            ameisenAIManager = AmeisenAIManager.GetInstance();

            // TODO vvvv
            //AmeisenCore.AmeisenCore.AntiAFK();

            // Fire up the CombatEngine
            ameisenCombatManager.Start();
            // Fire up the AI
            ameisenAIManager.StartAI(ameisenSettings.Settings.botMaxThreads);
            
        }

        public void StopBot()
        {
            ameisenManager.GetAmeisenHook().DisposeHooking();
            ameisenAIManager.StopAI();
            ameisenCombatManager.Stop();
            AmeisenLogger.GetInstance().StopLogging();
        }

        public List<WoWExe> RunningWoWs { get { return AmeisenCore.AmeisenCore.GetRunningWoWs(); } }

        public void LoadCombatCLass(string fileName)
        {
            ameisenSettings.Settings.combatClassPath = fileName;
            ameisenSettings.SaveToFile(ameisenSettings.loadedconfName);

            ameisenCombatManager.ReloadCombatClass();
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

        #region Objects
        public WoWExe GetWowExe() { return wowExe; }
        public Me Me { get { return ameisenManager.Me; } }
        public Unit Target { get { return ameisenManager.Target; } }
        public List<WoWObject> WoWObjects { get { return ameisenManager.GetObjects(); } }
        #endregion

        #region Combat
        public bool IsSupposedToAttack { get { return ameisenManager.IsSupposedToAttack; } set { ameisenManager.IsSupposedToAttack = value; } }
        public bool IsSupposedToTank { get { return ameisenManager.IsSupposedToTank; } set { ameisenManager.IsSupposedToAttack = value; } }
        public bool IsSupposedToHeal { get { return ameisenManager.IsSupposedToHeal; } set { ameisenManager.IsSupposedToAttack = value; } }
        #endregion

        #region AI
        public void AddActionToAIQueue(AmeisenAction ameisenAction) { ameisenAIManager.AddActionToQueue(ameisenAction); }
        public void AddActionToAIQueue(ref AmeisenAction ameisenAction) { ameisenAIManager.AddActionToQueue(ref ameisenAction); }
        #endregion

        #region Settings
        public Settings Settings { get { return ameisenSettings.Settings; } }

        public void SaveSettingsToFile(string filename) { ameisenSettings.SaveToFile(filename); }
        public void LoadSettingsFromFile(string filename) { ameisenSettings.LoadFromFile(filename); }
        public string GetLoadedConfigName() { return ameisenSettings.loadedconfName; }
        #endregion
    }
}
