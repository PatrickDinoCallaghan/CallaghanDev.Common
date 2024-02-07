// Keep this file CodeMaid organised and cleaned
namespace CallaghanDev.XML.Excel
{
    public interface IXLPivotStyleFormat
    {
        XLPivotStyleFormatElement AppliesTo { get; }
        IXLPivotField? PivotField { get; }
        IXLStyle Style { get; set; }
    }
}
