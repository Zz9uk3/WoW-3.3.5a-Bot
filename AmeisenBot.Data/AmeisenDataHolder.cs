using AmeisenBotUtilities;
using System.Collections.Generic;

namespace AmeisenBotData
{
    public class AmeisenDataHolder
    {
        public List<WowObject> ActiveWoWObjects { get; set; }
        public double FollowDistance { get; set; }
        public bool IsAllowedToAssistParty { get; set; }
        public bool IsAllowedToAttack { get; set; }
        public bool IsAllowedToBuff { get; set; }
        public bool IsAllowedToFollowParty { get; set; }
        public bool IsAllowedToHeal { get; set; }
        public bool IsAllowedToTank { get; set; }
        public Me Me { get; set; }
        public Unit Target { get; set; }
        public bool IsAllowedToReleaseSpirit { get; set; }
        public bool IsAllowedToRevive { get; set; }
        public Settings Settings { get; set; }

        public AmeisenDataHolder()
        {
        }
    }
}