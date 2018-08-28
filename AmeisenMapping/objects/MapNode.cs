using AmeisenUtilities;

namespace AmeisenMapping.objects
{
    public class MapNode
    {
        #region Public Constructors

        public MapNode()
        {
        }

        public MapNode(int x, int y, int z, int zoneID, int mapID)
        {
            X = x;
            Y = y;
            Z = z;
            ZoneID = zoneID;
            MapID = mapID;
        }

        public MapNode(Vector3 position, int zoneID, int mapID)
        {
            X = (int)position.X;
            Y = (int)position.Y;
            Z = (int)position.Z;
            ZoneID = zoneID;
            MapID = mapID;
        }

        #endregion Public Constructors

        #region Public Properties

        public int MapID { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }
        public int ZoneID { get; private set; }

        #endregion Public Properties
    }
}