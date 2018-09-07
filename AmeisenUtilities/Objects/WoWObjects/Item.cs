using Magic;
using System.Text;

namespace AmeisenBotUtilities
{
    public class Item : WowObject
    {
        public Item(uint baseAddress, BlackMagic blackMagic) : base(baseAddress, blackMagic)
        {
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("ITEM");
            sb.Append($" >> Address: {BaseAddress.ToString("X")}");
            sb.Append($" >> Name: {Name}");
            sb.Append($" >> GUID: {Guid}");
            sb.Append($" >> PosX: {pos.X}");
            sb.Append($" >> PosY: {pos.Y}");
            sb.Append($" >> PosZ: {pos.Z}");
            sb.Append($" >> Rotation: {Rotation}");
            sb.Append($" >> Distance: {Distance}");
            sb.Append($" >> MapID: {MapID}");
            sb.Append($" >> ZoneID: {ZoneID}");

            return sb.ToString();
        }

        public override void Update()
        {
            base.Update();
        }
    }
}