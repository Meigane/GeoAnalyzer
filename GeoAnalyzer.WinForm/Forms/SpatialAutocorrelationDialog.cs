using System;
using System.Windows.Forms;
using GeoAnalyzer.Core.Models.Geometry;
using System.Linq;
using GeoAnalyzer.Core.SpatialAnalysis;

namespace GeoAnalyzer.WinForm.Forms
{
    public partial class SpatialAutocorrelationDialog : Form
    {
        private Feature _layer;

        public string SelectedAttribute => cboAttributes.SelectedItem?.ToString();
        public SpatialWeightMethod WeightMethod => 
            cboWeightMethod.SelectedIndex == 0 ? SpatialWeightMethod.KNN : SpatialWeightMethod.Distance;
        public int KNeighbors => (int)numKNN.Value;
        public double DistanceThreshold => (double)numDistance.Value;

        public SpatialAutocorrelationDialog(Feature layer)
        {
            InitializeComponent();
            _layer = layer;
            LoadAttributes();
        }

        private void LoadAttributes()
        {
            try
            {
                Console.WriteLine("开始加载数值型属性...");
                var numericAttributes = _layer.GetNumericAttributes().ToList();
                Console.WriteLine($"找到 {numericAttributes.Count} 个数值型属性");

                cboAttributes.Items.Clear();
                foreach (var attr in numericAttributes)
                {
                    cboAttributes.Items.Add(attr);
                    Console.WriteLine($"添加属性: {attr}");
                }

                if (cboAttributes.Items.Count > 0)
                {
                    cboAttributes.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载属性时出错: {ex.Message}");
                MessageBox.Show($"加载属性失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}