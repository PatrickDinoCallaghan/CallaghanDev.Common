using System;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CallaghanDev.XML.Excel
{
    internal interface IXLCFConverter
    {
        ConditionalFormattingRule Convert(IXLConditionalFormat cf, Int32 priority, XLWorkbook.SaveContext context);
    }
}
