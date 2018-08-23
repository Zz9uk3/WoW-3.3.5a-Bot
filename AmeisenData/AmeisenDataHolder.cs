using AmeisenUtilities;
using System.Collections.Generic;

namespace AmeisenData
{
    public class AmeisenDataHolder
    {
        // Data to hold
        public Me Me { get; set; }

        public Unit Target { get; set; }
        public List<WoWObject> ActiveWoWObjects { get; set; }

        private static AmeisenDataHolder instance = null;
        private static readonly object padlock = new object();

        private AmeisenDataHolder()
        {
        }

        public static AmeisenDataHolder Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new AmeisenDataHolder();
                    return instance;
                }
            }
        }
    }
}