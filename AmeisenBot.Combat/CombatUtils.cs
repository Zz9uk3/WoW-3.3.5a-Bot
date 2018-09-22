using AmeisenBotCore;
using AmeisenBotUtilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AmeisenBotCombat
{
    public abstract class CombatUtils
    {
        public static void CastSpellByName(string name, bool onMyself)
        {
            SpellInfo spellInfo = GetSpellInfo(name, onMyself);
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

        public static void FaceTarget(Unit target)
        {
            if (target != null)
            {
                target.Update();
                AmeisenCore.InteractWithGUID(
                    target.pos,
                    target.Guid,
                    InteractionType.FACETARGET
                    );
                Thread.Sleep(200);
                AmeisenCore.InteractWithGUID(
                    target.pos,
                    target.Guid,
                    InteractionType.STOP
                    );
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
                AmeisenCore.TargetGUID(u.TargetGuid);
                me.Update();
                return u;
            }
            return null;
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