#nullable disable

using System;

namespace CallaghanDev.XML.Excel
{
    public interface IXLPivotValueFormat : IXLNumberFormatBase, IEquatable<IXLNumberFormatBase>
    {
        IXLPivotValue SetNumberFormatId(Int32 value);

        IXLPivotValue SetFormat(String value);
    }
}
