#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CallaghanDev.XML.Excel
{
    public interface IXLDrawingProperties
    {
        XLDrawingAnchor Positioning { get; set; }
        IXLDrawingStyle SetPositioning(XLDrawingAnchor value);

    }
}
