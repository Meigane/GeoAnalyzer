using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoAnalyzer.Core.Models.Geometry;

namespace GeoAnalyzer.Core.IO.Common
{
    public interface IDataReader
    {
        List<Feature> Read(string filePath);
        bool CanRead(string filePath);
    }
}
