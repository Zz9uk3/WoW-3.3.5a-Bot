using AmeisenUtilities;
using System.Collections.Generic;

namespace AmeisenData
{
    public class AmeisenDataHolder
    {
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

        public List<WoWObject> ActiveWoWObjects { get; set; }

        // Data to hold
        public Me Me { get; set; }

        public Unit Target { get; set; }
        private static readonly object padlock = new object();

        private static AmeisenDataHolder instance = null;

        private AmeisenDataHolder()
        {
        }
    }
}