using System;
using System.Windows.Forms;
using System.Collections.Generic;
using GeoAnalyzer.Core.Models.Geometry;
using System.Linq;

namespace GeoAnalyzer.WinForm.Forms
{
    public partial class SpatialClusteringDialog : Form
    {
        // 将控件声明为私有字段
        private readonly Feature _layer;

        public int ClusterCount => (int)numClusters.Value;
        public string[] SelectedAttributes => GetSelectedAttributes();
        public Feature Layer => _layer;

        public SpatialClusteringDialog(Feature layer)
        {
            _layer = layer ?? throw new ArgumentNullException(nameof(layer));
            InitializeComponent();
            LoadAttributes();
        }

        private void LoadAttributes()
        {
            Console.WriteLine("\n=== 加载聚类分析属性 ===");
            if (_layer != null)
            {
                lstAttributes.BeginUpdate();
                try
                {
                    lstAttributes.Items.Clear();
                    var attributes = _layer.GetAttributeNames().ToArray();
                    lstAttributes.Items.AddRange(attributes);
                    Console.WriteLine($"加载了 {attributes.Length} 个属性");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载属性失败: {ex.Message}");
                }
                finally
                {
                    lstAttributes.EndUpdate();
                }
            }
            else
            {
                Console.WriteLine("警告: 图层为空");
            }
            Console.WriteLine("=== 属性加载完成 ===\n");
        }

        private string[] GetSelectedAttributes()
        {
            var selected = new List<string>();
            foreach (var item in lstAttributes.CheckedItems)
            {
                selected.Add(item.ToString());
            }
            return selected.ToArray();
        }
    }
}