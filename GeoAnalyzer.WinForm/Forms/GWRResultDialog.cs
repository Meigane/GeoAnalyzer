using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace GeoAnalyzer.WinForm.Forms
{
    public partial class GWRResultDialog : Form
    {
        private readonly double globalR2;
        private readonly double[] localR2;
        private readonly double[][] coefficients;
        private readonly double[] residuals;
        private readonly string[] variableNames;
        private readonly string csvPath;

        public GWRResultDialog(
            double globalR2,
            double[] localR2,
            double[][] coefficients,
            double[] residuals,
            string[] variableNames,
            string csvPath)
        {
            Console.WriteLine("\n=== 初始化GWR结果对话框 ===");
            Console.WriteLine($"全局R²: {globalR2:F4}");
            Console.WriteLine($"局部R²数量: {localR2?.Length ?? 0}");
            Console.WriteLine($"系数矩阵维度: {coefficients?.Length ?? 0} x {coefficients?[0]?.Length ?? 0}");
            Console.WriteLine($"变量数量: {variableNames?.Length ?? 0}");
            Console.WriteLine($"CSV路径: {csvPath}");

            this.globalR2 = globalR2;
            this.localR2 = localR2 ?? throw new ArgumentNullException(nameof(localR2));
            this.coefficients = coefficients ?? throw new ArgumentNullException(nameof(coefficients));
            this.residuals = residuals ?? throw new ArgumentNullException(nameof(residuals));
            this.variableNames = variableNames ?? throw new ArgumentNullException(nameof(variableNames));
            this.csvPath = csvPath ?? throw new ArgumentNullException(nameof(csvPath));

            InitializeComponent();
            DisplayResults();
        }

        private void DisplayResults()
        {
            try
            {
                Console.WriteLine("\n开始显示GWR分析结果...");

                // 1. 显示全局统计结果
                Console.WriteLine("显示全局统计结果...");
                txtGlobalStats.Text = $"全局统计结果:\r\n\r\n" +
                    $"全局 R² = {globalR2:F4}\r\n" +
                    $"平均局部 R² = {localR2.Average():F4}\r\n" +
                    $"最小局部 R² = {localR2.Min():F4}\r\n" +
                    $"最大局部 R² = {localR2.Max():F4}\r\n\r\n" +
                    $"残差统计:\r\n" +
                    $"最小值 = {residuals.Min():F4}\r\n" +
                    $"最大值 = {residuals.Max():F4}\r\n" +
                    $"平均值 = {residuals.Average():F4}\r\n" +
                    $"标准差 = {Math.Sqrt(residuals.Select(x => x * x).Average() - Math.Pow(residuals.Average(), 2)):F4}";

                // 2. 创建局部R²的直方图
                Console.WriteLine("创建局部R²直方图...");
                CreateLocalR2Histogram();

                // 3. 显示回归系数统计
                Console.WriteLine("显示回归系数统计...");
                DisplayCoefficientsStats();

                // 4. 设置CSV路径
                Console.WriteLine("设置CSV路径...");
                txtCsvPath.Text = csvPath;

                Console.WriteLine("GWR结果显示完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"显示GWR结果时出错: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                throw;
            }
        }

        private void CreateLocalR2Histogram()
        {
            try
            {
                Console.WriteLine("开始创建局部R²直方图...");
                Console.WriteLine($"局部R²范围: {localR2.Min():F4} - {localR2.Max():F4}");

                var chart = new Chart();
                chart.Dock = DockStyle.Fill;

                var chartArea = new ChartArea();
                chart.ChartAreas.Add(chartArea);

                var series = new Series();
                series.ChartType = SeriesChartType.Column;
                series.Name = "局部R²分布";

                // 计算直方图数据
                int binCount = 20;
                double min = localR2.Min();
                double max = localR2.Max();
                
                // 处理min和max相等的情况
                if (Math.Abs(max - min) < 1e-10)
                {
                    min -= 0.5;
                    max += 0.5;
                }
                
                double binWidth = (max - min) / binCount;
                var bins = new int[binCount];

                foreach (var value in localR2)
                {
                    int binIndex = (int)Math.Floor((value - min) / binWidth);
                    binIndex = Math.Max(0, Math.Min(binIndex, binCount - 1));
                    bins[binIndex]++;
                }

                Console.WriteLine($"直方图统计:");
                Console.WriteLine($"- 分组数: {binCount}");
                Console.WriteLine($"- 组距: {binWidth:F4}");

                // 添加数据点
                for (int i = 0; i < binCount; i++)
                {
                    double binCenter = min + (i + 0.5) * binWidth;
                    series.Points.AddXY(binCenter, bins[i]);
                }

                chart.Series.Add(series);

                // 设置标题和轴标签
                chart.Titles.Add(new Title("局部R²分布"));
                chartArea.AxisX.Title = "局部R²值";
                chartArea.AxisY.Title = "频数";

                // 添加到面板
                panelChart.Controls.Clear();
                panelChart.Controls.Add(chart);

                Console.WriteLine("局部R²直方图创建完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"创建直方图时出错: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                throw;
            }
        }

        private void DisplayCoefficientsStats()
        {
            try
            {
                Console.WriteLine("开始显示回归系数统计...");
                Console.WriteLine($"变量数量: {variableNames.Length}");

                var grid = new DataGridView();
                grid.Dock = DockStyle.Fill;
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                grid.ReadOnly = true;

                // 添加列
                grid.Columns.Add("Variable", "变量");
                grid.Columns.Add("Mean", "平均值");
                grid.Columns.Add("StdDev", "标准差");
                grid.Columns.Add("Min", "最小值");
                grid.Columns.Add("Max", "最大值");

                // 添加每个变量的统计数据
                for (int i = 0; i < variableNames.Length; i++)
                {
                    Console.WriteLine($"处理变量: {variableNames[i]}");
                    var coefs = coefficients.Select(c => c[i]).ToArray();
                    grid.Rows.Add(
                        variableNames[i],
                        coefs.Average().ToString("F4"),
                        Math.Sqrt(coefs.Select(x => x * x).Average() - Math.Pow(coefs.Average(), 2)).ToString("F4"),
                        coefs.Min().ToString("F4"),
                        coefs.Max().ToString("F4")
                    );
                }

                panelCoefficients.Controls.Clear();
                panelCoefficients.Controls.Add(grid);

                Console.WriteLine("回归系数统计显示完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"显示回归系数统计时出错: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                throw;
            }
        }

        private void btnOpenCsv_Click(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine($"尝试打开CSV文件: {csvPath}");
                if (System.IO.File.Exists(csvPath))
                {
                    System.Diagnostics.Process.Start(csvPath);
                    Console.WriteLine("CSV文件打开成功");
                }
                else
                {
                    Console.WriteLine("CSV文件不存在");
                    MessageBox.Show("找不到CSV文件。", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"打开CSV文件时出错: {ex.Message}");
                MessageBox.Show($"打开CSV文件失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 