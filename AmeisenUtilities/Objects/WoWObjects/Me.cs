using Magic;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmeisenUtilities
{
    public class Me : Player
    {
        public int Exp { get; set; }
        public int MaxExp { get; set; }

        public uint PlayerBase { get; set; }

        public UnitState CurrentState { get; set; }

        public UInt64 TargetGUID { get; set; }

        public UInt64 PartyleaderGUID { get; set; }
        public List<UInt64> PartymemberGUIDs { get; set; }

        public Me(uint baseAddress, BlackMagic blackMagic) : base(baseAddress, blackMagic)
        {
            Update();
        }

        public override void Update()
        {
            base.Update();
            PlayerBase = BlackMagicInstance.ReadUInt(WoWOffsets.playerBase);
            PlayerBase = BlackMagicInstance.ReadUInt(PlayerBase + 0x34);
            PlayerBase = BlackMagicInstance.ReadUInt(PlayerBase + 0x24);

            Name = BlackMagicInstance.ReadASCIIString(WoWOffsets.playerName, 12);
            Exp = BlackMagicInstance.ReadInt(PlayerBase + 0x3794);
            MaxExp = BlackMagicInstance.ReadInt(PlayerBase + 0x3798);

            // Somehow this is really sketchy, need to replace this...
            uint castingState = BlackMagicInstance.ReadUInt((uint)BlackMagicInstance.MainModule.BaseAddress + WoWOffsets.localPlayerCharacterState);
            castingState = BlackMagicInstance.ReadUInt(castingState + WoWOffsets.localPlayerCharacterStateOffset1);
            castingState = BlackMagicInstance.ReadUInt(castingState + WoWOffsets.localPlayerCharacterStateOffset2);
            CurrentState = (UnitState)BlackMagicInstance.ReadInt(castingState + WoWOffsets.localPlayerCharacterStateOffset3);
            TargetGUID = BlackMagicInstance.ReadUInt64(BaseUnitFields + (0x12 * 4));

            PartymemberGUIDs = new List<UInt64>();

            PartyleaderGUID = BlackMagicInstance.ReadUInt64(WoWOffsets.partyLeader);
            if (PartyleaderGUID != 0)
            {
                PartymemberGUIDs.Add(BlackMagicInstance.ReadUInt64(WoWOffsets.partyPlayer1));
                PartymemberGUIDs.Add(BlackMagicInstance.ReadUInt64(WoWOffsets.partyPlayer2));
                PartymemberGUIDs.Add(BlackMagicInstance.ReadUInt64(WoWOffsets.partyPlayer3));
                PartymemberGUIDs.Add(BlackMagicInstance.ReadUInt64(WoWOffsets.partyPlayer4));
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("ME");
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

            if (TargetGUID != 0)
                sb.Append(" >> TargetGUID: " + TargetGUID.ToString());
            else
                sb.Append(" >> Target: none");

            sb.Append(" >> combatReach: " + CombatReach);
            sb.Append(" >> channelSpell: " + ChannelSpell);
            sb.Append(" >> currentState: " + CurrentState);
            sb.Append(" >> factionTemplate: " + FactionTemplate);
            sb.Append(" >> level: " + Level);
            sb.Append(" >> health: " + Health);
            sb.Append(" >> maxHealth: " + MaxHealth);
            sb.Append(" >> energy: " + Energy);
            sb.Append(" >> maxEnergy: " + MaxEnergy);

            sb.Append(" >> exp: " + Exp);
            sb.Append(" >> maxExp: " + MaxExp);

            sb.Append(" >> partyLeader: " + PartyleaderGUID);

            int count = 1;
            foreach (UInt64 guid in PartymemberGUIDs)
            {
                sb.Append(" >> partymember" + count + ": " + guid);
                count++;
            }
            return sb.ToString();
        }
    }
}