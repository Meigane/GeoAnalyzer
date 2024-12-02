// ViewModels/MainViewModel.cs
using GeoAnalyzer.Core.Data;
using GeoAnalyzer.WinForm.Controls;
using GeoAnalyzer.WinForm.Services;
using System;
using System.Windows.Forms;

namespace GeoAnalyzer.WinForm.ViewModels
{
    public class MainViewModel
    {
        private readonly DataManager _dataManager;
        private readonly MapControl _mapControl;
        private readonly LayerTreeView _layerTreeView;
        private readonly MapServices _mapServices;
        private readonly LayerServices _layerServices;

        public MainViewModel(MapControl mapControl, LayerTreeView layerTreeView)
        {
            try
            {
                // 初始化所有必需的组件
                _dataManager = new DataManager();
                _mapControl = mapControl ?? throw new System.ArgumentNullException(nameof(mapControl));
                _layerTreeView = layerTreeView ?? throw new System.ArgumentNullException(nameof(layerTreeView));

                // 先创建 LayerServices
                _layerServices = new LayerServices(_dataManager, _mapControl, _layerTreeView);

                // 然后创建 MapServices，传入所有必需的参数
                _mapServices = new MapServices(
                    panel: _mapControl.Parent as Panel,
                    dataManager: _dataManager,  // 传入 DataManager
                    layerTreeView: _layerTreeView
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 添加其他必要的方法
        public void LoadLayer(string filePath)
        {
            _mapServices.LoadLayer(filePath);
        }

        public void RefreshMap()
        {
            _mapControl.RefreshMap();
        }
    }
}
