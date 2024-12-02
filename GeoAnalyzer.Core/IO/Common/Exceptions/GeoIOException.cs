using System;

namespace GeoAnalyzer.Core.IO.Common.Exceptions
{
    public class GeoIOException : Exception
    {
        public string FilePath { get; }
        public string Operation { get; }

        public GeoIOException(string message, string filePath, string operation, Exception innerException = null)
            : base(message, innerException)
        {
            FilePath = filePath;
            Operation = operation;
        }
    }
} 