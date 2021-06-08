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
    public class Poly : Shape
    {
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

        private bool filled;
        public bool Filled
        {
            get { return filled; }
            set { filled = value; }
        }

        private Color fillColor;
        public Color FillColor
        {
            get { return fillColor; }
            set { fillColor = value; }
        }

        public Poly(String name, List<Point> points, int thickness, Color color) : base(name)
        {
            lines = new List<Line>();

            for (int i = 0; i < points.Count; i++)
            {
                lines.Add(new Line("Line_" + i, points[i], points[(i + 1) % points.Count], thickness, color));
            }

            this.points = points;
            this.thickness = thickness;
            this.color = color;
            this.filled = false;
            this.fillColor = new Color(0, 0, 255, 255);
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
            foreach (var line in lines) 
            {
                canvasInter = line.Draw(canvasInter);
            }

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
                toFill = this.pixelsToFill();

                foreach (Point p in toFill)
                {
                    Utils.SetPixel(pBuffer, bitmap, (int)p.X, (int)p.Y, fillColor);
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

        public List<Point> pixelsToFill()
        {
            int smallestY = this.points.OrderBy(p => p.Y).First().Y;
            int smallestX = this.points.OrderBy(p => p.X).First().X;
            int largestY = this.points.OrderBy(p => p.Y).Last().Y;
            int largestX = this.points.OrderBy(p => p.X).Last().X;
           
            // Create edge table
            var ET = new Dictionary<int, List<ETNode>>();

            // Fill buckets
            var linesProcessed = new List<Line>();
            for (int y = smallestY; y < largestY + 1; y++)
            {
                var linesIntersecting = lines.Where(l => l.GetPoints().Where(p => p.Y == y).Count() != 0).ToList(); // Lines that starts or ends in that y
                var linesStartingAtCurrentY = new List<Line>();
                foreach (var line in linesIntersecting)
                {
                    if (!linesProcessed.Contains(line))
                    {
                        linesStartingAtCurrentY.Add(line);
                        linesProcessed.Add(line);
                    }
                }

                if (linesStartingAtCurrentY.Count > 0)
                {
                    var nodes = new List<ETNode>();
                    nodes.AddRange(linesStartingAtCurrentY.Select(l => new ETNode(l))); // Convert lines to ETNodes
                    ET.Add(y, nodes);
                }              
            }

            // Get scanline points
            List<Point> fillPoints = new List<Point>();

            int currentY = smallestY;
            var AET = new List<ETNode>();
            while (AET.Count != 0 || ET.Count != 0)
            {
                List<ETNode> bucketContents;
                ET.TryGetValue(currentY, out bucketContents); //[currentY];

                if (bucketContents != null)
                {
                    bucketContents = bucketContents.Where(x => x.yMax != x.yMin).ToList(); // Remove horizontal lines
                    AET.AddRange(bucketContents); // move bucket from ET to AET
                    ET.Remove(currentY);            
                }

                AET = AET.OrderBy(l => l.xCurrent).ToList();

                // Add one scanline fill points
                if (AET.Count % 2 == 1)
                {
                    throw new Exception();
                }

                // Add points of scanline which are between the lines (inside the shape)
                for (int i = 0; i < AET.Count; i += 2)
                {
                    for (int x = (int)AET[i].xCurrent; x < AET[i + 1].xCurrent; x++)
                    {
                        fillPoints.Add(new Point(x, currentY));
                    }
                }
                currentY++;

                for (int i = AET.Count - 1; i >= 0; i--)
                {
                    if (AET[i].yMax == currentY)
                    {
                        AET.Remove(AET[i]);
                    }
                    else
                    {
                        AET[i].inc += AET[i].dx;
                        if (AET[i].inc > AET[i].dy)
                        {
                            int k;
                            if (AET[i].dy == 0)
                            {
                                k = int.MaxValue;
                            }
                            else
                            {
                                k = AET[i].inc / AET[i].dy;
                            }
                            AET[i].xCurrent += k * AET[i].sign;
                            AET[i].inc -= k * AET[i].dy;
                        }
                    }
                }
            }

            return fillPoints;
        }

        public class ETNode
        {
            public int yMin;
            public int yMax;
            public double xCurrent;
            public double xMax;
            public int dx;
            public int dy;
            public int sign;
            public double inv { get { return ((xMax - xCurrent) / (yMax - yMin)); } }
            public int inc = 0;

            public ETNode(Line line)
            {
                var p1 = line.GetPoints()[0];
                var p2 = line.GetPoints()[1];
                
                if (p1.Y < p2.Y)
                {
                    yMin = p1.Y;
                    yMax = p2.Y;
                    xCurrent = p1.X;
                    xMax = p2.X;
                }
                else if (p1.Y > p2.Y)
                {
                    yMin = p2.Y;
                    yMax = p1.Y;
                    xCurrent = p2.X;
                    xMax = p1.X;
                }
                else
                {
                    yMin = yMax = p1.Y;
                    xCurrent = p1.X <= p2.X ? p1.X : p2.X;
                    xMax = p1.X <= p2.X ? p2.X : p1.X;
                }
                if (xMax > xCurrent)
                {
                    sign = 1;
                }
                else
                {
                    sign = -1;
                }
                dx = Math.Abs(p1.X - p2.X);
                dy = Math.Abs(p1.Y - p2.Y);
            }
        }
    }
}
