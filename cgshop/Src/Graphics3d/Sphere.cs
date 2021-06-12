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
    public class Sphere : Shape3d
    {
        Point3d center;
        double radius;
        int meridians;
        int parallels;

        public Sphere(String name, Point3d center, double radius, int meridians, int parallels, Matrix4x4 model) : base(name, model)
        {
            this.center = center;
            this.radius = radius;
            this.meridians = meridians;
            this.parallels = parallels;
        }

        public List<Point3d> GetVertices()
        {
            List<Point3d> vertices = new List<Point3d>();

            vertices.Add(new Point3d(center.X, center.Y + radius, center.Z, 1)); // Add south pole

            for (int i = 0; i < parallels; i++)
            {
                for (int j = 0; j < meridians; j++)
                {
                    vertices.Add(
                        new Point3d
                        (
                            center.X + radius * Math.Cos(2 * Math.PI / meridians * (j - 1)) * Math.Sin(Math.PI / (parallels + 1) * i),
                            center.Y + radius * Math.Cos(Math.PI / (parallels + 1) * i),
                            center.Z + radius * Math.Sin(2 * Math.PI / meridians * (j - 1)) * Math.Sin(Math.PI / (parallels + 1) * i),
                            1
                        )
                    );
                }
            }

            vertices.Add(new Point3d(center.X, center.Y - radius, center.Z, 1)); // Add north pole

            return vertices;
        }

        public List<Triangle3d> GetTriangles(List<Point3d> vertices)
        {
            List<Triangle3d> triangles = new List<Triangle3d>();

            for (int i = 0; i < meridians - 1; i++)
            {
                triangles.Add(new Triangle3d(new List<Point3d>() { vertices[0], vertices[i + 1], vertices[i + 2] }));
                triangles.Add(new Triangle3d(new List<Point3d>() { vertices[meridians * parallels + 1], vertices[(parallels - 1) * meridians + i + 1], vertices[(parallels - 1) * meridians + i + 2] }));
            }
            triangles.Add(new Triangle3d(new List<Point3d>() { vertices[0], vertices[1], vertices[meridians] }));
            triangles.Add(new Triangle3d(new List<Point3d>() { vertices[meridians * parallels + 1], vertices[meridians * parallels], vertices[(parallels - 1) * meridians + 1] }));

            for (int i = 0; i < parallels - 1; i++)
            {
                for (int j = 0; j < meridians; j++)
                {
                    if (j + 1 == meridians)
                    {
                        triangles.Add(new Triangle3d(new List<Point3d>() { vertices[(i + 1) * meridians], vertices[(i + 1) * meridians + 1], vertices[(i + 2) * meridians] }));
                    }
                    else
                    {
                        triangles.Add(new Triangle3d(new List<Point3d>() { vertices[i * meridians + j + 1], vertices[(i + 1) * meridians + j + 2], vertices[(i + 1) * meridians + j + 1] }));
                    }
                }
            }

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
