using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using GeoAnalyzer.Core.Models.Geometry;
using GeoAnalyzer.WinForm.Services;
using GeoAnalyzer.WinForm.LayerEvents;
using DrawingPoint = System.Drawing.Point;
using GeometryPoint = GeoAnalyzer.Core.Models.Geometry.Point;
using GeoAnalyzer.WinForm.Forms;

namespace GeoAnalyzer.WinForm.Controls
{
    public class LayerTreeView : TreeView
    {
        private LayerServices _layerServices;

        public event EventHandler<LayerEventArgs> LayerSelected;

        public void Initialize(LayerServices layerServices)
        {
            _layerServices = layerServices;
            
            this.HideSelection = false;
            this.FullRowSelect = true;
        }

        protected override void OnAfterSelect(TreeViewEventArgs e)
        {
            try
            {
                if (e.Node?.Tag is Feature layer)
                {
                    _layerServices.SetCurrentLayer(layer);
                    OnLayerSelected(layer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"选择图层失败: {ex.Message}");
            }
        }

        protected virtual void OnLayerSelected(Feature layer)
        {
            LayerSelected?.Invoke(this, new LayerEventArgs(layer));
        }

        public void RefreshView(List<Feature> layers)
        {
            Console.WriteLine($"\n=== LayerTreeView.RefreshView 开始 [ThreadId: {System.Threading.Thread.CurrentThread.ManagedThreadId}] ===");
            Console.WriteLine($"当前节点数: {Nodes.Count}, 待更新图层数: {layers?.Count ?? 0}");
            
            try
            {
                BeginUpdate();
                var selectedNode = SelectedNode;
                var selectedLayer = selectedNode?.Tag as Feature;
                
                Nodes.Clear();
                Console.WriteLine("已清除现有节点");
                
                if (layers != null)
                {
                    foreach (var layer in layers)
                    {
                        TreeNode node = new TreeNode(layer.Name)
                        {
                            Tag = layer,
                            Checked = layer.IsVisible
                        };
                        
                        // 添加子图层（如标签图层）
                        if (layer.SubLayers != null)
                        {
                            foreach (var subLayer in layer.SubLayers)
                            {
                                TreeNode subNode = new TreeNode(subLayer.Name)
                                {
                                    Tag = subLayer,
                                    Checked = subLayer.IsVisible
                                };
                                node.Nodes.Add(subNode);
                            }
                        }
                        
                        Nodes.Add(node);
                        
                        // 恢复选中状态
                        if (layer == selectedLayer)
                        {
                            SelectedNode = node;
                        }
                    }
                    Console.WriteLine($"添加了 {layers.Count} 个新节点");
                }
            }
            finally
            {
                EndUpdate();
                Console.WriteLine($"=== LayerTreeView.RefreshView 完成 [ThreadId: {System.Threading.Thread.CurrentThread.ManagedThreadId}] ===\n");
            }
        }

        protected override void OnAfterCheck(TreeViewEventArgs e)
        {
            try
            {
                if (e.Node?.Tag is Feature layer)
                {
                    layer.IsVisible = e.Node.Checked;
                    OnLayerVisibilityChanged(layer, e.Node.Checked);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更改图层可见性失败: {ex.Message}");
            }
        }

        public event EventHandler<LayerEventArgs> LayerVisibilityChanged;
        public event EventHandler<LayerEventArgs> LayerStyleChanged;
        public event EventHandler<LayerEventArgs> LayerRemoved;

        protected virtual void OnLayerVisibilityChanged(Feature layer, bool isVisible)
        {
            LayerVisibilityChanged?.Invoke(this, new LayerEventArgs(layer) { IsVisible = isVisible });
        }
    }
}
