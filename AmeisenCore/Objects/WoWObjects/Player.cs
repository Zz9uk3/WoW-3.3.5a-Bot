using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenCore.Objects
{
    public class Player : Unit
    {
        public Player(uint baseAddress) : base(baseAddress)
        {
            try { name = AmeisenCore.GetPlayerNameFromGuid(guid); } catch { }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("PLAYER");
            sb.Append(" >> Name: " + name);
            sb.Append(" >> GUID: " + guid);
            sb.Append(" >> PosX: " + pos.x);
            sb.Append(" >> PosY: " + pos.y);
            sb.Append(" >> PosZ: " + pos.z);
            sb.Append(" >> Rotation: " + rotation);
            sb.Append(" >> Distance: " + distance);
            sb.Append(" >> MapID: " + mapID);
            sb.Append(" >> ZoneID: " + zoneID);

            /*if (target != null)
                sb.Append(" >> Target: " + target.guid);
            else
                sb.Append(" >> Target: null");*/

            sb.Append(" >> combatReach: " + combatReach);
            sb.Append(" >> channelSpell: " + channelSpell);
            sb.Append(" >> factionTemplate: " + factionTemplate);
            sb.Append(" >> level: " + level);
            sb.Append(" >> health: " + health);
            sb.Append(" >> maxHealth: " + maxHealth);
            sb.Append(" >> energy: " + energy);
            sb.Append(" >> maxEnergy: " + maxEnergy);

            return sb.ToString();
        }
    }
}
