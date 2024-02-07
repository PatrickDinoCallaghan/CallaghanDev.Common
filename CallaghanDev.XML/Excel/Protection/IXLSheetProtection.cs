#nullable disable

// Keep this file CodeMaid organised and cleaned
using System;
using static CallaghanDev.XML.Excel.XLProtectionAlgorithm;

namespace CallaghanDev.XML.Excel
{
    public interface IXLSheetProtection : IXLElementProtection<XLSheetProtectionElements>
    {
        IXLSheetProtection Protect(XLSheetProtectionElements allowedElements);

        IXLSheetProtection Protect(Algorithm algorithm, XLSheetProtectionElements allowedElements);

        IXLSheetProtection Protect(String password, Algorithm algorithm = DefaultProtectionAlgorithm, XLSheetProtectionElements allowedElements = XLSheetProtectionElements.SelectEverything);
    }
}
