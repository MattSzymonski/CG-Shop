using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Drawing;

namespace cgshop
{

    public partial class VectorEditing : Page
    {
        private BitmapImage currentCanvas;
        // private WriteableBitmap currentCanvas2;
        private Drawer drawer;

       // private List<MouseButtonEventArgs> prevMouseButtonEventArgs;

        private MouseButtonEventArgs prevMouseButtonEventArgs;

        public VectorEditing()
        {
            InitializeComponent();
            DataContext = this;
            SetupModule();
        }

        private void SetupModule()
        {
           // prevMouseButtonEventArgs = new List<MouseButtonEventArgs>();
            drawer = new Drawer();

            //currentCanvas2.WritePixels;

            //Bitmap ads = new Bitmap();

            currentCanvas = new BitmapImage();
            Viewer.Source = currentCanvas;

            Console.WriteLine("xx3xx");

        }

        private void Viewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("xxxx");
            Console.WriteLine(e);

            drawer.Point(currentCanvas, e.GetPosition(sender as IInputElement), "Draw");


            if (prevMouseButtonEventArgs != null)
            {

            }
            else
            {
                prevMouseButtonEventArgs = e;
            }
            //prevMouseButtonEventArgs.Add(e);



            
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("txxxx");
        }
    }

}
