using System;
using System.Collections.Generic;
using GeoAnalyzer.Core.IO.Shapefile;

namespace GeoAnalyzer.Core.IO.Common
{
    public class DataWriterFactory
    {
        private static Dictionary<string, IDataWriter> writers = new Dictionary<string, IDataWriter>
        {
            { ".shp", new ShapefileWriter() }
        };

        public static IDataWriter GetWriter(string filePath)
        {
            string extension = System.IO.Path.GetExtension(filePath).ToLower();
            if (writers.TryGetValue(extension, out IDataWriter writer))
            {
                return writer;
            }
            throw new NotSupportedException($"不支持的文件格式: {extension}");
        }
    }
}
