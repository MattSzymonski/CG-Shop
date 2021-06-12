using cgshop.point;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace cgshop
{
    public class Line3d : Shape3d
    {
        Point3d p1;
        Point3d p2;

        public Line3d(String name, Point3d p1, Point3d p2, Matrix4x4 model) : base(name, model)
        {
            this.p1 = p1;
            this.p2 = p2;
        }

        public List<Point3d> GetVertices()
        {
            List<Point3d> vertices = new List<Point3d>();

            vertices.Add(new Point3d(p1.X, p1.Y, p1.Z));
            vertices.Add(new Point3d(p2.X, p2.Y, p2.Z));

            return vertices;
        }

        public override unsafe BitmapImage Draw(BitmapImage canvas, Matrix4x4 M_ProjectionView, Color color)
        {
            BitmapImage canvasInter = canvas;

            var M_ModelViewProjection = M_ProjectionView * M_Model; // Multiply model and transformation matrices
            var bitmap = new WriteableBitmap(canvas);

            bitmap.Lock();

            byte* pBuffer = (byte*)bitmap.BackBuffer; // Pointer to actual image data in buffer (BGRA32 format (1 byte for each channel))

            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = bitmap.BackBufferStride;
            int bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;

            List<Point3d> rawPoints = new List<Point3d>() { new Point3d(this.p1), new Point3d(this.p2) };

            foreach (var point in rawPoints)
            {
                double x, y, z, w;
                x = point.X;
                y = point.Y;
                z = point.Z;
                w = point.W;

                point.X = M_ModelViewProjection.M11 * x + M_ModelViewProjection.M12 * y + M_ModelViewProjection.M13 * z + M_ModelViewProjection.M14 * w;
                point.Y = M_ModelViewProjection.M21 * x + M_ModelViewProjection.M22 * y + M_ModelViewProjection.M23 * z + M_ModelViewProjection.M24 * w;
                point.Z = M_ModelViewProjection.M31 * x + M_ModelViewProjection.M32 * y + M_ModelViewProjection.M33 * z + M_ModelViewProjection.M34 * w;
                point.W = M_ModelViewProjection.M41 * x + M_ModelViewProjection.M42 * y + M_ModelViewProjection.M43 * z + M_ModelViewProjection.M44 * w;
            }

            Point3d p1 = rawPoints[0];
            Point3d p2 = rawPoints[1];

            { // DDA Algorithm
                double dy = p2.Y - p1.Y;
                double dx = p2.X - p1.X;

                int step = Math.Abs(dx) > Math.Abs(dy) ? (int)Math.Abs(dx) : (int)Math.Abs(dy);

                double xInc = dx / (double)step;
                double yInc = dy / (double)step;

                double x = p1.X;
                double y = p1.Y;

                for (int j = 0; j <= step; j++)
                {
                    Utils.SetPixel(pBuffer, bitmap, (int)x, (int)y, color);
                    x += xInc;
                    y += yInc;
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
            return bitmapImage;
        }
    }
}
