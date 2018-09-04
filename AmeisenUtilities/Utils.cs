using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace AmeisenUtilities
{
    public abstract class Utils
    {
        public static BitmapImage Base64ToBitmapImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (var stream = new MemoryStream(imageBytes))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static string GenerateRandonString(int lenght, string chars)
        {
            Random rnd = new Random();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < lenght; i++)
                sb.Append(chars[rnd.Next(0, chars.Length - 1)]);
            return sb.ToString();
        }

        public static double GetDistance(Vector3 a, Vector3 b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) +
                             (a.Y - b.Y) * (a.Y - b.Y) +
                             (a.Z - b.Z) * (a.Z - b.Z));
        }

        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        public static bool IsFacing(Vector3 myPosition, float rotationA, Vector3 targetPosition)
        {
            float f = (float)Math.Atan2(targetPosition.Y - myPosition.Y, targetPosition.X - myPosition.X);

            if (f < 0.0f)
                f = f + (float)Math.PI * 2.0f;
            else if (f > (float)Math.PI * 2)
                f = f - (float)Math.PI * 2.0f;

            return (f >= (rotationA * 0.7)) && (f <= (rotationA * 1.3)) ? true : false;
        }

        public static string FirstCharToUpper(string input)
        {
            if (input.Length > 1)
                return input?.First().ToString().ToUpper() + input?.Substring(1);
            else
                return input;
        }

        public static System.Windows.Media.Color InterpolateColors(System.Windows.Media.Color[] colors, double factor)
        {
            double r = 0.0, g = 0.0, b = 0.0;
            double total = 0.0;
            double step = 1.0 / (double)(colors.Length - 1);
            double mu = 0.0;
            double sigma_2 = 0.035;

            foreach (System.Windows.Media.Color color in colors)
            {
                total += Math.Exp(-(factor - mu) * (factor - mu) / (2.0 * sigma_2)) / Math.Sqrt(2.0 * Math.PI * sigma_2);
                mu += step;
            }

            mu = 0.0;
            foreach (System.Windows.Media.Color color in colors)
            {
                double percent = Math.Exp(-(factor - mu) * (factor - mu) / (2.0 * sigma_2)) / Math.Sqrt(2.0 * Math.PI * sigma_2);
                mu += step;

                r += color.R * percent / total;
                g += color.G * percent / total;
                b += color.B * percent / total;
            }

            return System.Windows.Media.Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
        }
    }
}