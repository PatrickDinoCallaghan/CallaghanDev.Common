using System;

namespace CallaghanDev.XML.Excel
{
    internal record XLSortElement(
        Int32 ElementNumber,
        XLSortOrder SortOrder,
        Boolean IgnoreBlanks,
        Boolean MatchCase) : IXLSortElement;
}
