using Magic;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmeisenUtilities
{
    public class Me : Player
    {
        public Me(uint baseAddress, BlackMagic blackMagic) : base(baseAddress, blackMagic)
        {
            Update();
        }

        public UnitState CurrentState { get; set; }
        public int Exp { get; set; }
        public int MaxExp { get; set; }
        public ulong PartyleaderGUID { get; set; }
        public List<ulong> PartymemberGuids { get; set; }
        public uint PlayerBase { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("ME");
            sb.Append(" >> Address: " + BaseAddress.ToString("X"));
            sb.Append(" >> Descriptor: " + Descriptor.ToString("X"));
            sb.Append(" >> InCombat: " + InCombat.ToString());
            sb.Append(" >> Name: " + Name);
            sb.Append(" >> GUID: " + Guid);
            sb.Append(" >> PosX: " + pos.X);
            sb.Append(" >> PosY: " + pos.Y);
            sb.Append(" >> PosZ: " + pos.Z);
            sb.Append(" >> Rotation: " + Rotation);
            sb.Append(" >> Distance: " + Distance);
            sb.Append(" >> MapID: " + MapID);
            sb.Append(" >> ZoneID: " + ZoneID);

            if (TargetGuid != 0)
                sb.Append(" >> TargetGUID: " + TargetGuid.ToString());
            else
                sb.Append(" >> Target: none");
            sb.Append(" >> currentState: " + CurrentState);
            sb.Append(" >> level: " + Level);
            sb.Append(" >> health: " + Health);
            sb.Append(" >> maxHealth: " + MaxHealth);
            sb.Append(" >> energy: " + Energy);
            sb.Append(" >> maxEnergy: " + MaxEnergy);

            sb.Append(" >> exp: " + Exp);
            sb.Append(" >> maxExp: " + MaxExp);

            sb.Append(" >> partyLeader: " + PartyleaderGUID);

            int count = 1;
            foreach (ulong guid in PartymemberGuids)
            {
                sb.Append(" >> partymember" + count + ": " + guid);
                count++;
            }
            return sb.ToString();
        }

        public override void Update()
        {
            base.Update();

            if (PlayerBase == 0)
            {
                PlayerBase = BlackMagicInstance.ReadUInt(Offsets.playerBase);
                PlayerBase = BlackMagicInstance.ReadUInt(PlayerBase + 0x34);
                PlayerBase = BlackMagicInstance.ReadUInt(PlayerBase + 0x24);
            }

            Name = BlackMagicInstance.ReadASCIIString(Offsets.playerName, 12);
            Exp = BlackMagicInstance.ReadInt(PlayerBase + 0x3794);
            MaxExp = BlackMagicInstance.ReadInt(PlayerBase + 0x3798);

            // Somehow this is really sketchy, need to replace this...
            uint castingState = BlackMagicInstance.ReadUInt((uint)BlackMagicInstance.MainModule.BaseAddress + Offsets.localPlayerCharacterState);
            castingState = BlackMagicInstance.ReadUInt(castingState + Offsets.localPlayerCharacterStateOffset1);
            castingState = BlackMagicInstance.ReadUInt(castingState + Offsets.localPlayerCharacterStateOffset2);
            CurrentState = (UnitState)BlackMagicInstance.ReadInt(castingState + Offsets.localPlayerCharacterStateOffset3);

            TargetGuid = BlackMagicInstance.ReadUInt64(Descriptor + 0x48);

            PartymemberGuids = new List<ulong>();
            PartyleaderGUID = BlackMagicInstance.ReadUInt64(Offsets.partyLeader);

            if (PartyleaderGUID != 0)
            {
                PartymemberGuids.Add(BlackMagicInstance.ReadUInt64(Offsets.partyPlayer1));
                PartymemberGuids.Add(BlackMagicInstance.ReadUInt64(Offsets.partyPlayer2));
                PartymemberGuids.Add(BlackMagicInstance.ReadUInt64(Offsets.partyPlayer3));
                PartymemberGuids.Add(BlackMagicInstance.ReadUInt64(Offsets.partyPlayer4));
            }
        }
    }
}