using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeoAnalyzer.Core.Models.Geometry;

namespace GeoAnalyzer.Core.IO.Common
{
    public interface IDataWriter
    {
        void Write(string filePath, List<Feature> features);
        bool CanWrite(string filePath);
        Task WriteAsync(string filePath, List<Feature> features, IProgress<int> progress = null, CancellationToken cancellationToken = default);
    }
}
