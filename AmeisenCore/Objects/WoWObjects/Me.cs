using System.Collections.Generic;
using System.Text;

namespace AmeisenCore.Objects
{
    public class Me : Player
    {
        public int exp;
        public int maxExp;

        public Unit partyLeader;
        public List<Unit> partymembers;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("ME");
            sb.Append(" >> Name: " + name);
            sb.Append(" >> GUID: " + guid);
            sb.Append(" >> PosX: " + pos.x);
            sb.Append(" >> PosY: " + pos.y);
            sb.Append(" >> PosZ: " + pos.z);
            sb.Append(" >> Rotation: " + rotation);
            sb.Append(" >> Distance: " + distance);
            sb.Append(" >> MapID: " + mapID);
            sb.Append(" >> ZoneID: " + zoneID);

            sb.Append(" >> Target: " + target.ToString());
            sb.Append(" >> combatReach: " + combatReach);
            sb.Append(" >> channelSpell: " + channelSpell);
            sb.Append(" >> currentState: " + currentState);
            sb.Append(" >> factionTemplate: " + factionTemplate);
            sb.Append(" >> level: " + level);
            sb.Append(" >> health: " + health);
            sb.Append(" >> maxHealth: " + maxHealth);
            sb.Append(" >> energy: " + energy);
            sb.Append(" >> maxEnergy: " + maxEnergy);

            sb.Append(" >> exp: " + exp);
            sb.Append(" >> maxExp: " + maxExp);
            sb.Append(" >> partyLeader: " + partyLeader.guid);

            int count = 1;
            foreach (Player p in partymembers)
            {
                sb.Append(" >> partymember" + count + ": " + p.guid);
                count++;
            }
            return sb.ToString();
        }
    }
}
