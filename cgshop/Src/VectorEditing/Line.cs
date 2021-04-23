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
    public class Line : Shape
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
            set { antialiased = value;  }
        }

        public Line(String name, Point p1, Point p2, int thickness, Color color) : base(name)
        {
            points = new List<Point>();
            points.Add(p1);
            points.Add(p2);
            this.thickness = thickness;
            this.color = color;
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

                if (!antialiased)
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
                else // Gupta-Sproull Algorithm
                {
                    //initial values in Bresenham;s algorithm
                    int dx = p2.X - p1.X, dy = p2.Y - p1.Y;
                    int dE = 2 * dy, dNE = 2 * (dy - dx);
                    int d = 2 * dy - dx;
                    int two_v_dx = 0; //numerator, v=0 for the first pixel
                    float invDenom = 1 / (2 * (float)Math.Sqrt(dx * dx + dy * dy)); //inverted denominator
                    float two_dx_invDenom = 2 * dx * invDenom; //precomputed constant
                    int x = p1.X, y = p1.Y;
                    int i;
                    IntensifyPixel(x, y, thickness, 0);
                    for (i = 1; IntensifyPixel(x, y + i, thickness, i * two_dx_invDenom) == 1; ++i) ;
                    for (i = 1; IntensifyPixel(x, y - i, thickness, i * two_dx_invDenom) == 1; ++i) ;

                    while (x < p2.X)
                    {
                        ++x;
                        if (d < 0) // move to E
                        {
                            two_v_dx = d + dx;
                            d += dE;
                        }
                        else // move to NE
                        {
                            two_v_dx = d - dx;
                            d += dNE;
                            ++y;
                        }
                        // Now set the chosen pixel and its neighbors
                        IntensifyPixel(x, y, thickness, two_v_dx * invDenom);
                        for (i = 1; IntensifyPixel(x, y + i, thickness, i * two_dx_invDenom - two_v_dx * invDenom) == 1; ++i) ;
                        for (i = 1; IntensifyPixel(x, y - i, thickness, i * two_dx_invDenom + two_v_dx * invDenom) == 1; ++i) ;
                    }

                    float Coverage(int thickness, float distance, float r)
                    {
                        if (distance < r)
                        {
                            return (float)(1 / Math.PI * Math.Acos(distance / r) - distance / (Math.PI * Math.Pow(r, 2)) * Math.Sqrt(Math.Pow(r, 2) + Math.Pow(distance, 2)));
                        }
                        else
                        {
                            return 0;
                        }
                    }

                    float IntensifyPixel(int xc, int yc, int thickness, float distance)
                    {
                        float r = 0.5f;
                        float coverage = Coverage(thickness, distance, r);
                        if (coverage > 0)
                            Utils.SetPixel(pBuffer, bitmap, xc, yc, Color.Lerp(new Color(255, 255, 255, 255), color, coverage));
                        return coverage;
                    }
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
    }
}
