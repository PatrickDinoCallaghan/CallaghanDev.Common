#nullable disable

// Keep this file CodeMaid organised and cleaned
using DocumentFormat.OpenXml.Packaging;
using System;
using System.Linq;

namespace CallaghanDev.XML.Extensions
{
    internal static class OpenXmlPartContainerExtensions
    {
        public static Boolean HasPartWithId(this OpenXmlPartContainer container, String relId)
        {
            return container.Parts.Any(p => p.RelationshipId.Equals(relId));
        }
    }
}
