using System;

namespace CallaghanDev.XML.Excel
{
    public interface IXLDrawingWeb
    {
        String? AlternateText { get; set; }
        IXLDrawingStyle SetAlternateText(String? value);
    }
}
