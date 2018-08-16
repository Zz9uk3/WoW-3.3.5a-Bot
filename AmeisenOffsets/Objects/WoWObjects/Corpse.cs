using Magic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenCore.Objects
{
    public class Corpse : WoWObject
    {
        UInt64 Owner { get; set; }

        public Corpse(uint baseAddress, BlackMagic blackMagic) : base(baseAddress, blackMagic)
        {
            Update();
        }

        public override void Update()
        {
            base.Update();

            pos.x = BlackMagicInstance.ReadFloat(BaseAddress + 0x24);
            pos.y = BlackMagicInstance.ReadFloat(BaseAddress + 0x28);
            pos.z = BlackMagicInstance.ReadFloat(BaseAddress + 0x2C);
            Rotation = BlackMagicInstance.ReadFloat(BaseAddress + 0x20);
            Owner = BlackMagicInstance.ReadUInt64(BaseAddress + 0x18);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("CORPSE");
            sb.Append(" >> Address: " + BaseAddress.ToString("X"));
            sb.Append(" >> OwnerGUID: " + Owner);
            sb.Append(" >> GUID: " + Guid);
            sb.Append(" >> PosX: " + pos.x);
            sb.Append(" >> PosY: " + pos.y);
            sb.Append(" >> PosZ: " + pos.z);
            sb.Append(" >> Rotation: " + Rotation);
            sb.Append(" >> Distance: " + Distance);
            sb.Append(" >> MapID: " + MapID);
            sb.Append(" >> ZoneID: " + ZoneID);

            return sb.ToString();
        }
    }
}
