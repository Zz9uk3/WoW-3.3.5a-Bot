using AmeisenMapping.objects;
using System.Collections.Generic;

namespace AmeisenMapping
{
    public class Map
    {
        public Map(MapNode initialNode)
        {
            Nodes = new List<MapNode> { initialNode };
        }

        public Map(List<MapNode> initialNodes)
        {
            Nodes = initialNodes;
        }

        public List<MapNode> Nodes { get; private set; }
    }
}