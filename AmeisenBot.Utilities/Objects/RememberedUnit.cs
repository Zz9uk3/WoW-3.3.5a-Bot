using AmeisenBotUtilities.Enums;
using System.Collections.Generic;

namespace AmeisenBotUtilities.Objects
{
    public class RememberedUnit
    {
        public string Name { get; set; }
        public List<UnitTrait> UnitTraits { get; set; }
        public Vector3 Position { get; set; }
        public int MapID { get; set; }
        public int ZoneID { get; set; }
        public string UnitTraitsString { get; set; }
    }
}