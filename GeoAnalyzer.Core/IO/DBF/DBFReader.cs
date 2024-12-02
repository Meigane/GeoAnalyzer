using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GeoAnalyzer.Core.IO.DBF
{
    public class DBFReader : IDisposable
    {
        private BinaryReader _reader;
        private List<DBFField> _fields;
        private int _recordCount;
        private int _recordLength;

        public DBFReader(string filePath)
        {
            _reader = new BinaryReader(File.OpenRead(filePath));
            ReadHeader();
        }

        private void ReadHeader()
        {
            try
            {
                // 读取文件类型
                byte fileType = _reader.ReadByte();
                if (fileType != 0x03) // 标准dBASE III文件
                {
                    throw new Exception("不支持的DBF文件类型");
                }

                // 读取最后更新日期
                byte year = _reader.ReadByte();
                byte month = _reader.ReadByte();
                byte day = _reader.ReadByte();

                // 读取记录数
                _recordCount = _reader.ReadInt32();
                Console.WriteLine($"DBF记录数: {_recordCount}");

                // 读取头部长度和记录长度
                short headerLength = _reader.ReadInt16();
                _recordLength = _reader.ReadInt16();
                Console.WriteLine($"DBF头部长度: {headerLength}, 记录长度: {_recordLength}");

                // 跳过保留字节
                _reader.BaseStream.Seek(20, SeekOrigin.Current);

                _fields = new List<DBFField>();
                
                // 读取字段描述符
                int fieldCount = (headerLength - 33) / 32;  // 32是每个字段描述符的长度
                Console.WriteLine($"DBF字段数: {fieldCount}");

                for (int i = 0; i < fieldCount; i++)
                {
                    try
                    {
                        byte[] nameBytes = _reader.ReadBytes(11);
                        string name = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0');
                        char type = (char)_reader.ReadByte();
                        _reader.BaseStream.Seek(4, SeekOrigin.Current);  // 跳过字段地址
                        int length = _reader.ReadByte();
                        int decimalCount = _reader.ReadByte();
                        _reader.BaseStream.Seek(14, SeekOrigin.Current);  // 跳过保留字节

                        _fields.Add(new DBFField(name, type, length, decimalCount));
                        Console.WriteLine($"读取字段: {name}, 类型: {type}, 长度: {length}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"读取字段描述符失败: {ex.Message}");
                        throw;
                    }
                }

                // 检查结束标记
                byte terminator = _reader.ReadByte();
                if (terminator != 0x0D)
                {
                    Console.WriteLine("警告: DBF头部结束标记不正确");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取DBF头部失败: {ex.Message}");
                throw;
            }
        }

        public Dictionary<string, object> ReadRecord()
        {
            var record = new Dictionary<string, object>();
            byte recordMarker = _reader.ReadByte();
            
            if (recordMarker == 0x1A) // EOF标记
                return null;

            foreach (var field in _fields)
            {
                byte[] valueBytes = _reader.ReadBytes(field.Length);
                string valueStr = Encoding.ASCII.GetString(valueBytes).Trim();
                
                object value = ConvertValue(valueStr, field.Type);
                record[field.Name] = value;
            }

            return record;
        }

        private object ConvertValue(string value, char type)
        {
            switch (type)
            {
                case 'N':
                    return double.TryParse(value, out double numValue) ? numValue : 0.0;
                case 'D':
                    return DateTime.TryParseExact(value, "yyyyMMdd", null, 
                        System.Globalization.DateTimeStyles.None, out DateTime dateValue) 
                        ? dateValue : (DateTime?)null;
                case 'L':
                    return "YyTt".Contains(value) ? true : false;
                default:
                    return value;
            }
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
} 