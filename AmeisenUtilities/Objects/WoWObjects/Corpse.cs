using Magic;
using System;
using System.Text;

namespace AmeisenUtilities
{
    public class Corpse : WoWObject
    {
        #region Public Constructors

        public Corpse(uint baseAddress, BlackMagic blackMagic) : base(baseAddress, blackMagic)
        {
            Update();
        }

        #endregion Public Constructors

        #region Private Properties

        private UInt64 Owner { get; set; }

        #endregion Private Properties

        #region Public Methods

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

        public override void Update()
        {
            base.Update();

            pos.x = BlackMagicInstance.ReadFloat(BaseAddress + 0x24);
            pos.y = BlackMagicInstance.ReadFloat(BaseAddress + 0x28);
            pos.z = BlackMagicInstance.ReadFloat(BaseAddress + 0x2C);
            Rotation = BlackMagicInstance.ReadFloat(BaseAddress + 0x20);
            Owner = BlackMagicInstance.ReadUInt64(BaseAddress + 0x18);
        }

        #endregion Public Methods
    }
}