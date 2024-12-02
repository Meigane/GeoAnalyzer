using System;
using System.Windows.Forms;
using System.Collections.Generic;
using GeoAnalyzer.Core.Models.Geometry;
using GeoAnalyzer.WinForm.Services;

namespace GeoAnalyzer.WinForm.Forms
{
    public partial class LayerManagerDialog : Form
    {
        private readonly List<Feature> _layers;
        private readonly LayerServices _layerServices;

        public LayerManagerDialog(List<Feature> layers, LayerServices layerServices)
        {
            Console.WriteLine("\n=== 初始化图层管理器 ===");
            _layers = layers ?? throw new ArgumentNullException(nameof(layers));
            _layerServices = layerServices ?? throw new ArgumentNullException(nameof(layerServices));
            InitializeComponent();
            LoadLayers();
            Console.WriteLine($"加载了 {_layers.Count} 个图层");
        }

        private void LoadLayers()
        {
            Console.WriteLine("\n开始加载图层列表");
            layerListView.Items.Clear();
            if (_layers != null)
            {
                // 反向遍历图层列表，使显示顺序与实际顺序一致
                for (int i = _layers.Count - 1; i >= 0; i--)
                {
                    var layer = _layers[i];
                    var item = new ListViewItem(layer.Name)
                    {
                        Checked = layer.IsVisible,
                        Tag = layer
                    };
                    item.SubItems.Add(layer.GetFeatureType().ToString());
                    layerListView.Items.Add(item);
                    Console.WriteLine($"添加图层: {layer.Name}, 类型: {layer.GetFeatureType()}, 可见性: {layer.IsVisible}");
                }
            }
            Console.WriteLine("图层列表加载完成\n");
        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            Console.WriteLine("\n=== 尝试上移图层 ===");
            try
            {
                if (layerListView.SelectedItems.Count == 0)
                {
                    Console.WriteLine("未选中任何图层");
                    return;
                }

                int selectedIndex = layerListView.SelectedItems[0].Index;
                Console.WriteLine($"当前选中图层索引: {selectedIndex}");

                if (selectedIndex <= 0)
                {
                    Console.WriteLine("已经是第一个图层，无法上移");
                    return;
                }

                // 更新ListView
                var selectedItem = layerListView.Items[selectedIndex];
                layerListView.Items.RemoveAt(selectedIndex);
                layerListView.Items.Insert(selectedIndex - 1, selectedItem);
                selectedItem.Selected = true;

                // 更新数据层 - 注意这里的索引转换
                int layerIndex = selectedIndex;  // ListView中的索引现在直接对应实际图层索引
                if (layerIndex >= 0 && layerIndex < _layers.Count)
                {
                    var layer = _layers[layerIndex];
                    _layers.RemoveAt(layerIndex);
                    _layers.Insert(layerIndex - 1, layer);
                    Console.WriteLine($"图层 {layer.Name} 上移到索引 {layerIndex - 1}");

                    // 通知服务层
                    _layerServices?.ReorderLayers(new List<Feature>(_layers));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"上移图层失败: {ex.Message}");
                MessageBox.Show($"上移图层失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            Console.WriteLine("\n=== 尝试下移图层 ===");
            try
            {
                if (layerListView.SelectedItems.Count == 0)
                {
                    Console.WriteLine("未选中任何图层");
                    return;
                }

                int selectedIndex = layerListView.SelectedItems[0].Index;
                Console.WriteLine($"当前选中图层索引: {selectedIndex}");

                if (selectedIndex >= layerListView.Items.Count - 1)
                {
                    Console.WriteLine("已经是最后一个图层，无法下移");
                    return;
                }

                // 更新ListView
                var selectedItem = layerListView.Items[selectedIndex];
                layerListView.Items.RemoveAt(selectedIndex);
                layerListView.Items.Insert(selectedIndex + 1, selectedItem);
                selectedItem.Selected = true;

                // 更新数据层 - 注意这里的索引转换
                int layerIndex = selectedIndex;  // ListView中的索引现在直接对应实际图层索引
                if (layerIndex >= 0 && layerIndex < _layers.Count)
                {
                    var layer = _layers[layerIndex];
                    _layers.RemoveAt(layerIndex);
                    _layers.Insert(layerIndex + 1, layer);
                    Console.WriteLine($"图层 {layer.Name} 下移到索引 {layerIndex + 1}");

                    // 通知服务层
                    _layerServices?.ReorderLayers(new List<Feature>(_layers));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"下移图层失败: {ex.Message}");
                MessageBox.Show($"下移图层失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            Console.WriteLine("\n=== 尝试移除图层 ===");
            try
            {
                if (layerListView.SelectedItems.Count == 0)
                {
                    Console.WriteLine("未选中任何图层");
                    return;
                }

                var selectedItem = layerListView.SelectedItems[0];
                int selectedIndex = selectedItem.Index;
                Console.WriteLine($"准备移除索引为 {selectedIndex} 的图层");

                if (selectedIndex >= 0 && selectedIndex < _layers.Count)
                {
                    var layer = _layers[selectedIndex];
                    Console.WriteLine($"移除图层: {layer.Name}");

                    // 从ListView和数据层移除
                    layerListView.Items.Remove(selectedItem);
                    _layers.RemoveAt(selectedIndex);

                    // 通知服务层
                    _layerServices?.RemoveLayer(layer);
                    Console.WriteLine($"图层 {layer.Name} 已成功移除");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"移除图层失败: {ex.Message}");
                MessageBox.Show($"移除图层失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void layerListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            Console.WriteLine("\n=== 图层可见性改变 ===");
            if (e.Item.Tag is Feature layer)
            {
                layer.IsVisible = e.Item.Checked;
                _layerServices.UpdateLayerVisibility(layer, e.Item.Checked);
                Console.WriteLine($"图层 {layer.Name} 可见性更改为: {e.Item.Checked}");
            }
            Console.WriteLine("=== 可见性更改完成 ===\n");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("\n=== 关闭图层管理器 ===");
                
                // 确保所有更改都已应用
                if (_layerServices != null)
                {
                    _layerServices.ReorderLayers(_layers);
                    Console.WriteLine("已应用最终的图层顺序");
                }
                
                this.DialogResult = DialogResult.OK;
                this.Close();
                
                Console.WriteLine("=== 图层管理器已关闭 ===\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"关闭图层管理器失败: {ex.Message}");
                MessageBox.Show($"保存图层顺序失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 