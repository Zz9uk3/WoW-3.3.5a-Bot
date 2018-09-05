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
                    {
                        instance = new AmeisenDataHolder();
                    }

                    return instance;
                }
            }
        }

        public List<WowObject> ActiveWoWObjects { get; set; }
        public double FollowDistance { get; set; }
        public bool IsAllowedToAttack { get; set; }
        public bool IsAllowedToBuff { get; set; }
        public bool IsAllowedToHeal { get; set; }
        public bool IsAllowedToTank { get; set; }
        public Me Me { get; set; }
        public Unit Target { get; set; }
        public bool IsAllowedToAssistParty { get; set; }
        public bool IsAllowedToFollowParty { get; set; }

        private static readonly object padlock = new object();
        private static AmeisenDataHolder instance = null;

        private AmeisenDataHolder()
        {
        }
    }
}