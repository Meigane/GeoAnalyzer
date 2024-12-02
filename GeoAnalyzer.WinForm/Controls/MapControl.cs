// Controls/MapControl.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;
using GeoAnalyzer.Core.Models.Geometry;
using GeoAnalyzer.Core.Models.Styles;
using GeoAnalyzer.WinForm.Services;
using GeoAnalyzer.WinForm.LayerEvents;
using DrawingPoint = System.Drawing.Point;
using GeometryPoint = GeoAnalyzer.Core.Models.Geometry.Point;

namespace GeoAnalyzer.WinForm.Controls
{
    public class MapControl : Panel
    {
        private LayerServices _layerServices;
        private List<Feature> layers = new List<Feature>();
        private BoundingBox _bounds;
        private float _zoomLevel = 1.0f;
        private PointF _panOffset = new PointF(0, 0);
        private Bitmap _backBuffer;
        private bool _isRefreshing = false;
        private Graphics _bufferGraphics;

        public BoundingBox Bounds
        {
            get { return _bounds; }
            private set { _bounds = value; }
        }

        public MapControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint, true);
            
            UpdateBackBuffer();
        }

        private void UpdateBackBuffer()
        {
            if (Width <= 0 || Height <= 0)
            {
                Console.WriteLine("警告: 控件大小无效，跳过缓冲区更新");
                return;
            }

            Console.WriteLine($"\n=== 更新缓冲区 ===");
            Console.WriteLine($"控件大小: {Width}x{Height}");
            
            try
            {
                if (_backBuffer != null)
                {
                    _bufferGraphics?.Dispose();
                    _backBuffer.Dispose();
                    Console.WriteLine("已释放旧缓冲区");
                }
                
                _backBuffer = new Bitmap(Width, Height);
                _bufferGraphics = Graphics.FromImage(_backBuffer);
                Console.WriteLine($"创建新缓冲区: {Width}x{Height}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新缓冲区失败: {ex.Message}");
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (Width > 0 && Height > 0)  // 只在有效大小时更新
            {
                Console.WriteLine($"\n=== MapControl大小改变 ===");
                Console.WriteLine($"新大小: {Width}x{Height}");
                UpdateBackBuffer();
                RefreshMap();
            }
        }

        public void RefreshMap()
        {
            if (_isRefreshing)
            {
                Console.WriteLine("跳过刷新：正在进行中的刷新未完成");
                return;
            }

            if (Width <= 0 || Height <= 0)
            {
                Console.WriteLine("警告: 控件大小无效，跳过刷新");
                return;
            }

            _isRefreshing = true;
            try
            {
                if (_bufferGraphics == null || _backBuffer == null)
                {
                    UpdateBackBuffer();
                }

                if (_bufferGraphics != null)
                {
                    _bufferGraphics.Clear(Color.White);
                    var layers = _layerServices?.GetAllLayers();
                    if (layers != null)
                    {
                        foreach (var layer in layers)
                        {
                            if (layer.IsVisible)
                            {
                                DrawLayer(layer, _bufferGraphics);
                            }
                        }
                    }
                    Invalidate();
                }
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                if (_backBuffer != null)
                {
                    Console.WriteLine("OnPaint: 绘制缓冲区");
                    e.Graphics.DrawImage(_backBuffer, 0, 0);
                }
                else
                {
                    Console.WriteLine("OnPaint: 缓冲区为空");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnPaint 异常: {ex.Message}");
            }
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (Bounds == null) return;

            // 记录鼠标位置对应的世界坐标
            PointF mouseWorldPos = ScreenToWorld(new PointF(e.Location.X, e.Location.Y));

            // 更新缩放级别
            if (e.Delta > 0)
                _zoomLevel *= 1.1f;
            else
                _zoomLevel *= 0.9f;

            // 限制缩放范围
            _zoomLevel = Math.Max(0.1f, Math.Min(_zoomLevel, 10.0f));

            // 更新平移偏移以保持鼠标位置不变
            PointF newScreenPos = WorldToScreen(mouseWorldPos);
            _panOffset.X += e.Location.X - newScreenPos.X;
            _panOffset.Y += e.Location.Y - newScreenPos.Y;

            RefreshMap();
        }

        private bool isDragging = false;
        private DrawingPoint lastLocation;

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            Console.WriteLine($"\n=== MapControl鼠标按下事件 ===");
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastLocation = e.Location;
                Console.WriteLine($"开始拖拽 - 位置: ({e.Location.X}, {e.Location.Y})");
            }
            Console.WriteLine("=== MapControl鼠标按下事件结束 ===\n");
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                int deltaX = e.X - lastLocation.X;
                int deltaY = e.Y - lastLocation.Y;
                
                _panOffset.X += deltaX;
                _panOffset.Y += deltaY;
                
                lastLocation = e.Location;
                RefreshMap();
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            Console.WriteLine($"\n=== MapControl鼠标释放事件 ===");
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
                Console.WriteLine("结束拖拽");
            }
            Console.WriteLine("=== MapControl鼠标释放事件结束 ===\n");
        }

        public void DrawLayer(Feature layer, Graphics g)
        {
            if (layer == null || g == null) return;

            try
            {
                Console.WriteLine($"\n绘制图层: {layer.Name}");
                Console.WriteLine($"图层类型: {(layer.Properties.ContainsKey("LayerType") ? layer.Properties["LayerType"] : "普通图层")}");
                Console.WriteLine($"要素数量: {layer.Features?.Count ?? 0}");

                // 检查是否是标签图层
                if (layer.Properties.ContainsKey("LayerType") && 
                    layer.Properties["LayerType"].ToString() == "Label")
                {
                    DrawLabels(layer, g);
                    return;
                }

                // 绘制普通要素
                int drawnCount = 0;
                foreach (var feature in layer.Features)
                {
                    if (feature?.Geometry is GeometryPoint point)
                    {
                        var worldPoint = new PointF((float)point.X, (float)point.Y);
                        var screenPoint = WorldToScreen(worldPoint);
                        DrawFeature(feature, screenPoint, g);
                        drawnCount++;
                    }
                }

                // 绘制子图层（如标签）
                if (layer.SubLayers != null)
                {
                    Console.WriteLine($"处理子图层，数量: {layer.SubLayers.Count}");
                    foreach (var subLayer in layer.SubLayers)
                    {
                        if (subLayer.IsVisible)
                        {
                            DrawLayer(subLayer, g);
                        }
                    }
                }

                Console.WriteLine($"完成绘制 {drawnCount} 个要素");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DrawLayer 异常: {ex.Message}");
            }
        }

        private void DrawFeature(Feature feature, PointF screenPoint, Graphics g)
        {
            try
            {
                float size = 6;
                Color color = GetFeatureColor(feature);

                using (var brush = new SolidBrush(color))
                using (var pen = new Pen(Color.Black, 1))
                {
                    g.FillEllipse(brush, screenPoint.X - size/2, screenPoint.Y - size/2, size, size);
                    g.DrawEllipse(pen, screenPoint.X - size/2, screenPoint.Y - size/2, size, size);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DrawFeature 异常: {ex.Message}");
            }
        }

        private Color GetFeatureColor(Feature feature)
        {
            if (feature.Properties.TryGetValue("Cluster", out object clusterObj) && 
                clusterObj is int clusterId)
            {
                var colors = new Color[] 
                {
                    Color.Red,
                    Color.Green,
                    Color.Blue,
                    Color.Yellow,
                    Color.Purple
                };
                return colors[clusterId % colors.Length];
            }
            return Color.Blue;
        }

        private PointF WorldToScreen(PointF worldPoint)
        {
            if (_bounds == null)
            {
                Console.WriteLine("警告: 边界框为空，尝试重新计算边界");
                RecalculateBounds();
                if (_bounds == null)
                {
                    Console.WriteLine("错误: 无法计算边界框");
                    return worldPoint;
                }
            }

            // 计算世界坐标范围
            double worldWidth = _bounds.MaxX - _bounds.MinX;
            double worldHeight = _bounds.MaxY - _bounds.MinY;

            if (worldWidth <= 0 || worldHeight <= 0)
            {
                Console.WriteLine($"警告: 无效的边界框尺寸: {worldWidth}x{worldHeight}");
                return worldPoint;
            }

            // 计算缩放比例
            float scaleX = Width / (float)worldWidth;
            float scaleY = Height / (float)worldHeight;
            float scale = Math.Min(scaleX, scaleY) * _zoomLevel;

            // 计算居中偏移
            float offsetX = (Width - (float)worldWidth * scale) / 2;
            float offsetY = (Height - (float)worldHeight * scale) / 2;

            // 转换坐标
            float screenX = ((worldPoint.X - (float)_bounds.MinX) * scale) + offsetX + _panOffset.X;
            float screenY = (Height - ((worldPoint.Y - (float)_bounds.MinY) * scale)) - offsetY + _panOffset.Y;

            Console.WriteLine($"坐标转换: 世界({worldPoint.X}, {worldPoint.Y}) -> 屏幕({screenX}, {screenY})");
            Console.WriteLine($"转换参数: 缩放={scale}, 偏移=({offsetX}, {offsetY}), 平移=({_panOffset.X}, {_panOffset.Y})");

            return new PointF(screenX, screenY);
        }

        private void AddLayer(Feature layer)
        {
            if (layer != null)
            {
                layers.Add(layer);
                
                if (layers.Count == 1)
                {
                    var extent = CalculateLayerExtent(layer);
                    if (extent != null)
                    {
                        double width = extent.MaxX - extent.MinX;
                        double height = extent.MaxY - extent.MinY;
                        double margin = Math.Max(width, height) * 0.1;
                        extent = new BoundingBox(
                            extent.MinX - margin,
                            extent.MinY - margin,
                            extent.MaxX + margin,
                            extent.MaxY + margin
                        );
                        SetBounds(extent);
                    }
                }
                
                RefreshMap();
            }
        }

        private BoundingBox CalculateLayerExtent(Feature layer)
        {
            if (layer == null || (layer.Features == null && layer.Geometry == null))
                return null;

            BoundingBox extent = null;

            if (layer.Features != null && layer.Features.Count > 0)
            {
                foreach (var feature in layer.Features)
                {
                    if (feature.Geometry != null)
                    {
                        var featureExtent = feature.Geometry.GetBoundingBox();
                        if (extent == null)
                            extent = featureExtent;
                        else
                            extent = new BoundingBox(
                                Math.Min(extent.MinX, featureExtent.MinX),
                                Math.Min(extent.MinY, featureExtent.MinY),
                                Math.Max(extent.MaxX, featureExtent.MaxX),
                                Math.Max(extent.MaxY, featureExtent.MaxY)
                            );
                    }
                }
            }
            else if (layer.Geometry != null)
            {
                extent = layer.Geometry.GetBoundingBox();
            }

            return extent;
        }

        public void SetBounds(BoundingBox bounds)
        {
            if (bounds != null)
            {
                double mapAspectRatio = (double)Width / Height;
                double width = bounds.MaxX - bounds.MinX;
                double height = bounds.MaxY - bounds.MinY;
                double boundsAspectRatio = width / height;

                if (mapAspectRatio > boundsAspectRatio)
                {
                    double centerX = bounds.MinX + width / 2;
                    double newWidth = height * mapAspectRatio;
                    bounds = new BoundingBox(
                        centerX - newWidth / 2,
                        bounds.MinY,
                        centerX + newWidth / 2,
                        bounds.MaxY
                    );
                }
                else
                {
                    double centerY = bounds.MinY + height / 2;
                    double newHeight = width / mapAspectRatio;
                    bounds = new BoundingBox(
                        bounds.MinX,
                        centerY - newHeight / 2,
                        bounds.MaxX,
                        centerY + newHeight / 2
                    );
                }

                this.Bounds = bounds;
                RefreshMap();
            }
        }

        public void ZoomToAllLayers()
        {
            Console.WriteLine("\n=== 缩放到所有图层 ===");
            var extent = CalculateFullExtent();
            if (extent != null)
            {
                Console.WriteLine($"计算得到的范围: ({extent.MinX}, {extent.MinY}) - ({extent.MaxX}, {extent.MaxY})");
                
                // 添加边距
                double width = extent.MaxX - extent.MinX;
                double height = extent.MaxY - extent.MinY;
                double margin = Math.Max(width, height) * 0.1; // 10% 的边距

                extent = new BoundingBox(
                    extent.MinX - margin,
                    extent.MinY - margin,
                    extent.MaxX + margin,
                    extent.MaxY + margin
                );

                SetBounds(extent);
                _zoomLevel = 1.0f;  // 重置缩放级别
                _panOffset = new PointF(0, 0);  // 重置平移偏移
                
                Console.WriteLine($"设置边界框(含边距): ({extent.MinX}, {extent.MinY}) - ({extent.MaxX}, {extent.MaxY})");
                Console.WriteLine("已重置视图参数");
                
                RefreshMap();
            }
            else
            {
                Console.WriteLine("警告: 无法计算图层范围");
            }
        }

        private BoundingBox CalculateFullExtent()
        {
            if (layers == null || layers.Count == 0) return null;

            BoundingBox fullExtent = null;
            foreach (var layer in layers)
            {
                var layerExtent = CalculateLayerExtent(layer);
                if (layerExtent != null)
                {
                    if (fullExtent == null)
                        fullExtent = layerExtent;
                    else
                        fullExtent = new BoundingBox(
                            Math.Min(fullExtent.MinX, layerExtent.MinX),
                            Math.Min(fullExtent.MinY, layerExtent.MinY),
                            Math.Max(fullExtent.MaxX, layerExtent.MaxX),
                            Math.Max(fullExtent.MaxY, layerExtent.MaxY)
                        );
                }
            }
            return fullExtent;
        }

        private PointF ScreenToWorld(PointF screenPoint)
        {
            if (Bounds == null) return screenPoint;

            double worldWidth = Bounds.MaxX - Bounds.MinX;
            double worldHeight = Bounds.MaxY - Bounds.MinY;

            if (worldWidth == 0 || worldHeight == 0) return screenPoint;

            float scaleX = Width / (float)worldWidth;
            float scaleY = Height / (float)worldHeight;
            float scale = Math.Min(scaleX, scaleY) * _zoomLevel;

            float offsetX = (Width - (float)worldWidth * scale) / 2;
            float offsetY = (Height - (float)worldHeight * scale) / 2;

            // 反向转换
            float worldX = ((screenPoint.X - offsetX - _panOffset.X) / scale) + (float)Bounds.MinX;
            float worldY = ((Height - (screenPoint.Y - _panOffset.Y + offsetY)) / scale) + (float)Bounds.MinY;

            return new PointF(worldX, worldY);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_backBuffer != null)
                {
                    _backBuffer.Dispose();
                    _backBuffer = null;
                }
            }
            base.Dispose(disposing);
        }

        public void Initialize(LayerServices layerServices)
        {
            _layerServices = layerServices ?? throw new ArgumentNullException(nameof(layerServices));
            
            this.MouseWheel += OnMouseWheel;
            this.MouseDown += OnMouseDown;
            this.MouseMove += OnMouseMove;
            this.MouseUp += OnMouseUp;

            // 初始化时计算边界框
            RecalculateBounds();
        }

        public void RecalculateBounds()
        {
            Console.WriteLine("\n=== 重新计算边界框 ===");
            var layers = _layerServices?.GetAllLayers();
            if (layers != null && layers.Count > 0)
            {
                BoundingBox totalExtent = null;
                foreach (var layer in layers)
                {
                    var layerExtent = CalculateLayerExtent(layer);
                    if (layerExtent != null)
                    {
                        if (totalExtent == null)
                            totalExtent = layerExtent;
                        else
                        {
                            totalExtent = new BoundingBox(
                                Math.Min(totalExtent.MinX, layerExtent.MinX),
                                Math.Min(totalExtent.MinY, layerExtent.MinY),
                                Math.Max(totalExtent.MaxX, layerExtent.MaxX),
                                Math.Max(totalExtent.MaxY, layerExtent.MaxY)
                            );
                        }
                    }
                }

                if (totalExtent != null)
                {
                    // 添加一些边距
                    double width = totalExtent.MaxX - totalExtent.MinX;
                    double height = totalExtent.MaxY - totalExtent.MinY;
                    double margin = Math.Max(width, height) * 0.1;

                    _bounds = new BoundingBox(
                        totalExtent.MinX - margin,
                        totalExtent.MinY - margin,
                        totalExtent.MaxX + margin,
                        totalExtent.MaxY + margin
                    );

                    Console.WriteLine($"设置新边界框: ({_bounds.MinX}, {_bounds.MinY}) - ({_bounds.MaxX}, {_bounds.MaxY})");
                }
            }
        }

        private void CreateBuffer()
        {
            _bufferGraphics = Graphics.FromImage(_backBuffer);
        }

        private void DrawLabels(Feature labelLayer, Graphics g)
        {
            if (labelLayer == null || !labelLayer.IsVisible) return;

            var format = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            };

            using (var font = new Font("Arial", 10, FontStyle.Bold))
            {
                foreach (var feature in labelLayer.Features)
                {
                    if (feature?.Geometry is GeometryPoint point &&
                        feature.Properties.TryGetValue("Text", out object textObj))
                    {
                        var worldPoint = new PointF((float)point.X, (float)point.Y);
                        var screenPoint = WorldToScreen(worldPoint);
                        string text = textObj.ToString();

                        // 获取颜色（如果是图例）
                        Color textColor = Color.Black;
                        if (feature.Properties.TryGetValue("Color", out object colorObj) &&
                            colorObj is Color color)
                        {
                            textColor = color;
                        }

                        // 绘制文本背景
                        SizeF textSize = g.MeasureString(text, font);
                        RectangleF shadowRect = new RectangleF(
                            screenPoint.X - 2,
                            screenPoint.Y - textSize.Height/2 - 2,
                            textSize.Width + 4,
                            textSize.Height + 4
                        );

                        // 绘制半透明背景
                        using (var bgBrush = new SolidBrush(Color.FromArgb(230, Color.White)))
                        {
                            g.FillRectangle(bgBrush, shadowRect);
                        }

                        // 绘制边框
                        using (var borderPen = new Pen(Color.FromArgb(100, Color.Black), 1))
                        {
                            g.DrawRectangle(borderPen, shadowRect.X, shadowRect.Y, shadowRect.Width, shadowRect.Height);
                        }

                        // 绘制文本
                        using (var brush = new SolidBrush(textColor))
                        {
                            g.DrawString(text, font, brush, screenPoint.X, screenPoint.Y, format);
                        }
                    }
                }
            }
        }
    }
}
