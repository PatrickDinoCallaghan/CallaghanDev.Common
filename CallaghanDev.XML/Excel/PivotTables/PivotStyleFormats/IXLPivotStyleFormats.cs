#nullable disable

// Keep this file CodeMaid organised and cleaned
using System.Collections.Generic;

namespace CallaghanDev.XML.Excel
{
    public interface IXLPivotStyleFormats : IEnumerable<IXLPivotStyleFormat>
    {
        IXLPivotStyleFormat ForElement(XLPivotStyleFormatElement element);
    }
}
