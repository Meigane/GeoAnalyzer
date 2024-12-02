using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using GeoAnalyzer.Core.Models.Geometry;
using GeometryPoint = GeoAnalyzer.Core.Models.Geometry.Point;

namespace GeoAnalyzer.Core.SpatialAnalysis
{
    public class ClusteringService
    {
        public (Feature layer, string csvPath) PerformClustering(Feature layer, string[] attributes, int clusterCount, string outputPath = null)
        {
            Console.WriteLine("\n=== 开始执行聚类分析 ===");
            Console.WriteLine($"原始图层: {layer.Name}, 要素数量: {layer.Features.Count}");

            try
            {
                var data = ExtractFeatureData(layer, attributes);
                var normalizedData = NormalizeData(data);
                var clusters = KMeansClustering(normalizedData, clusterCount);
                
                // 创建结果图层
                var resultLayer = CreateResultLayer(layer, clusters);
                Console.WriteLine($"创建聚类结果图层: {resultLayer.Name}");
                Console.WriteLine($"- 主图层要素数量: {resultLayer.Features.Count}");
                Console.WriteLine($"- 标签图层要素数量: {resultLayer.SubLayers?[0].Features.Count ?? 0}");

                // 导出CSV结果
                string csvPath = ExportToCsv(layer, attributes, clusters, outputPath);

                return (resultLayer, csvPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"聚类分析失败: {ex.Message}");
                throw;
            }
        }

        private List<double[]> ExtractFeatureData(Feature layer, string[] attributes)
        {
            var data = new List<double[]>();
            foreach (var feature in layer.Features)
            {
                var values = new double[attributes.Length];
                for (int i = 0; i < attributes.Length; i++)
                {
                    if (feature.Properties.TryGetValue(attributes[i], out object value))
                    {
                        values[i] = Convert.ToDouble(value);
                    }
                }
                data.Add(values);
            }
            return data;
        }

        private List<double[]> NormalizeData(List<double[]> data)
        {
            int dimensions = data[0].Length;
            var mins = new double[dimensions];
            var maxs = new double[dimensions];

            // 找到每个维度的最大最小值
            for (int d = 0; d < dimensions; d++)
            {
                mins[d] = data.Min(row => row[d]);
                maxs[d] = data.Max(row => row[d]);
            }

            // 标准化
            return data.Select(row =>
                row.Select((val, d) => (val - mins[d]) / (maxs[d] - mins[d])).ToArray()
            ).ToList();
        }

        private int[] KMeansClustering(List<double[]> data, int k)
        {
            int n = data.Count;
            int dimensions = data[0].Length;
            var clusters = new int[n];
            var centroids = new double[k][];
            
            // 随机初始化聚类中心
            var random = new Random();
            for (int i = 0; i < k; i++)
            {
                centroids[i] = data[random.Next(n)].ToArray();
            }

            bool changed;
            int maxIterations = 100;
            int iteration = 0;

            do
            {
                changed = false;
                iteration++;

                // 分配点到最近的聚类中心
                for (int i = 0; i < n; i++)
                {
                    int nearestCluster = FindNearestCluster(data[i], centroids);
                    if (clusters[i] != nearestCluster)
                    {
                        clusters[i] = nearestCluster;
                        changed = true;
                    }
                }

                // 更新聚类中心
                for (int i = 0; i < k; i++)
                {
                    var clusterPoints = data.Where((p, idx) => clusters[idx] == i).ToList();
                    if (clusterPoints.Any())
                    {
                        for (int d = 0; d < dimensions; d++)
                        {
                            centroids[i][d] = clusterPoints.Average(p => p[d]);
                        }
                    }
                }

            } while (changed && iteration < maxIterations);

            Console.WriteLine($"聚类迭代次数: {iteration}");
            return clusters;
        }

        private int FindNearestCluster(double[] point, double[][] centroids)
        {
            int nearest = 0;
            double minDistance = double.MaxValue;

            for (int i = 0; i < centroids.Length; i++)
            {
                double distance = CalculateDistance(point, centroids[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = i;
                }
            }

            return nearest;
        }

        private double CalculateDistance(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
            {
                double diff = a[i] - b[i];
                sum += diff * diff;
            }
            return Math.Sqrt(sum);
        }

        private Feature CreateResultLayer(Feature originalLayer, int[] clusters)
        {
            Console.WriteLine("\n=== 开始创建聚类结果图层 ===");
            
            // 创建聚类结果图层
            var resultLayer = new Feature
            {
                Name = $"{originalLayer.Name}_Clustered",
                Geometry = originalLayer.Geometry,
                Properties = new Dictionary<string, object>(originalLayer.Properties),
                Features = new List<Feature>()
            };
            Console.WriteLine($"创建主图层: {resultLayer.Name}");

            // 创建标签图层
            var labelLayer = new Feature
            {
                Name = $"{originalLayer.Name}_Labels",
                Geometry = null,
                Properties = new Dictionary<string, object>
                {
                    { "LayerType", "Label" }
                },
                Features = new List<Feature>(),
                IsVisible = true
            };
            Console.WriteLine($"创建标签图层: {labelLayer.Name}");

            // 统计每个聚类的中心点位置
            var clusterCenters = new Dictionary<int, (double sumX, double sumY, int count)>();
            
            // 统计每个聚类的数量
            var clusterCounts = new int[clusters.Max() + 1];
            
            for (int i = 0; i < originalLayer.Features.Count; i++)
            {
                var originalFeature = originalLayer.Features[i];
                var newFeature = new Feature
                {
                    Geometry = originalFeature.Geometry,
                    Properties = new Dictionary<string, object>(originalFeature.Properties)
                };
                newFeature.Properties["Cluster"] = clusters[i];
                resultLayer.Features.Add(newFeature);
                clusterCounts[clusters[i]]++;

                // 计算聚类中心
                if (originalFeature.Geometry is GeometryPoint point)
                {
                    if (!clusterCenters.ContainsKey(clusters[i]))
                    {
                        clusterCenters[clusters[i]] = (point.X, point.Y, 1);
                    }
                    else
                    {
                        var current = clusterCenters[clusters[i]];
                        clusterCenters[clusters[i]] = (
                            current.sumX + point.X,
                            current.sumY + point.Y,
                            current.count + 1
                        );
                    }
                }
            }

            // 计算图层边界
            BoundingBox bounds = null;
            foreach (var feature in originalLayer.Features)
            {
                if (feature.Geometry is GeometryPoint point)
                {
                    if (bounds == null)
                    {
                        bounds = new BoundingBox(point.X, point.Y, point.X, point.Y);
                    }
                    else
                    {
                        bounds = new BoundingBox(
                            Math.Min(bounds.MinX, point.X),
                            Math.Min(bounds.MinY, point.Y),
                            Math.Max(bounds.MaxX, point.X),
                            Math.Max(bounds.MaxY, point.Y)
                        );
                    }
                }
            }

            // 创建图例图层
            var legendLayer = new Feature
            {
                Name = $"{originalLayer.Name}_Legend",
                Properties = new Dictionary<string, object>
                {
                    { "LayerType", "Legend" }
                },
                Features = new List<Feature>()
            };

            // 定义颜色和对应的名称
            var colorInfo = new[]
            {
                (Color: Color.Red, Name: "红色"),
                (Color: Color.Green, Name: "绿色"),
                (Color: Color.Blue, Name: "蓝色")
            };

            // 添加图例标签
            if (bounds != null)
            {
                // 将图例放在右上角
                double margin = (bounds.MaxX - bounds.MinX) * 0.05;  // 5% 边距
                double legendX = bounds.MaxX - margin;  // 靠右
                double legendY = bounds.MaxY - margin;  // 靠上

                for (int i = 0; i < clusterCounts.Length; i++)
                {
                    var legendFeature = new Feature
                    {
                        Geometry = new GeometryPoint 
                        { 
                            X = legendX,
                            Y = legendY - (margin * i)  // 向上排列
                        },
                        Properties = new Dictionary<string, object>
                        {
                            { "Text", $"● 聚类 {i} ({clusterCounts[i]}个要素)" },
                            { "ClusterId", i },
                            { "Color", colorInfo[i].Color }
                        }
                    };
                    legendLayer.Features.Add(legendFeature);
                }
            }

            // 修改标签位置计算
            foreach (var kvp in clusterCenters)
            {
                int clusterId = kvp.Key;
                var center = kvp.Value;
                var centerX = center.sumX / center.count;
                var centerY = center.sumY / center.count;

                // 计算标签偏移，避免压盖数据点
                var labelFeature = new Feature
                {
                    Geometry = new GeometryPoint 
                    { 
                        X = centerX + 15,
                        Y = centerY + 15
                    },
                    Properties = new Dictionary<string, object>
                    {
                        { "Text", $"● 聚类 {clusterId} ({clusterCounts[clusterId]}个要素)" },  // 添加圆点和数量
                        { "ClusterId", clusterId },
                        { "Color", colorInfo[clusterId].Color }  // 添加颜色信息
                    }
                };
                labelLayer.Features.Add(labelFeature);
            }

            // 将标签图层和图例图层作为子图层添加到结果图层
            resultLayer.SubLayers = new List<Feature> { labelLayer, legendLayer };
            Console.WriteLine($"标签图层中的标签数量: {labelLayer.Features.Count}");
            Console.WriteLine("=== 聚类结果图层创建完成 ===\n");

            return resultLayer;
        }

        private string ExportToCsv(Feature layer, string[] attributes, int[] clusters, string outputPath = null)
        {
            string csvPath;
            if (string.IsNullOrEmpty(outputPath))
            {
                // 默认路径：桌面
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                csvPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"聚类分析结果_{layer.Name}_{timestamp}.csv"
                );
            }
            else
            {
                csvPath = outputPath;
            }

            Console.WriteLine("\n=== 开始导出CSV ===");
            try
            {
                // 确保目标目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(csvPath));

                using (var writer = new StreamWriter(csvPath, false, System.Text.Encoding.UTF8))
                {
                    // 写入表头
                    var headers = new List<string> { "要素ID" };
                    headers.AddRange(attributes);
                    headers.Add("聚类编号");
                    writer.WriteLine(string.Join(",", headers));

                    // 写入数据
                    for (int i = 0; i < layer.Features.Count; i++)
                    {
                        var feature = layer.Features[i];
                        var values = new List<string> { i.ToString() };
                        
                        foreach (var attr in attributes)
                        {
                            if (feature.Properties.TryGetValue(attr, out object value))
                            {
                                values.Add(value.ToString());
                            }
                            else
                            {
                                values.Add("");
                            }
                        }
                        
                        values.Add(clusters[i].ToString());
                        writer.WriteLine(string.Join(",", values));
                    }

                    // 写入统计信息
                    writer.WriteLine();
                    writer.WriteLine("聚类统计");
                    var clusterCounts = new int[clusters.Max() + 1];
                    foreach (var cluster in clusters)
                    {
                        clusterCounts[cluster]++;
                    }
                    
                    for (int i = 0; i < clusterCounts.Length; i++)
                    {
                        writer.WriteLine($"聚类 {i},{clusterCounts[i]} 个要素");
                    }
                }

                Console.WriteLine($"CSV文件已保存到: {csvPath}");
                return csvPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"导出CSV失败: {ex.Message}");
                throw;
            }
        }
    }
} 