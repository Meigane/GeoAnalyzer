using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using GeoAnalyzer.Core.Models.Geometry;

namespace GeoAnalyzer.Core.SpatialAnalysis
{
    public class GeographicallyWeightedRegressionService
    {
        public class GWRParameters
        {
            public double Bandwidth { get; set; } = 1.0;  // 带宽参数
            public KernelType KernelFunction { get; set; } = KernelType.Gaussian;  // 核函数类型
            public string OutputPath { get; set; }  // 结果输出路径
        }

        public class GWRResults
        {
            public double GlobalR2 { get; set; }  // 全局R方
            public double[] LocalR2 { get; set; }  // 局部R方
            public double[][] Coefficients { get; set; }  // 回归系数
            public double[] Residuals { get; set; }  // 残差
            public string CsvPath { get; set; }  // 结果文件路径
        }

        public enum KernelType
        {
            Gaussian,
            Bisquare,
            Tricube
        }

        public GWRResults PerformAnalysis(
            Feature layer,
            string dependentVar,
            string[] independentVars,
            GWRParameters parameters)
        {
            Console.WriteLine("\n=== 开始地理加权回归分析 ===");
            Console.WriteLine($"因变量: {dependentVar}");
            Console.WriteLine($"自变量: {string.Join(", ", independentVars)}");
            Console.WriteLine($"带宽: {parameters.Bandwidth}");
            Console.WriteLine($"核函数: {parameters.KernelFunction}");

            try
            {
                // 1. 提取数据
                var y = ExtractValues(layer, dependentVar);
                var X = ExtractMatrix(layer, independentVars);
                var coordinates = ExtractCoordinates(layer);

                // 2. 计算权重矩阵
                var weights = CalculateWeights(coordinates, parameters.Bandwidth, parameters.KernelFunction);

                // 3. 执行GWR计算
                var results = CalculateGWR(y, X, weights);

                // 4. 导出结果
                string csvPath = ExportResults(
                    parameters.OutputPath,
                    layer,
                    dependentVar,
                    independentVars,
                    results
                );

                return new GWRResults
                {
                    GlobalR2 = results.globalR2,
                    LocalR2 = results.localR2,
                    Coefficients = results.coefficients,
                    Residuals = results.residuals,
                    CsvPath = csvPath
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"地理加权回归分析失败: {ex.Message}");
                throw;
            }
        }

        private double[] ExtractValues(Feature layer, string variable)
        {
            return layer.Features
                .Select(f => Convert.ToDouble(f.Properties[variable]))
                .ToArray();
        }

        private double[][] ExtractMatrix(Feature layer, string[] variables)
        {
            return layer.Features
                .Select(f => variables
                    .Select(v => Convert.ToDouble(f.Properties[v]))
                    .ToArray())
                .ToArray();
        }

        private (double[] x, double[] y) ExtractCoordinates(Feature layer)
        {
            var coordinates = layer.Features
                .Select(f => f.Geometry as Point)
                .Where(p => p != null)
                .Select(p => (p.X, p.Y))
                .ToList();

            return (coordinates.Select(c => c.X).ToArray(),
                   coordinates.Select(c => c.Y).ToArray());
        }

        // 这里实现一个基础版本，后续可以扩展更多功能
        private double[][] CalculateWeights(
            (double[] x, double[] y) coordinates,
            double bandwidth,
            KernelType kernelType)
        {
            int n = coordinates.x.Length;
            var weights = new double[n][];
            
            for (int i = 0; i < n; i++)
            {
                weights[i] = new double[n];
                for (int j = 0; j < n; j++)
                {
                    double distance = Math.Sqrt(
                        Math.Pow(coordinates.x[i] - coordinates.x[j], 2) +
                        Math.Pow(coordinates.y[i] - coordinates.y[j], 2));
                    
                    weights[i][j] = CalculateKernelWeight(distance, bandwidth, kernelType);
                }
            }

            return weights;
        }

        private double CalculateKernelWeight(double distance, double bandwidth, KernelType kernelType)
        {
            switch (kernelType)
            {
                case KernelType.Gaussian:
                    return Math.Exp(-0.5 * Math.Pow(distance / bandwidth, 2));
                case KernelType.Bisquare:
                    return distance <= bandwidth ? 
                        Math.Pow(1 - Math.Pow(distance / bandwidth, 2), 2) : 0;
                case KernelType.Tricube:
                    return distance <= bandwidth ? 
                        Math.Pow(1 - Math.Pow(distance / bandwidth, 3), 3) : 0;
                default:
                    throw new ArgumentException("不支持的核函数类型");
            }
        }

        // 继续实现其他必要的方法...
        // TODO: 实现GWR计算核心算法
        private (double globalR2, double[] localR2, double[][] coefficients, double[] residuals) 
            CalculateGWR(double[] y, double[][] X, double[][] weights)
        {
            int n = y.Length;  // 样本数量
            int p = X[0].Length + 1;  // 变量数量（包括常数项）
            
            // 添加常数项到X矩阵
            var X_with_intercept = new double[n][];
            for (int i = 0; i < n; i++)
            {
                X_with_intercept[i] = new double[p];
                X_with_intercept[i][0] = 1.0;  // 常数项
                for (int j = 0; j < X[i].Length; j++)
                {
                    X_with_intercept[i][j + 1] = X[i][j];
                }
            }

            // 使用添加了常数项的X矩阵
            var coefficients = new double[n][];  // 每个位置的回归系数
            for (int i = 0; i < n; i++)
            {
                coefficients[i] = new double[p];  // p包括常数项
            }

            // 初始化结果数组
            var localR2 = new double[n];         // 每个位置的R²
            var residuals = new double[n];       // 残差
            var yHat = new double[n];            // 预测值

            // 计算y的均值（用于计算R²）
            double yMean = y.Average();
            double totalSS = y.Select(yi => Math.Pow(yi - yMean, 2)).Sum();

            // 对每个位置进行局部回归
            for (int i = 0; i < n; i++)
            {
                // 1. 准备加权数据
                var W = new double[n, n];  // 对角权重矩阵
                for (int j = 0; j < n; j++)
                {
                    W[j, j] = weights[i][j];
                }

                // 2. 计算 (X'WX)^(-1)X'Wy
                // 首先计算 X'W
                var XtW = new double[p, n];
                for (int k = 0; k < p; k++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        XtW[k, j] = X_with_intercept[j][k] * W[j, j];
                    }
                }

                // 然后计算 X'WX
                var XtWX = new double[p, p];
                for (int k = 0; k < p; k++)
                {
                    for (int j = 0; j < p; j++)
                    {
                        double sum = 0;
                        for (int m = 0; m < n; m++)
                        {
                            sum += XtW[k, m] * X_with_intercept[m][j];
                        }
                        XtWX[k, j] = sum;
                    }
                }

                // 计算 (X'WX)的逆矩阵
                var XtWXInv = MatrixInverse(XtWX);

                // 计算 X'Wy
                var XtWy = new double[p];
                for (int k = 0; k < p; k++)
                {
                    double sum = 0;
                    for (int j = 0; j < n; j++)
                    {
                        sum += XtW[k, j] * y[j];
                    }
                    XtWy[k] = sum;
                }

                // 最后计算回归系数 β = (X'WX)^(-1)X'Wy
                for (int k = 0; k < p; k++)
                {
                    double sum = 0;
                    for (int j = 0; j < p; j++)
                    {
                        sum += XtWXInv[k, j] * XtWy[j];
                    }
                    coefficients[i][k] = sum;
                }

                // 3. 计算拟合值和残差
                double yHatI = 0;
                for (int k = 0; k < p; k++)
                {
                    yHatI += coefficients[i][k] * X_with_intercept[i][k];
                }
                yHat[i] = yHatI;
                residuals[i] = y[i] - yHatI;

                // 4. 计算局部R²
                double weightedTSS = 0;
                double weightedRSS = 0;
                double weightedYMean = 0;
                double weightSum = 0;

                for (int j = 0; j < n; j++)
                {
                    weightSum += weights[i][j];
                    weightedYMean += weights[i][j] * y[j];
                }
                weightedYMean /= weightSum;

                for (int j = 0; j < n; j++)
                {
                    weightedTSS += weights[i][j] * Math.Pow(y[j] - weightedYMean, 2);
                    weightedRSS += weights[i][j] * Math.Pow(y[j] - yHat[j], 2);
                }

                localR2[i] = 1 - (weightedRSS / weightedTSS);
            }

            // 计算全局R²
            double globalRSS = residuals.Select(r => r * r).Sum();
            double globalR2 = 1 - (globalRSS / totalSS);

            return (globalR2, localR2, coefficients, residuals);
        }

        // 矩阵求逆的辅助方法
        private double[,] MatrixInverse(double[,] matrix)
        {
            int n = (int)Math.Sqrt(matrix.Length);
            double[,] result = new double[n, n];
            double[,] augmented = new double[n, 2 * n];

            // 创建增广矩阵
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    augmented[i, j] = matrix[i, j];
                    augmented[i, j + n] = (i == j) ? 1 : 0;
                }
            }

            // 高斯-约当消元法
            for (int i = 0; i < n; i++)
            {
                // 主元归一化
                double pivot = augmented[i, i];
                for (int j = 0; j < 2 * n; j++)
                {
                    augmented[i, j] /= pivot;
                }

                // 消元
                for (int k = 0; k < n; k++)
                {
                    if (k != i)
                    {
                        double factor = augmented[k, i];
                        for (int j = 0; j < 2 * n; j++)
                        {
                            augmented[k, j] -= factor * augmented[i, j];
                        }
                    }
                }
            }

            // 提取逆矩阵
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    result[i, j] = augmented[i, j + n];
                }
            }

            return result;
        }

        private string ExportResults(
            string outputPath,
            Feature layer,
            string dependentVar,
            string[] independentVars,
            (double globalR2, double[] localR2, double[][] coefficients, double[] residuals) results)
        {
            try
            {
                Console.WriteLine("开始导出GWR分析结果...");
                // 使用 UTF8 编码并添加 BOM
                using (var writer = new StreamWriter(outputPath, false, new System.Text.UTF8Encoding(true)))
                {
                    // 写入全局统计结果
                    writer.WriteLine("地理加权回归分析结果");
                    writer.WriteLine($"全局R²,{results.globalR2:F6}");
                    writer.WriteLine();

                    // 写入变量信息
                    writer.WriteLine($"因变量,{dependentVar}");
                    writer.WriteLine($"自变量,{string.Join(",", independentVars)}");
                    writer.WriteLine();

                    // 写入表头
                    var headers = new List<string> { "要素ID", "局部R²" };
                    headers.Add("常数项");  // 添加常数项列
                    headers.AddRange(independentVars.Select(v => $"系数_{v}"));
                    headers.Add("残差");
                    writer.WriteLine(string.Join(",", headers));

                    // 写入局部统计结果
                    for (int i = 0; i < results.localR2.Length; i++)
                    {
                        var row = new List<string>
                        {
                            i.ToString(),
                            results.localR2[i].ToString("F6")
                        };

                        // 添加所有系数（包括常数项）
                        foreach (var coef in results.coefficients[i])
                        {
                            row.Add(coef.ToString("F6"));
                        }

                        row.Add(results.residuals[i].ToString("F6"));
                        writer.WriteLine(string.Join(",", row));
                    }

                    Console.WriteLine($"结果已导出到: {outputPath}");
                }

                return outputPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"导出结果时出错: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                throw;
            }
        }

        public double CalculateOptimalBandwidth(Feature layer)
        {
            try
            {
                Console.WriteLine("开始计算最优带宽...");
                var coordinates = ExtractCoordinates(layer);
                
                // 计算所有点对之间的距离
                var distances = new List<double>();
                for (int i = 0; i < coordinates.x.Length; i++)
                {
                    for (int j = i + 1; j < coordinates.x.Length; j++)
                    {
                        double dist = Math.Sqrt(
                            Math.Pow(coordinates.x[i] - coordinates.x[j], 2) +
                            Math.Pow(coordinates.y[i] - coordinates.y[j], 2));
                        distances.Add(dist);
                    }
                }

                // 计算统计量
                double meanDist = distances.Average();
                double stdDist = Math.Sqrt(distances.Select(d => Math.Pow(d - meanDist, 2)).Average());
                double medianDist = distances.OrderBy(d => d).ElementAt(distances.Count / 2);

                Console.WriteLine($"距离统计:");
                Console.WriteLine($"- 最小距离: {distances.Min():F4}");
                Console.WriteLine($"- 最大距离: {distances.Max():F4}");
                Console.WriteLine($"- 平均距离: {meanDist:F4}");
                Console.WriteLine($"- 中位距离: {medianDist:F4}");
                Console.WriteLine($"- 标准差: {stdDist:F4}");

                // 使用中位距离作为默认带宽
                // 这是一个比较保守的选择，确保每个点都有足够的邻居
                double optimalBandwidth = medianDist;
                Console.WriteLine($"计算得到的最优带宽: {optimalBandwidth:F4}");

                return optimalBandwidth;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"计算最优带宽时出错: {ex.Message}");
                throw;
            }
        }
    }
} 