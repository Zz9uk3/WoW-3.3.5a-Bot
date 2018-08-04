using System.Collections.Generic;

namespace AmeisenCore.Objects
{
    public class Me : Player
    {
        public int exp;
        public int maxExp;

        public Unit partyLeader;
        public List<Unit> partymembers;
    }
}
