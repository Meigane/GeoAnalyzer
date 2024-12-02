using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using GeoAnalyzer.Core.Models.Geometry;
using GeoAnalyzer.Core.SpatialAnalysis;

namespace GeoAnalyzer.Core.SpatialAnalysis
{
    public class SpatialAutocorrelationService
    {
        public class AutocorrelationResults
        {
            public double GlobalMoranI { get; set; }
            public double ZScore { get; set; }
            public double PValue { get; set; }
            public double[] LocalMoranI { get; set; }
            public string CsvPath { get; set; }
        }

        public class AnalysisParameters
        {
            public SpatialWeightMethod WeightMethod { get; set; } = SpatialWeightMethod.KNN;
            public int KNeighbors { get; set; } = 8;                  // KNN方法的K值
            public double DistanceThreshold { get; set; } = -1;       // 距离阈值法的阈值，-1表示自动计算
            public string OutputPath { get; set; }                    // 输出文件路径
        }

        public AutocorrelationResults PerformAnalysis(
            Feature layer, 
            string attribute,
            AnalysisParameters parameters)
        {
            Console.WriteLine($"\n=== 开始空间自相关分析 ===");
            Console.WriteLine($"分析图层: {layer.Name}");
            Console.WriteLine($"分析属性: {attribute}");
            Console.WriteLine($"空间权重方法: {parameters.WeightMethod}");

            try
            {
                // 1. 提取属性值
                var values = ExtractValues(layer, attribute);
                Console.WriteLine($"提取了 {values.Length} 个属性值");

                // 2. 计算空间权重矩阵
                double[,] weights;
                switch (parameters.WeightMethod)
                {
                    case SpatialWeightMethod.KNN:
                        weights = CalculateSpatialWeightsKNN(layer, parameters.KNeighbors);
                        break;
                    case SpatialWeightMethod.Distance:
                        weights = CalculateSpatialWeightsDistance(layer, parameters.DistanceThreshold);
                        break;
                    default:
                        throw new ArgumentException("不支持的空间权重计算方法");
                }
                Console.WriteLine("空间权重矩阵计算完成");

                // 3. 计算全局Moran's I
                var globalI = CalculateGlobalMoranI(values, weights);
                Console.WriteLine($"全局Moran's I: {globalI:F4}");

                // 4. 计算统计显著性
                var (zScore, pValue) = CalculateSignificance(values, weights, globalI);
                Console.WriteLine($"Z分数: {zScore:F4}, P值: {pValue:F4}");

                // 5. 计算局部Moran's I
                var localI = CalculateLocalMoranI(values, weights);
                Console.WriteLine("局部Moran's I计算完成");

                // 6. 导出结果到CSV
                string csvPath = ExportResults(
                    parameters.OutputPath ?? GetDefaultOutputPath(layer.Name),
                    layer, 
                    attribute, 
                    values, 
                    globalI, 
                    zScore, 
                    pValue, 
                    localI
                );
                Console.WriteLine($"结果已导出到: {csvPath}");

                return new AutocorrelationResults
                {
                    GlobalMoranI = globalI,
                    ZScore = zScore,
                    PValue = pValue,
                    LocalMoranI = localI,
                    CsvPath = csvPath
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"空间自相关分析失败: {ex.Message}");
                throw;
            }
        }

        private string GetDefaultOutputPath(string layerName)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"空间自相关分析结果_{layerName}_{timestamp}.csv"
            );
        }

        private double[] ExtractValues(Feature layer, string attribute)
        {
            return layer.Features
                .Select(f => Convert.ToDouble(f.Properties[attribute]))
                .ToArray();
        }

        private double[,] CalculateSpatialWeightsKNN(Feature layer, int k = 8)
        {
            int n = layer.Features.Count;
            var weights = new double[n, n];

            Console.WriteLine($"开始计算KNN空间权重矩阵 (n={n}, k={k})");

            for (int i = 0; i < n; i++)
            {
                var point1 = layer.Features[i].Geometry as Point;
                if (point1 == null) continue;

                // 计算到其他所有点的距离
                var distances = new List<(int index, double distance)>();
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    var point2 = layer.Features[j].Geometry as Point;
                    if (point2 == null) continue;

                    double dist = CalculateDistance(point1, point2);
                    distances.Add((j, dist));
                }

                // 获取k个最近邻
                var kNearest = distances.OrderBy(x => x.distance).Take(k);
                foreach (var (j, dist) in kNearest)
                {
                    weights[i, j] = 1.0 / dist;
                }
            }

            // 行标准化
            for (int i = 0; i < n; i++)
            {
                double rowSum = 0;
                for (int j = 0; j < n; j++)
                {
                    rowSum += weights[i, j];
                }
                if (rowSum > 0)
                {
                    for (int j = 0; j < n; j++)
                    {
                        weights[i, j] /= rowSum;
                    }
                }
            }

            return weights;
        }

        private double CalculateDistance(Point p1, Point p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private double CalculateGlobalMoranI(double[] values, double[,] weights)
        {
            int n = values.Length;
            double mean = values.Average();
            double sum = 0;
            double denominator = 0;

            Console.WriteLine($"计算全局Moran's I (n={n})");
            Console.WriteLine($"属性均值: {mean:F4}");

            // 计算S0（权重总和）
            double S0 = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    S0 += weights[i, j];
                }
            }
            Console.WriteLine($"权重总和(S0): {S0:F4}");

            for (int i = 0; i < n; i++)
            {
                double zi = values[i] - mean;
                for (int j = 0; j < n; j++)
                {
                    double zj = values[j] - mean;
                    sum += weights[i, j] * zi * zj;
                }
                denominator += zi * zi;
            }

            Console.WriteLine($"分子: {sum:F4}");
            Console.WriteLine($"分母: {denominator:F4}");

            double I = n * sum / (denominator * S0);
            Console.WriteLine($"计算得到的Moran's I: {I:F4}");

            return I;
        }

        private (double zScore, double pValue) CalculateSignificance(double[] values, double[,] weights, double moranI)
        {
            int permutations = 999;
            var random = new Random();
            int n = values.Length;
            double[] randomIs = new double[permutations];

            Console.WriteLine($"开始蒙特卡洛模拟 ({permutations} 次置换)");

            // 记录一些统计信息
            double minI = double.MaxValue;
            double maxI = double.MinValue;

            for (int p = 0; p < permutations; p++)
            {
                var shuffledValues = values.OrderBy(x => random.Next()).ToArray();
                randomIs[p] = CalculateGlobalMoranI(shuffledValues, weights);
                
                minI = Math.Min(minI, randomIs[p]);
                maxI = Math.Max(maxI, randomIs[p]);

                if ((p + 1) % 100 == 0)
                {
                    Console.WriteLine($"完成 {p + 1}/{permutations} 次模拟");
                }
            }

            double mean = randomIs.Average();
            double std = Math.Sqrt(randomIs.Select(x => Math.Pow(x - mean, 2)).Average());
            double zScore = (moranI - mean) / std;
            double pValue = randomIs.Count(x => Math.Abs(x) >= Math.Abs(moranI)) / (double)permutations;

            Console.WriteLine($"随机模拟结果统计:");
            Console.WriteLine($"最小值: {minI:F4}");
            Console.WriteLine($"最大值: {maxI:F4}");
            Console.WriteLine($"平均值: {mean:F4}");
            Console.WriteLine($"标准差: {std:F4}");

            return (zScore, pValue);
        }

        private double[] CalculateLocalMoranI(double[] values, double[,] weights)
        {
            int n = values.Length;
            double mean = values.Average();
            var localI = new double[n];

            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                for (int j = 0; j < n; j++)
                {
                    sum += weights[i, j] * (values[j] - mean);
                }
                localI[i] = (values[i] - mean) * sum;
            }

            return localI;
        }

        private string ExportResults(
            string outputPath,
            Feature layer, 
            string attribute, 
            double[] values, 
            double globalI, 
            double zScore, 
            double pValue, 
            double[] localI)
        {
            using (var writer = new StreamWriter(outputPath, false, System.Text.Encoding.UTF8))
            {
                // 写入全局统计结果
                writer.WriteLine("全局空间自相关分析结果");
                writer.WriteLine($"全局Moran's I,{globalI:F6}");
                writer.WriteLine($"Z分数,{zScore:F6}");
                writer.WriteLine($"P值,{pValue:F6}");
                writer.WriteLine();

                // 写入局部统计结果
                writer.WriteLine("局部空间自相关分析结果");
                writer.WriteLine($"要素ID,{attribute},局部Moran's I");
                
                for (int i = 0; i < values.Length; i++)
                {
                    writer.WriteLine($"{i},{values[i]:F6},{localI[i]:F6}");
                }
            }

            return outputPath;
        }

        private double[,] CalculateSpatialWeightsDistance(Feature layer, double threshold = -1)
        {
            int n = layer.Features.Count;
            var weights = new double[n, n];

            // 如果阈值为-1，自动计算合适的阈值
            if (threshold <= 0)
            {
                threshold = CalculateAutoThreshold(layer);
            }

            Console.WriteLine($"开始计算基于距离的空间权重矩阵 (n={n}, threshold={threshold:F2})");

            for (int i = 0; i < n; i++)
            {
                var point1 = layer.Features[i].Geometry as Point;
                if (point1 == null) continue;

                int neighborCount = 0;
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    var point2 = layer.Features[j].Geometry as Point;
                    if (point2 == null) continue;

                    double dist = CalculateDistance(point1, point2);
                    if (dist <= threshold)
                    {
                        weights[i, j] = 1.0 / dist;
                        neighborCount++;
                    }
                }

                if (i % 50 == 0 || i == n - 1)
                {
                    Console.WriteLine($"处理第 {i+1}/{n} 个要素，找到 {neighborCount} 个邻居");
                }
            }

            // 行标准化
            for (int i = 0; i < n; i++)
            {
                double rowSum = 0;
                for (int j = 0; j < n; j++)
                {
                    rowSum += weights[i, j];
                }
                if (rowSum > 0)
                {
                    for (int j = 0; j < n; j++)
                    {
                        weights[i, j] /= rowSum;
                    }
                }
            }

            return weights;
        }

        private double CalculateAutoThreshold(Feature layer)
        {
            // 计算所有点对之间距离的平均值作为阈值
            int n = layer.Features.Count;
            double sumDist = 0;
            int count = 0;

            for (int i = 0; i < n; i++)
            {
                var point1 = layer.Features[i].Geometry as Point;
                if (point1 == null) continue;

                for (int j = i + 1; j < n; j++)
                {
                    var point2 = layer.Features[j].Geometry as Point;
                    if (point2 == null) continue;

                    sumDist += CalculateDistance(point1, point2);
                    count++;
                }
            }

            return sumDist / count;
        }
    }
} 