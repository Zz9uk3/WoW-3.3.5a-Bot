namespace AmeisenMapping.objects
{
    public class MapPath
    {
        #region Public Constructors

        public MapPath(MapNode nodeA, MapNode nodeB, int quality)
        {
            NodeA = nodeA;
            NodeB = nodeB;
            Quality = quality;
        }

        #endregion Public Constructors

        #region Public Properties

        public MapNode NodeA { get; private set; }
        public MapNode NodeB { get; private set; }
        public int Quality { get; private set; }

        #endregion Public Properties
    }
}