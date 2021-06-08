using System;
using System.Windows.Media.Imaging;

namespace cgshop
{
    [System.Xml.Serialization.XmlRoot("Color")]
    public class Color
    {
        private byte[] channels = new byte[4];
        public byte this[int index]
        {
            get { return channels[index];  }
            set { channels[index] = value; }
        }

        public Color()
        {

        }

        //byte b, g, r, a;

        public Color(byte b, byte g, byte r, byte a)
        {
            channels[0] = b;
            channels[1] = g;
            channels[2] = r;
            channels[3] = a;
        }

        public Color(System.Windows.Media.Color color)
        {
            channels[0] = color.B;
            channels[1] = color.G;
            channels[2] = color.R;
            channels[3] = color.A;
        }

        public byte GetChannel(int channel)
        {
            return channels[channel];
        }

        public static Color Lerp(Color a, Color b, float factor)
        {
            int rb = (int)Utils.Lerp(a[0], b[0], factor);
            int rg = (int)Utils.Lerp(a[1], b[1], factor);
            int rr = (int)Utils.Lerp(a[2], b[2], factor);
            int ra = (int)Utils.Lerp(a[3], b[3], factor);

            return new Color((byte)rb, (byte)rg, (byte)rr, (byte)ra);
        }

        public static bool Same(Color c1, Color c2)
        {
            for (int i = 0; i < 4; i++)
            {
                if (c1[i] != c2[i])
                {
                    return false;
                }
            }

            return true;
        }


    }

    namespace point
    {
        public class Point
        {
            public int X;
            public int Y;

            public Point(System.Windows.Point point)
            {
                this.X = (int)point.X;
                this.Y = (int)point.Y;
            }

            public Point(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public Point(double x, double y)
            {
                this.X = (int)x;
                this.Y = (int)y;
            }

            public double Distance(Point other)
            {
                return Math.Sqrt(Math.Pow((this.X - other.X), 2) + Math.Pow((this.Y - other.Y), 2));
            }

            public override string ToString()
            {
                return "Point: " + X + ", " + Y;
            }
        }

        public class Point3d
        {
            public int X;
            public int Y;
            public int Z;
            public int W;

            public Point3d(int x, int y, int z, int w)
            {
                this.X = x;
                this.Y = y;
                this.Z = z;
                this.W = w;
            }

            public Point3d(double x, double y, double z, double w)
            {
                this.X = (int)x;
                this.Y = (int)y;
                this.Z = (int)z;
                this.W = (int)w;
            }

            public override string ToString()
            {
                return "Point: " + X + ", " + Y + ", " + Z + ", " + W;
            }
        }


    }

   


    public static class Utils
    {
        public static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        public static float Lerp(float a, float b, float factor)
        {
            return a * (1 - factor) + b * factor;
        }


        public unsafe static void SetPixel(byte* pBuffer, WriteableBitmap bitmap, int x, int y, Color color)
        {
            if (y > bitmap.Height - 1 || y < 0 || x > bitmap.Width - 1 || x < 0)
                return;

            for (int i = 0; i < 3; i++) // For each color channel
            {
                try
                {
                    int index = 4 * x + (y * bitmap.BackBufferStride) + i;
                    pBuffer[index] = color[i];
                }
                catch
                {
                    continue;
                }
                
            }
        }

        public unsafe static Color GetPixel(byte* pBuffer, WriteableBitmap bitmap, int x, int y)
        {
            if (y > bitmap.Height - 1 || y < 0 || x > bitmap.Width - 1 || x < 0)
                return null;

            Color pixelColor = new Color();

            for (int i = 0; i < 3; i++) // For each color channel
            {
                try
                {
                    int index = 4 * x + (y * bitmap.BackBufferStride) + i;
                    pixelColor[i] = pBuffer[index];
                }
                catch
                {
                    continue;
                }
            }

            return pixelColor;
        }

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public static readonly Byte[] bitMask = new Byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
    }
}
