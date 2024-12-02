using System;

namespace GeoAnalyzer.Core.Models.Geometry
{
    public class Point : IGeometry
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point() { }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(X, Y, X, Y);
        }

        public bool Intersects(IGeometry other)
        {
            if (other is Point otherPoint)
            {
                // 对于点来说，相交意味着坐标相同
                return Math.Abs(X - otherPoint.X) < double.Epsilon && 
                       Math.Abs(Y - otherPoint.Y) < double.Epsilon;
            }

            // 与其他几何类型的相交检查
            return other.GetBoundingBox().Intersects(this.GetBoundingBox());
        }

        public double GetArea()
        {
            // 点的面积为0
            return 0;
        }

        public double GetLength()
        {
            // 点的长度为0
            return 0;
        }

        public double DistanceTo(Point other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public override bool Equals(object obj)
        {
            if (obj is Point other)
            {
                return Math.Abs(X - other.X) < double.Epsilon && 
                       Math.Abs(Y - other.Y) < double.Epsilon;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return $"Point({X}, {Y})";
        }

        public Point Clone()
        {
            return new Point(X, Y);
        }

        // 静态工厂方法
        public static Point FromCoordinates(double x, double y)
        {
            return new Point(x, y);
        }

        // 运算符重载
        public static bool operator ==(Point left, Point right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);
            return left.Equals(right);
        }

        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }

        // 添加转换方法
        public System.Drawing.Point ToDrawingPoint()
        {
            return new System.Drawing.Point((int)X, (int)Y);
        }

        public System.Drawing.PointF ToDrawingPointF()
        {
            return new System.Drawing.PointF((float)X, (float)Y);
        }
    }
}
