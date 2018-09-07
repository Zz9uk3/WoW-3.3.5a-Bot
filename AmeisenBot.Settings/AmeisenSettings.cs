using AmeisenUtilities;
using System;
using System.IO;

namespace AmeisenSettings
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

        public Settings Settings { get; private set; }
        public string loadedconfName;

        private AmeisenSettings()
        {
            Settings = new Settings();
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
            File.WriteAllText(configPath + filename.ToLower() + extension, Newtonsoft.Json.JsonConvert.SerializeObject(Settings));
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
                Settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(File.ReadAllText(configPath + filename.ToLower() + extension));
            else
            {
                // Load default settings
                Settings = new Settings();

                SaveToFile(filename);
            }

            loadedconfName = filename;
        }
    }
}
