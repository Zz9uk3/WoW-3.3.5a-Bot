using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
