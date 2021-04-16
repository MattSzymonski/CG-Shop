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

    public interface Shape
    {
        unsafe BitmapImage Draw(BitmapImage canvas);
    }

    public class Drawer
    {
        public BitmapImage clearCanvas;
        public BitmapImage canvas;

        public List<Shape> shapes = new List<Shape>();

        public Drawer(BitmapImage canvas)
        {
            this.canvas = this.clearCanvas = canvas;
        }

        public BitmapImage AddShape(Shape shape)
        {
            shapes.Add(shape);
            return RedrawCanvas();
        }

        public BitmapImage ToggleLineAntialiasing(bool value)
        {
            foreach (var shape in shapes)
            {
                if (shape is Line)
                {
                    (shape as Line).ToggleAntialiasing(value);
                }
            }
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
    }
}
