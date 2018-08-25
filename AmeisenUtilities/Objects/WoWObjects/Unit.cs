using Magic;
using System.Text;

namespace AmeisenUtilities
{
    public partial class Unit : WoWObject
    {
        public uint BaseUnitFields { get; set; }

        public int CombatReach { get; set; }
        public int ChannelSpell { get; set; }
        public int FactionTemplate { get; set; }
        public int SummonedBy { get; set; }

        public int Level { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Energy { get; set; }
        public int MaxEnergy { get; set; }

        public Unit(uint baseAddress, BlackMagic blackMagic) : base(baseAddress, blackMagic)
        {
            Update();
        }

        public override void Update()
        {
            base.Update();
            BaseUnitFields = BlackMagicInstance.ReadUInt(BaseAddress + 0x8);

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

            try
            {
                SummonedBy = BlackMagicInstance.ReadInt(BaseUnitFields + (0xE * 4));
                FactionTemplate = BlackMagicInstance.ReadInt(BaseUnitFields + (0x37 * 4));
                Level = BlackMagicInstance.ReadInt(BaseUnitFields + (0x36 * 4));
                Health = BlackMagicInstance.ReadInt(BaseUnitFields + (0x18 * 4));
                MaxHealth = BlackMagicInstance.ReadInt(BaseUnitFields + (0x20 * 4));
                Energy = BlackMagicInstance.ReadInt(BaseUnitFields + (0x19 * 4));
                MaxEnergy = BlackMagicInstance.ReadInt(BaseUnitFields + (0x21 * 4));
                CombatReach = BlackMagicInstance.ReadInt(BaseUnitFields + (0x42 * 4));
                ChannelSpell = BlackMagicInstance.ReadInt(BaseUnitFields + (0x16 * 4));
            }
            catch { }

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
            sb.Append(" >> UnitFields: " + BaseUnitFields.ToString("X"));
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

            sb.Append(" >> combatReach: " + CombatReach);
            sb.Append(" >> channelSpell: " + ChannelSpell);
            sb.Append(" >> factionTemplate: " + FactionTemplate);
            sb.Append(" >> level: " + Level);
            sb.Append(" >> health: " + Health);
            sb.Append(" >> maxHealth: " + MaxHealth);
            sb.Append(" >> energy: " + Energy);
            sb.Append(" >> maxEnergy: " + MaxEnergy);

            return sb.ToString();
        }
    }
}