using Magic;
using System;
using System.Text;

namespace AmeisenUtilities
{
    public class WoWObject
    {
        #region Public Fields

        public Vector3 pos;

        #endregion Public Fields

        #region Public Constructors

        public WoWObject(uint baseAddress, BlackMagic blackMagic)
        {
            BaseAddress = baseAddress;
            BlackMagicInstance = blackMagic;

            Descriptor = BlackMagicInstance.ReadUInt(BaseAddress + 0x8);

            Guid = BlackMagicInstance.ReadUInt64(BaseAddress + 0x30);
            MapID = BlackMagicInstance.ReadInt(WoWOffsets.mapID);
            ZoneID = BlackMagicInstance.ReadInt(WoWOffsets.zoneID);

            Update();
        }

        #endregion Public Constructors

        #region Public Properties

        public uint BaseAddress { get; set; }
        public BlackMagic BlackMagicInstance { get; set; }

        public uint Descriptor { get; set; }
        public double Distance { get; set; }
        public UInt64 Guid { get; set; }
        public int MapID { get; set; }
        public string Name { get; set; }
        public float Rotation { get; set; }
        public int ZoneID { get; set; }

        #endregion Public Properties

        #region Public Methods

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("WOWOBJECT");
            sb.Append(" >> Address: " + BaseAddress.ToString("X"));
            sb.Append(" >> Descriptor: " + Descriptor.ToString("X"));
            sb.Append(" >> Name: " + Name);
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

        public virtual void Update()
        {
        }

        #endregion Public Methods
    }
}