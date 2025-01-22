using System;
using System.Reflection;
using System.Text;

public static class ObjectExtensions
{
    public static string ToDetailedString(this object obj, HashSet<object> visited = null)
    {
        if (obj == null)
        {
            return "null";
        }

        if (visited == null)
        {
            visited = new HashSet<object>();
        }

        // Avoid processing the same object multiple times
        if (visited.Contains(obj))
        {
            return "<Circular Reference Detected>";
        }

        visited.Add(obj);

        var type = obj.GetType();
        var sb = new StringBuilder();
        sb.AppendLine($"{type.Name}:");

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Skip indexed properties
            if (prop.GetIndexParameters().Length > 0)
            {
                sb.AppendLine($"  {prop.Name}: <Indexed Property - Skipped>");
                continue;
            }

            var value = prop.GetValue(obj);
            string valueStr;

            if (value == null)
            {
                valueStr = "null";
            }
            else if (!prop.PropertyType.IsPrimitive && prop.PropertyType != typeof(string) && prop.PropertyType != typeof(DateTime))
            {
                // Recursive call for nested objects, passing the visited set
                valueStr = value.ToDetailedString(visited);
            }
            else
            {
                valueStr = value.ToString();
            }

            sb.AppendLine($"  {prop.Name}: {valueStr}");
        }

        return sb.ToString();
    }
}