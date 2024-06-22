using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.Web.Enums
{

    public enum WebControlElementType
    {
        None,
        // Input elements
        InputText,
        InputPassword,
        InputCheckbox,
        InputRadio,
        InputSubmit,
        InputButton,
        InputFile,
        InputHidden,
        InputEmail,
        InputNumber,
        InputDate,
        InputRange,
        InputSearch,
        InputTel,
        InputUrl,
        InputColor,

        // Button elements
        Button,

        // Select elements
        Select,
        Option,

        // TextArea elements
        TextArea,

        // Label elements
        Label,

        // Fieldset elements
        Fieldset,
        Legend,

        // Form elements
        Form,

        // Output elements
        Output,

        // Datalist elements
        Datalist,

        // Keygen elements
        Keygen,

        // Progress elements
        Progress,

        // Meter elements
        Meter
    }

}
