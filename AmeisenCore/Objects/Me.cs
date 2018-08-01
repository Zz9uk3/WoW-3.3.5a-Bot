using System.Collections.Generic;

namespace AmeisenCore.Objects
{
    public class Me : WoWObject
    {
        public Target target;

        public int exp;
        public int maxExp;

        public Target partyLeader;
        public List<Target> partymembers;
    }
}
