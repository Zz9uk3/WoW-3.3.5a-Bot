using AmeisenBotUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotUtilities.Objects
{
    public class RememberedUnit
    {
        public string Name { get; set; }
        public List<UnitTrait> UnitTraits{ get; set; }
        public Vector3 Position { get; set; }
        public int MapID { get; set; }
        public int ZoneID { get; set; }
        public string UnitTraitsString { get; set; }
    }
}
