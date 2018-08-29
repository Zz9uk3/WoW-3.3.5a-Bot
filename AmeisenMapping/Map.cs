using AmeisenMapping.objects;
using System.Collections.Generic;

namespace AmeisenMapping
{
    public class Map
    {
        #region Public Constructors

        public Map(MapNode initialNode)
        {
            Nodes = new List<MapNode> { initialNode };
        }

        public Map(List<MapNode> initialNodes)
        {
            Nodes = initialNodes;
        }

        #endregion Public Constructors

        #region Public Properties

        public List<MapNode> Nodes { get; private set; }

        #endregion Public Properties
    }
}