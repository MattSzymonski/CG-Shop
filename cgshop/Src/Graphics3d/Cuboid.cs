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
    public class Cuboid : Shape3d
    {
        Point3d center;
        double a;
        double b;
        double c;

        public Cuboid(String name, Point3d center, double a, double b, double c, Matrix4x4 model) : base(name, model)
        {
            this.center = center;
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public List<Point3d> GetVertices()
        {
            List<Point3d> vertices = new List<Point3d>();

            vertices.Add(new Point3d(center.X - a / 2, center.Y - b / 2, center.Z - c / 2, 1));
            vertices.Add(new Point3d(center.X - a / 2, center.Y + b / 2, center.Z - c / 2, 1));
            vertices.Add(new Point3d(center.X + a / 2, center.Y - b / 2, center.Z - c / 2, 1));
            vertices.Add(new Point3d(center.X + a / 2, center.Y + b / 2, center.Z - c / 2, 1));
            vertices.Add(new Point3d(center.X - a / 2, center.Y - b / 2, center.Z + c / 2, 1));
            vertices.Add(new Point3d(center.X - a / 2, center.Y + b / 2, center.Z + c / 2, 1));
            vertices.Add(new Point3d(center.X + a / 2, center.Y - b / 2, center.Z + c / 2, 1));
            vertices.Add(new Point3d(center.X + a / 2, center.Y + b / 2, center.Z + c / 2, 1));

            return vertices;
        }

        public List<Triangle3d> GetTriangles(List<Point3d> vertices)
        {
            List<Triangle3d> triangles = new List<Triangle3d>();

            triangles.Add(new Triangle3d(new List<Point3d>() { vertices[0], vertices[1], vertices[2] }));
            triangles.Add(new Triangle3d(new List<Point3d>() { vertices[1], vertices[2], vertices[3] }));

            triangles.Add(new Triangle3d(new List<Point3d>() { vertices[1], vertices[3], vertices[7] }));
            triangles.Add(new Triangle3d(new List<Point3d>() { vertices[1], vertices[5], vertices[7] }));

            triangles.Add(new Triangle3d(new List<Point3d>() { vertices[4], vertices[5], vertices[7] }));
            triangles.Add(new Triangle3d(new List<Point3d>() { vertices[4], vertices[6], vertices[7] }));

            triangles.Add(new Triangle3d(new List<Point3d>() { vertices[0], vertices[2], vertices[4] }));
            triangles.Add(new Triangle3d(new List<Point3d>() { vertices[2], vertices[4], vertices[6] }));

            triangles.Add(new Triangle3d(new List<Point3d>() { vertices[0], vertices[1], vertices[4] }));
            triangles.Add(new Triangle3d(new List<Point3d>() { vertices[1], vertices[4], vertices[5] }));

            triangles.Add(new Triangle3d(new List<Point3d>() { vertices[2], vertices[3], vertices[7] }));
            triangles.Add(new Triangle3d(new List<Point3d>() { vertices[2], vertices[6], vertices[7] }));

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
