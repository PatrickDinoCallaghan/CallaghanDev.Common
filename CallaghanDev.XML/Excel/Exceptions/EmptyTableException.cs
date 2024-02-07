#nullable disable

using System;

namespace CallaghanDev.XML.Excel.Exceptions
{
    public class EmptyTableException : ClosedXMLException
    {
        public EmptyTableException()
            : base()
        { }

        public EmptyTableException(String message)
            : base(message)
        { }

        public EmptyTableException(String message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
