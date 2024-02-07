#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace CallaghanDev.XML.Excel
{
    internal interface IXLCFConverterExtension
    {
        ConditionalFormattingRule Convert(IXLConditionalFormat cf, XLWorkbook.SaveContext context);
    }
}
