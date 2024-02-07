#nullable disable

// Keep this file CodeMaid organised and cleaned
using DocumentFormat.OpenXml;
using System;

namespace CallaghanDev.XML.Excel
{
    internal static class DoubleValueExtensions
    {
        public static DoubleValue SaveRound(this DoubleValue value)
        {
            return value.HasValue ? new DoubleValue(Math.Round(value, 6)) : value;
        }
    }
}
