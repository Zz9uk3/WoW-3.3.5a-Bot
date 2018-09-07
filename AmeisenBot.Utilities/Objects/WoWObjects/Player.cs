using Magic;
using System.Text;

namespace AmeisenBotUtilities
{
    public class Player : Unit
    {
        public Player(uint baseAddress, BlackMagic blackMagic) : base(baseAddress, blackMagic)
        {
        }

        /// <summary>
        /// Get a player's name from its GUID
        /// </summary>
        /// <param name="guid">player's GUID</param>
        /// <returns>name of the player</returns>
        public string GetPlayerNameFromGuid(ulong guid)
        {
            uint playerMask, playerBase, shortGUID, testGUID, offset, current;

            playerMask = BlackMagicInstance.ReadUInt((Offsets.nameStore + Offsets.nameMask));
            playerBase = BlackMagicInstance.ReadUInt((Offsets.nameStore + Offsets.nameBase));

            // Shorten the GUID
            shortGUID = (uint)guid & 0xfffffff;
            offset = 12 * (playerMask & shortGUID);

            current = BlackMagicInstance.ReadUInt(playerBase + offset + 8);
            offset = BlackMagicInstance.ReadUInt(playerBase + offset);

            // Check for empty name
            if ((current & 0x1) == 0x1) { return ""; }

            testGUID = BlackMagicInstance.ReadUInt(current);

            while (testGUID != shortGUID)
            {
                current = BlackMagicInstance.ReadUInt(current + offset + 4);

                // Check for empty name
                if ((current & 0x1) == 0x1) { return ""; }
                testGUID = BlackMagicInstance.ReadUInt(current);
            }

            return BlackMagicInstance.ReadASCIIString(current + Offsets.nameString, 12);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("PLAYER");
            sb.Append($" >> Address: {BaseAddress.ToString("X")}");
            sb.Append($" >> Descriptor: {Descriptor.ToString("X")}");
            sb.Append($" >> InCombat: {InCombat.ToString()}");
            sb.Append($" >> Name: {Name}");
            sb.Append($" >> GUID: {Guid}");
            sb.Append($" >> PosX: {pos.X}");
            sb.Append($" >> PosY: {pos.Y}");
            sb.Append($" >> PosZ: {pos.Z}");
            sb.Append($" >> Rotation: {Rotation}");
            sb.Append($" >> Distance: {Distance}");
            sb.Append($" >> MapID: {MapID}");
            sb.Append($" >> ZoneID: {ZoneID}");
            sb.Append($" >> Target: {TargetGuid}");
            sb.Append($" >> level: {Level}");
            sb.Append($" >> health: {Health}");
            sb.Append($" >> maxHealth: {MaxHealth}");
            sb.Append($" >> energy: {Energy}");
            sb.Append($" >> maxEnergy: {MaxEnergy}");

            return sb.ToString();
        }

        public override void Update()
        {
            base.Update();

            if (Name == null)
                try { Name = GetPlayerNameFromGuid(Guid); } catch { }
        }
    }
}