using GeoAnalyzer.Core.IO.Common;
using GeoAnalyzer.Core.Models.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using GeoAnalyzer.Core.IO.DBF;
using GeoAnalyzer.Core.IO.Common.Exceptions;
using GeoAnalyzer.Core.Models.Attributes;

namespace GeoAnalyzer.Core.IO.Shapefile
{
    public class ShapefileReader
    {
        // 定义文件头结构
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct ShapefileHeader
        {
            public int Unused1, Unused2, Unused3, Unused4;
            public int Unused5, Unused6, Unused7, Unused8;
            public int ShapeType;
            public double Xmin;
            public double Ymin;
            public double Xmax;
            public double Ymax;
            public double Unused9, Unused10, Unused11, Unused12;
        }

        // 定义记录头结构
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct RecordHeader
        {
            public int RecordNumber;
            public int RecordLength;
            public int ShapeType;
        }

        private const int NULL_SHAPE = 0;
        private const int POINT_TYPE = 1;
        private const int POLYLINE_TYPE = 3;
        private const int POLYGON_TYPE = 5;

        public List<Feature> ReadAll(string filename)
        {
            Console.WriteLine("========= 开始读取Shapefile =========");
            Console.WriteLine($"文件路径: {filename}");
            if (!CanRead(filename))
                throw new ArgumentException("不支持的文件格式");

            var features = new List<Feature>();
            var dbfPath = Path.ChangeExtension(filename, ".dbf");

            try
            {
                Console.WriteLine($"开始读取Shapefile: {filename}");
                
                using (var reader = new BinaryReader(File.Open(filename, FileMode.Open)))
                {
                    // 读取文件头
                    ShapefileHeader header = (ShapefileHeader)FromBytes2Struct(reader, typeof(ShapefileHeader));
                    Console.WriteLine($"几何类型: {header.ShapeType}");
                    Console.WriteLine($"边界框: ({header.Xmin},{header.Ymin}) - ({header.Xmax},{header.Ymax})");

                    // 读取要素记录
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        try
                        {
                            // 读取记录头
                            RecordHeader recordHeader = (RecordHeader)FromBytes2Struct(reader, typeof(RecordHeader));
                            int byteLength = ReverseInt(recordHeader.RecordLength) * 2 - 4;
                            byte[] recordContent = reader.ReadBytes(byteLength);

                            Feature feature = null;
                            switch (header.ShapeType)
                            {
                                case POINT_TYPE:
                                    feature = ReadPointFeature(recordContent);
                                    break;
                                case POLYLINE_TYPE:
                                    feature = ReadPolylineFeature(recordContent);
                                    break;
                                case POLYGON_TYPE:
                                    feature = ReadPolygonFeature(recordContent);
                                    break;
                            }

                            if (feature != null)
                            {
                                Console.WriteLine($"成功读取要素: 类型={feature.GetFeatureType()}");
                                features.Add(feature);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"读取单个要素时出错: {ex.Message}");
                            continue;
                        }
                    }
                }

                // 读取DBF属性表
                if (File.Exists(dbfPath))
                {
                    ReadDBFAttributes(dbfPath, features);
                }

                Console.WriteLine($"成功读取 {features.Count} 个要素");
                Console.WriteLine($"读取完成，总要素数: {features.Count}");
                Console.WriteLine("要素列表结构:");
                foreach (var feature in features)
                {
                    Console.WriteLine($"要素类型: {feature.GetFeatureType()}, 属性数量: {feature.Properties?.Count ?? 0}");
                }
                Console.WriteLine("========= Shapefile读取完成 =========");
                return features;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取Shapefile失败: {ex.Message}");
                throw new GeoIOException($"读取Shapefile失败: {ex.Message}", filename, "Read", ex);
            }
        }

        private Feature ReadPointFeature(byte[] recordContent)
        {
            double x = BitConverter.ToDouble(recordContent, 0);
            double y = BitConverter.ToDouble(recordContent, 8);
            return new Feature(new Point(x, y));
        }

        private Feature ReadPolylineFeature(byte[] recordContent)
        {
            int numParts = BitConverter.ToInt32(recordContent, 32);
            int numPoints = BitConverter.ToInt32(recordContent, 36);
            
            int[] parts = new int[numParts + 1];
            for (int i = 0; i < numParts; i++)
            {
                parts[i] = BitConverter.ToInt32(recordContent, 40 + i * 4);
            }
            parts[numParts] = numPoints;

            var points = new List<Point>();
            for (int i = 0; i < numPoints; i++)
            {
                double x = BitConverter.ToDouble(recordContent, 40 + numParts * 4 + i * 16);
                double y = BitConverter.ToDouble(recordContent, 40 + numParts * 4 + i * 16 + 8);
                points.Add(new Point(x, y));
            }

            return new Feature(new Polyline(points));
        }

        private Feature ReadPolygonFeature(byte[] recordContent)
        {
            int numParts = BitConverter.ToInt32(recordContent, 32);
            int numPoints = BitConverter.ToInt32(recordContent, 36);
            
            int[] parts = new int[numParts + 1];
            for (int i = 0; i < numParts; i++)
            {
                parts[i] = BitConverter.ToInt32(recordContent, 40 + i * 4);
            }
            parts[numParts] = numPoints;

            var points = new List<Point>();
            for (int i = 0; i < numPoints; i++)
            {
                double x = BitConverter.ToDouble(recordContent, 40 + numParts * 4 + i * 16);
                double y = BitConverter.ToDouble(recordContent, 40 + numParts * 4 + i * 16 + 8);
                points.Add(new Point(x, y));
            }

            return new Feature(new Polygon(points));
        }

        private void ReadDBFAttributes(string dbfPath, List<Feature> features)
        {
            try
            {
                Console.WriteLine($"开始读取DBF文件: {dbfPath}");
                
                using (var dbfReader = new DBFReader(dbfPath))
                {
                    int featureIndex = 0;
                    Dictionary<string, object> record;
                    
                    while ((record = dbfReader.ReadRecord()) != null && featureIndex < features.Count)
                    {
                        if (record.Count > 0)
                        {
                            // 添加属性
                            foreach (var field in record)
                            {
                                features[featureIndex].SetAttribute(field.Key, field.Value);
                            }
                            
                            Console.WriteLine($"读取要素 {featureIndex} 的属性: {record.Count} 个字段");
                            
                            // 打印属性信息用于调试
                            foreach (var attrName in features[featureIndex].GetAttributeNames())
                            {
                                Console.WriteLine($"  字段: {attrName} = {features[featureIndex].GetAttribute(attrName)}");
                            }
                        }
                        featureIndex++;
                    }
                    
                    Console.WriteLine($"成功读取 {featureIndex} 条属性记录");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取DBF文件失败: {ex.Message}");
                throw new GeoIOException($"读取DBF文件失败: {ex.Message}", dbfPath, "ReadDBF", ex);
            }
        }

        private static Object FromBytes2Struct(BinaryReader br, Type type)
        {
            byte[] buff = br.ReadBytes(Marshal.SizeOf(type));
            GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
            Object result = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type);
            handle.Free();
            return result;
        }

        private int ReverseInt(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public bool CanRead(string filePath)
        {
            return Path.GetExtension(filePath).ToLower() == ".shp";
        }
    }
}
