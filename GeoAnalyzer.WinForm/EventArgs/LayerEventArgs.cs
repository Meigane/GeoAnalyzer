using System;
using GeoAnalyzer.Core.Models.Geometry;

namespace GeoAnalyzer.WinForm.LayerEvents
{
    public class LayerEventArgs : EventArgs
    {
        public Feature Layer { get; }
        public bool IsVisible { get; set; }
        public bool IsSelected { get; set; }
        public string Action { get; set; }
        public bool IsLabelLayer => Layer?.Properties?.ContainsKey("LayerType") == true && 
                                   Layer.Properties["LayerType"].ToString() == "Label";

        public LayerEventArgs(Feature layer)
        {
            Layer = layer;
            IsVisible = layer.IsVisible;
            IsSelected = false;
            Action = string.Empty;
        }

        public LayerEventArgs(Feature layer, string action) : this(layer)
        {
            Action = action;
        }
    }
}