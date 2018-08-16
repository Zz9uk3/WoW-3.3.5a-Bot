using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenCore.Objects
{
    public class Corpse : WoWObject
    {
        UInt64 owner;

        public Corpse(uint baseAddress) : base(baseAddress)
        {
            Update();
        }

        public override void Update()
        {
            base.Update();

            pos.x = AmeisenManager.GetInstance().GetBlackMagic().ReadFloat(baseAddress + 0x24);
            pos.y = AmeisenManager.GetInstance().GetBlackMagic().ReadFloat(baseAddress + 0x28);
            pos.z = AmeisenManager.GetInstance().GetBlackMagic().ReadFloat(baseAddress + 0x2C);
            rotation = AmeisenManager.GetInstance().GetBlackMagic().ReadFloat(baseAddress + 0x20);
            owner = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt64(baseAddress + 0x18);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("CORPSE");
            sb.Append(" >> Address: " + baseAddress.ToString("X"));
            sb.Append(" >> OwnerGUID: " + owner);
            sb.Append(" >> GUID: " + guid);
            sb.Append(" >> PosX: " + pos.x);
            sb.Append(" >> PosY: " + pos.y);
            sb.Append(" >> PosZ: " + pos.z);
            sb.Append(" >> Rotation: " + rotation);
            sb.Append(" >> Distance: " + distance);
            sb.Append(" >> MapID: " + mapID);
            sb.Append(" >> ZoneID: " + zoneID);

            return sb.ToString();
        }
    }
}
