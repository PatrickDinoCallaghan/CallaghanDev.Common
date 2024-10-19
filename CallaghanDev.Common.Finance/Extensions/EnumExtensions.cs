
using System;
using System.ComponentModel;
using System.Reflection;


namespace CallaghanDev.IG.Trade.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            // Get the field information for the enum value
            FieldInfo field = value.GetType().GetField(value.ToString());

            // Get the Description attribute if it exists
            DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();

            // Return the description if it exists, otherwise return the enum name
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }

}
