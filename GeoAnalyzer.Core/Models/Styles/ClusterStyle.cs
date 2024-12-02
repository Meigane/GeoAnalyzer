using System;
using System.Drawing;

namespace GeoAnalyzer.Core.Models.Styles
{
    public class ClusterStyle
    {
        private static readonly Color[] DefaultColors = new Color[]
        {
            Color.FromArgb(180, 255, 99, 71),   // 红色
            Color.FromArgb(180, 30, 144, 255),  // 蓝色
            Color.FromArgb(180, 50, 205, 50),   // 绿色
            Color.FromArgb(180, 255, 215, 0),   // 金色
            Color.FromArgb(180, 147, 112, 219), // 紫色
            Color.FromArgb(180, 255, 127, 80),  // 珊瑚色
            Color.FromArgb(180, 100, 149, 237), // 矢车菊蓝
            Color.FromArgb(180, 34, 139, 34)    // 森林绿
        };

        public static Color GetClusterColor(int clusterIndex)
        {
            return DefaultColors[clusterIndex % DefaultColors.Length];
        }
    }
} 