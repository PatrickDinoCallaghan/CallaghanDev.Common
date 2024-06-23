using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.Web.Enums
{
    public enum LocatorTypeEnum
    {
        None,
        CssSelector,
        XPath,
        ClassName,
        TagName,
        LinkText,
        PartialLinkText,
        Text
    }
}
