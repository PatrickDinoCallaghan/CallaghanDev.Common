using DocumentFormat.OpenXml.Spreadsheet;
using System;

namespace CallaghanDev.XML.Excel
{
    internal class XLCFNotBlankConverter : IXLCFConverter
    {
        public ConditionalFormattingRule Convert(IXLConditionalFormat cf, int priority, XLWorkbook.SaveContext context)
        {
            var conditionalFormattingRule = XLCFBaseConverter.Convert(cf, priority);
            var cfStyle = ((XLStyle)cf.Style).Value;
            if (!cfStyle.Equals(XLWorkbook.DefaultStyleValue))
                conditionalFormattingRule.FormatId = (UInt32)context.DifferentialFormats[cfStyle];

            var formula = new Formula { Text = "LEN(TRIM(" + cf.Range.RangeAddress.FirstAddress.ToStringRelative(false) + "))>0" };

            conditionalFormattingRule.Append(formula);

            return conditionalFormattingRule;
        }
    }
}
