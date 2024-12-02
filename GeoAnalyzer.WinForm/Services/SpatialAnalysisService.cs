using GeoAnalyzer.Core.Models.Geometry;
using GeoAnalyzer.Core.SpatialAnalysis;
using System;
using System.Windows.Forms;

namespace GeoAnalyzer.WinForm.Services
{
    public class SpatialAnalysisService
    {
        private readonly LayerServices _layerServices;
        private readonly ClusteringService _clusteringService;

        public SpatialAnalysisService(LayerServices layerServices)
        {
            _layerServices = layerServices;
            _clusteringService = new ClusteringService();
        }

        public void PerformClustering(Feature layer, int clusterCount, string[] attributes)
        {
            try
            {
                // 执行聚类分析
                var (resultLayer, csvPath) = _clusteringService.PerformClustering(layer, attributes, clusterCount);
                
                // 添加结果图层到图层服务
                _layerServices.AddLayer(resultLayer, resultLayer.Name);
                
                // 通知用户分析完成
                MessageBox.Show(
                    $"聚类分析完成！\n结果已导出到：{csvPath}",
                    "分析完成",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"聚类分析失败：{ex.Message}",
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        public double CalculateMoranI(Feature layer, string attribute)
        {
            // 实现空间自相关分析
            return 0.0;
        }

        public void PerformGWR(Feature layer, string dependentVar, string[] independentVars, double bandwidth)
        {
            // 实现地理加权回归分析
        }
    }
} 