using System;
using System.Linq;  // 新添加的
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using GeoAnalyzer.Core.Data;
using GeoAnalyzer.Core.Visualization;
using GeoAnalyzer.Core.SpatialAnalysis;
using GeoAnalyzer.Core.Models.Geometry;
using GeoAnalyzer.Core.Models.Attributes;
using GeoAnalyzer.Core.IO.Common;
using GeoAnalyzer.Core.IO.Shapefile;
using GeoAnalyzer.Core.Utils;
using Point = System.Drawing.Point;  // 使用System.Drawing.Point作为默认Point
using GeoPoint = GeoAnalyzer.Core.Models.Geometry.Point;  // 为我们的Point类型创建别名
using System.IO;
using GeoAnalyzer.WinForm.Services;
using GeoAnalyzer.WinForm.Controls;
using GeoAnalyzer.WinForm.Forms;

namespace GeoAnalyzer.WinForm.Forms
{
    public partial class MainForm : Form
    {
        private DataManager _dataManager;
        private MapControl _mapControl;
        private LayerTreeView _layerTreeView;
        private LayerServices _layerServices;
        private SpatialAutocorrelationService spatialAutocorrelationService;
        private GeographicallyWeightedRegressionService gwrService;

        public MainForm()
        {
            InitializeComponent();
            
            InitializeComponents();
            InitializeIO();
            BindEventHandlers();

            // 确保控件正确初始化
            _mapControl.Initialize(_layerServices);
            _layerTreeView.Initialize(_layerServices);

            // 绑定图层变化事件
            _layerServices.LayerChanged += (s, e) => 
            {
                _mapControl.RefreshMap();
                _layerTreeView.RefreshView(_layerServices.GetAllLayers());
            };

            // 初始化空间自相关服务
            spatialAutocorrelationService = new SpatialAutocorrelationService();

            // 初始化 GWR 服务
            gwrService = new GeographicallyWeightedRegressionService();
        }

        private void InitializeComponents()
        {
            // 确保在使用控件之前已经初始化
            if (leftPanel == null || rightPanel == null)
            {
                throw new InvalidOperationException("面板未正确初始化");
            }

            // 初始化我们的自定义控件
            _dataManager = new DataManager();
            _mapControl = new MapControl();
            _layerTreeView = new LayerTreeView();

            // 创建并初始化 LayerServices
            _layerServices = new LayerServices(_dataManager, _mapControl, _layerTreeView);
            
            // 将控件添加到面板
            leftPanel.Controls.Add(_layerTreeView);
            rightPanel.Controls.Add(_mapControl);

            // 设置控件的停靠方式
            _layerTreeView.Dock = DockStyle.Fill;
            _mapControl.Dock = DockStyle.Fill;
        }

        private void InitializeIO()
        {
            // 初始化数据读取和写入对象
            // 例如：dataReader = new ShapefileReader();
            // dataWriter = new ShapefileWriter();
            // features = new List<Feature>();
        }

        private void BindEventHandlers()
        {
            Console.WriteLine("开始绑定事件处理程序...");

            // 文件菜单事件绑定
            try 
            {
                Console.WriteLine($"exitMenuItem: {(exitMenuItem == null ? "null" : "不为null")}");
                if (exitMenuItem != null)
                    exitMenuItem.Click += OnExit;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"文件菜单事件绑定出错: {ex.Message}");
            }

            // 视图菜单事件绑定
            try
            {
                Console.WriteLine($"layerManagerMenuItem: {(layerManagerMenuItem == null ? "null" : "不为null")}");
                if (layerManagerMenuItem != null)
                    layerManagerMenuItem.Click += OnShowLayerManager;

                Console.WriteLine($"attributeTableMenuItem: {(attributeTableMenuItem == null ? "null" : "不为null")}");
                if (attributeTableMenuItem != null)
                    attributeTableMenuItem.Click += OnShowAttributeTable;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"视图菜单事件绑定出错: {ex.Message}");
            }

            // 数据菜单事件绑定
            try
            {
                Console.WriteLine($"importShapefileMenuItem: {(importShapefileMenuItem == null ? "null" : "不为null")}");
                if (importShapefileMenuItem != null)
                    importShapefileMenuItem.Click += OnImportShapefile;

                Console.WriteLine($"exportShapefileMenuItem: {(exportShapefileMenuItem == null ? "null" : "不为null")}");
                if (exportShapefileMenuItem != null)
                    exportShapefileMenuItem.Click += OnExportShapefile;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据菜单事件绑定出错: {ex.Message}");
            }

            // 空间分析菜单事件绑定
            try
            {
                Console.WriteLine($"spatialClusteringMenuItem: {(spatialClusteringMenuItem == null ? "null" : "不为null")}");
                if (spatialClusteringMenuItem != null)
                    spatialClusteringMenuItem.Click += OnSpatialClustering;

                Console.WriteLine($"spatialAutocorrelationMenuItem: {(spatialAutocorrelationMenuItem == null ? "null" : "不为null")}");
                if (spatialAutocorrelationMenuItem != null)
                    spatialAutocorrelationMenuItem.Click += OnSpatialAutocorrelation;

                Console.WriteLine($"gwrMenuItem: {(gwrMenuItem == null ? "null" : "不为null")}");
                if (gwrMenuItem != null)
                    gwrMenuItem.Click += OnGeographicallyWeightedRegression;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"空间分析菜单事件绑定出错: {ex.Message}");
            }

            Console.WriteLine("事件处理程序绑定完成");
        }

        private void OnExit(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要退出程序吗？", "确认", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void OnShowLayerManager(object sender, EventArgs e)
        {
            Console.WriteLine("\n=== 显示图层管理器 ===");
            try
            {
                if (_layerServices == null)
                {
                    throw new InvalidOperationException("LayerServices 未初始化");
                }

                var layers = _layerServices.GetAllLayers();
                if (layers == null || layers.Count == 0)
                {
                    MessageBox.Show("当前没有可管理的图层。", "提示", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var dialog = new LayerManagerDialog(layers, _layerServices))
                {
                    Console.WriteLine($"打开图层管理器对话框，当前图层数量: {layers.Count}");
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Console.WriteLine("图层管理操作完成，准备更新地图");
                        UpdateMap();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"显示图层管理器失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                MessageBox.Show($"显示图层管理器失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Console.WriteLine("=== 图层管理器操作结束 ===\n");
        }

        private void OnShowAttributeTable(object sender, EventArgs e)
        {
            var currentLayer = _dataManager.GetCurrentLayer();
            if (currentLayer == null)
            {
                MessageBox.Show("请先选择一个图层。", "提示", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dialog = new AttributeTableDialog(currentLayer);
            dialog.ShowDialog(this);
        }

        private void UpdateMap()
        {
            try
            {
                Console.WriteLine("开始更新地图...");
                
                if (_mapControl == null)
                {
                    throw new InvalidOperationException("MapControl 未初始化");
                }
                
                _mapControl.RefreshMap();
                if (statusLabel != null)
                {
                    statusLabel.Text = "地图更新完成";
                }
                
                Console.WriteLine("地图更新完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新地图时发生错误: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                MessageBox.Show($"更新地图失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnImportShapefile(object sender, EventArgs e)
        {
            Console.WriteLine("开始执行OnImportShapefile方法");
            
            try 
            {
                // 检查服务是否正确初始化
                Console.WriteLine($"_layerServices: {(_layerServices == null ? "null" : "已初始化")}");
                Console.WriteLine($"_mapControl: {(_mapControl == null ? "null" : "已初始化")}");
                Console.WriteLine($"statusLabel: {(statusLabel == null ? "null" : "已初始化")}");

                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    Console.WriteLine("创建OpenFileDialog");
                    openFileDialog.Filter = "Shapefile (*.shp)|*.shp";
                    openFileDialog.Title = "选择Shapefile文件";
                    
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = openFileDialog.FileName;
                        Console.WriteLine($"选择的文件路径: {filePath}");
                        string layerName = Path.GetFileNameWithoutExtension(filePath);
                        Console.WriteLine($"图层名称: {layerName}");
                        
                        Console.WriteLine("开始创建ShapefileReader");
                        var reader = new ShapefileReader();
                        
                        Console.WriteLine("开始读取Shapefile");
                        var features = reader.ReadAll(filePath);
                        Console.WriteLine($"读取到的要素数量: {features?.Count ?? 0}");
                        
                        if (features == null)
                        {
                            throw new InvalidOperationException("读取的要素集合为null");
                        }
                        
                        Console.WriteLine("开添加图层");
                        _layerServices.AddLayers(features, layerName);
                        
                        Console.WriteLine("开始刷新地图");
                        _mapControl.RefreshMap();
                        
                        Console.WriteLine("更新状态标签");
                        if (statusLabel != null)
                        {
                            statusLabel.Text = $"成功导入Shapefile: {filePath}";
                        }
                        else
                        {
                            Console.WriteLine("警告: statusLabel为null");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生异常: {ex.GetType().Name}");
                Console.WriteLine($"异常消息: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                
                MessageBox.Show(
                    $"导入Shapefile失败\n\n异常类型: {ex.GetType().Name}\n" +
                    $"异常消息: {ex.Message}\n\n" +
                    $"详细信息已写入控制台日志",
                    "错误", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }

        private void OnExportShapefile(object sender, EventArgs e)
        {
            if (_dataManager.GetCurrentLayer() == null)
            {
                MessageBox.Show("先选导出的图层。", "提示", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Shapefile (*.shp)|*.shp";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var writer = new ShapefileWriter();
                        writer.Write(saveFileDialog.FileName, _dataManager.GetCurrentLayer());
                        statusLabel.Text = $"成功导出Shapefile: {saveFileDialog.FileName}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"导出Shapefile失败: {ex.Message}", "错误", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // 示例：添加新图层的方法
        private void AddNewLayer(Feature feature, string name)
        {
            _layerServices.AddLayer(feature, name);
        }

        private void OnSpatialClustering(object sender, EventArgs e)
        {
            Console.WriteLine("\n=== 开始空间聚类分析 ===");
            try
            {
                var currentLayer = _layerServices?.GetCurrentLayer();
                if (currentLayer == null)
                {
                    MessageBox.Show("请先选择要分析的图层。", "提示", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Console.WriteLine($"当前选中图层: {currentLayer.Name}");
                using (var dialog = new SpatialClusteringDialog(currentLayer))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Console.WriteLine($"选择的聚类数量: {dialog.ClusterCount}");
                        Console.WriteLine($"选择的属性: {string.Join(", ", dialog.SelectedAttributes)}");

                        // 让用户选择保存路径
                        string csvPath = null;
                        using (SaveFileDialog saveDialog = new SaveFileDialog())
                        {
                            saveDialog.Filter = "CSV文件 (*.csv)|*.csv";
                            saveDialog.Title = "选择聚类结果保存位置";
                            saveDialog.FileName = $"聚类分析结果_{currentLayer.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                            if (saveDialog.ShowDialog() == DialogResult.OK)
                            {
                                csvPath = saveDialog.FileName;
                            }
                        }

                        var clusteringService = new ClusteringService();
                        var (resultLayer, outputPath) = clusteringService.PerformClustering(
                            currentLayer, 
                            dialog.SelectedAttributes, 
                            dialog.ClusterCount,
                            csvPath);

                        if (resultLayer != null)
                        {
                            _layerServices.AddLayer(resultLayer, resultLayer.Name);
                            Console.WriteLine("聚类结果图层已添加");

                            var message = "聚类分析完成。";
                            if (!string.IsNullOrEmpty(outputPath))
                            {
                                message += $"\n\n结果已导出到：\n{outputPath}";
                            }

                            MessageBox.Show(
                                message,
                                "成功", 
                                MessageBoxButtons.OK, 
                                MessageBoxIcon.Information);

                            UpdateMap();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"空间聚类分析失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                MessageBox.Show($"空间聚类分析失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Console.WriteLine("=== 空间聚类分析结束 ===\n");
        }

        private void OnSpatialAutocorrelation(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("\n=== 开始空间自相关分析 ===");
                
                var currentLayer = _layerServices?.GetCurrentLayer();
                Console.WriteLine($"当前图层: {currentLayer?.Name ?? "null"}");
                
                if (currentLayer == null)
                {
                    MessageBox.Show("请先选择要分析的图层。", "提示", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var dialog = new SpatialAutocorrelationDialog(currentLayer))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string selectedAttribute = dialog.SelectedAttribute;
                        if (string.IsNullOrEmpty(selectedAttribute))
                        {
                            MessageBox.Show("请选择要分析的属性。", "提示",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        // 选择保存位置
                        using (var saveDialog = new SaveFileDialog
                        {
                            Filter = "CSV文件|*.csv",
                            Title = "选择结果保存位置",
                            DefaultExt = "csv",
                            FileName = $"空间自相关分析结果_{currentLayer.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                        })
                        {
                            if (saveDialog.ShowDialog() != DialogResult.OK)
                            {
                                return;
                            }

                            var parameters = new SpatialAutocorrelationService.AnalysisParameters
                            {
                                WeightMethod = dialog.WeightMethod,
                                KNeighbors = dialog.KNeighbors,
                                DistanceThreshold = dialog.DistanceThreshold,
                                OutputPath = saveDialog.FileName
                            };

                            var results = spatialAutocorrelationService.PerformAnalysis(
                                currentLayer,
                                selectedAttribute,
                                parameters
                            );

                            var resultDialog = new SpatialAutocorrelationResultDialog(
                                results.GlobalMoranI,
                                results.ZScore,
                                results.PValue,
                                results.LocalMoranI,
                                selectedAttribute,
                                results.CsvPath
                            );
                            resultDialog.ShowDialog();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"空间自相关分析失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                MessageBox.Show($"空间自相关分析失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnGeographicallyWeightedRegression(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("\n=== 开始地理加权回归分析 ===");
                
                var currentLayer = _layerServices?.GetCurrentLayer();
                Console.WriteLine($"当前图层: {currentLayer?.Name ?? "null"}");
                
                if (currentLayer == null)
                {
                    MessageBox.Show("请先选择要分析的图层。", "提示", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var dialog = new SpatialRegressionDialog(currentLayer))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string dependentVar = dialog.DependentVar;
                        string[] independentVars = dialog.IndependentVars;

                        if (string.IsNullOrEmpty(dependentVar))
                        {
                            MessageBox.Show("请选择因变量。", "提示",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        if (independentVars == null || independentVars.Length == 0)
                        {
                            MessageBox.Show("请至少选择一个自变量。", "提示",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        using (var paramDialog = new GWRParametersDialog(currentLayer))
                        {
                            if (paramDialog.ShowDialog() == DialogResult.OK)
                            {
                                try
                                {
                                    Console.WriteLine("\n=== 执行GWR分析 ===");
                                    Console.WriteLine($"选择的参数:");
                                    Console.WriteLine($"- 带宽: {paramDialog.Bandwidth:F4}");
                                    Console.WriteLine($"- 核函数: {paramDialog.KernelFunction}");
                                    Console.WriteLine($"- 输出路径: {paramDialog.OutputPath}");

                                    var parameters = new GeographicallyWeightedRegressionService.GWRParameters
                                    {
                                        Bandwidth = paramDialog.Bandwidth,
                                        KernelFunction = paramDialog.KernelFunction,
                                        OutputPath = paramDialog.OutputPath
                                    };

                                    var results = gwrService.PerformAnalysis(
                                        currentLayer,
                                        dependentVar,
                                        independentVars,
                                        parameters
                                    );

                                    Console.WriteLine("\n分析结果:");
                                    Console.WriteLine($"- 全局R²: {results.GlobalR2:F4}");
                                    Console.WriteLine($"- 局部R²范围: {results.LocalR2.Min():F4} - {results.LocalR2.Max():F4}");
                                    Console.WriteLine($"- 系数数量: {results.Coefficients.Length}");
                                    Console.WriteLine($"- 每组系数维度: {results.Coefficients[0].Length}");

                                    var resultDialog = new GWRResultDialog(
                                        results.GlobalR2,
                                        results.LocalR2,
                                        results.Coefficients,
                                        results.Residuals,
                                        new[] { "常数项" }.Concat(independentVars).ToArray(),
                                        results.CsvPath
                                    );
                                    resultDialog.ShowDialog();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"执行GWR分析时出错: {ex.Message}");
                                    Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                                    throw;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"地理加权回归分析失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                MessageBox.Show($"地理加权回归分析失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
