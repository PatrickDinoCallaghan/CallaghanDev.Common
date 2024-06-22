using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.Web.Enums
{
    public enum WebElementActionEnum
    {
        None = 0,
        CheckIfElementDisplayed,
        Click,
        SelectItemInList,
        SelectedItemValue,
        ElementText,
        IsCheckboxChecked,
        SendKeys,
        FillTextBox
    }
}
