using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using cgshop.point;

namespace cgshop
{
    // Draw polygon and then clipping line, in this exact order. Drawn line will be clipping with this polygon only


    [System.Xml.Serialization.XmlInclude(typeof(Color))]
    [System.Xml.Serialization.XmlRoot("Shape")]
    public class ClippingLine : Shape
    {
        List<Point> points;

        private int thickness;
        public int Thickness
        {
            get { return thickness; }
            set { thickness = value; }
        }


        private Color color;
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        private bool antialiased;
        public bool Antialiased
        {
            get { return antialiased; }
            set { antialiased = value; }
        }

        Poly targetPoly;

        public ClippingLine() : base("")
        {

        }

        public ClippingLine(String name, Point p1, Point p2, int thickness, Color color, Poly targetPoly) : base(name)
        {
            points = new List<Point>();
            points.Add(p1);
            points.Add(p2);
            this.thickness = thickness;
            this.color = color;
            this.targetPoly = targetPoly;
        }

        public override List<Point> GetPoints()
        {
            return points;
        }

        public override unsafe BitmapImage Draw(BitmapImage canvas)
        {
            var bitmap = new WriteableBitmap(canvas);

            bitmap.Lock();

            byte* pBuffer = (byte*)bitmap.BackBuffer; // Pointer to actual image data in buffer (BGRA32 format (1 byte for each channel))

            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = bitmap.BackBufferStride;
            int bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;

            {
                Point p1 = points[0];
                Point p2 = points[1];

                Point t1 = null;
                Point t2 = null;

                LineClipping(targetPoly.points, p1, p2, out t1, out t2);

                if (t1 != null)
                {
                    DrawLine(p1, t1, new Color(50, 200, 0, 255), ref pBuffer, ref bitmap, width, height);
                }
                if (t2 != null)
                {
                    DrawLine(t2, p2, new Color(50, 200, 0, 255), ref pBuffer, ref bitmap, width, height);
                }
                if (t1 != null && t2 != null)
                {
                    DrawLine(t1, t2, new Color(100, 0, 200, 255), ref pBuffer, ref bitmap, width, height);
                }
            }

            bitmap.Unlock();

            // Convert WritableBitmap to BitmapImage
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }
            return bitmapImage;
        }

        unsafe void DrawLine(Point p1, Point p2, Color color, ref byte* pBuffer, ref WriteableBitmap bitmap, int width, int height)
        {
            { // DDA Algorithm
                int dy = p2.Y - p1.Y;
                int dx = p2.X - p1.X;

                int step = Math.Abs(dx) > Math.Abs(dy) ? Math.Abs(dx) : Math.Abs(dy);

                float xInc = dx / (float)step;
                float yInc = dy / (float)step;

                float x = p1.X;
                float y = p1.Y;

                for (int i = 0; i <= step; i++)
                {
                    Utils.SetPixel(pBuffer, bitmap, (int)x, (int)y, color);

                    // Thickness
                    if (thickness > 1)
                    {
                        for (int t = 1; t <= thickness - 1; t++)
                        {
                            if (Math.Abs(dx) > Math.Abs(dy)) // Horizontal
                            {
                                if ((int)y + t > height - 1 || (int)y - t < 0)
                                    continue;

                                Utils.SetPixel(pBuffer, bitmap, (int)x, (int)y + t, color);
                                Utils.SetPixel(pBuffer, bitmap, (int)x, (int)y - t, color);
                            }
                            else // Vertical
                            {
                                if ((int)x + t > width - 1 || (int)x - t < 0)
                                    continue;

                                Utils.SetPixel(pBuffer, bitmap, (int)x + t, (int)y, color);
                                Utils.SetPixel(pBuffer, bitmap, (int)x - t, (int)y, color);
                            }
                        }
                    }

                    x += xInc;
                    y += yInc;
                }
            }
        }

        bool LineClipping(List<Point> polyVertices, Point startPoint, Point endPoint, out Point trimmedStartPoint, out Point trimmedEndPoint)
        {
            List<Point> normals = new List<Point>();

            // Calculating the normals 
            for (int i = 0; i < polyVertices.Count; i++)
            {
                normals.Add(new Point(polyVertices[i].Y - polyVertices[(i + 1) % polyVertices.Count].Y, polyVertices[(i + 1) % polyVertices.Count].X - polyVertices[i].X));
            }

            // Calculating P1 - P0 
            Point P1_P0 = new Point(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);

            // Initializing all values of P0 - PEi 
            List<Point> P0_PEi = new List<Point>();


            // Calculating the values of P0 - PEi for all edges 
            for (int i = 0; i < polyVertices.Count; i++)
            {
                // Calculating PEi - P0, so that the denominator won't have to multiply by -1 while calculating 't' 
                P0_PEi.Add(new Point(polyVertices[i].X - startPoint.X, polyVertices[i].Y - startPoint.Y));
            }

            List<double> numerator = new List<double>();
            List<double> denominator = new List<double>();

            // Calculating the numerator and denominators 
            // using the dot function 
            for (int i = 0; i < polyVertices.Count; i++)
            {
                numerator.Add(dotProduct(normals[i], P0_PEi[i]));
                denominator.Add(dotProduct(normals[i], P1_P0));
            }

            // Initializing the 't' values dynamically 
            List<double> t = new List<double>();

            // Making two vectors called 't entering' 
            // and 't leaving' to group the 't's 
            // according to their denominators 
            List<double> tEntering = new List<double>();
            List<double> tLeaving = new List<double>();

            // Calculating 't' and grouping them accordingly 
            for (int i = 0; i < polyVertices.Count; i++)
            {
                if (denominator[i] == 0)
                    t.Add(numerator[i]);
                else
                    t.Add(numerator[i] / denominator[i]);

                if (denominator[i] >= 0)
                    tEntering.Add(t[i]);
                else
                    tLeaving.Add(t[i]);
            }

            // Initializing the final two values of 't' 
            double tEnteringMax;
            double tLeavingMin;

            // Taking the max of all 'tE' and 0, so pushing 0 
            tEntering.Add(0);
            tEnteringMax = tEntering.Max();

            // Taking the min of all 'tL' and 1, so pushing 1 
            tLeaving.Add(1);
            tLeavingMin = tLeaving.Min();

            // Entering 't' value cannot be greater than exiting 't' value, hence, this is the case when the line 
            // is completely outside 
            if (tEnteringMax > tLeavingMin)
            {
                trimmedStartPoint = new Point(0, 0);
                trimmedEndPoint = new Point(0, 0);
                return false;
            }

            // Calculating the coordinates in terms of the trimmed x and y 
            trimmedStartPoint = new Point(startPoint.X + P1_P0.X * tEnteringMax, startPoint.Y + P1_P0.Y * tEnteringMax);
            trimmedEndPoint = new Point(startPoint.X + P1_P0.X * tLeavingMin, startPoint.Y + P1_P0.Y * tLeavingMin);

            bool StartTrimmed = IsSame(startPoint, trimmedStartPoint) ? false : true;
            bool EndTrimmed = IsSame(endPoint, trimmedEndPoint) ? false : true;

            return true;
        }

        bool IsSame(Point p1, Point p2)
        {
            if (Math.Abs(p1.X - p2.X) < 0.00001 == false)
                return false;
            if (Math.Abs(p1.Y - p2.Y) < 0.00001 == false)
                return false;
            return true;
        }

        double dotProduct(Point v1, Point v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

    }
}
