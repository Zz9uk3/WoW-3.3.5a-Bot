using AmeisenMapping.objects;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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