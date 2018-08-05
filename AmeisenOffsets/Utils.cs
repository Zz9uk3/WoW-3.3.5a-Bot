using System;

namespace AmeisenUtilities
{
    public abstract class Utils
    {
        public static double GetDistance(Vector3 a, Vector3 b)
        {
            return Math.Sqrt((a.x - b.x) * (a.x - b.x) +
                             (a.y - b.y) * (a.y - b.y) +
                             (a.z - b.z) * (a.z - b.z));
        }
    }
}
