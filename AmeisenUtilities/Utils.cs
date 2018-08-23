using System;
using System.Text;

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


        public static string GenerateRandonString(int lenght, string chars)
        {
            Random rnd = new Random();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < lenght; i++)
                sb.Append(chars[rnd.Next(0, chars.Length - 1)]);
            return sb.ToString();
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
