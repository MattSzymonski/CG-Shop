using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
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
    public abstract class Shape3d
    {
        public Matrix4x4 M_Model;
        public string name { get; set; }
        public abstract unsafe BitmapImage Draw(BitmapImage canvas, Matrix4x4 M_ProjectionView, Color color);

        public Shape3d(string name, Matrix4x4 model)
        {
            this.name = name;
            this.M_Model = model;
        }
    }

    public class Drawer3d
    {
        public BitmapImage clearCanvas;
        public BitmapImage canvas; // Reference to canvas

        public bool stereoscopy;

        public ObservableCollection<Shape3d> shapes3d = new ObservableCollection<Shape3d>();

        public Drawer3d(BitmapImage canvas)
        {
            this.canvas = this.clearCanvas = canvas;
        }

        public void AddShape(Shape3d shape)
        {
            shapes3d.Add(shape);
        }

        public BitmapImage RedrawCanvas(Matrix4x4 M_ProjectionView)
        {
            canvas = clearCanvas;

            Line3d xLine = new Line3d("X", new Point3d(-1000, 0, 0), new Point3d(1000, 0, 0), Matrix4x4.Identity);
            Line3d yLine = new Line3d("Y", new Point3d(0, -1000, 0), new Point3d(0, 1000, 0), Matrix4x4.Identity);
            Line3d zLine = new Line3d("Z", new Point3d(0, 0, -1000), new Point3d(0, 0, 1000), Matrix4x4.Identity);
            canvas = xLine.Draw(canvas, M_ProjectionView, new Color(0, 0, 255, 255));
            canvas = yLine.Draw(canvas, M_ProjectionView, new Color(0, 255, 0, 255));
            canvas = zLine.Draw(canvas, M_ProjectionView, new Color(255, 0, 0, 255));

            Color color = new Color(0, 0, 0, 255);
            foreach (var shape3d in shapes3d)
            {
                canvas = shape3d.Draw(canvas, M_ProjectionView, color);
            }
           
            return canvas;
        }

        public BitmapImage RedrawCanvasStereoscopy(Matrix4x4 M_ProjectionView_EyeL, Matrix4x4 M_ProjectionView_EyeR)
        {
            canvas = clearCanvas;

            Line3d xLine = new Line3d("X", new Point3d(-1000, 0, 0), new Point3d(1000, 0, 0), Matrix4x4.Identity);
            Line3d yLine = new Line3d("Y", new Point3d(0, -1000, 0), new Point3d(0, 1000, 0), Matrix4x4.Identity);
            Line3d zLine = new Line3d("Z", new Point3d(0, 0, -1000), new Point3d(0, 0, 1000), Matrix4x4.Identity);
            canvas = xLine.Draw(canvas, M_ProjectionView_EyeL, new Color(0, 0, 255, 255));
            canvas = yLine.Draw(canvas, M_ProjectionView_EyeL, new Color(0, 255, 0, 255));
            canvas = zLine.Draw(canvas, M_ProjectionView_EyeL, new Color(255, 0, 0, 255));

            // Green
            Color color1 = new Color(100, 255, 0, 255);
            foreach (var shape3d in shapes3d)
            {
                canvas = shape3d.Draw(canvas, M_ProjectionView_EyeL, color1);
            }

            // Red
            Color color2 = new Color(0, 0, 255, 255);
            foreach (var shape3d in shapes3d)
            {
                canvas = shape3d.Draw(canvas, M_ProjectionView_EyeR, color2);
            }
            
            return canvas;
        }

        public void Clear()
        {
            shapes3d.Clear();
        }

    }
}
