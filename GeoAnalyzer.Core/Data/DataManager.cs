using GeoAnalyzer.Core.Models.Geometry;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GeoAnalyzer.Core.Data
{
    public class DataManager
    {
        private List<Feature> _layers = new List<Feature>();
        private Feature _currentLayer;

        public List<Feature> GetAllLayers()
        {
            return _layers;
        }

        public Feature GetCurrentLayer()
        {
            return _currentLayer;
        }

        public void AddLayer(Feature layer)
        {
            Console.WriteLine($"DataManager添加图层: {layer.Name}, 类型: {layer.Geometry?.GetType().Name}");
            _layers.Add(layer);
            _currentLayer = layer;
        }

        public void RemoveLayer(Feature layer)
        {
            _layers.Remove(layer);
            if (_currentLayer == layer)
            {
                _currentLayer = _layers.FirstOrDefault();
            }
        }

        public void SetCurrentLayer(Feature layer)
        {
            if (_layers.Contains(layer))
            {
                _currentLayer = layer;
            }
        }

        public void ReorderLayers(List<Feature> newOrder)
        {
            Console.WriteLine("\n=== DataManager: 重新排序图层 ===");
            _layers.Clear();
            _layers.AddRange(newOrder);
            Console.WriteLine($"图层重新排序完成，当前图层数量: {_layers.Count}");
        }

        // 添加其他数据管理相关的方法
    }
}
