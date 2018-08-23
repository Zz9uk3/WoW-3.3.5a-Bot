using AmeisenUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magic;

namespace AmeisenUtilities
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

        public Unit(uint baseAddress, BlackMagic blackMagic) : base(baseAddress, blackMagic)
        {
            Update();
        }

        public override void Update()
        {
            base.Update();
            baseUnitFields = BlackMagicInstance.ReadUInt(BaseAddress + 0x8);

            pos.x = BlackMagicInstance.ReadFloat(BaseAddress + 0x798);
            pos.y = BlackMagicInstance.ReadFloat(BaseAddress + 0x79C);
            pos.z = BlackMagicInstance.ReadFloat(BaseAddress + 0x7A0);
            Rotation = BlackMagicInstance.ReadFloat(BaseAddress + 0x7A8);

            // too cpu heavy
            /*try
            {
                distance = Utils.GetDistance(pos, AmeisenManager.GetInstance().Me().pos);
            }
            catch { }*/

            summonedBy = BlackMagicInstance.ReadInt(baseUnitFields + (0xE * 4));
            factionTemplate = BlackMagicInstance.ReadInt(baseUnitFields + (0x37 * 4));
            level = BlackMagicInstance.ReadInt(baseUnitFields + (0x36 * 4));
            health = BlackMagicInstance.ReadInt(baseUnitFields + (0x18 * 4));
            maxHealth = BlackMagicInstance.ReadInt(baseUnitFields + (0x20 * 4));
            energy = BlackMagicInstance.ReadInt(baseUnitFields + (0x19 * 4));
            maxEnergy = BlackMagicInstance.ReadInt(baseUnitFields + (0x21 * 4));
            combatReach = BlackMagicInstance.ReadInt(baseUnitFields + (0x42 * 4));
            channelSpell = BlackMagicInstance.ReadInt(baseUnitFields + (0x16 * 4));
            //guid = BlackMagic.ReadUInt64(baseUnitFields + (0x12 * 4));

            try
            {
                Name = GetMobNameFromBase(BaseAddress);
            }
            catch { }
        }

        /// <summary>
        /// Get any NPC's name by its BaseAdress
        /// </summary>
        /// <param name="objBase">BaseAdress of the npc to search the name for</param>
        /// <returns>name of the npc</returns>
        public string GetMobNameFromBase(uint objBase)
        {
            uint objName = BlackMagicInstance.ReadUInt(objBase + 0x964);
            objName = BlackMagicInstance.ReadUInt(objName + 0x05C);

            return BlackMagicInstance.ReadASCIIString(objName, 24);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("UNIT");
            sb.Append(" >> Address: " + BaseAddress.ToString("X"));
            sb.Append(" >> UnitFields: " + baseUnitFields.ToString("X"));
            sb.Append(" >> Name: " + Name);
            sb.Append(" >> GUID: " + Guid);
            sb.Append(" >> PosX: " + pos.x);
            sb.Append(" >> PosY: " + pos.y);
            sb.Append(" >> PosZ: " + pos.z);
            sb.Append(" >> Rotation: " + Rotation);
            sb.Append(" >> Distance: " + Distance);
            sb.Append(" >> MapID: " + MapID);
            sb.Append(" >> ZoneID: " + ZoneID);

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
