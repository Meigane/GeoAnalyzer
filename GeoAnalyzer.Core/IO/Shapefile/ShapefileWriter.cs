using GeoAnalyzer.Core.IO.Common;
using GeoAnalyzer.Core.Models.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using GeoAnalyzer.Core.IO.DBF;
using GeoAnalyzer.Core.IO.Common.Exceptions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GeoAnalyzer.Core.IO.Shapefile
{
    public class ShapefileWriter : IDataWriter
    {
        public bool CanWrite(string filePath)
        {
            return Path.GetExtension(filePath).ToLower() == ".shp";
        }

        public void Write(string filePath, List<Feature> features)
        {
            if (!CanWrite(filePath))
                throw new ArgumentException("不支持的文件格式");

            try
            {
                using (var writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
                {
                    // 写入文件头
                    writer.Write(SwapBytes(9994)); // 文件代码
                    for (int i = 0; i < 5; i++)
                        writer.Write(SwapBytes(0)); // 5个未使用的32位整数

                    // 临时写入文件长度（后面更新）
                    writer.Write(SwapBytes(0));

                    // 写入版本
                    writer.Write(1000);

                    // 确定几何类型
                    int shapeType = DetermineShapeType(features);
                    writer.Write(shapeType);

                    // 计算并写入边界框
                    var boundingBox = CalculateBoundingBox(features);
                    writer.Write(boundingBox.MinX);
                    writer.Write(boundingBox.MinY);
                    writer.Write(boundingBox.MaxX);
                    writer.Write(boundingBox.MaxY);

                    // 写入Z范围（如果需要）
                    writer.Write(0.0); // zMin
                    writer.Write(0.0); // zMax

                    // 写入M范围（如果需要）
                    writer.Write(0.0); // mMin
                    writer.Write(0.0); // mMax

                    // 写入要素
                    int recordNumber = 1;
                    foreach (var feature in features)
                    {
                        WriteFeature(writer, feature, recordNumber++);
                    }

                    // 更新文件长度
                    long fileLength = writer.BaseStream.Length / 2; // 以16位字为单位
                    if (fileLength > int.MaxValue)
                    {
                        throw new GeoIOException("文件太大，超出Shapefile格式限制", 
                            filePath, 
                            "Write", 
                            new InvalidOperationException("File length exceeds maximum allowed size"));
                    }
                    writer.BaseStream.Seek(24, SeekOrigin.Begin);
                    writer.Write(SwapBytes((int)fileLength));
                }

                // 写入 DBF 文件
                var dbfPath = Path.ChangeExtension(filePath, ".dbf");
                var fields = DetermineDBFFields(features);
                using (var dbfWriter = new DBFWriter(dbfPath, fields))
                {
                    foreach (var feature in features)
                    {
                        if (feature.Properties != null)
                            dbfWriter.WriteRecord(feature.Properties);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new GeoIOException("写入Shapefile文件失败", filePath, "Write", ex);
            }
        }

        public async Task WriteAsync(string filePath, List<Feature> features, 
            IProgress<int> progress = null, CancellationToken cancellationToken = default)
        {
            if (!CanWrite(filePath))
                throw new ArgumentException("不支持的文件格式");

            try
            {
                await Task.Run(() =>
                {
                    using (var writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
                    {
                        // 写入文件头
                        writer.Write(SwapBytes(9994)); // 文件代码
                        for (int i = 0; i < 5; i++)
                            writer.Write(SwapBytes(0)); // 5个未使用的32位整数

                        cancellationToken.ThrowIfCancellationRequested();

                        // 临时写入文件长度（后面更新）
                        writer.Write(SwapBytes(0));

                        // 写入版本
                        writer.Write(1000);

                        // 确定几何类型
                        int shapeType = DetermineShapeType(features);
                        writer.Write(shapeType);

                        cancellationToken.ThrowIfCancellationRequested();

                        // 计算并写入边界框
                        var boundingBox = CalculateBoundingBox(features);
                        writer.Write(boundingBox.MinX);
                        writer.Write(boundingBox.MinY);
                        writer.Write(boundingBox.MaxX);
                        writer.Write(boundingBox.MaxY);

                        // 写入Z范围（如果需要）
                        writer.Write(0.0); // zMin
                        writer.Write(0.0); // zMax

                        // 写入M范围（如果需要）
                        writer.Write(0.0); // mMin
                        writer.Write(0.0); // mMax

                        cancellationToken.ThrowIfCancellationRequested();

                        // 写入要素
                        int recordNumber = 1;
                        int totalFeatures = features.Count;
                        for (int i = 0; i < features.Count; i++)
                        {
                            WriteFeature(writer, features[i], recordNumber++);
                            
                            // 报告进度
                            progress?.Report((i + 1) * 100 / totalFeatures);
                            
                            cancellationToken.ThrowIfCancellationRequested();
                        }

                        // 更新文件长度
                        long fileLength = writer.BaseStream.Length / 2; // 以16位字为单位
                        if (fileLength > int.MaxValue)
                        {
                            throw new GeoIOException("文件太大，超出Shapefile格式限制", 
                                filePath, 
                                "WriteAsync", 
                                new InvalidOperationException("File length exceeds maximum allowed size"));
                        }
                        writer.BaseStream.Seek(24, SeekOrigin.Begin);
                        writer.Write(SwapBytes((int)fileLength));
                    }

                    // 写入 DBF 文件
                    var dbfPath = Path.ChangeExtension(filePath, ".dbf");
                    var fields = DetermineDBFFields(features);
                    using (var dbfWriter = new DBFWriter(dbfPath, fields))
                    {
                        int count = 0;
                        foreach (var feature in features)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            if (feature.Properties != null)
                                dbfWriter.WriteRecord(feature.Properties);

                            count++;
                            progress?.Report(count * 100 / features.Count);
                        }
                    }
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // 清理未完成的文件
                if (File.Exists(filePath))
                    File.Delete(filePath);
                var dbfPath = Path.ChangeExtension(filePath, ".dbf");
                if (File.Exists(dbfPath))
                    File.Delete(dbfPath);

                throw; // 重新抛出取消异常
            }
            catch (Exception ex)
            {
                throw new GeoIOException("异步写入Shapefile文件失败", filePath, "WriteAsync", ex);
            }
        }

        private int DetermineShapeType(List<Feature> features)
        {
            if (features.Count == 0)
                return 0;

            var firstFeature = features[0];
            switch (firstFeature.GetFeatureType())
            {
                case FeatureType.Point:
                    return 1;
                case FeatureType.Polyline:
                    return 3;
                case FeatureType.Polygon:
                    return 5;
                default:
                    throw new NotSupportedException("不支持的几何类型");
            }
        }

        private BoundingBox CalculateBoundingBox(List<Feature> features)
        {
            if (features.Count == 0)
                return new BoundingBox(0, 0, 0, 0);

            var bbox = features[0].Geometry.GetBoundingBox();
            double minX = bbox.MinX, minY = bbox.MinY;
            double maxX = bbox.MaxX, maxY = bbox.MaxY;

            foreach (var feature in features.Skip(1))
            {
                var featureBbox = feature.Geometry.GetBoundingBox();
                minX = Math.Min(minX, featureBbox.MinX);
                minY = Math.Min(minY, featureBbox.MinY);
                maxX = Math.Max(maxX, featureBbox.MaxX);
                maxY = Math.Max(maxY, featureBbox.MaxY);
            }

            return new BoundingBox(minX, minY, maxX, maxY);
        }

        private void WriteFeature(BinaryWriter writer, Feature feature, int recordNumber)
        {
            // 写入记录头
            writer.Write(SwapBytes(recordNumber));

            // 获取要素内容的起始位置
            long contentStart = writer.BaseStream.Position;

            // 写入几何数据
            switch (feature.GetFeatureType())
            {
                case FeatureType.Point:
                    WritePointFeature(writer, feature);
                    break;
                case FeatureType.Polyline:
                    WritePolylineFeature(writer, feature);
                    break;
                case FeatureType.Polygon:
                    WritePolygonFeature(writer, feature);
                    break;
            }

            // 计算并写入内容长度
            long currentPosition = writer.BaseStream.Position;
            long contentLength = (currentPosition - contentStart) / 2;
            
            // 检查内容长度是否超出int范围
            if (contentLength > int.MaxValue)
            {
                throw new GeoIOException(
                    message: "要素内容太大，超出Shapefile格式限制", 
                    filePath: "WriteFeature", 
                    operation: "WriteFeature", 
                    innerException: new InvalidOperationException($"Content length {contentLength} exceeds maximum allowed size {int.MaxValue}")
                );
            }

            // 保存当前位置
            long savedPosition = writer.BaseStream.Position;

            try
            {
                // 回到内容长度位置
                if (contentStart - 4 < 0)
                {
                    throw new GeoIOException(
                        message: "文件位置错误",
                        filePath: "WriteFeature",
                        operation: "WriteFeature",
                        innerException: new InvalidOperationException("Invalid content start position")
                    );
                }

                writer.BaseStream.Seek(contentStart - 4, SeekOrigin.Begin);
                
                // 写入内容长度（现在安全地转换为int）
                writer.Write(SwapBytes((int)contentLength));
            }
            finally
            {
                // 确保总是回到原来的位置
                writer.BaseStream.Seek(savedPosition, SeekOrigin.Begin);
            }
        }

        private void WritePointFeature(BinaryWriter writer, Feature feature)
        {
            var point = feature.Geometry as Point;
            writer.Write(point.X);
            writer.Write(point.Y);
        }

        private void WritePolylineFeature(BinaryWriter writer, Feature feature)
        {
            var polyline = feature.Geometry as Polyline;
            var bbox = polyline.GetBoundingBox();

            // 写入边界框
            writer.Write(bbox.MinX);
            writer.Write(bbox.MinY);
            writer.Write(bbox.MaxX);
            writer.Write(bbox.MaxY);

            // 写入部件数和点数
            writer.Write(1); // numParts
            writer.Write(polyline.Vertices.Count); // numPoints

            // 写入部件索引
            writer.Write(0);

            // 写入点
            foreach (var point in polyline.Vertices)
            {
                writer.Write(point.X);
                writer.Write(point.Y);
            }
        }

        private void WritePolygonFeature(BinaryWriter writer, Feature feature)
        {
            var polygon = feature.Geometry as Polygon;
            var bbox = polygon.GetBoundingBox();

            // 写入边界框
            writer.Write(bbox.MinX);
            writer.Write(bbox.MinY);
            writer.Write(bbox.MaxX);
            writer.Write(bbox.MaxY);

            // 写入部件数和点数
            int numParts = 1 + polygon.Holes.Count;
            int numPoints = polygon.Vertices.Count;
            
            // 计算总点数并检查是否超出int范围
            long totalPoints = numPoints;
            foreach (var hole in polygon.Holes)
            {
                long holePoints = hole.Count;
                totalPoints += holePoints;
                if (totalPoints > int.MaxValue)
                {
                    throw new GeoIOException(
                        message: "多边形点数过多，超出Shapefile格式限制",
                        filePath: "WritePolygonFeature",
                        operation: "WritePolygonFeature",
                        innerException: new InvalidOperationException($"Total points {totalPoints} exceeds maximum allowed size {int.MaxValue}")
                    );
                }
            }
            numPoints = (int)totalPoints;

            writer.Write(numParts);
            writer.Write(numPoints);

            // 写入部件索引
            long currentIndexLong = 0;
            writer.Write((int)currentIndexLong);  // 第一个部件的索引总是0
            
            currentIndexLong += polygon.Vertices.Count;
            foreach (var hole in polygon.Holes)
            {
                if (currentIndexLong > int.MaxValue)
                {
                    throw new GeoIOException(
                        message: "多边形索引超出范围",
                        filePath: "WritePolygonFeature",
                        operation: "WritePolygonFeature",
                        innerException: new InvalidOperationException($"Part index {currentIndexLong} exceeds maximum allowed size {int.MaxValue}")
                    );
                }
                writer.Write((int)currentIndexLong);
                currentIndexLong += hole.Count;
            }

            // 写入外部边界点
            foreach (var point in polygon.Vertices)
            {
                writer.Write(point.X);
                writer.Write(point.Y);
            }

            // 写入洞的点
            foreach (var hole in polygon.Holes)
            {
                foreach (var point in hole)
                {
                    writer.Write(point.X);
                    writer.Write(point.Y);
                }
            }
        }

        private int SwapBytes(long value)
        {
            // 首先检查值是否在int范围内
            if (value > int.MaxValue || value < int.MinValue)
            {
                throw new GeoIOException(
                    message: "值超出int范围",
                    filePath: "SwapBytes",
                    operation: "SwapBytes",
                    innerException: new InvalidOperationException($"Value {value} exceeds int range")
                );
            }

            int intValue = (int)value;
            return (int)(((intValue & 0xFF000000) >> 24) |
                   ((intValue & 0x00FF0000) >> 8) |
                   ((intValue & 0x0000FF00) << 8) |
                   ((intValue & 0x000000FF) << 24));
        }

        private List<DBFField> DetermineDBFFields(List<Feature> features)
        {
            var fields = new List<DBFField>();
            if (features.Count == 0 || features[0].Properties == null)
                return fields;

            foreach (var kvp in features[0].Properties)
            {
                char type = DetermineFieldType(kvp.Value);
                int length = DetermineFieldLength(type);
                fields.Add(new DBFField(kvp.Key, type, length));
            }
            return fields;
        }

        private char DetermineFieldType(object value)
        {
            if (value == null) return 'C';
            switch (value)
            {
                case int _:
                case double _:
                case float _:
                    return 'N';
                case DateTime _:
                    return 'D';
                case bool _:
                    return 'L';
                default:
                    return 'C';
            }
        }

        private int DetermineFieldLength(char type)
        {
            switch (type)
            {
                case 'N': return 18;
                case 'D': return 8;
                case 'L': return 1;
                default: return 254;
            }
        }

        // 添加新的重载方法，用于写入单个Feature
        public void Write(string filePath, Feature feature)
        {
            if (feature == null)
                throw new ArgumentNullException(nameof(feature));

            // 将单个Feature转换为List后调用原有的Write方法
            Write(filePath, new List<Feature> { feature });
        }
    }
}
