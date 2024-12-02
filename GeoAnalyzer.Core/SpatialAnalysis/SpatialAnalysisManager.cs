using System;
using System.Collections.Generic;
using System.Linq;
using GeoAnalyzer.Core.Models.Geometry;
using GeoAnalyzer.Core.Utils;

namespace GeoAnalyzer.Core.SpatialAnalysis
{
    public class SpatialAnalysisManager
    {
        // 空间自相关分析（Moran's I）
        public double PerformSpatialAutocorrelation(Feature layer, string attributeName)
        {
            if (layer == null || !layer.HasAttribute(attributeName))
                throw new ArgumentException("无效的图层或属性");

            var features = new List<Feature> { layer };
            var values = features.Select(f => Convert.ToDouble(f.GetAttribute(attributeName))).ToList();
            var n = values.Count;
            
            // 计算权重矩阵
            var weights = CalculateSpatialWeights(features);
            
            // 计算Moran's I
            double mean = values.Average();
            double sum_w = weights.Sum(row => row.Sum());
            
            double numerator = 0;
            double denominator = 0;
            
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    numerator += weights[i][j] * (values[i] - mean) * (values[j] - mean);
                }
                denominator += Math.Pow(values[i] - mean, 2);
            }
            
            return (n / sum_w) * (numerator / denominator);
        }

        // 空间聚类分析（K-means）
        public List<int> PerformSpatialClustering(Feature layer, int k, string[] attributes)
        {
            if (layer == null)
                throw new ArgumentException("无效的图层");

            var features = new List<Feature> { layer };
            var points = features.Select(f => 
            {
                var values = attributes.Select(attr => Convert.ToDouble(f.GetAttribute(attr))).ToList();
                return new double[] { f.Geometry.GetBoundingBox().MinX, f.Geometry.GetBoundingBox().MinY }
                    .Concat(values).ToArray();
            }).ToList();

            return KMeansClustering(points, k);
        }

        // 空间回归分析（OLS）
        public (double[] coefficients, double r2) PerformSpatialRegression(
            Feature layer, 
            string dependentVar, 
            string[] independentVars)
        {
            var features = new List<Feature> { layer };
            var y = features.Select(f => Convert.ToDouble(f.GetAttribute(dependentVar))).ToArray();
            var X = features.Select(f => 
                new[] { 1.0 }.Concat(independentVars.Select(var => 
                    Convert.ToDouble(f.GetAttribute(var)))).ToArray()
            ).ToArray();

            return OrdinaryLeastSquares(X, y);
        }

        // 空间插值（IDW）
        public double PerformSpatialInterpolation(
            Feature layer, 
            string attributeName, 
            Point location, 
            int power = 2)
        {
            var features = new List<Feature> { layer };
            var points = features.Select(f => new
            {
                Point = f.Geometry as Point,
                Value = Convert.ToDouble(f.GetAttribute(attributeName))
            }).ToList();

            double numerator = 0;
            double denominator = 0;

            foreach (var point in points)
            {
                double distance = GeometryUtils.CalculateDistance(location, point.Point);
                if (distance < double.Epsilon) return point.Value;

                double weight = 1.0 / Math.Pow(distance, power);
                numerator += weight * point.Value;
                denominator += weight;
            }

            return numerator / denominator;
        }

        // 地理加权回归（GWR）
        public Dictionary<Point, double[]> PerformGeographicallyWeightedRegression(
            Feature layer,
            string dependentVar,
            string[] independentVars,
            double bandwidth)
        {
            var features = new List<Feature> { layer };
            var results = new Dictionary<Point, double[]>();

            foreach (var feature in features)
            {
                var location = feature.Geometry as Point;
                var weights = CalculateGWRWeights(features, location, bandwidth);
                
                var y = features.Select(f => Convert.ToDouble(f.GetAttribute(dependentVar))).ToArray();
                var X = features.Select(f => 
                    new[] { 1.0 }.Concat(independentVars.Select(var => 
                        Convert.ToDouble(f.GetAttribute(var)))).ToArray()
                ).ToArray();

                var coefficients = WeightedLeastSquares(X, y, weights);
                results[location] = coefficients;
            }

            return results;
        }

        #region Helper Methods

        private double[][] CalculateSpatialWeights(List<Feature> features)
        {
            int n = features.Count;
            var weights = new double[n][];
            
            for (int i = 0; i < n; i++)
            {
                weights[i] = new double[n];
                for (int j = 0; j < n; j++)
                {
                    if (i != j && features[i].Geometry.Intersects(features[j].Geometry))
                    {
                        weights[i][j] = 1;
                    }
                }
            }
            
            return weights;
        }

        private List<int> KMeansClustering(List<double[]> points, int k)
        {
            int n = points.Count;
            int dim = points[0].Length;
            var clusters = new List<int>(new int[n]);
            var centroids = points.OrderBy(x => Guid.NewGuid()).Take(k).ToList();

            bool changed;
            do
            {
                changed = false;
                // 分配点到最近的中心
                for (int i = 0; i < n; i++)
                {
                    int nearestCluster = 0;
                    double minDistance = double.MaxValue;
                    
                    for (int j = 0; j < k; j++)
                    {
                        double distance = EuclideanDistance(points[i], centroids[j]);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nearestCluster = j;
                        }
                    }
                    
                    if (clusters[i] != nearestCluster)
                    {
                        clusters[i] = nearestCluster;
                        changed = true;
                    }
                }

                // 更新中心点
                for (int i = 0; i < k; i++)
                {
                    var clusterPoints = points.Where((p, idx) => clusters[idx] == i).ToList();
                    if (clusterPoints.Any())
                    {
                        centroids[i] = new double[dim];
                        for (int d = 0; d < dim; d++)
                        {
                            centroids[i][d] = clusterPoints.Average(p => p[d]);
                        }
                    }
                }
            } while (changed);

            return clusters;
        }

        private (double[] coefficients, double r2) OrdinaryLeastSquares(double[][] X, double[] y)
        {
            int n = X.Length;
            int p = X[0].Length;
            
            // 计算 (X'X)^(-1)X'y
            var Xt = Transpose(X);
            var XtX = MatrixMultiply(Xt, X);
            var XtXinv = MatrixInverse(XtX);
            var Xty = MatrixVectorMultiply(Xt, y);
            var coefficients = MatrixVectorMultiply(XtXinv, Xty);

            // 计算R²
            var yHat = MatrixVectorMultiply(X, coefficients);
            double yMean = y.Average();
            double tss = y.Sum(yi => Math.Pow(yi - yMean, 2));
            double rss = y.Zip(yHat, (yi, yHati) => Math.Pow(yi - yHati, 2)).Sum();
            double r2 = 1 - (rss / tss);

            return (coefficients, r2);
        }

        private double[] WeightedLeastSquares(double[][] X, double[] y, double[] weights)
        {
            int n = X.Length;
            int p = X[0].Length;
            
            // 应用权重
            var Xw = X.Select((row, i) => row.Select(x => x * Math.Sqrt(weights[i])).ToArray()).ToArray();
            var yw = y.Select((yi, i) => yi * Math.Sqrt(weights[i])).ToArray();
            
            // 计算 (X'WX)^(-1)X'Wy
            var Xt = Transpose(Xw);
            var XtX = MatrixMultiply(Xt, Xw);
            var XtXinv = MatrixInverse(XtX);
            var Xty = MatrixVectorMultiply(Xt, yw);
            
            return MatrixVectorMultiply(XtXinv, Xty);
        }

        private double[] CalculateGWRWeights(List<Feature> features, Point location, double bandwidth)
        {
            return features.Select(f => 
            {
                var distance = GeometryUtils.CalculateDistance(location, f.Geometry as Point);
                return Math.Exp(-0.5 * Math.Pow(distance / bandwidth, 2));
            }).ToArray();
        }

        private double EuclideanDistance(double[] a, double[] b)
        {
            return Math.Sqrt(a.Zip(b, (x, y) => Math.Pow(x - y, 2)).Sum());
        }

        // 矩阵运算辅助方法
        private double[][] Transpose(double[][] matrix)
        {
            int m = matrix.Length;
            int n = matrix[0].Length;
            var result = new double[n][];
            for (int i = 0; i < n; i++)
            {
                result[i] = new double[m];
                for (int j = 0; j < m; j++)
                {
                    result[i][j] = matrix[j][i];
                }
            }
            return result;
        }

        private double[][] MatrixMultiply(double[][] a, double[][] b)
        {
            int m = a.Length;
            int n = b[0].Length;
            int p = a[0].Length;
            var result = new double[m][];
            for (int i = 0; i < m; i++)
            {
                result[i] = new double[n];
                for (int j = 0; j < n; j++)
                {
                    for (int k = 0; k < p; k++)
                    {
                        result[i][j] += a[i][k] * b[k][j];
                    }
                }
            }
            return result;
        }

        private double[] MatrixVectorMultiply(double[][] a, double[] v)
        {
            int m = a.Length;
            var result = new double[m];
            for (int i = 0; i < m; i++)
            {
                result[i] = a[i].Zip(v, (x, y) => x * y).Sum();
            }
            return result;
        }

        private double[][] MatrixInverse(double[][] matrix)
        {
            // 使用LU分解或其他方法实现矩阵求逆
            // 这里为简化，假设矩阵是2x2的
            int n = matrix.Length;
            var result = new double[n][];
            for (int i = 0; i < n; i++)
            {
                result[i] = new double[n];
            }

            double det = matrix[0][0] * matrix[1][1] - matrix[0][1] * matrix[1][0];
            result[0][0] = matrix[1][1] / det;
            result[0][1] = -matrix[0][1] / det;
            result[1][0] = -matrix[1][0] / det;
            result[1][1] = matrix[0][0] / det;

            return result;
        }

        #endregion
    }
}
