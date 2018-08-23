using AmeisenUtilities;
using Magic;
using System;
using System.Text;

namespace AmeisenUtilities
{
    public class WoWObject
    {
        public BlackMagic BlackMagicInstance { get; set; }

        public string Name { get; set; }

        public UInt64 Guid { get; set; }

        public uint BaseAddress { get; set; }

        public Vector3 pos;
        public float Rotation { get; set; }
        public double Distance { get; set; }

        public int MapID { get; set; }
        public int ZoneID { get; set; }

        public WoWObject(uint baseAddress, BlackMagic blackMagic)
        {
            BaseAddress = baseAddress;
            BlackMagicInstance = blackMagic;
        }

        public virtual void Update()
        {
            Guid = BlackMagicInstance.ReadUInt64(BaseAddress + 0x30);
            MapID = BlackMagicInstance.ReadInt(WoWOffsets.mapID);
            ZoneID = BlackMagicInstance.ReadInt(WoWOffsets.zoneID);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("WOWOBJECT");
            sb.Append(" >> Address: " + BaseAddress.ToString("X"));
            sb.Append(" >> Name: " + Name);
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
