using cgshop.point;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace cgshop
{
    public class Cylinder : Shape3d
    {
        Point3d center;
        double radius;
        double height;
        int meridians;

        public Cylinder(String name, Point3d center, double radius, double height, int meridians, Matrix4x4 model) : base(name, model)
        {
            this.center = center;
            this.radius = radius;
            this.height = height;
            this.meridians = meridians;
        }

        public List<Point3d> GetVertices()
        {
            List<Point3d> vertices = new List<Point3d>();

            vertices.Add(new Point3d(center.X, center.Y - height / 2, center.Z, 1)); // Add bottom
            for (int i = 0; i < meridians; i++)
            {
                vertices.Add(
                    new Point3d
                    (
                        radius * Math.Cos(2 * Math.PI * i / meridians) + center.X,
                        center.Y - height / 2,
                        radius * Math.Sin(2 * Math.PI * i / meridians) + center.Z,
                        1
                    )
                );
            }

            vertices.Add(new Point3d(center.X, center.Y + height / 2, center.Z, 1)); // Add top
            for (int i = 0; i < meridians; i++)
            {
                vertices.Add(
                    new Point3d
                    (
                        radius * Math.Cos(2 * Math.PI * i / meridians) + center.X,
                        center.Y + height / 2,
                        radius * Math.Sin(2 * Math.PI * i / meridians) + center.Z,
                        1
                    )
                );
            }

            return vertices;
        }

        public List<Triangle3d> GetTriangles(List<Point3d> vertices)
        {
            List<Triangle3d> triangles = new List<Triangle3d>();

            // Bottom
            for (int i = 0; i < meridians - 2; i++)
            {
                triangles.Add(new Triangle3d(new List<Point3d>() { vertices[0], vertices[i + 1], vertices[i + 2] }));
            }
            triangles.Add(new Triangle3d(new List<Point3d>() { vertices[0], vertices[meridians], vertices[1] }));

            // Top
            for (int i = 0; i < meridians - 2; i++)
            {
                triangles.Add(new Triangle3d(new List<Point3d>() { vertices[meridians + 1], vertices[i + meridians + 2], vertices[i + meridians + 3] }));
            }
            triangles.Add(new Triangle3d(new List<Point3d>() { vertices[meridians + 1], vertices[2 * meridians + 1], vertices[meridians + 2] }));

            // Side
            for (int i = 0; i < meridians - 1; i++)
            {
                triangles.Add(new Triangle3d(new List<Point3d>() { vertices[i + 1], vertices[i + 2], vertices[i + meridians + 2] }));
                triangles.Add(new Triangle3d(new List<Point3d>() { vertices[i + 2], vertices[i + meridians + 2], vertices[i + meridians + 3] }));
            }
            triangles.Add(new Triangle3d(new List<Point3d>() { vertices[meridians], vertices[1], vertices[2 * meridians + 1] }));
            triangles.Add(new Triangle3d(new List<Point3d>() { vertices[1], vertices[2 * meridians + 1], vertices[meridians + 2] }));

            return triangles;
        }

        public override BitmapImage Draw(BitmapImage canvas, Matrix4x4 M_ProjectionView, Color color)
        {
            BitmapImage canvasInter = canvas;

            double a = 0;
            Matrix4x4 Ry = new Matrix4x4((float)Math.Cos(a), 0, (float)Math.Sin(a), 0,
                                        0, 1, 0, 0,
                                        (float)-Math.Sin(a), 0, (float)Math.Cos(a), 0,
                                        0, 0, 0, 1);
            double b = 0;
            Matrix4x4 Rx = new Matrix4x4(1, 0, 0, 0,
                                       0, (float)Math.Cos(b), (float)Math.Sin(b), 0,
                                       0, (float)-Math.Sin(b), (float)Math.Cos(b), 0,
                                       0, 0, 0, 1);

            // Multiply model and transformation matrices
            var M_ModelViewProjection = M_ProjectionView * Ry * Rx * M_Model;
            var triangles = GetTriangles(GetVertices());
            foreach (var triangle in triangles)
            {
                canvasInter = triangle.Draw(canvasInter, M_ModelViewProjection, color);
            }

            return canvasInter;
        }
    }
}
