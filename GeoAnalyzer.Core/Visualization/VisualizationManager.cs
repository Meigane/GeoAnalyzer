using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using GeometryPoint = GeoAnalyzer.Core.Models.Geometry.Point;
using DrawingPoint = System.Drawing.Point;
using DrawingPointF = System.Drawing.PointF;
using GeoAnalyzer.Core.Models.Geometry;

namespace GeoAnalyzer.Core.Visualization
{
    public class VisualizationManager : IDisposable
    {
        private Panel contentPanel;
        private Graphics graphics;
        private float zoomLevel = 1.0f;
        private PointF panOffset = new PointF(0, 0);
        private BoundingBox currentBounds;
        private Bitmap bitmap;
        private bool disposed = false;

        public VisualizationManager(Panel panel)
        {
            contentPanel = panel;
            contentPanel.Paint += ContentPanel_Paint;
            contentPanel.Resize += ContentPanel_Resize;
            InitializeGraphics();
        }

        private void InitializeGraphics()
        {
            if (contentPanel.Width > 0 && contentPanel.Height > 0)
            {
                bitmap = new Bitmap(contentPanel.Width, contentPanel.Height);
                graphics = Graphics.FromImage(bitmap);
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            }
        }

        private void ContentPanel_Resize(object sender, EventArgs e)
        {
            InitializeGraphics();
            contentPanel.Invalidate();
        }

        private void ContentPanel_Paint(object sender, PaintEventArgs e)
        {
            if (bitmap != null)
            {
                e.Graphics.DrawImage(bitmap, 0, 0);
            }
        }

        public void CreateQuantileMap(Feature layer)
        {
            if (layer == null) return;

            // 获取数值属性
            var numericValues = GetNumericValues(layer);
            if (!numericValues.Any()) return;

            // 计算分位数
            var quantiles = CalculateQuantiles(numericValues, 5);
            
            // 设置颜色方案
            var colors = new Color[] 
            {
                Color.FromArgb(255, 255, 237, 160),
                Color.FromArgb(255, 254, 178, 76),
                Color.FromArgb(255, 253, 141, 60),
                Color.FromArgb(255, 252, 78, 42),
                Color.FromArgb(255, 227, 26, 28)
            };

            // 绘制地图
            DrawFeatureWithColors(layer, numericValues, quantiles, colors);
        }

        public void CreateNaturalBreaksMap(Feature layer)
        {
            if (layer == null) return;

            // 获取数值属性
            var numericValues = GetNumericValues(layer);
            if (!numericValues.Any()) return;

            // 计算自然断点
            var breaks = CalculateNaturalBreaks(numericValues, 5);
            
            // 设置颜色方案
            var colors = new Color[] 
            {
                Color.FromArgb(255, 237, 248, 251),
                Color.FromArgb(255, 179, 205, 227),
                Color.FromArgb(255, 140, 150, 198),
                Color.FromArgb(255, 136, 86, 167),
                Color.FromArgb(255, 129, 15, 124)
            };

            // 绘制地图
            DrawFeatureWithColors(layer, numericValues, breaks, colors);
        }

        public void CreateEqualIntervalsMap(Feature layer)
        {
            if (layer == null) return;

            // 获取数值属性
            var numericValues = GetNumericValues(layer);
            if (!numericValues.Any()) return;

            // 计算等间距
            var intervals = CalculateEqualIntervals(numericValues, 5);
            
            // 设置颜色方案
            var colors = new Color[] 
            {
                Color.FromArgb(255, 237, 248, 251),
                Color.FromArgb(255, 179, 205, 227),
                Color.FromArgb(255, 140, 150, 198),
                Color.FromArgb(255, 136, 86, 167),
                Color.FromArgb(255, 129, 15, 124)
            };

            // 绘制地图
            DrawFeatureWithColors(layer, numericValues, intervals, colors);
        }

        public void CreateStandardDeviationMap(Feature layer)
        {
            if (layer == null) return;

            // 获取数值属性
            var numericValues = GetNumericValues(layer);
            if (!numericValues.Any()) return;

            // 计算标准差分类
            var stdDevBreaks = CalculateStandardDeviationBreaks(numericValues);
            
            // 设置颜色方案
            var colors = new Color[] 
            {
                Color.FromArgb(255, 237, 248, 251),
                Color.FromArgb(255, 179, 205, 227),
                Color.FromArgb(255, 140, 150, 198),
                Color.FromArgb(255, 136, 86, 167),
                Color.FromArgb(255, 129, 15, 124)
            };

            // 绘制地图
            DrawFeatureWithColors(layer, numericValues, stdDevBreaks, colors);
        }

        private List<double> GetNumericValues(Feature layer)
        {
            // 从属性中获取数值
            var values = new List<double>();
            // TODO: 实现从Feature中提取数值属性的逻辑
            return values;
        }

        private double[] CalculateQuantiles(List<double> values, int numClasses)
        {
            var sortedValues = values.OrderBy(v => v).ToList();
            var breaks = new double[numClasses + 1];
            for (int i = 0; i <= numClasses; i++)
            {
                int index = (int)Math.Round(i * (sortedValues.Count - 1) / (double)numClasses);
                breaks[i] = sortedValues[index];
            }
            return breaks;
        }

        private double[] CalculateNaturalBreaks(List<double> values, int numClasses)
        {
            // 实现Jenks自然断点算法
            // TODO: 实现自然断点计算逻辑
            return new double[numClasses + 1];
        }

        private double[] CalculateEqualIntervals(List<double> values, int numClasses)
        {
            double min = values.Min();
            double max = values.Max();
            double interval = (max - min) / numClasses;

            var breaks = new double[numClasses + 1];
            for (int i = 0; i <= numClasses; i++)
            {
                breaks[i] = min + i * interval;
            }
            return breaks;
        }

        private double[] CalculateStandardDeviationBreaks(List<double> values)
        {
            double mean = values.Average();
            double stdDev = Math.Sqrt(values.Average(v => Math.Pow(v - mean, 2)));

            return new double[] 
            {
                double.MinValue,
                mean - 2 * stdDev,
                mean - stdDev,
                mean,
                mean + stdDev,
                mean + 2 * stdDev,
                double.MaxValue
            };
        }

        private void DrawFeatureWithColors(Feature layer, List<double> values, double[] breaks, Color[] colors)
        {
            if (graphics == null || layer == null) return;
            Console.WriteLine("\n=== 开始绘制图层 ===");
            Console.WriteLine($"图层名称: {layer.Name}");
            Console.WriteLine($"Features数量: {layer.Features?.Count ?? 0}");
            Console.WriteLine($"子图层数量: {layer.SubLayers?.Count ?? 0}");

            // 清除画布
            graphics.Clear(Color.White);

            // 根据聚类结果为每个要素设置颜色
            int drawnFeatures = 0;
            foreach (var feature in layer.Features)
            {
                if (feature.Properties.TryGetValue("Cluster", out object clusterObj) && 
                    clusterObj is int clusterId)
                {
                    Color color = colors[clusterId % colors.Length];
                    DrawFeatureWithColor(feature, color, graphics);
                    drawnFeatures++;
                }
            }
            Console.WriteLine($"已绘制要素数量: {drawnFeatures}");

            // 绘制标签
            if (layer.SubLayers != null)
            {
                Console.WriteLine("\n=== 处理子图层 ===");
                foreach (var subLayer in layer.SubLayers)
                {
                    Console.WriteLine($"子图层名称: {subLayer.Name}");
                    Console.WriteLine($"子图层类型: {(subLayer.Properties.ContainsKey("LayerType") ? subLayer.Properties["LayerType"].ToString() : "Unknown")}");
                    Console.WriteLine($"子图层可见性: {subLayer.IsVisible}");
                    Console.WriteLine($"子图层Features数量: {subLayer.Features?.Count ?? 0}");

                    if (subLayer.IsVisible && 
                        subLayer.Properties.ContainsKey("LayerType") && 
                        subLayer.Properties["LayerType"].ToString() == "Label")
                    {
                        Console.WriteLine("开始绘制标签...");
                        DrawLabels(subLayer, graphics);
                    }
                }
            }

            // 刷新显示
            contentPanel.Invalidate();
            Console.WriteLine("=== 绘制完成 ===\n");
        }

        private void DrawFeatureWithColor(Feature feature, Color color, Graphics g)
        {
            using (var brush = new SolidBrush(color))
            using (var pen = new Pen(Color.Black, 1))
            {
                if (feature.Geometry is GeometryPoint geometryPoint)
                {
                    var point = geometryPoint.ToDrawingPointF();
                    float size = 8;

                    g.FillEllipse(brush, point.X - size/2, point.Y - size/2, size, size);
                    g.DrawEllipse(pen, point.X - size/2, point.Y - size/2, size, size);
                }
            }
        }

        private void DrawLabels(Feature labelLayer, Graphics g)
        {
            if (labelLayer == null || !labelLayer.IsVisible)
            {
                Console.WriteLine("标签图层为空或不可见，跳过绘制");
                return;
            }

            if (!labelLayer.Properties.ContainsKey("LayerType") || 
                labelLayer.Properties["LayerType"].ToString() != "Label")
            {
                Console.WriteLine("不是标签图层，跳过绘制");
                return;
            }

            Console.WriteLine("\n=== 开始绘制标签 ===");
            Console.WriteLine($"标签图层名称: {labelLayer.Name}");
            Console.WriteLine($"标签数量: {labelLayer.Features?.Count ?? 0}");

            var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            using (var font = new Font("Arial", 10, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.Black))
            using (var shadowBrush = new SolidBrush(Color.White))
            {
                int drawnLabels = 0;
                foreach (var feature in labelLayer.Features)
                {
                    if (feature.Geometry is GeometryPoint geometryPoint)
                    {
                        Console.WriteLine($"\n处理标签要素:");
                        Console.WriteLine($"位置: ({geometryPoint.X}, {geometryPoint.Y})");
                        
                        if (feature.Properties.TryGetValue("Text", out object textObj))
                        {
                            string text = textObj.ToString();
                            Console.WriteLine($"标签文本: {text}");
                            
                            var point = geometryPoint.ToDrawingPointF();
                            SizeF textSize = g.MeasureString(text, font);
                            
                            RectangleF shadowRect = new RectangleF(
                                point.X - textSize.Width/2 - 2,
                                point.Y - textSize.Height/2 - 2,
                                textSize.Width + 4,
                                textSize.Height + 4
                            );
                            
                            g.FillRectangle(shadowBrush, shadowRect);
                            g.DrawString(text, font, brush, point.X, point.Y, format);
                            drawnLabels++;
                        }
                        else
                        {
                            Console.WriteLine("未找到Text属性");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"几何类型不是Point: {feature.Geometry?.GetType().Name ?? "null"}");
                    }
                }
                Console.WriteLine($"\n实际绘制的标签数量: {drawnLabels}");
            }
            Console.WriteLine("=== 标签绘制完成 ===\n");
        }

        // 其他可视化方法的实现...
        public void CreateBoxPlot(Feature layer)
        {
            // TODO: 实现箱线图绘制
        }

        public void CreateHistogram(Feature layer)
        {
            // TODO: 实现直方图绘制
        }

        public void CreateScatterPlotMatrix(Feature layer)
        {
            // TODO: 实现散点图矩阵绘制
        }

        public void CreateParallelCoordinatePlot(Feature layer)
        {
            // TODO: 实现平行坐标图绘制
        }

        public void Create3DVisualization(Feature layer)
        {
            // TODO: 实现3D可视化
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (graphics != null)
                    {
                        graphics.Dispose();
                        graphics = null;
                    }
                    if (bitmap != null)
                    {
                        bitmap.Dispose();
                        bitmap = null;
                    }
                }
                disposed = true;
            }
        }

        ~VisualizationManager()
        {
            Dispose(false);
        }
    }
}
