public class DBFField
{
    public string Name { get; set; }
    public char Type { get; set; }  // C=字符型, N=数值型, D=日期型, L=逻辑型
    public int Length { get; set; }
    public int DecimalCount { get; set; }

    public DBFField(string name, char type, int length, int decimalCount = 0)
    {
        Name = name;
        Type = type;
        Length = length;
        DecimalCount = decimalCount;
    }
} 