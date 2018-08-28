using AmeisenUtilities;

namespace AmeisenMapping.objects
{
    public class MapNode
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }

        public MapNode(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public MapNode(Vector3 position)
        {
            X = (int)position.x;
            Y = (int)position.y;
            Z = (int)position.z;
        }
    }
}
