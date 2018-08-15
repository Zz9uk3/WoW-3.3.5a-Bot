using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenCore.Objects
{
    public class DynObject : WoWObject
    {
        public DynObject(uint baseAddress) : base(baseAddress)
        {

        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("DYNOBJECT");
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
