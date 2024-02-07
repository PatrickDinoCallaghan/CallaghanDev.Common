#nullable disable

// Keep this file CodeMaid organised and cleaned
namespace CallaghanDev.XML.Excel
{
    public interface IXLPivotFieldStyleFormats
    {
        IXLPivotValueStyleFormat DataValuesFormat { get; }
        IXLPivotStyleFormat Header { get; }
        IXLPivotStyleFormat Label { get; }
        IXLPivotStyleFormat Subtotal { get; }
    }
}
