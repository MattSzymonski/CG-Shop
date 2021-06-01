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

        public BitmapImage FloodFill(Point p, Color color)
        {
            FloodFiller floodFiller = new FloodFiller();
            canvas = floodFiller.AddFloodFill(canvas, p, color);
            return canvas;
        }
    }

    class FloodFiller 
    {
        public unsafe BitmapImage AddFloodFill(BitmapImage canvas, Point p, Color color)
        {
            var bitmap = new WriteableBitmap(canvas);

            if (p.X < 0 || p.X > bitmap.Width - 1 || p.Y < 0 || p.Y > bitmap.Height - 1) { throw new Exception("Point is outside bitmap"); }

            bitmap.Lock();

            byte* pBuffer = (byte*)bitmap.BackBuffer; // Pointer to actual image data in buffer (BGRA32 format (1 byte for each channel))

            {
                Color seedColor = Utils.GetPixel(pBuffer, bitmap, p.X, p.Y); // Clicked color (this color inside boundaries will be filled with fill color)
                Stack<Point> pixelsToFill = new Stack<Point>();
                pixelsToFill.Push(p);
                while (pixelsToFill.Count > 0)
                {
                    Point pixel = pixelsToFill.Pop();
                    if (pixel.X < 0 || pixel.X > bitmap.Width - 1 || pixel.Y < 0 || pixel.Y > bitmap.Height - 1) { continue; } // Skip pixel which is outside the bitmap
                    Color pixelColor = Utils.GetPixel(pBuffer, bitmap, pixel.X, pixel.Y);
                    if (Color.Same(pixelColor, seedColor))
                    {
                        Utils.SetPixel(pBuffer, bitmap, pixel.X, pixel.Y, color);
                        pixelsToFill.Push(new Point(pixel.X + 1, pixel.Y)); // North
                        pixelsToFill.Push(new Point(pixel.X - 1, pixel.Y)); // South
                        pixelsToFill.Push(new Point(pixel.X, pixel.Y + 1)); // East
                        pixelsToFill.Push(new Point(pixel.X, pixel.Y - 1)); // West
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

            //canvas = bitmapImage;
            return bitmapImage;
        }

    }

}
