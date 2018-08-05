using AmeisenUtilities;
using System;
using System.Text;

namespace AmeisenCore.Objects
{
    public abstract class WoWObject
    {
        public string name;

        public UInt64 guid;
        public uint memoryLocation;

        public int summonedBy;

        public Vector3 pos;
        public float rotation;
        public double distance;

        public int mapID;
        public int zoneID;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("WOWOBJECT");
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
