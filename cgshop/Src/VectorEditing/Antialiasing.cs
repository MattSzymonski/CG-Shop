using cgshop.point;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cgshop
{
    //public class Antialiasing
    //{
    //    float Lerp(float a, float b, float factor)
    //    {
    //        return a * (1 - factor) + b * factor;
    //    }

    //    Color Lerp(Color a, Color b, float factor)
    //    {
    //        int rb = (int)Lerp(a[0], b[0], factor);
    //        int rg = (int)Lerp(a[1], b[1], factor);
    //        int rr = (int)Lerp(a[2], b[2], factor);
    //        int ra = (int)Lerp(a[3], b[3], factor);

    //        return new Color((byte)rb, (byte)rg, (byte)rr, (byte)ra);
    //    }


    //    float Coverage(int thickness, float distance, float r)
    //    {
    //        if (distance < r)
    //        {
    //            return (float)(1 / Math.PI * Math.Acos(distance / r) - distance / (Math.PI * Math.Pow(r, 2)) * Math.Sqrt(Math.Pow(r, 2) + Math.Pow(distance, 2)));
    //        }
    //        else
    //        {
    //            return 0;
    //        }
    //    }

    //    float IntensifyPixel(int x, int y, int thickness, float distance, pBuffer, bitmap, Color color)
    //    {
    //        float r = 0.5f;
    //        float coverage = Coverage(thickness, distance, r);
    //        if (coverage > 0)
    //            Utils.SetPixel(pBuffer, bitmap, x, y, Lerp(new Color(255, 255, 255, 255), color, coverage);
    //        return coverage;
    //    }

    //    void ThickAntialiasedLine(Point p1, Point p2, int thickness)
    //    {
    //        //initial values in Bresenham;s algorithm
    //        int dx = p2.X - p1.X, dy = p2.Y - p1.Y;
    //        int dE = 2 * dy, dNE = 2 * (dy - dx);
    //        int d = 2 * dy - dx;
    //        int two_v_dx = 0; //numerator, v=0 for the first pixel
    //        float invDenom = 1 / (2 * (float)Math.Sqrt(dx * dx + dy * dy)); //inverted denominator
    //        float two_dx_invDenom = 2 * dx * invDenom; //precomputed constant
    //        int x = p1.X, y = p1.Y;
    //        int i;
    //        IntensifyPixel(x, y, thickness, 0);
    //        for (i = 1; IntensifyPixel(x, y + i, thickness, i * two_dx_invDenom) == 1; ++i) ;
    //        for (i = 1; IntensifyPixel(x, y - i, thickness, i * two_dx_invDenom) == 1; ++i) ;

    //        while (x < p2.X)
    //        {
    //            ++x;
    //            if (d < 0) // move to E
    //            {
    //                two_v_dx = d + dx;
    //                d += dE;
    //            }
    //            else // move to NE
    //            {
    //                two_v_dx = d - dx;
    //                d += dNE;
    //                ++y;
    //            }
    //            // Now set the chosen pixel and its neighbors
    //            IntensifyPixel(x, y, thickness, two_v_dx * invDenom);
    //            for (i = 1; IntensifyPixel(x, y + i, thickness, i * two_dx_invDenom - two_v_dx * invDenom) == 1; ++i) ;
    //            for (i = 1; IntensifyPixel(x, y - i, thickness, i * two_dx_invDenom + two_v_dx * invDenom) == 1; ++i) ;
    //        }
    //    }



    //}

}
