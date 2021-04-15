using System;


namespace cgshop
{
    public class Color
    {
        private byte[] channels = new byte[4];
        public byte this[int index]
        {
            get { return channels[index];  }
            set { channels[index] = value; }
        }


        byte b, g, r, a;

        public Color(byte b, byte g, byte r, byte a)
        {
            channels[0] = b;
            channels[1] = g;
            channels[2] = r;
            channels[3] = a;
        }

        public byte GetChannel(int channel)
        {
            return channels[channel];
        }
    }


    public static class Utils
    {



        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public static readonly Byte[] bitMask = new Byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
    }
}
