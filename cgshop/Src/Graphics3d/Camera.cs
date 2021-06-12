using cgshop.point;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace cgshop
{
    public class Camera
    {
        public Point3d position = new Point3d(5,3,-5);
        public Point3d viewTarget = new Point3d(0, 0, 0);
        public Point3d worldUpDirection = new Point3d(0, 1, 0);
        public double width = 500;
        public double height = 500;
        public double distance = 6;

        public Matrix4x4 ViewMatrix()
        {
            Point3d cameraPosition, cTarget, cUp, cX, cY, cZ, tmp;
            double length;
            cameraPosition = new Point3d(position.X, position.Y, 5);
            cTarget = new Point3d(viewTarget.X, viewTarget.Y, viewTarget.Z);
            cUp = new Point3d(0, 1, 0);
            tmp = new Point3d(cameraPosition.X - cTarget.X, cameraPosition.Y - cTarget.Y, cameraPosition.Z - cTarget.Z);
            length = VectorLength(tmp);
            cZ = new Point3d(tmp.X / length, tmp.Y / length, tmp.Z / length);
            tmp = VectorCrossProduct(cUp, cZ);
            length = VectorLength(tmp);
            cX = new Point3d(tmp.X / length, tmp.Y / length, tmp.Z / length);
            tmp = VectorCrossProduct(cZ, cX);
            length = VectorLength(tmp);
            cY = new Point3d(tmp.X / length, tmp.Y / length, tmp.Z / length);
      
            Matrix4x4 result = new Matrix4x4((float)cX.X, (float)cX.Y, (float)cX.Z, (float)VectorsMultiplication(cX, cameraPosition),
                                             (float)cY.X, (float)cY.Y, (float)cY.Z, (float)VectorsMultiplication(cY, cameraPosition),
                                             (float)cZ.X, (float)cZ.Y, (float)cZ.Z, (float)VectorsMultiplication(cZ, cameraPosition),
                                             0, 0, 0, 1);
            return result;
        }

        public double VectorLength(Point3d vec)
        {
            return Math.Pow(Math.Pow(vec.X, 2) + Math.Pow(vec.Y, 2) + Math.Pow(vec.Z, 2), (double)1 / 3);
        }

        public Point3d VectorCrossProduct(Point3d vec1, Point3d vec2)
        {
            Point3d result = new Point3d((vec1.Y * vec2.Z) - (vec1.Z * vec2.Y), (vec1.Z * vec2.X) - (vec1.X * vec2.Z), (vec1.X * vec2.Y) - (vec1.Y * vec2.X)); 
            return result;
        }

        public double VectorsMultiplication(Point3d vec1, Point3d vec2)
        {
            double result = vec1.X * vec2.X + vec1.Y * vec2.Y + vec1.Z * vec2.Z;
            return result;
        }
    }



}
