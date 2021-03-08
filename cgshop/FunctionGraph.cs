using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace cgshop
{
    public class GraphPoint
    {
        private Point value;
        public Point Value { 
            get 
            { 
                return value; 
            } 
            set 
            {
                this.value = new Point(Utils.Clamp((int)value.X, 0, 255), Utils.Clamp((int)value.Y, 0, 255)); 
            } 
        }

        public GraphPoint(int x, int y)
        {
            Value = new Point(x, y);
        }
    }

    public class Graph
    {
        public List<GraphPoint> points { get; set;  }

        public Graph(List<GraphPoint> points)
        {
            this.points = points;
        }

        public int AddPoint(GraphPoint point)
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (point.Value.X < points[i].Value.X)
                {
                    points.Insert(i, point);
                    return i;
                }
            }

            throw new Exception("Trying to add point with wrong coordinates to the function graph");
        }
    }

}
