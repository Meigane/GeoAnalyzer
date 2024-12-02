using System.Drawing;

namespace GeoAnalyzer.Core.Models.Styles
{
    public class LayerStyle
    {
        public Color FillColor { get; set; } = Color.White;
        public Color OutlineColor { get; set; } = Color.Black;
        public float OutlineWidth { get; set; } = 1.0f;
        public float PointSize { get; set; } = 5.0f;

        public LayerStyle()
        {
        }

        public LayerStyle Clone()
        {
            return new LayerStyle
            {
                FillColor = this.FillColor,
                OutlineColor = this.OutlineColor,
                OutlineWidth = this.OutlineWidth,
                PointSize = this.PointSize
            };
        }
    }
} 