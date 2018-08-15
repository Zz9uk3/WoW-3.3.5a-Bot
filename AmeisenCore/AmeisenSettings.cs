using System;
using System.IO;

namespace AmeisenCore
{
    /// <summary>
    /// Singleton class to manage our currently loaded settings.
    /// 
    /// Load a settings file by calling LoadFromFile("EXAMPLE"),
    /// save it by calling SaveToFile("EXAMPLE").
    /// 
    /// The currently loaded settings name will be available in
    /// the loadedconfName variable.
    /// </summary>
    public class AmeisenSettings
    {
        private static AmeisenSettings i;

        private static readonly string configPath = AppDomain.CurrentDomain.BaseDirectory + "/config/";
        private static readonly string extension = ".json";

        public Settings settings;
        public string loadedconfName;

        private AmeisenSettings()
        {

        }

        public static AmeisenSettings GetInstance()
        {
            if (i == null)
                i = new AmeisenSettings();
            return i;
        }

        /// <summary>
        /// Save a config file from ./config/ folder by its name as a JSON.
        /// </summary>
        /// <param name="filename">Filename without extension</param>
        public void SaveToFile(string filename)
        {
            if (!Directory.Exists(configPath))
                Directory.CreateDirectory(configPath);

            // Serialize our object with the help of NewtosoftJSON
            File.WriteAllText(configPath + filename.ToLower() + extension, Newtonsoft.Json.JsonConvert.SerializeObject(settings));
        }

        /// <summary>
        /// Load a config file from ./config/ folder by its name. No need to append .json to the end.
        /// </summary>
        /// <param name="filename">Filename without extension</param>
        public void LoadFromFile(string filename)
        {
            if (!Directory.Exists(configPath))
                Directory.CreateDirectory(configPath);

            if (File.Exists(configPath + filename.ToLower() + extension))
                // Deserialize our object with the help of NewtosoftJSON
                settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(File.ReadAllText(configPath + filename.ToLower() + extension));
            else
            {
                // Load default settings
                settings = new Settings();

                SaveToFile(filename);
            }

            loadedconfName = filename;
        }
    }

    /// <summary>
    /// Class containing the default and loaded settings
    /// </summary>
    public class Settings
    {
        public int dataRefreshRate = 250;
        public int botMaxThreads = 2;

        public string accentColor = "#FFAAAAAA";
        public string fontColor = "#FFFFFFFF";
        public string backgroundColor = "#FF303030";

<<<<<<< HEAD
        public string combatClassName = "none";

        public string ameisenServerIP = "127.0.0.1";
        public string ameisenServerPort = "16200";
        public string ameisenServerName = AmeisenUtilities.Utils.GenerateRandonString(12, "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");
=======
        public string combatClassPath = "none";

        public bool behaviourAttack = false;
        public bool behaviourHeal = false;
        public bool behaviourTank = false;

        public bool followMaster = false;

        public string masterName = "";
>>>>>>> 0f341a9d01f4341d5ad3f14b13b8997e983d5eeb
    }
}
