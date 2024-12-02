using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoAnalyzer.Core.Models.Attributes;
using GeoAnalyzer.Core.Models.Styles;

namespace GeoAnalyzer.Core.Models.Geometry
{
    public class Feature
    {
        public int Id { get; set; }
        public IGeometry Geometry { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        public bool IsVisible { get; set; } = true;
        public LayerStyle Style { get; set; }
        public string Name { get; set; }
        public List<Feature> Features { get; set; } = new List<Feature>();
        public List<Feature> SubLayers { get; set; } = new List<Feature>();

        public Feature()
        {
            Style = new LayerStyle();
            Name = GetFeatureType().ToString();
        }

        public Feature(IGeometry geometry)
        {
            Geometry = geometry;
            Style = new LayerStyle();
            Name = GetFeatureType().ToString();
        }

        public Feature(IGeometry geometry, Dictionary<string, object> properties)
        {
            Geometry = geometry;
            Properties = properties ?? new Dictionary<string, object>();
            Style = new LayerStyle();
            Name = GetFeatureType().ToString();
        }

        public object GetAttribute(string name)
        {
            return Properties.GetAttribute(name);
        }

        public void SetAttribute(string name, object value)
        {
            Properties.SetAttribute(name, value);
        }

        public bool HasAttribute(string name)
        {
            return Properties.HasAttribute(name);
        }

        public IEnumerable<string> GetAttributeNames()
        {
            return Properties.GetAttributeNames();
        }

        public FeatureType GetFeatureType()
        {
            if (Geometry == null)
                return FeatureType.Unknown;

            switch (Geometry)
            {
                case Point _:
                    return FeatureType.Point;
                case Polyline _:
                    return FeatureType.Polyline;
                case Polygon _:
                    return FeatureType.Polygon;
                default:
                    return FeatureType.Unknown;
            }
        }

        // 添加一个新方法来输出调试信息
        public void PrintDebugInfo()
        {
            Console.WriteLine($"Feature结构:");
            Console.WriteLine($"- Geometry类型: {Geometry?.GetType().Name}");
            Console.WriteLine($"- Properties数量: {Properties?.Count ?? 0}");
        }

        public IEnumerable<string> GetNumericAttributes()
        {
            try
            {
                Console.WriteLine("\n=== 获取数值型属性开始 ===");
                Console.WriteLine($"Feature名称: {Name}");
                Console.WriteLine($"Properties是否为空: {Properties == null}");
                Console.WriteLine($"Features数量: {Features?.Count ?? 0}");
                
                // 打印所有属性的信息
                if (Properties != null)
                {
                    Console.WriteLine("\n当前Properties中的所有属性:");
                    foreach (var prop in Properties)
                    {
                        Console.WriteLine($"- 属性名: {prop.Key}");
                        Console.WriteLine($"  类型: {prop.Value?.GetType().FullName ?? "null"}");
                        Console.WriteLine($"  值: {prop.Value ?? "null"}");
                    }
                }

                // 检查否是图层级别的Feature
                if (Features != null && Features.Count > 0)
                {
                    Console.WriteLine("\n使用第一个子Feature的属性:");
                    var firstFeature = Features.First();
                    if (firstFeature.Properties != null)
                    {
                        var numericAttributes = firstFeature.Properties
                            .Where(p => p.Value != null && IsNumericType(p.Value))
                            .Select(p => p.Key)
                            .ToList();

                        Console.WriteLine($"\n找到 {numericAttributes.Count} 个数值型属性:");
                        foreach (var attr in numericAttributes)
                        {
                            var value = firstFeature.Properties[attr];
                            Console.WriteLine($"- {attr}: {value?.GetType().Name} = {value}");
                        }

                        return numericAttributes;
                    }
                }
                else
                {
                    Console.WriteLine("\n使用当前Feature的属性:");
                    var numericAttributes = Properties
                        .Where(p => p.Value != null && IsNumericType(p.Value))
                        .Select(p => p.Key)
                        .ToList();

                    Console.WriteLine($"\n找到 {numericAttributes.Count} 个数值型属性:");
                    foreach (var attr in numericAttributes)
                    {
                        var value = Properties[attr];
                        Console.WriteLine($"- {attr}: {value?.GetType().Name} = {value}");
                    }

                    return numericAttributes;
                }

                Console.WriteLine("没有找到任何数值型属性");
                return Enumerable.Empty<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n获取数值型属性时出错:");
                Console.WriteLine($"错误类型: {ex.GetType().Name}");
                Console.WriteLine($"错误信息: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                return Enumerable.Empty<string>();
            }
            finally
            {
                Console.WriteLine("=== 获取数值型属性结束 ===\n");
            }
        }

        private bool IsNumericType(object value)
        {
            if (value == null) return false;
            
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }

    public enum FeatureType
    {
        Unknown,
        Point,
        Polyline,
        Polygon
    }

    public interface IGeometry
    {
        BoundingBox GetBoundingBox();
        bool Intersects(IGeometry other);
        double GetArea();
        double GetLength();
    }

    public class BoundingBox
    {
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }

        public BoundingBox(double minX, double minY, double maxX, double maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        public bool Intersects(BoundingBox other)
        {
            return !(other.MinX > MaxX || 
                     other.MaxX < MinX || 
                     other.MinY > MaxY || 
                     other.MaxY < MinY);
        }
    }
}
