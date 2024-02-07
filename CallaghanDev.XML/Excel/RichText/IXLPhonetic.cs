using System;

namespace CallaghanDev.XML.Excel
{
    public interface IXLPhonetic: IEquatable<IXLPhonetic>
    {
        String Text { get; }
        Int32 Start { get; }
        Int32 End { get; }
    }
}
