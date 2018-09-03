using Magic;
using System;
using System.Text;

namespace AmeisenUtilities
{
    public class Corpse : WoWObject
    {
        public Corpse(uint baseAddress, BlackMagic blackMagic) : base(baseAddress, blackMagic)
        {
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("CORPSE");
            sb.Append(" >> Address: " + BaseAddress.ToString("X"));
            sb.Append(" >> OwnerGUID: " + Owner);
            sb.Append(" >> GUID: " + Guid);
            sb.Append(" >> PosX: " + pos.X);
            sb.Append(" >> PosY: " + pos.Y);
            sb.Append(" >> PosZ: " + pos.Z);
            sb.Append(" >> Rotation: " + Rotation);
            sb.Append(" >> Distance: " + Distance);
            sb.Append(" >> MapID: " + MapID);
            sb.Append(" >> ZoneID: " + ZoneID);

            return sb.ToString();
        }

        public override void Update()
        {
            base.Update();

            pos.X = BlackMagicInstance.ReadFloat(BaseAddress + 0x24);
            pos.Y = BlackMagicInstance.ReadFloat(BaseAddress + 0x28);
            pos.Z = BlackMagicInstance.ReadFloat(BaseAddress + 0x2C);
            Rotation = BlackMagicInstance.ReadFloat(BaseAddress + 0x20);
            Owner = BlackMagicInstance.ReadUInt64(BaseAddress + 0x18);
        }

        private ulong Owner { get; set; }
    }
}