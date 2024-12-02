using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoAnalyzer.Core.Models.Geometry
{
    public class Polyline : IGeometry
    {
        private List<Point> vertices;
        public List<Point> Points { get; set; } = new List<Point>();

        public IReadOnlyList<Point> Vertices => vertices.AsReadOnly();

        public Polyline()
        {
            vertices = new List<Point>();
            Points = new List<Point>();
        }

        public Polyline(IEnumerable<Point> points)
        {
            vertices = new List<Point>(points);
            ValidatePolyline();
        }

        public void AddVertex(Point point)
        {
            vertices.Add(point);
        }

        public void InsertVertex(int index, Point point)
        {
            vertices.Insert(index, point);
        }

        public void RemoveVertex(int index)
        {
            if (index >= 0 && index < vertices.Count)
            {
                vertices.RemoveAt(index);
            }
        }

        public BoundingBox GetBoundingBox()
        {
            if (vertices.Count == 0)
                throw new InvalidOperationException("折线没有顶点");

            double minX = vertices.Min(p => p.X);
            double minY = vertices.Min(p => p.Y);
            double maxX = vertices.Max(p => p.X);
            double maxY = vertices.Max(p => p.Y);

            return new BoundingBox(minX, minY, maxX, maxY);
        }

        public bool Intersects(IGeometry other)
        {
            // 首先检查边界框是否相交
            if (!this.GetBoundingBox().Intersects(other.GetBoundingBox()))
                return false;

            if (other is Point point)
            {
                return IsPointOnPolyline(point);
            }
            else if (other is Polyline otherPolyline)
            {
                // 检查任意线段是否相交
                for (int i = 0; i < vertices.Count - 1; i++)
                {
                    for (int j = 0; j < otherPolyline.vertices.Count - 1; j++)
                    {
                        if (LineSegmentsIntersect(
                            vertices[i], vertices[i + 1],
                            otherPolyline.vertices[j], otherPolyline.vertices[j + 1]))
                        {
                            return true;
                        }
                    }
                }
            }
            else if (other is Polygon polygon)
            {
                // 检查是否与多边形的任意边相交
                for (int i = 0; i < vertices.Count - 1; i++)
                {
                    for (int j = 0; j < polygon.Vertices.Count - 1; j++)
                    {
                        if (LineSegmentsIntersect(
                            vertices[i], vertices[i + 1],
                            polygon.Vertices[j], polygon.Vertices[j + 1]))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public double GetArea()
        {
            // 折线没有面积
            return 0;
        }

        public double GetLength()
        {
            double length = 0;
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                length += vertices[i].DistanceTo(vertices[i + 1]);
            }
            return length;
        }

        private bool IsPointOnPolyline(Point point)
        {
            const double tolerance = 1e-10; // 容差值

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                Point p1 = vertices[i];
                Point p2 = vertices[i + 1];

                // 计算点到线段的距离
                double lineLength = p1.DistanceTo(p2);
                if (lineLength == 0) continue;

                double u = ((point.X - p1.X) * (p2.X - p1.X) + (point.Y - p1.Y) * (p2.Y - p1.Y)) / (lineLength * lineLength);

                if (u < 0 || u > 1) continue;

                double x = p1.X + u * (p2.X - p1.X);
                double y = p1.Y + u * (p2.Y - p1.Y);

                double distance = Math.Sqrt((x - point.X) * (x - point.X) + (y - point.Y) * (y - point.Y));

                if (distance < tolerance)
                    return true;
            }

            return false;
        }

        private bool LineSegmentsIntersect(Point p1, Point p2, Point p3, Point p4)
        {
            double denominator = ((p4.Y - p3.Y) * (p2.X - p1.X)) - ((p4.X - p3.X) * (p2.Y - p1.Y));

            if (Math.Abs(denominator) < double.Epsilon)
                return false;

            double ua = (((p4.X - p3.X) * (p1.Y - p3.Y)) - ((p4.Y - p3.Y) * (p1.X - p3.X))) / denominator;
            double ub = (((p2.X - p1.X) * (p1.Y - p3.Y)) - ((p2.Y - p1.Y) * (p1.X - p3.X))) / denominator;

            return (ua >= 0 && ua <= 1) && (ub >= 0 && ub <= 1);
        }

        private void ValidatePolyline()
        {
            if (vertices.Count < 2)
                throw new ArgumentException("折线至少需要2个顶点");
        }

        public Polyline Clone()
        {
            return new Polyline(vertices.Select(v => v.Clone()));
        }

        public override string ToString()
        {
            return $"Polyline({vertices.Count} vertices)";
        }

        // 获取指定位置的点
        public Point GetPoint(int index)
        {
            if (index < 0 || index >= vertices.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            return vertices[index];
        }

        // 获取折线的所有线段
        public IEnumerable<(Point Start, Point End)> GetSegments()
        {
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                yield return (vertices[i], vertices[i + 1]);
            }
        }

        // 计算折线上某个参数位置的点
        public Point GetPointAtParameter(double t)
        {
            if (t < 0 || t > 1)
                throw new ArgumentOutOfRangeException(nameof(t), "参数t必须在0到1之间");

            double totalLength = GetLength();
            double targetLength = totalLength * t;
            double currentLength = 0;

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                double segmentLength = vertices[i].DistanceTo(vertices[i + 1]);
                if (currentLength + segmentLength >= targetLength)
                {
                    double segmentParameter = (targetLength - currentLength) / segmentLength;
                    return new Point(
                        vertices[i].X + segmentParameter * (vertices[i + 1].X - vertices[i].X),
                        vertices[i].Y + segmentParameter * (vertices[i + 1].Y - vertices[i].Y)
                    );
                }
                currentLength += segmentLength;
            }

            return vertices[vertices.Count - 1];
        }
    }
}
