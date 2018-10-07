using AmeisenBotCore;
using AmeisenBotUtilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AmeisenBotCombat
{
    public abstract class CombatUtils
    {
        public static void CastSpellByName(Me me, Unit target, string name, bool onMyself)
        {
            SpellInfo spellInfo = GetSpellInfo(name, onMyself);

            if(!onMyself && !AmeisenCore.IsSpellInRage(name))
            {
                MoveToPos(me, target);
            }
            else
            {
                AmeisenCore.InteractWithGUID(
                       target.pos,
                       target.Guid,
                       InteractionType.STOP);
            }
            FaceTarget(me, target);
            AmeisenCore.CastSpellByName(name, onMyself);
            Thread.Sleep(spellInfo.castTime + 100);
        }

        public static SpellInfo GetSpellInfo(string name, bool onMyself)
        {
            return AmeisenCore.GetSpellInfo(name);
        }

        public static List<string> GetAuras(LuaUnit luaUnit)
        {
            return AmeisenCore.GetAuras(luaUnit).ToList();
        }

        public static void FaceTarget(Me me, Unit target)
        {
            if (target != null)
            {
                target.Update();
                if (!Utils.IsFacing(me.pos, me.Rotation, target.pos))
                {
                    AmeisenCore.InteractWithGUID(
                        target.pos,
                        target.Guid,
                        InteractionType.FACETARGET);
                }
            }
        }

        public static void AttackTarget()
        {
            AmeisenCore.LuaDoString("AttackTarget();");
        }

        public static Unit AssistParty(Me me, List<WowObject> activeWowObjects)
        {
            // Get the one with the lowest hp and assist him/her
            List<Unit> units = PartymembersInCombat(me, activeWowObjects);
            if (units.Count > 0)
            {
                Unit u = units.OrderBy(o => o.HealthPercentage).ToList()[0];
                u.Update();

                Unit targetToAttack = (Unit)GetWoWObjectFromGUID(u.TargetGuid, activeWowObjects);
                targetToAttack.Update();
                AmeisenCore.TargetGUID(targetToAttack.Guid);
                me.Update();
                return targetToAttack;
            }
            return null;
        }

        /// <summary>
        /// Return a Player by the given GUID
        /// </summary>
        /// <param name="guid">guid of the player you want to get</param>
        /// <param name="activeWoWObjects">all wowo objects to search in</param>
        /// <returns>Player that you want to get</returns>
        public static WowObject GetWoWObjectFromGUID(ulong guid, List<WowObject> activeWoWObjects)
        {
            foreach (WowObject p in activeWoWObjects)
            {
                if (p.Guid == guid)
                {
                    return p;
                }
            }
            return null;
        }

        public static void MoveToPos(Me me, Unit unitToAttack, double distance = 2.0)
        {
            if (Utils.GetDistance(me.pos, unitToAttack.pos) >= distance)
            {
                AmeisenCore.MovePlayerToXYZ(unitToAttack.pos, InteractionType.MOVE);
            }
        }

        public static Unit TargetTargetToHeal(Me me, List<WowObject> activeWowObjects)
        {
            // Get the one with the lowest hp and target him/her
            List<Unit> units = PartymembersInCombat(me, activeWowObjects);
            if (units.Count > 0)
            {
                Unit u = units.OrderBy(o => o.HealthPercentage).ToList()[0];
                u.Update();
                AmeisenCore.TargetGUID(u.Guid);
                return u;
            }
            return null;
        }

        /// <summary>
        /// Check if any of our partymembers are in combat
        /// </summary>
        /// <returns>returns all partymembers in combat</returns>
        public static List<Unit> PartymembersInCombat(Me me, List<WowObject> activeWowObjects)
        {
            List<Unit> inCombatUnits = new List<Unit>();
            try
            {
                foreach (ulong guid in me.PartymemberGuids)
                {
                    foreach (WowObject obj in activeWowObjects)
                    {
                        if (guid == obj.Guid)
                        {
                            if (((Unit)obj).InCombat)
                            {
                                inCombatUnits.Add(((Unit)obj));
                            }

                            break;
                        }
                    }
                }
            }
            catch { }
            return inCombatUnits;
        }
    }
}