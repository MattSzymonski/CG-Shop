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
    public class Capsule : Shape
    {
        List<Point> points;

        List<Line> lines;
        List<Circle> circles;

        private Color color;
        public Color Color
        {
            get { return color; }
            set { 
                color = value;
                foreach (var line in lines) { line.Color = color; }
                foreach (var circle in circles) { circle.Color = color; }
            }
        }

        public Capsule(String name, Point midPoint1, Point midPoint2, Point circumferencePoint, Color color) : base(name)
        {
            points = new List<Point>();
            points.Add(midPoint1);
            points.Add(midPoint2);
            points.Add(circumferencePoint);

            Point perpVector = new Point((midPoint2.Y - midPoint1.Y), -(midPoint2.X - midPoint1.X)); // Vector perpendicular to symmetry line of the capsule

            // Resize vector to size of radius
            var perpVectorLength = Math.Sqrt(Math.Pow(perpVector.X, 2) + Math.Pow(perpVector.Y, 2));
            double radius = Math.Sqrt(Math.Pow(midPoint2.X - circumferencePoint.X, 2) + Math.Pow(midPoint2.Y - circumferencePoint.Y, 2));
            perpVector.X = (int)(perpVector.X * radius / perpVectorLength);
            perpVector.Y = (int)(perpVector.Y * radius / perpVectorLength);

            Point startLine1 = new Point(midPoint1.X - perpVector.X, midPoint1.Y - perpVector.Y);
            Point endLine1 = new Point(midPoint2.X - perpVector.X, midPoint2.Y - perpVector.Y);
            Point startLine2 = new Point(midPoint1.X + perpVector.X, midPoint1.Y + perpVector.Y);
            Point endLine2 = new Point(midPoint2.X + perpVector.X, midPoint2.Y + perpVector.Y);
            lines = new List<Line>();

            lines.Add(new Line("Line_" + 1, startLine1, endLine1, 1, color));
            lines.Add(new Line("Line_" + 2, startLine2, endLine2, 1, color));

            circles = new List<Circle>();
            circles.Add(new Circle("Circle_" + 1, midPoint1, startLine1, color));
            circles.Add(new Circle("Circle_" + 2, midPoint2, circumferencePoint, color));

            this.color = color;
        }

        public override List<Point> GetPoints()
        {
            return points;
        }

        public override unsafe BitmapImage Draw(BitmapImage canvas)
        {
            BitmapImage canvasInter = canvas;
            foreach (var line in lines)
            {
                canvasInter = line.Draw(canvasInter);
            }

            foreach (var circle in circles)
            {
                canvasInter = circle.Draw(canvasInter);
            }

            return canvasInter;
        }

    }

}
