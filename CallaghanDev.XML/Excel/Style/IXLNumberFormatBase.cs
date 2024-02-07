#nullable disable

using System;

namespace CallaghanDev.XML.Excel
{
    public interface IXLNumberFormatBase
    {
        Int32 NumberFormatId { get; set; }

        String Format { get; set; }
    }
}
