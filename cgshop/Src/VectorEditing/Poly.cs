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
        List<Point> points;
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

            return canvasInter;
        }
    }
}
