namespace AmeisenBotUtilities
{
    /// <summary> Simple X,Y & Z struct </summary>
    public struct Vector3
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        public Vector3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}