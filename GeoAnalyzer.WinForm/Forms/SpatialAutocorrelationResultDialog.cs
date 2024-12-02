using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace GeoAnalyzer.WinForm.Forms
{
    public partial class SpatialAutocorrelationResultDialog : Form
    {
        public SpatialAutocorrelationResultDialog(
            double globalMoranI, 
            double zScore, 
            double pValue,
            double[] localMoranI,
            string attributeName,
            string csvPath)
        {
            InitializeComponent();
            DisplayResults(globalMoranI, zScore, pValue, localMoranI, attributeName);
            txtCsvPath.Text = csvPath;
        }

        private void DisplayResults(double globalMoranI, double zScore, double pValue, double[] localMoranI, string attributeName)
        {
            try 
            {
                // 检查控件是否为空
                if (lblGlobalMoran == null) throw new NullReferenceException("lblGlobalMoran 为空");
                if (lblZScore == null) throw new NullReferenceException("lblZScore 为空");
                if (lblPValue == null) throw new NullReferenceException("lblPValue 为空");
                if (txtInterpretation == null) throw new NullReferenceException("txtInterpretation 为空");
                if (panelChart == null) throw new NullReferenceException("panelChart 为空");

                // 检查输入参数
                if (localMoranI == null) throw new ArgumentNullException(nameof(localMoranI));
                if (string.IsNullOrEmpty(attributeName)) throw new ArgumentNullException(nameof(attributeName));

                // 1. 显示全局Moran's I结果
                Console.WriteLine("正在设置全局Moran's I结果...");
                lblGlobalMoran.Text = $"全局Moran's I: {globalMoranI:F4}";
                lblZScore.Text = $"Z分数: {zScore:F4}";
                lblPValue.Text = $"P值: {pValue:F4}";

                // 2. 解释结果
                Console.WriteLine("正在生成解释结果...");
                string interpretation = InterpretResults(globalMoranI, zScore, pValue);
                txtInterpretation.Text = interpretation;

                // 3. 绘制局部Moran's I的直方图
                Console.WriteLine("正在创建直方图...");
                CreateHistogram(localMoranI, attributeName);

                Console.WriteLine("DisplayResults 执行完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"错误信息: {ex.Message}\n\n调用堆栈: {ex.StackTrace}", 
                    "错误", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                throw; // 重新抛出异常以便进一步调试
            }
        }

        private string InterpretResults(double globalMoranI, double zScore, double pValue)
        {
            string result = "分析结果解释：\r\n\r\n";

            // Moran's I 解释
            result += $"1. Moran's I 值为 {globalMoranI:F4}：\r\n";
            if (globalMoranI > 0)
                result += "   表现为正相关，即相似值趋于聚集\r\n";
            else if (globalMoranI < 0)
                result += "   表现为负相关，即不相似值趋于聚集\r\n";
            else
                result += "   表现为随机分布\r\n";

            // 统计显著性解释
            result += $"\r\n2. 统计显著性：\r\n";
            result += $"   Z分数为 {zScore:F4}，P值为 {pValue:F4}\r\n";
            if (pValue < 0.01)
                result += "   在99%的置信水平上统计显著\r\n";
            else if (pValue < 0.05)
                result += "   在95%的置信水平上统计显著\r\n";
            else
                result += "   统计上不显著\r\n";

            return result;
        }

        private void CreateHistogram(double[] localMoranI, string attributeName)
        {
            try
            {
                // 确保localMoranI不为空
                if (localMoranI == null || localMoranI.Length == 0)
                {
                    MessageBox.Show("没有可用的局部Moran's I数据", "错误", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var chart = new Chart();
                chart.Dock = DockStyle.Fill;
                
                // 创建图表区域
                var chartArea = new ChartArea();
                chart.ChartAreas.Add(chartArea);

                // 创建数据系列
                var series = new Series();
                series.ChartType = SeriesChartType.Column;
                series.Name = "局部Moran's I分布";

                // 计算直方图数据
                int binCount = 20;
                double min = localMoranI.Min();
                double max = localMoranI.Max();
                
                // 处理min和max相等的情况
                if (Math.Abs(max - min) < 1e-10)
                {
                    min -= 0.5;
                    max += 0.5;
                }
                
                double binWidth = (max - min) / binCount;
                var bins = new int[binCount];

                foreach (var value in localMoranI)
                {
                    // 安全地计算bin索引
                    int binIndex = (int)Math.Floor((value - min) / binWidth);
                    // 确保索引在有效范围内
                    binIndex = Math.Max(0, Math.Min(binIndex, binCount - 1));
                    bins[binIndex]++;
                }

                // 添加数据点
                for (int i = 0; i < binCount; i++)
                {
                    double binCenter = min + (i + 0.5) * binWidth;
                    series.Points.AddXY(binCenter, bins[i]);
                }

                chart.Series.Add(series);

                // 设置标题和轴标签
                var title = new Title($"局部Moran's I分布 - {attributeName}");
                chart.Titles.Add(title);
                chartArea.AxisX.Title = "局部Moran's I值";
                chartArea.AxisY.Title = "频数";

                // 清除旧的图表（如果有）
                panelChart.Controls.Clear();
                // 添加新图表
                panelChart.Controls.Add(chart);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"创建直方图时出错: {ex.Message}\n\n{ex.StackTrace}", 
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnOpenCsv_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(txtCsvPath.Text))
            {
                System.Diagnostics.Process.Start(txtCsvPath.Text);
            }
            else
            {
                MessageBox.Show("找不到CSV文件。", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 