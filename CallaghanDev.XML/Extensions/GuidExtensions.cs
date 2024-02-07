#nullable disable

// Keep this file CodeMaid organised and cleaned
using System;

namespace CallaghanDev.XML.Extensions
{
    internal static class GuidExtensions
    {
        internal static String WrapInBraces(this Guid guid)
        {
            return string.Concat('{', guid.ToString(), '}');
        }
    }
}
