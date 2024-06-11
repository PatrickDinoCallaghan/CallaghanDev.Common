using DocumentFormat.OpenXml.Drawing;
using Newtonsoft.Json;
using System.ComponentModel;


namespace CallaghanDev.Utilities.MathTools
{
    [TypeConverter(typeof(MatrixKeyTypeConverter))]
    public struct MatrixKey : IEquatable<MatrixKey>
    {
        [JsonProperty]
        public int Row;
        [JsonProperty]
        public int Column;

        public MatrixKey(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public override bool Equals(object? obj)
        {
            return obj is MatrixKey other && Equals(other);
        }

        public bool Equals(MatrixKey other)
        {
            return Row == other.Row && Column == other.Column;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Row, Column);
        }
    }

    public class MatrixKeyTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string str)
            {
                var parts = str.Split(',');
                return new MatrixKey(int.Parse(parts[0]), int.Parse(parts[1]));
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is MatrixKey matrixKey)
            {
                return $"{matrixKey.Row},{matrixKey.Column}";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

}
