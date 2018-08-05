using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenCore.Objects
{
    public class Unit : WoWObject
    {
        public Unit target;

        public int combatReach;
        public int channelSpell;
        public bool casting;
        public int factionTemplate;

        public int level;
        public int health;
        public int maxHealth;
        public int energy;
        public int maxEnergy;
    }
}
