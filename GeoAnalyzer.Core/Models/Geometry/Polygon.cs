using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoAnalyzer.Core.Models.Geometry
{
    public class Polygon : IGeometry
    {
        private List<Point> vertices;
        private List<List<Point>> holes;  // 内部环（洞）

        public IReadOnlyList<Point> Vertices => vertices.AsReadOnly();
        public IReadOnlyList<List<Point>> Holes => holes.AsReadOnly();

        public List<Point> Points { get; set; } = new List<Point>();

        public Polygon()
        {
            vertices = new List<Point>();
            holes = new List<List<Point>>();
            Points = new List<Point>();
        }

        public Polygon(IEnumerable<Point> points)
        {
            vertices = new List<Point>(points);
            holes = new List<List<Point>>();
            Points = new List<Point>(points);
            ValidatePolygon();
        }

        public void AddVertex(Point point)
        {
            vertices.Add(point);
        }

        public void AddHole(List<Point> holeVertices)
        {
            if (holeVertices.Count < 3)
                throw new ArgumentException("多边形洞至少需要3个顶点");

            holes.Add(new List<Point>(holeVertices));
        }

        public BoundingBox GetBoundingBox()
        {
            if (vertices.Count == 0)
                throw new InvalidOperationException("多边形没有顶点");

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
                return ContainsPoint(point);
            }
            else if (other is Polygon otherPolygon)
            {
                // 检查是否有任何边相交
                for (int i = 0; i < vertices.Count; i++)
                {
                    Point current = vertices[i];
                    Point next = vertices[(i + 1) % vertices.Count];

                    for (int j = 0; j < otherPolygon.vertices.Count; j++)
                    {
                        Point otherCurrent = otherPolygon.vertices[j];
                        Point otherNext = otherPolygon.vertices[(j + 1) % otherPolygon.vertices.Count];

                        if (LineSegmentsIntersect(current, next, otherCurrent, otherNext))
                            return true;
                    }
                }
            }

            return false;
        }

        public double GetArea()
        {
            // 使用Shoelace公式计算面积
            double area = Math.Abs(CalculateSignedArea(vertices));

            // 减去所有洞的面积
            foreach (var hole in holes)
            {
                area -= Math.Abs(CalculateSignedArea(hole));
            }

            return area;
        }

        public double GetLength()
        {
            // 计算外部边界的周长
            double perimeter = CalculatePerimeter(vertices);

            // 加上所有洞的周长
            foreach (var hole in holes)
            {
                perimeter += CalculatePerimeter(hole);
            }

            return perimeter;
        }

        private double CalculateSignedArea(List<Point> points)
        {
            double area = 0;
            for (int i = 0; i < points.Count; i++)
            {
                Point current = points[i];
                Point next = points[(i + 1) % points.Count];
                area += (current.X * next.Y) - (next.X * current.Y);
            }
            return area / 2;
        }

        private double CalculatePerimeter(List<Point> points)
        {
            double perimeter = 0;
            for (int i = 0; i < points.Count; i++)
            {
                Point current = points[i];
                Point next = points[(i + 1) % points.Count];
                perimeter += current.DistanceTo(next);
            }
            return perimeter;
        }

        private bool ContainsPoint(Point point)
        {
            bool inside = IsPointInPolygon(vertices, point);
            
            // 如果点在外部边界内，检查是否在任何洞内
            if (inside)
            {
                foreach (var hole in holes)
                {
                    if (IsPointInPolygon(hole, point))
                        return false; // 点在洞内，因此不在多边形内
                }
            }
            
            return inside;
        }

        private bool IsPointInPolygon(List<Point> polygonVertices, Point point)
        {
            bool inside = false;
            for (int i = 0, j = polygonVertices.Count - 1; i < polygonVertices.Count; j = i++)
            {
                if (((polygonVertices[i].Y > point.Y) != (polygonVertices[j].Y > point.Y)) &&
                    (point.X < (polygonVertices[j].X - polygonVertices[i].X) * (point.Y - polygonVertices[i].Y) / 
                    (polygonVertices[j].Y - polygonVertices[i].Y) + polygonVertices[i].X))
                {
                    inside = !inside;
                }
            }
            return inside;
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

        private void ValidatePolygon()
        {
            if (vertices.Count < 3)
                throw new ArgumentException("多边形至少需要3个顶点");

            // 检查首尾点是否相同
            if (!vertices[0].Equals(vertices[vertices.Count - 1]))
                vertices.Add(vertices[0]);
        }

        public Polygon Clone()
        {
            var newPolygon = new Polygon(vertices.Select(v => v.Clone()));
            foreach (var hole in holes)
            {
                newPolygon.AddHole(hole.Select(v => v.Clone()).ToList());
            }
            return newPolygon;
        }

        public override string ToString()
        {
            return $"Polygon({vertices.Count} vertices, {holes.Count} holes)";
        }
    }
}
