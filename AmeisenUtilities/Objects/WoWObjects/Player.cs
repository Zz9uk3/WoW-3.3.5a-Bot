using Magic;
using System;
using System.Text;

namespace AmeisenUtilities
{
    public class Player : Unit
    {
        public Player(uint baseAddress, BlackMagic blackMagic) : base(baseAddress, blackMagic)
        {
            Update();
        }

        public override void Update()
        {
            base.Update();

            if (Name == null)
                try { Name = GetPlayerNameFromGuid(Guid); } catch { }
        }

        /// <summary>
        /// Get a player's name from its GUID
        /// </summary>
        /// <param name="guid">player's GUID</param>
        /// <returns>name of the player</returns>
        public string GetPlayerNameFromGuid(UInt64 guid)
        {
            uint playerMask, playerBase, shortGUID, testGUID, offset, current;

            playerMask = BlackMagicInstance.ReadUInt((WoWOffsets.nameStore + WoWOffsets.nameMask));
            playerBase = BlackMagicInstance.ReadUInt((WoWOffsets.nameStore + WoWOffsets.nameBase));

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

            return BlackMagicInstance.ReadASCIIString(current + WoWOffsets.nameString, 12);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("PLAYER");
            sb.Append(" >> Address: " + BaseAddress.ToString("X"));
            sb.Append(" >> UnitFields: " + BaseUnitFields.ToString("X"));
            sb.Append(" >> Name: " + Name);
            sb.Append(" >> GUID: " + Guid);
            sb.Append(" >> PosX: " + pos.x);
            sb.Append(" >> PosY: " + pos.y);
            sb.Append(" >> PosZ: " + pos.z);
            sb.Append(" >> Rotation: " + Rotation);
            sb.Append(" >> Distance: " + Distance);
            sb.Append(" >> MapID: " + MapID);
            sb.Append(" >> ZoneID: " + ZoneID);

            /*if (target != null)
                sb.Append(" >> Target: " + target.guid);
            else
                sb.Append(" >> Target: null");*/

            sb.Append(" >> combatReach: " + CombatReach);
            sb.Append(" >> channelSpell: " + ChannelSpell);
            sb.Append(" >> factionTemplate: " + FactionTemplate);
            sb.Append(" >> level: " + Level);
            sb.Append(" >> health: " + Health);
            sb.Append(" >> maxHealth: " + MaxHealth);
            sb.Append(" >> energy: " + Energy);
            sb.Append(" >> maxEnergy: " + MaxEnergy);

            return sb.ToString();
        }
    }
}