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
    public class AET
    {
        Poly polygon;
        public List<AETnode> nodes = new List<AETnode>();
        public List<AETnode> sortedNodes = new List<AETnode>();
        public int minimalY;
        public int maximalY;
        public AET(Poly poly)
        {
            polygon = poly;
            for (int i = 0; i < polygon.points.Count - 1; i++)
            {
                nodes.Add(new AETnode(polygon.points[i].Y, polygon.points[i + 1].Y,
                                      polygon.points[i].X, polygon.points[i + 1].X));
            }
            sortedNodes = nodes.Where(x => x.isSlopeZero == false).
                                OrderBy(x => x.yMin).
                                ThenBy(x => x.yMax).
                                ThenBy(x => x.xVal).ToList();
            minimalY = sortedNodes.Min(x => x.yMin);
            maximalY = sortedNodes.Max(x => x.yMax);
        }
    }

    public class AETnode
    {
        public int yMin;
        public int yMax;
        public double xVal;
        public double xMax;
        public int dx;
        public int dy;
        public int signCoeff;
        public int dummy = 0;

        public double mInv
        {
            get
            {
                return ((xMax - xVal) / (yMax - yMin));
            }
            set { }
        }
        public bool isSlopeZero { get { return (yMax - yMin == 0 ? true : false); } set { } }
        public AETnode(int y1, int y2, int x1, int x2)
        {
            if (y1 < y2)
            {
                yMin = y1;
                yMax = y2;
                xVal = x1;
                xMax = x2;
            }
            else if (y1 > y2)
            {
                yMin = y2;
                yMax = y1;
                xVal = x2;
                xMax = x1;
            }
            else
            {
                yMin = yMax = y1;
                xVal = x1 <= x2 ? x1 : x2;
                xMax = x1 <= x2 ? x2 : x1;
            }
            if (xMax > xVal)
            {
                signCoeff = 1;
            }
            else
            {
                signCoeff = -1;
            }
            dx = Math.Abs(x1 - x2);
            dy = Math.Abs(y1 - y2);


        }
    }


    public class Poly : Shape
    {
        public List<AETnode> polygonTable = new List<AETnode>();
        private int yMinTemp;

        public List<Point> points;
        List<Line> lines;

        private int thickness;
        public int Thickness { 
            get { return thickness; }
            set { thickness = value;  foreach (var line in lines) { line.Thickness = thickness; } } 
        }

        private Color color;
        public Color Color {
            get { return color; }
            set { color = value; foreach (var line in lines) { line.Color = color; } }
        }

        private bool antialiased;
        public bool Antialiased
        {
            get { return antialiased; }
            set { antialiased = value; foreach (var line in lines) { line.Antialiased = antialiased; } }
        }


        private bool filled = true;
        public bool Filled
        {
            get { return filled; }
            set { filled = value; }
        }

        Dictionary<int, List<AETnode>> edgeTable = new Dictionary<int, List<AETnode>>();


        public Poly(String name, List<Point> points, int thickness, Color color) : base(name)
        {
            //this.points = points;
            lines = new List<Line>();

            for (int i = 0; i < points.Count; i++)
            {
                lines.Add(new Line("Line_" + i, points[i], points[(i + 1) % points.Count], thickness, color));
            }

            this.points = points;
            // this.points = new List<Point>();
            //this.points = points;
            this.thickness = thickness;
            this.color = color;
        }

        public override List<Point> GetPoints()
        {
            return points;
        }



        public override unsafe BitmapImage Draw(BitmapImage canvas)
        {
            BitmapImage canvasInter = canvas;

            // Draw fill
            if (filled)
            {
                canvasInter = DrawFill(canvasInter);
            }  

            // Draw lines
           // BitmapImage canvasInter = canvas;
            foreach (var line in lines) 
            {
                canvasInter = line.Draw(canvasInter);
            }


            //if (filled)
            //{
            //    List<Point> toFill = new List<Point>();
            //    toFill = this.pointsToFill();

            //    foreach (Point p in toFill)
            //    {
            //        tempBit.SetPixel(p.X, p.Y, new cgshop.Color(0, 0, 255, 255));
            //    }

            //}
           

            return canvasInter;
        }

        public unsafe BitmapImage DrawFill(BitmapImage canvas)
        {
            var bitmap = new WriteableBitmap(canvas);

            bitmap.Lock();

            byte* pBuffer = (byte*)bitmap.BackBuffer; // Pointer to actual image data in buffer (BGRA32 format (1 byte for each channel))

            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = bitmap.BackBufferStride;
            int bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;

            {
                List<Point> toFill = new List<Point>();
                toFill = this.pointsToFill();

                foreach (Point p in toFill)
                {
                    Utils.SetPixel(pBuffer, bitmap, (int)p.X, (int)p.Y, new cgshop.Color(0, 0, 255, 255));

                    //tempBit.SetPixel(p.X, p.Y, new cgshop.Color(0, 0, 255, 255));
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







        public List<Point> pointsToFill()
        {
            AET tempAET = new AET(this);
            List<AETnode> temp = tempAET.nodes.OrderBy(x => x.yMin).ToList();
            yMinTemp = temp[0].yMin;
            int tempY = yMinTemp;
            while (temp.Count != 0)
            {
                if (temp.Where(x => x.yMin == tempY).Count() != 0)
                {
                    List<AETnode> bucketTable = temp.Where(x => x.yMin == tempY).ToList();
                    temp.RemoveAll(x => x.yMin == tempY);
                    edgeTable.Add(tempY, bucketTable.OrderBy(x => x.xVal).ToList());
                }
                tempY++;
            }
            List<Point> pointsFill = new List<Point>();
            int y = yMinTemp;
            polygonTable.Clear();
            do
            {
                List<AETnode> bucket;
                if (edgeTable.TryGetValue(y, out bucket))
                {
                    polygonTable.AddRange(bucket);

                }
                polygonTable = polygonTable.OrderBy(x => x.xVal).ToList();
                for (int i = 0; i < polygonTable.Count - 1; i += 2)
                {
                    int x = (int)polygonTable[i].xVal;
                    while (x != polygonTable[i + 1].xVal)
                    {
                        pointsFill.Add(new Point(x, y));
                        x++;
                    }
                }
                y++;
                polygonTable.RemoveAll(x => x.yMax == y);
                foreach (AETnode n in polygonTable)
                {
                    n.dummy += n.dx;
                    if (n.dummy > n.dy)
                    {
                        int k;
                        try
                        {
                            k = n.dummy / n.dy;
                        }
                        catch (DivideByZeroException e)
                        {
                            k = int.MaxValue;
                        }
                        n.xVal += k * n.signCoeff;
                        n.dummy -= k * n.dy;
                    }
                }
            }
            while (polygonTable.Count > 0);
            edgeTable.Clear();
            return pointsFill;
        }
    }
}
