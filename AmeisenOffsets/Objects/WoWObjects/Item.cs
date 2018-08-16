using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenCore.Objects
{
    public class Item : WoWObject
    {
        public Item(uint baseAddress) : base(baseAddress)
        {
            Update();
        }

        public override void Update()
        {
            base.Update();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("ITEM");
            sb.Append(" >> Address: " + baseAddress.ToString("X"));
            sb.Append(" >> Name: " + name);
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
