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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Numerics;

namespace cgshop
{
    public enum ShapeType3d
    {
        Cylinder,
        Sphere,
        Cuboid,
        Cone,
    }

    public partial class Graphics3d : Page, INotifyPropertyChanged
    {
        private Drawer3d drawer;
        private Camera camera;

        private ShapeType3d selectedShapeType;

        public bool stereoscopy = false;

        public event PropertyChangedEventHandler PropertyChanged;



        public Graphics3d()
        {
            InitializeComponent();
            DataContext = this;
            SetupModule();
        }

        private void SetupModule()
        {
            var canvas = new BitmapImage(new Uri("/Res/Test.png", UriKind.Relative));

            drawer = new Drawer3d(canvas);
            CanvasImage.Source = drawer.canvas;

            //CanvasImage.Source = drawer.RedrawCanvas();


            SetupScene();
        }

        private void SetupScene()
        {
            camera = new Camera();

            Matrix4x4 M_Model_Cuboid1 = new Matrix4x4( 
                1, 0, 0, 0, // X
                0, 1, 0, 0, // Y
                0, 0, 1, 0, // Z
                0, 0, 0, 1
            );
            Cuboid cuboid1 = new Cuboid("Cuboid1", new Point3d(-200, 0, -450), 100, 100, 100, M_Model_Cuboid1);
            drawer.AddShape(cuboid1);

            Matrix4x4 M_Model_Sphere1 = new Matrix4x4(
               1, 0, 0, 0, // X
               0, 1, 0, 0, // Y
               0, 0, 1, 0, // Z
               0, 0, 0, 1
            );
            Sphere sphere1 = new Sphere("Sphere1", new Point3d(-200, 0, -350), 50, 10, 6, M_Model_Sphere1);
            drawer.AddShape(sphere1);

            Matrix4x4 M_Model_Cone1 = new Matrix4x4(
               1, 0, 0, 0, // X
               0, 1, 0, 0, // Y
               0, 0, 1, 0, // Z
               0, 0, 0, 1
            );
            Cone cone1 = new Cone("Cone1", new Point3d(100, 0, -400), 40, -80, 10, M_Model_Cone1);
            drawer.AddShape(cone1);

            Matrix4x4 M_Model_Cylinder1 = new Matrix4x4(
              1, 0, 0, 0, // X
              0, 1, 0, 0, // Y
              0, 0, 1, 0, // Z
              0, 0, 0, 1
            );
            Cylinder cylinder1 = new Cylinder("Cylinder1", new Point3d(0, 0, -150), 50, 70, 9, M_Model_Cylinder1);
            drawer.AddShape(cylinder1);
        }

        private void Refresh()
        {
            if (stereoscopy)
            {
                float e = 50; // distance between the eyes
                float d = 600; // phisical distance to the screen

                float stereoscopyFactor = d * e / 17000;

                Matrix4x4 M_Projection_EyeL = new Matrix4x4(1, 0, 0, stereoscopyFactor,
                                                            0, 1, 0, 0,
                                                            0, 0, 0, 1,
                                                            0, 0, -1, 0);

                Matrix4x4 M_Projection_EyeR = new Matrix4x4(1, 0, 0, -stereoscopyFactor,
                                                            0, 1, 0, 0,
                                                            0, 0, 0, 1,
                                                            0, 0, -1, 0);

                Matrix4x4 M_View = camera.ViewMatrix();

                Matrix4x4 M_ProjectionView_EyeL = M_Projection_EyeL * M_View;
                Matrix4x4 M_ProjectionView_EyeR = M_Projection_EyeR * M_View;

                CanvasImage.Source = drawer.RedrawCanvasStereoscopy(M_ProjectionView_EyeL, M_ProjectionView_EyeR);
            }
            else
            {
                Matrix4x4 M_Projection = new Matrix4x4(1, 0, 0, 0,
                                                       0, 1, 0, 0,
                                                       0, 0, 0, 1,
                                                       0, 0, -1, 0);

                Matrix4x4 M_View = camera.ViewMatrix();

                Matrix4x4 M_ProjectionView = M_Projection * M_View;

                CanvasImage.Source = drawer.RedrawCanvas(M_ProjectionView);
            }
            
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Refresh();
        }

        private void MoveCamera(object sender, System.Windows.RoutedEventArgs e)
        {
            var tag = ((Button)sender).Tag.ToString();
            switch (tag)
            {
                case ("Up"):
                    camera.position.Y += 1;
                    break;
                case ("Down"):
                    camera.position.Y -= 1;
                    break;
                case ("Right"):
                    camera.position.X += 1;
                    break;
                case ("Left"):
                    camera.position.X -= 1;
                    break;
                default:
                    break;
            }

            Console.WriteLine("Camera position: (x: " + camera.position.X + ", " + camera.position.Y + ", " + camera.position.Z + ")");
            Refresh();
        }

        private void MoveObject(object sender, System.Windows.RoutedEventArgs e)
        {
            float intensity = 20;

            var tag = ((Button)sender).Tag.ToString();
            switch (tag)
            {
                case ("Up"):
                    (drawer.shapes3d[0] as Cuboid).M_Model.M34 += intensity;
                    break;
                case ("Down"):
                    (drawer.shapes3d[0] as Cuboid).M_Model.M34 -= intensity;
                    break;
                case ("Right"):
                    (drawer.shapes3d[0] as Cuboid).M_Model.M14 += intensity;
                    break;
                case ("Left"):
                    (drawer.shapes3d[0] as Cuboid).M_Model.M14 -= intensity;
                    break;
                default:
                    break;
            }
            Refresh();
        }

        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Refresh();
        }

        private void ToggleStereoscopy(object sender, System.Windows.RoutedEventArgs e)
        {
            stereoscopy = !stereoscopy;
            Refresh();
        }
    }
}
