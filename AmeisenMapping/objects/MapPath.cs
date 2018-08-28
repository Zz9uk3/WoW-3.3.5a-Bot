namespace AmeisenMapping.objects
{
    public class MapPath
    {
        public MapNode NodeA { get; private set; }
        public MapNode NodeB { get; private set; }
        public int Quality { get; private set; }

        public MapPath(MapNode nodeA, MapNode nodeB, int quality)
        {
            NodeA = nodeA;
            NodeB = nodeB;
            Quality = quality;
        }
    }
}
