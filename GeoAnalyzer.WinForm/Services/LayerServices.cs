using GeoAnalyzer.Core.Data;
using GeoAnalyzer.Core.Models.Geometry;
using System.Collections.Generic;
using System;
using System.Linq;
using MapControl = GeoAnalyzer.WinForm.Controls.MapControl;
using LayerTreeView = GeoAnalyzer.WinForm.Controls.LayerTreeView;
using GeoAnalyzer.WinForm.LayerEvents;
using GeoAnalyzer.Core.Models;
using System.Drawing;

namespace GeoAnalyzer.WinForm.Services
{
    public class LayerServices
    {
        private readonly DataManager _dataManager;
        private readonly MapControl _mapControl;
        private readonly LayerTreeView _layerTreeView;
        private readonly List<Feature> _layers = new List<Feature>();
        private Feature _currentLayer;
        private bool _isRefreshing = false;
        private bool _isUpdating = false;

        public event EventHandler<LayerEventArgs> LayerChanged;

        public LayerServices(
            DataManager dataManager, 
            MapControl mapControl, 
            LayerTreeView layerTreeView)
        {
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            _mapControl = mapControl ?? throw new ArgumentNullException(nameof(mapControl));
            _layerTreeView = layerTreeView ?? throw new ArgumentNullException(nameof(layerTreeView));

            _mapControl.Initialize(this);
            _layerTreeView.Initialize(this);

            _layerTreeView.LayerVisibilityChanged += OnLayerVisibilityChanged;
            _layerTreeView.LayerStyleChanged += OnLayerStyleChanged;
            _layerTreeView.LayerRemoved += OnLayerRemoved;
        }

        protected virtual void OnLayerChanged(Feature layer)
        {
            if (!_isUpdating)
            {
                LayerChanged?.Invoke(this, new LayerEventArgs(layer));
            }
        }

        public void AddLayer(Feature feature, string name)
        {
            Console.WriteLine($"\n=== 添加图层: {name} ===");
            Console.WriteLine($"要素数量: {feature.Features?.Count ?? 0}");
            Console.WriteLine($"子图层数量: {feature.SubLayers?.Count ?? 0}");

            feature.Name = name;
            _layers.Add(feature);
            
            if (_currentLayer == null)
            {
                _currentLayer = feature;
            }
            
            RefreshViews();
            OnLayerChanged(feature);
        }

        public void AddLayers(List<Feature> features, string layerName)
        {
            Console.WriteLine($"准备添加要素到图层 - 要素数量: {features.Count}");
            
            // 创建一个新图层
            var layer = new Feature
            {
                Name = layerName,
                Properties = new Dictionary<string, object>(),
                IsVisible = true  // 确保图层可见
            };
            
            // 将所有要素添加到这个图层中
            foreach (var feature in features)
            {
                // 存储原始要素
                layer.Features.Add(feature);
                
                // 不要为每个feature创建新图层，而是将feature的属性添加到layer中
                if (layer.Geometry == null)
                {
                    // 第一个要素的几何类型决定图层类型
                    layer.Geometry = feature.Geometry;
                }
                
                // 合并属性
                foreach (var prop in feature.Properties)
                {
                    if (!layer.Properties.ContainsKey(prop.Key))
                    {
                        layer.Properties.Add(prop.Key, new List<object>());
                    }
                    ((List<object>)layer.Properties[prop.Key]).Add(prop.Value);
                }
            }
            
            // 添加到本地图层列表
            _layers.Add(layer);
            
            // 添加到数据管理器
            _dataManager.AddLayer(layer);
            
            // 刷新视图
            RefreshViews();
            
            // 触发图层改变事件
            OnLayerChanged(layer);
            
            // 缩放到所有图层
            _mapControl.ZoomToAllLayers();
            
            if (_currentLayer == null)
            {
                _currentLayer = layer;
            }
        }

        private void RefreshViews()
        {
            if (_isRefreshing)
            {
                Console.WriteLine("跳过刷新：正在进行中的刷新未完成");
                return;
            }

            _isRefreshing = true;
            try
            {
                Console.WriteLine($"\n=== LayerServices.RefreshViews 开始 [ThreadId: {System.Threading.Thread.CurrentThread.ManagedThreadId}] ===");
                
                // 先刷新树视图
                if (_layerTreeView != null)
                {
                    Console.WriteLine("准备刷新树视图...");
                    _layerTreeView.RefreshView(_layers);
                }
                
                // 最后刷新地图
                if (_mapControl != null)
                {
                    Console.WriteLine("准备刷新地图控件...");
                    _mapControl.RefreshMap();
                }
                
                Console.WriteLine($"=== LayerServices.RefreshViews 完成 [ThreadId: {System.Threading.Thread.CurrentThread.ManagedThreadId}] ===\n");
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        public List<Feature> GetAllLayers()
        {
            return _layers;
        }

        private void OnLayerVisibilityChanged(object sender, LayerEventArgs e)
        {
            e.Layer.IsVisible = e.IsVisible;
            _mapControl.RefreshMap();
            OnLayerChanged(e.Layer);
        }

        private void OnLayerStyleChanged(object sender, LayerEventArgs e)
        {
            _mapControl.RefreshMap();
            OnLayerChanged(e.Layer);
        }

        private void OnLayerRemoved(object sender, LayerEventArgs e)
        {
            _dataManager.RemoveLayer(e.Layer);
            
            if (e.Layer == _currentLayer)
            {
                _currentLayer = _layers.FirstOrDefault();
            }
            
            _mapControl.RefreshMap();
            OnLayerChanged(e.Layer);
        }

        public void DrawLayers()
        {
            if (_mapControl == null) return;

            using (Graphics g = _mapControl.CreateGraphics())
            {
                var layers = _dataManager.GetAllLayers();
                foreach (var layer in layers)
                {
                    if (layer.IsVisible)
                    {
                        _mapControl.DrawLayer(layer, g);
                    }
                }
            }
        }

        public Feature GetCurrentLayer()
        {
            return _currentLayer ?? _layers.FirstOrDefault();
        }

        public void SetCurrentLayer(Feature layer)
        {
            if (_isUpdating) return;
            
            _isUpdating = true;
            try
            {
                _currentLayer = layer;
                OnLayerChanged(layer);
            }
            finally
            {
                _isUpdating = false;
            }
        }

        public void ReorderLayers(List<Feature> newOrder)
        {
            Console.WriteLine("\n=== LayerServices: 重新排序图层 ===");
            try
            {
                if (newOrder == null)
                {
                    throw new ArgumentNullException(nameof(newOrder));
                }

                _layers.Clear();
                _layers.AddRange(newOrder);
                _dataManager.ReorderLayers(newOrder);
                
                // 仅刷新树视图，不刷新地图
                _layerTreeView?.RefreshView(_layers);
                
                Console.WriteLine($"图层重新排序完成，当前图层数量: {_layers.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"重新排序图层失败: {ex.Message}");
                throw;
            }
        }

        public void RemoveLayer(Feature layer)
        {
            Console.WriteLine($"\n=== 移除图层: {layer.Name} ===");
            _layers.Remove(layer);
            _dataManager.RemoveLayer(layer);
            
            if (_currentLayer == layer)
            {
                _currentLayer = _layers.FirstOrDefault();
                Console.WriteLine($"当前图层被移除，新的当前图层: {_currentLayer?.Name ?? "无"}");
            }
            
            RefreshViews();
            OnLayerChanged(layer);
            Console.WriteLine("=== 图层移除完成 ===\n");
        }

        public void UpdateLayerVisibility(Feature layer, bool isVisible)
        {
            Console.WriteLine($"\n=== 更新图层可见性: {layer.Name} ===");
            layer.IsVisible = isVisible;
            RefreshViews();
            OnLayerChanged(layer);
            Console.WriteLine($"图层可见性已更新为: {isVisible}");
        }
    }
}
