using AmeisenBotCore;
using AmeisenBotUtilities;

namespace AmeisenBotCombat
{
    public abstract class CombatUtillities
    {
        public static void CastSpellByName(string name, bool onMyself)
        {
            AmeisenCore.CastSpellByName(name, onMyself);
        }

        public static SpellInfo GetSpellInfo(string name, bool onMyself)
        {
            return AmeisenCore.GetSpellInfo(name);
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
            }
        }
    }
}