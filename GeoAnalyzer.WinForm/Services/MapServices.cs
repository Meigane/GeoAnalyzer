// Services/MapService.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using GeoAnalyzer.Core.Models.Geometry;
using GeoAnalyzer.Core.IO.Common;
using System.Collections.Generic;
using DrawingPoint = System.Drawing.Point;
using GeoPoint = GeoAnalyzer.Core.Models.Geometry.Point;
using GeoAnalyzer.WinForm.Controls;
using GeoAnalyzer.Core.Data;
using System.Linq;

namespace GeoAnalyzer.WinForm.Services
{
    public class MapServices
    {
        private readonly Panel _panel;
        private readonly MapControl _mapControl;
        private readonly LayerServices _layerServices;
        private readonly DataManager _dataManager;
        private readonly LayerTreeView _layerTreeView;
        private BoundingBox _mapExtent;
        private float _zoomLevel = 1.0f;
        private PointF _panOffset = new PointF(0, 0);
        private Graphics _graphics;
        private Bitmap _bitmap;

        public MapServices(Panel panel, DataManager dataManager, LayerTreeView layerTreeView)
        {
            _panel = panel ?? throw new ArgumentNullException(nameof(panel));
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            _layerTreeView = layerTreeView ?? throw new ArgumentNullException(nameof(layerTreeView));
            
            _mapControl = new MapControl();
            _panel.Controls.Add(_mapControl);
            
            // 初始化 LayerServices，传入所需的参数
            _layerServices = new LayerServices(_dataManager, _mapControl, _layerTreeView);
            
            InitializeGraphics();
            
            // 添加事件处理
            _panel.Paint += MapPanel_Paint;
            _panel.Resize += MapPanel_Resize;
            _panel.MouseWheel += MapPanel_MouseWheel;
            _panel.MouseDown += MapPanel_MouseDown;
            _panel.MouseMove += MapPanel_MouseMove;
            _panel.MouseUp += MapPanel_MouseUp;
        }

        private void InitializeGraphics()
        {
            if (_panel.Width > 0 && _panel.Height > 0)
            {
                _bitmap = new Bitmap(_panel.Width, _panel.Height);
                _graphics = Graphics.FromImage(_bitmap);
                _graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            }
        }

        public void LoadLayer(string filePath)
        {
            try
            {
                var reader = DataWriterFactory.GetWriter(filePath) as IDataReader;
                if (reader == null)
                {
                    throw new InvalidOperationException("无法创建适当的数据读取器");
                }
                var features = reader.Read(filePath);
                foreach (var feature in features)
                {
                    // 为每个图层生成一个名称
                    string layerName = $"Layer_{DateTime.Now.Ticks}";
                    _layerServices.AddLayer(feature, layerName);
                }
                UpdateMapExtent();
                RefreshMap();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载图层失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateMapExtent()
        {
            // 计算所有图层的边界范围
            var layers = _dataManager.GetAllLayers();
            if (layers.Count > 0)
            {
                var bounds = layers[0].Geometry.GetBoundingBox();
                foreach (var layer in layers.Skip(1))
                {
                    var layerBounds = layer.Geometry.GetBoundingBox();
                    bounds = new BoundingBox(
                        Math.Min(bounds.MinX, layerBounds.MinX),
                        Math.Min(bounds.MinY, layerBounds.MinY),
                        Math.Max(bounds.MaxX, layerBounds.MaxX),
                        Math.Max(bounds.MaxY, layerBounds.MaxY)
                    );
                }
                _mapExtent = bounds;
            }
        }

        private void RefreshMap()
        {
            _graphics.Clear(Color.White);
            _layerServices.DrawLayers();  // 移除不需要的参数
            _panel.Invalidate();
        }

        private void MapPanel_Paint(object sender, PaintEventArgs e)
        {
            if (_bitmap != null)
            {
                e.Graphics.DrawImage(_bitmap, 0, 0);
            }
        }

        private void MapPanel_Resize(object sender, EventArgs e)
        {
            InitializeGraphics();
            RefreshMap();
        }

        private void MapPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            Console.WriteLine($"\n=== 鼠标滚轮事件触发 ===");
            Console.WriteLine($"Delta: {e.Delta}");
            Console.WriteLine($"当前缩放级别: {_zoomLevel}");
            
            // 实现缩放功能
            if (e.Delta > 0)
            {
                _zoomLevel *= 1.1f;
                Console.WriteLine($"放大 - 新缩放级别: {_zoomLevel}");
            }
            else
            {
                _zoomLevel /= 1.1f;
                Console.WriteLine($"缩小 - 新缩放级别: {_zoomLevel}");
            }

            Console.WriteLine("准备刷新地图");
            RefreshMap();
            Console.WriteLine("=== 鼠标滚轮事件处理完成 ===\n");
        }

        private bool isPanning = false;
        private DrawingPoint lastMousePos;

        private void MapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            Console.WriteLine($"\n=== 鼠标按下事件触发 ===");
            if (e.Button == MouseButtons.Left)
            {
                isPanning = true;
                lastMousePos = e.Location;
                Console.WriteLine($"开始平移 - 起始位置: ({lastMousePos.X}, {lastMousePos.Y})");
            }
            Console.WriteLine("=== 鼠标按下事件处理完成 ===\n");
        }

        private void MapPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                Console.WriteLine($"\n=== 鼠标移动事件触发 ===");
                float deltaX = e.X - lastMousePos.X;
                float deltaY = e.Y - lastMousePos.Y;
                
                Console.WriteLine($"平移增量: deltaX={deltaX}, deltaY={deltaY}");
                Console.WriteLine($"原平移偏移: offsetX={_panOffset.X}, offsetY={_panOffset.Y}");
                
                _panOffset.X += deltaX;
                _panOffset.Y += deltaY;
                
                Console.WriteLine($"新平移偏移: offsetX={_panOffset.X}, offsetY={_panOffset.Y}");
                
                lastMousePos = e.Location;
                RefreshMap();
                Console.WriteLine("=== 鼠标移动事件处理完成 ===\n");
            }
        }

        private void MapPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isPanning = false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_graphics != null)
                {
                    _graphics.Dispose();
                    _graphics = null;
                }
                if (_bitmap != null)
                {
                    _bitmap.Dispose();
                    _bitmap = null;
                }
            }
        }
    }
}
