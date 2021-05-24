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
    public class Rectangle : Poly
    {
        public Rectangle(String name, Point point1, Point point2, Color color) : base(name, new List<Point>() { point1, new Point(point1.X, point2.Y), point2, new Point(point2.X, point1.Y) }, 1, color)
        {
           // points = new List<Point>() { point1, new Point(point2.X, point1.Y), point2, new Point(point1.X, point2.Y) };
        }

        //public override List<Point> GetPoints()
        //{
        //    return points;
        //}

        //public override unsafe BitmapImage Draw(BitmapImage canvas)
        //{
        //    BitmapImage canvasInter = canvas;
        //    foreach (var line in lines)
        //    {
        //        canvasInter = line.Draw(canvasInter);
        //    }

        //    foreach (var circle in circles)
        //    {
        //        canvasInter = circle.Draw(canvasInter);
        //    }

        //    return canvasInter;
        //}

    }

}
