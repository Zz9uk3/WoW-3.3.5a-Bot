using AmeisenUtilities;
using System;
using System.Text;

namespace AmeisenCore.Objects
{
    public class WoWObject
    {
        public string name;

        public UInt64 guid;

        public uint baseAddress;
        
        public Vector3 pos;
        public float rotation;
        public double distance;

        public int mapID;
        public int zoneID;

        public WoWObject(uint baseAddress)
        {
            this.baseAddress = baseAddress;
        }

        public virtual void Update()
        {
            guid = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt64(baseAddress + 0x30);
            mapID = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(WoWOffsets.mapID);
            zoneID = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(WoWOffsets.zoneID);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("WOWOBJECT");
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
