using AmeisenUtilities;
using Magic;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmeisenUtilities
{
    public class Me : Player
    {
        public int exp;
        public int maxExp;

        public uint playerBase;

        public UnitState currentState;

        public UInt64 targetGUID;

        public Unit partyLeader;
        public List<Unit> partymembers;

        public Me(uint baseAddress, BlackMagic blackMagic) : base(baseAddress, blackMagic)
        {
            Update();
        }

        public override void Update()
        {
            base.Update();
            playerBase = BlackMagicInstance.ReadUInt(WoWOffsets.playerBase);
            playerBase = BlackMagicInstance.ReadUInt(playerBase + 0x34);
            playerBase = BlackMagicInstance.ReadUInt(playerBase + 0x24);

            Name = BlackMagicInstance.ReadASCIIString(WoWOffsets.playerName, 12);
            exp = BlackMagicInstance.ReadInt(playerBase + 0x3794);
            maxExp = BlackMagicInstance.ReadInt(playerBase + 0x3798);

            // Somehow this is really sketchy, need to replace this...
            uint castingState = BlackMagicInstance.ReadUInt((uint)BlackMagicInstance.MainModule.BaseAddress + WoWOffsets.localPlayerCharacterState);
            castingState = BlackMagicInstance.ReadUInt(castingState + WoWOffsets.localPlayerCharacterStateOffset1);
            castingState = BlackMagicInstance.ReadUInt(castingState + WoWOffsets.localPlayerCharacterStateOffset2);
            currentState = (UnitState)BlackMagicInstance.ReadInt(castingState + WoWOffsets.localPlayerCharacterStateOffset3);
            targetGUID = BlackMagicInstance.ReadUInt64(baseUnitFields + (0x12 * 4));

            partymembers = new List<Unit>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("ME");
            sb.Append(" >> Address: " + BaseAddress.ToString("X"));
            sb.Append(" >> UnitFields: " + baseUnitFields.ToString("X"));
            sb.Append(" >> Name: " + Name);
            sb.Append(" >> GUID: " + Guid);
            sb.Append(" >> PosX: " + pos.x);
            sb.Append(" >> PosY: " + pos.y);
            sb.Append(" >> PosZ: " + pos.z);
            sb.Append(" >> Rotation: " + Rotation);
            sb.Append(" >> Distance: " + Distance);
            sb.Append(" >> MapID: " + MapID);
            sb.Append(" >> ZoneID: " + ZoneID);

            if (targetGUID != 0)
                sb.Append(" >> TargetGUID: " + targetGUID.ToString());
            else
                sb.Append(" >> Target: none");

            sb.Append(" >> combatReach: " + combatReach);
            sb.Append(" >> channelSpell: " + channelSpell);
            sb.Append(" >> currentState: " + currentState);
            sb.Append(" >> factionTemplate: " + factionTemplate);
            sb.Append(" >> level: " + level);
            sb.Append(" >> health: " + health);
            sb.Append(" >> maxHealth: " + maxHealth);
            sb.Append(" >> energy: " + energy);
            sb.Append(" >> maxEnergy: " + maxEnergy);

            sb.Append(" >> exp: " + exp);
            sb.Append(" >> maxExp: " + maxExp);

            if (partyLeader != null)
                sb.Append(" >> partyLeader: " + partyLeader.Guid);
            else
                sb.Append(" >> partyLeader: none");

            int count = 1;
            foreach (Player p in partymembers)
            {
                if (p != null)
                {
                    sb.Append(" >> partymember" + count + ": " + p.Guid);
                    count++;
                }
            }
            return sb.ToString();
        }
    }
}
