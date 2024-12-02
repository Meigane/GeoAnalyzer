using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
namespace GeoAnalyzer.Core.IO.DBF
{
    public class DBFWriter : IDisposable
    {
        private BinaryWriter _writer;
        private List<DBFField> _fields;
        private int _recordCount;
        public DBFWriter(string filePath, List<DBFField> fields)
        {
            _writer = new BinaryWriter(File.Open(filePath, FileMode.Create));
            _fields = fields;
            _recordCount = 0;
            WriteHeader();
        }
        private void WriteHeader()
        {
            _writer.Write((byte)0x03); // 版本号
            _writer.Write((byte)(DateTime.Now.Year - 1900));
            _writer.Write((byte)DateTime.Now.Month);
            _writer.Write((byte)DateTime.Now.Day);
            _writer.Write(_recordCount); // 记录数，稍后更新
            short headerLength = (short)(32 + (_fields.Count * 32) + 1);
            _writer.Write(headerLength);
            short recordLength = 1; // 1字节用于删除标记
            foreach (var field in _fields)
                recordLength += (short)field.Length;
            _writer.Write(recordLength);
            // 保留字节
            for (int i = 0; i < 20; i++)
                _writer.Write((byte)0);
            // 写入字段描述符
            foreach (var field in _fields)
            {
                byte[] nameBytes = new byte[11];
                Encoding.ASCII.GetBytes(field.Name).CopyTo(nameBytes, 0);
                _writer.Write(nameBytes);
                _writer.Write((byte)field.Type);
                _writer.Write(new byte[4]); // 保留字节
                _writer.Write((byte)field.Length);
                _writer.Write((byte)field.DecimalCount);
                _writer.Write(new byte[14]); // 保留字节
            }
            // 写入头部结束标记
            _writer.Write((byte)0x0D);
        }
        public void WriteRecord(Dictionary<string, object> record)
        {
            _writer.Write((byte)0x20); // 未删除记录标记
            foreach (var field in _fields)
            {
                object fieldValue;
                record.TryGetValue(field.Name, out fieldValue);
                string value = FormatValue(fieldValue, field);
                byte[] valueBytes = new byte[field.Length];
                Encoding.ASCII.GetBytes(value).CopyTo(valueBytes, 0);
                _writer.Write(valueBytes);
            }
            _recordCount++;
        }
        private string FormatValue(object value, DBFField field)
        {
            if (value == null)
                return new string(' ', field.Length);
            switch (field.Type)
            {
                case 'N':
                    return value.ToString().PadLeft(field.Length, ' ');
                case 'D':
                    return value is DateTime date ? date.ToString("yyyyMMdd") : new string(' ', field.Length);
                case 'L':
                    return value is bool b ? (b ? "T" : "F") : "F";
                default:
                    return value.ToString().PadRight(field.Length, ' ').Substring(0, field.Length);
            }
        }
        public void Dispose()
        {
            if (_writer != null)
            {
                // 更新记录数
                _writer.BaseStream.Seek(4, SeekOrigin.Begin);
                _writer.Write(_recordCount);
                // 写入文件结束标记
                _writer.BaseStream.Seek(0, SeekOrigin.End);
                _writer.Write((byte)0x1A);
                _writer.Dispose();
            }
        }
    }
}
