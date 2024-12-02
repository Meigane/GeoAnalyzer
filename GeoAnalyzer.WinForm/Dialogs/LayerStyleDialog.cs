using System;
using System.Windows.Forms;
using GeoAnalyzer.Core.Models.Geometry;
using GeoAnalyzer.WinForm.LayerEvents;  // 更新命名空间

namespace GeoAnalyzer.WinForm.Dialogs
{
    public class LayerStyleDialog : Form
    {
        private readonly Feature _layer;
        
        public LayerStyleDialog(Feature layer)
        {
            _layer = layer;
            InitializeComponent();
            // ... 其他初始化代码 ...
        }
        
        // ... 其他代码 ...
    }
} 