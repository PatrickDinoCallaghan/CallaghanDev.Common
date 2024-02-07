#nullable disable

using System;

namespace CallaghanDev.XML.Excel
{
    public interface IXLNumberFormat : IXLNumberFormatBase, IEquatable<IXLNumberFormatBase>
    {
        IXLStyle SetNumberFormatId(Int32 value);

        IXLStyle SetFormat(String value);
    }
}
