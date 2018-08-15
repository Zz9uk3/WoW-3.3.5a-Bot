using AmeisenUtilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmeisenCore.Objects
{
    public class Me : Player
    {
        public int exp;
        public int maxExp;

        public uint playerBase;

        public UnitState currentState;

        public Unit target;

        public Unit partyLeader;
        public List<Unit> partymembers;

        public Me(uint baseAddress) : base(baseAddress)
        {
            Update();
        }

        public override void Update()
        {
            playerBase = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(WoWOffsets.playerBase);
            playerBase = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(playerBase + 0x34);
            playerBase = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(playerBase + 0x24);

            name = AmeisenManager.GetInstance().GetBlackMagic().ReadASCIIString(WoWOffsets.playerName, 12);
            exp = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(playerBase + 0x3794);
            maxExp = AmeisenManager.GetInstance().GetBlackMagic().ReadInt(playerBase + 0x3798);

            // Somehow this is really sketchy, need to replace this...
            uint castingState = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt((uint)AmeisenManager.GetInstance().GetBlackMagic().MainModule.BaseAddress + WoWOffsets.localPlayerCharacterState);
            castingState = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(castingState + WoWOffsets.localPlayerCharacterStateOffset1);
            castingState = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt(castingState + WoWOffsets.localPlayerCharacterStateOffset2);
            currentState = (UnitState)AmeisenManager.GetInstance().GetBlackMagic().ReadInt(castingState + WoWOffsets.localPlayerCharacterStateOffset3);

            partymembers = new List<Unit>();
            UInt64 leaderGUID = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt64((WoWOffsets.partyLeader));

            if (leaderGUID != 0)
            {
                partymembers.Add(AmeisenCore.TryReadPartymember(WoWOffsets.partyPlayer1));
                partymembers.Add(AmeisenCore.TryReadPartymember(WoWOffsets.partyPlayer2));
                partymembers.Add(AmeisenCore.TryReadPartymember(WoWOffsets.partyPlayer3));
                partymembers.Add(AmeisenCore.TryReadPartymember(WoWOffsets.partyPlayer4));

                foreach (Unit u in partymembers)
                    if (u != null)
                        if (u.guid == leaderGUID)
                        {
                            partyLeader = u;
                            partyLeader.distance = Utils.GetDistance(pos, partyLeader.pos);
                        }
            }

            UInt64 targetGuid = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt64(baseUnitFields + (0x12 * 4));
            // If we have a target lets read it
            if (targetGuid != 0)
            {
                // Read all information from memory
                target = (Unit)AmeisenCore.ReadWoWObjectFromWoW(AmeisenCore.GetMemLocByGUID(targetGuid), WoWObjectType.UNIT);

                // Calculate the distance
                target.distance = Utils.GetDistance(AmeisenManager.GetInstance().GetMe().pos, target.pos);

                //uint targetCastingstate = AmeisenManager.GetInstance().GetBlackMagic().ReadUInt((uint)AmeisenManager.GetInstance().GetBlackMagic().MainModule.BaseAddress + WoWOffsets.staticTargetCastingstate);
                //((Me)tmpResult).target.isCasting = (targetCastingstate == 640138312) ? true : false;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("ME");
            sb.Append(" >> Address: " + baseAddress.ToString("X"));
            sb.Append(" >> UnitFields: " + baseUnitFields.ToString("X"));
            sb.Append(" >> Name: " + name);
            sb.Append(" >> GUID: " + guid);
            sb.Append(" >> PosX: " + pos.x);
            sb.Append(" >> PosY: " + pos.y);
            sb.Append(" >> PosZ: " + pos.z);
            sb.Append(" >> Rotation: " + rotation);
            sb.Append(" >> Distance: " + distance);
            sb.Append(" >> MapID: " + mapID);
            sb.Append(" >> ZoneID: " + zoneID);

            if (target != null)
                sb.Append(" >> Target: " + target.ToString());
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
                sb.Append(" >> partyLeader: " + partyLeader.guid);
            else
                sb.Append(" >> partyLeader: none");

            int count = 1;
            foreach (Player p in partymembers)
            {
                if (p != null)
                {
                    sb.Append(" >> partymember" + count + ": " + p.guid);
                    count++;
                }
            }
            return sb.ToString();
        }
    }
}
