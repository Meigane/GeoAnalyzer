using System;
using System.Windows.Forms;
using GeoAnalyzer.Core.SpatialAnalysis;
using System.IO;
using GeoAnalyzer.Core.Models.Geometry;
using System.Linq;

namespace GeoAnalyzer.WinForm.Forms
{
    public partial class GWRParametersDialog : Form
    {
        private readonly GeoAnalyzer.Core.Models.Geometry.Feature _layer;
        private readonly double _defaultBandwidth;

        public double Bandwidth { get; private set; }
        public GeographicallyWeightedRegressionService.KernelType KernelFunction { get; private set; }
        public string OutputPath { get; private set; }

        public GWRParametersDialog(GeoAnalyzer.Core.Models.Geometry.Feature layer)
        {
            _layer = layer ?? throw new ArgumentNullException(nameof(layer));
            
            // 计算默认带宽
            var service = new GeographicallyWeightedRegressionService();
            _defaultBandwidth = service.CalculateOptimalBandwidth(layer);
            
            InitializeComponent();
            InitializeValues();
        }

        private void InitializeValues()
        {
            try
            {
                Console.WriteLine("初始化GWR参数对话框...");
                
                // 设置带宽范围和默认值
                numBandwidth.DecimalPlaces = 2;
                numBandwidth.Increment = 0.1M;
                numBandwidth.Minimum = 0.01M;
                numBandwidth.Maximum = 10000M;
                numBandwidth.Value = (decimal)_defaultBandwidth;
                
                Console.WriteLine($"设置默认带宽: {_defaultBandwidth:F2}");

                // 设置核函数选项
                cboKernelType.Items.Clear();
                cboKernelType.Items.AddRange(Enum.GetNames(typeof(GeographicallyWeightedRegressionService.KernelType)));
                cboKernelType.SelectedIndex = 0;
                
                Console.WriteLine($"可用的核函数类型: {string.Join(", ", cboKernelType.Items.Cast<string>())}");

                // 设置默认输出路径
                txtOutputPath.Text = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"GWR分析结果_{_layer.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                );

                // 添加事件处理
                btnOK.Click += BtnOK_Click;
                btnBrowse.Click += BtnBrowse_Click;
                numBandwidth.ValueChanged += NumBandwidth_ValueChanged;
                cboKernelType.SelectedIndexChanged += CboKernelType_SelectedIndexChanged;

                UpdateButtonState();
                Console.WriteLine("GWR参数对话框初始化完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化GWR参数对话框时出错: {ex.Message}");
                throw;
            }
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog
            {
                Filter = "CSV文件|*.csv",
                Title = "选择结果保存位置",
                DefaultExt = "csv",
                FileName = Path.GetFileName(txtOutputPath.Text)
            })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtOutputPath.Text = dialog.FileName;
                    UpdateButtonState();
                }
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                try
                {
                    Console.WriteLine("保存参数设置...");
                    Bandwidth = (double)numBandwidth.Value;
                    KernelFunction = (GeographicallyWeightedRegressionService.KernelType)
                        Enum.Parse(typeof(GeographicallyWeightedRegressionService.KernelType), 
                        cboKernelType.SelectedItem.ToString());
                    OutputPath = txtOutputPath.Text;

                    Console.WriteLine($"设置的参数:");
                    Console.WriteLine($"- 带宽: {Bandwidth}");
                    Console.WriteLine($"- 核函数: {KernelFunction}");
                    Console.WriteLine($"- 输出路径: {OutputPath}");

                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"保存参数设置时出错: {ex.Message}");
                    throw;
                }
            }
        }

        private void NumBandwidth_ValueChanged(object sender, EventArgs e)
        {
            UpdateButtonState();
        }

        private void CboKernelType_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonState();
        }

        private bool ValidateInputs()
        {
            if (numBandwidth.Value <= 0)
            {
                MessageBox.Show("带宽必须大于0。", "验证错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (cboKernelType.SelectedIndex == -1)
            {
                MessageBox.Show("请选择核函数类型。", "验证错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrEmpty(txtOutputPath.Text))
            {
                MessageBox.Show("请选择结果保存位置。", "验证错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void UpdateButtonState()
        {
            btnOK.Enabled = numBandwidth.Value > 0 && 
                           cboKernelType.SelectedIndex != -1 &&
                           !string.IsNullOrEmpty(txtOutputPath.Text);
        }
    }
}