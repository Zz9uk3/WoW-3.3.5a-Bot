using AmeisenUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenCore.Objects
{
    public partial class Unit : WoWObject
    {
        public uint baseUnitFields;

        public int combatReach;
        public int channelSpell;
        public int factionTemplate;
        public int summonedBy;

        public int level;
        public int health;
        public int maxHealth;
        public int energy;
        public int maxEnergy;

        public Unit(uint baseAddress) : base(baseAddress)
        {
            baseUnitFields = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(baseAddress + 0x8);

            summonedBy = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(baseUnitFields + (0xE * 4));
            factionTemplate = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(baseUnitFields + (0x37 * 4));
            level = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(baseUnitFields + (0x36 * 4));
            health = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(baseUnitFields + (0x18 * 4));
            maxHealth = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(baseUnitFields + (0x20 * 4));
            energy = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(baseUnitFields + (0x19 * 4));
            maxEnergy = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(baseUnitFields + (0x21 * 4));
            combatReach = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(baseUnitFields + (0x42 * 4));
            channelSpell = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(baseUnitFields + (0x16 * 4));
            //guid = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt64(baseUnitFields + (0x12 * 4));

            name = AmeisenCore.GetMobNameFromBase(baseAddress);

            if (name == "")
                name = AmeisenCore.GetPlayerNameFromGuid(guid);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("UNIT");
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
