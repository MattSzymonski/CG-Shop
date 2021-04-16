using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.IO;

using cgshop.point;

namespace cgshop
{

    public partial class VectorEditing : Page
    {
        //private BitmapImage currentCanvas;
        // private WriteableBitmap currentCanvas2;
        private Drawer drawer;

        // private List<MouseButtonEventArgs> prevMouseButtonEventArgs;

        // private MouseButtonEventArgs prevMouseButtonEventArgs;


        private Point previousClickPoint;

        public VectorEditing()
        {
            InitializeComponent();
            DataContext = this;
            SetupModule();
        }

        private void SetupModule()
        {
          

            var canvas = new BitmapImage(new Uri("/Res/Test.png", UriKind.Relative));

            //int width = (int)128;
            //int height = (int)128;

            //int bytesPerPixel = PixelFormats.Bgra32.BitsPerPixel; // 32
            //int stride = width * bytesPerPixel / 8; // Width * 4;
            //byte[] pixels = new byte[height * stride];
            ////BitmapPalette bitmapPalette = new BitmapPalette(new List<System.Windows.Media.Color>() { Colors.Green });
            //BitmapPalette bitmapPalette = new BitmapPalette(new List<System.Windows.Media.Color>() { Colors.Blue, Colors.Green, Colors.Red, Colors.Transparent });
            //BitmapSource bitmapSource = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, bitmapPalette, pixels, stride);

            //BitmapImage bitmapImage = new BitmapImage();
            //using (MemoryStream stream = new MemoryStream())
            //{
            //    PngBitmapEncoder encoder = new PngBitmapEncoder();
            //    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            //    encoder.Save(stream);
            //    bitmapImage.BeginInit();
            //    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            //    bitmapImage.StreamSource = stream;
            //    bitmapImage.EndInit();
            //    bitmapImage.Freeze();
            //}

            drawer = new Drawer(canvas);
            Viewer.Source = drawer.canvas;
        }

        private void Viewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (previousClickPoint != null)
            //{
            //    Console.WriteLine("2");
            //    Point p1 = previousClickPoint;
            //    Point p2 = new Point(e.GetPosition(sender as System.Windows.IInputElement));

            //    Console.WriteLine(p1.ToString());
            //    Console.WriteLine(p2.ToString());

            //    Viewer.Source = drawer.AddShape(new cgshop.Line(p1, p2, 3, new Color(0, 0, 0, 255)));
            //    previousClickPoint = null;
            //}
            //else
            //{
            //    Console.WriteLine("1");
            //    previousClickPoint = new Point(e.GetPosition(sender as System.Windows.IInputElement));
            //} 

            if (previousClickPoint != null)
            {
                Console.WriteLine("2");
                Point p1 = previousClickPoint;
                Point p2 = new Point(e.GetPosition(sender as System.Windows.IInputElement));

                Console.WriteLine(p1.ToString());
                Console.WriteLine(p2.ToString());

                Viewer.Source = drawer.AddShape(new cgshop.Circle(p1, p2, 3, new Color(0, 0, 0, 255)));
                previousClickPoint = null;
            }
            else
            {
                Console.WriteLine("1");
                previousClickPoint = new Point(e.GetPosition(sender as System.Windows.IInputElement));
            }


        }

    }

}
