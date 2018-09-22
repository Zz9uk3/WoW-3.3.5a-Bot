using Magic;
using System.Collections.Specialized;
using System.Text;

namespace AmeisenBotUtilities
{
    public partial class Unit : WowObject
    {
        public BitVector32 DynamicUFlags { get; set; }
        public int Energy { get; set; }
        public int Health { get; set; }
        public int HealthPercentage { get { return (Health / MaxHealth) * 100; } }
        public int EnergyPercentage { get { return (Energy / MaxEnergy) * 100; } }
        public bool InCombat { get { return UFlags[(int)UnitFlags.COMBAT]; } }
        public bool IsCasting { get; set; }
        public bool IsDead { get; set; }
        public bool IsLootable { get { return UFlags[(int)DynamicUnitFlags.LOOTABLE]; } }
        public int Level { get; set; }
        public int MaxEnergy { get; set; }
        public int MaxHealth { get; set; }
        public bool NeedToRevive { get { return Health == 0; } }
        public ulong TargetGuid { get; set; }
        public BitVector32 UFlags { get; set; }
        public BitVector32 UFlags2 { get; set; }

        public Unit(uint baseAddress, BlackMagic blackMagic) : base(baseAddress, blackMagic)
        {
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
            sb.Append($" >> Address: {BaseAddress.ToString("X")}");
            sb.Append($" >> Descriptor: {Descriptor.ToString("X")}");
            sb.Append($" >> InCombat: {InCombat.ToString()}");
            sb.Append($" >> Name: {Name}");
            sb.Append($" >> GUID: {Guid}");
            sb.Append($" >> PosX: {pos.X}");
            sb.Append($" >> PosY: {pos.Y}");
            sb.Append($" >> PosZ: {pos.Z}");
            sb.Append($" >> Rotation: {Rotation}");
            sb.Append($" >> Distance: {Distance}");
            sb.Append($" >> MapID: {MapID}");
            sb.Append($" >> ZoneID: {ZoneID}");
            sb.Append($" >> Target: {TargetGuid}");
            sb.Append($" >> level: {Level}");
            sb.Append($" >> health: {Health}");
            sb.Append($" >> maxHealth: {MaxHealth}");
            sb.Append($" >> energy: {Energy}");
            sb.Append($" >> maxEnergy: {MaxEnergy}");

            return sb.ToString();
        }

        public override void Update()
        {
            base.Update();

            if (Name == null)
            {
                try { Name = GetMobNameFromBase(BaseAddress); } catch { }
            }

            pos.X = BlackMagicInstance.ReadFloat(BaseAddress + 0x798);
            pos.Y = BlackMagicInstance.ReadFloat(BaseAddress + 0x79C);
            pos.Z = BlackMagicInstance.ReadFloat(BaseAddress + 0x7A0);
            Rotation = BlackMagicInstance.ReadFloat(BaseAddress + 0x7A8);

            TargetGuid = BlackMagicInstance.ReadUInt64(Descriptor + 0x48);

            int currentlyCastingID = BlackMagicInstance.ReadInt(BaseAddress + 0xA6C);
            int currentlyChannelingID = BlackMagicInstance.ReadInt(BaseAddress + 0xA80);

            int combined = currentlyCastingID + currentlyChannelingID;

            if (combined > 0)
            {
                IsCasting = true;
            }
            else
            {
                IsCasting = false;
            }

            // too cpu heavy
            /*try
            {
                distance = Utils.GetDistance(pos, AmeisenManager.GetInstance().Me().pos);
            }
            catch { }*/

            try
            {
                Level = BlackMagicInstance.ReadInt(Descriptor + 0xD8);
                Health = BlackMagicInstance.ReadInt(Descriptor + 0x60);
                MaxHealth = BlackMagicInstance.ReadInt(Descriptor + 0x80);
                Energy = BlackMagicInstance.ReadInt(Descriptor + 0x64);
                MaxEnergy = BlackMagicInstance.ReadInt(Descriptor + 0x84);
                //CombatReach = BlackMagicInstance.ReadInt(BaseUnitFields + (0x42 * 4));
                //ChannelSpell = BlackMagicInstance.ReadInt(BaseUnitFields + (0x16 * 4));
                //SummonedBy = BlackMagicInstance.ReadInt(BaseUnitFields + (0xE * 4));
                //FactionTemplate = BlackMagicInstance.ReadInt(BaseUnitFields + (0x37 * 4));
            }
            catch { }

            try
            {
                UFlags = (BitVector32)BlackMagicInstance.ReadObject(Descriptor + 0xEC, typeof(BitVector32));
            }
            catch { }

            try
            {
                UFlags2 = (BitVector32)BlackMagicInstance.ReadObject(Descriptor + 0xF0, typeof(BitVector32));
            }
            catch { }

            try
            {
                IsDead = BlackMagicInstance.ReadByte(Descriptor + 0x12B) == 1;
            }
            catch { }
        }
    }
}