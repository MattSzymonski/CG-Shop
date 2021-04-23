using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using cgshop.point;


namespace cgshop
{
    [System.Serializable]
    public abstract class Shape
    {
        public string name { get; set; }
        public abstract unsafe BitmapImage Draw(BitmapImage canvas);
        public abstract List<Point> GetPoints();


        public Shape(string name)
        {
            this.name = name;
        }
    }

    public class Drawer
    {
        public BitmapImage clearCanvas;
        public BitmapImage canvas; // Reference to canvas

        public ObservableCollection<Shape> shapes = new ObservableCollection<Shape>();
        public Shape selectedShape;


        public Drawer(BitmapImage canvas)
        {
            this.canvas = this.clearCanvas = canvas;
        }

        public BitmapImage AddShape(Shape shape)
        {
            shapes.Add(shape);
            return RedrawCanvas();
        }

        public BitmapImage RedrawCanvas()
        {
            canvas = clearCanvas;
            foreach (var shape in shapes)
            {
                canvas = shape.Draw(canvas);
            }
            return canvas;
        }

        public BitmapImage Clear()
        {
            shapes.Clear();
            return RedrawCanvas();
        }
    }
}
