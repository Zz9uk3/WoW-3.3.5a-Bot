using AmeisenUtilities;
using System;
using System.Text;

namespace AmeisenCore.Objects
{
    public class WoWObject
    {
        public string name;

        public UInt64 guid;
        public UInt64 targetGUID;
        public bool isPartyLeader;

        public int combatReach;
        public int channelSpell;
        public int factionTemplate;

        public int objectType;
        public int summonedBy;

        public int level;
        public int health;
        public int maxHealth;
        public int energy;
        public int maxEnergy;

        public Vector3 pos;
        public float rotation;
        public double distance;

        public int mapID;
        public int zoneID;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\nName: " + name);
            sb.Append("\nGUID: " + guid);
            sb.Append("\nTargetGUID: " + targetGUID);
            sb.Append("\nisPartyLeader:" + isPartyLeader);
            sb.Append("\nCombatReach: " + combatReach);
            sb.Append("\nChannelSpell: " + channelSpell);
            sb.Append("\nFactionTemplate: " + factionTemplate);
            sb.Append("\nObjectType: " + objectType);
            sb.Append("\nSummonedBy: " + summonedBy);
            sb.Append("\nLevel: " + level);
            sb.Append("\nHealth: " + health);
            sb.Append("\nMaxHealth: " + maxHealth);
            sb.Append("\nEnergy: " + energy);
            sb.Append("\nMaxEnergy: " + maxEnergy);
            sb.Append("\nPosX: " + pos.x);
            sb.Append("\nPosY: " + pos.y);
            sb.Append("\nPosZ: " + pos.z);
            sb.Append("\nRotation: " + rotation);
            sb.Append("\nDistance: " + distance);
            sb.Append("\nMapID: " + mapID);
            sb.Append("\nZoneID: " + zoneID);

            return sb.ToString();
        }

        public string ToShortString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\nName: " + name);
            sb.Append("\nGUID: " + guid);
            sb.Append("\nTargetGUID: " + targetGUID);
            sb.Append("\nisPartyLeader: " + isPartyLeader);
            sb.Append("\nLevel: " + level);
            sb.Append("\nHealth: " + health);
            sb.Append("\nMaxHealth: " + maxHealth);
            sb.Append("\nEnergy: " + energy);
            sb.Append("\nMaxEnergy: " + maxEnergy);
            sb.Append("\nPosX: " + pos.x);
            sb.Append("\nPosY: " + pos.y);
            sb.Append("\nPosZ: " + pos.z);
            sb.Append("\nRotation: " + rotation);
            sb.Append("\nDistance: " + distance);
            sb.Append("\nMapID: " + mapID);
            sb.Append("\nZoneID: " + zoneID);

            return sb.ToString();
        }
    }
}
