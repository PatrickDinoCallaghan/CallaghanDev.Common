using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.Code
{

    /// <summary>
    /// The DeepComparer class provides a method AreObjectsEqual for deep comparison of two objects. 
    /// It first checks for reference equality and null values, followed by type comparison to ensure the 
    /// objects are of the same type. The method handles comparisons of dictionaries and enumerables (like
    /// lists or arrays) specially, employing respective helper methods CompareDictionaries and CompareEnumerables.
    /// For other types, it iterates over all properties (both public and non-public) and checks if primitive
    /// properties or strings are equal. For complex properties, it recursively calls AreObjectsEqual. 
    /// Differences are reported to the console, indicating mismatches in property values, enumerable lengths,
    /// or dictionary contents. The method returns true if all compared properties are equal, otherwise false.
    /// </summary>
    public class DeepComparer
    {
        public bool AreObjectsEqual(object obj1, object obj2)
        {
            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }

            if (obj1 == null || obj2 == null)
            {
                return false;
            }

            if (obj1.GetType() != obj2.GetType())
            {
                System.Console.WriteLine($"Type mismatch: {obj1.GetType()} vs {obj2.GetType()}");
                return false;
            }

            if (obj1 is IDictionary dictionaryObj1 && obj2 is IDictionary dictionaryObj2)
            {
                return CompareDictionaries(dictionaryObj1, dictionaryObj2);
            }

            if (obj1 is IEnumerable enumerableObj1 && obj2 is IEnumerable enumerableObj2)
            {
                return CompareEnumerables(enumerableObj1, enumerableObj2);
            }
            var properties = obj1.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(string))
                {
                    object value1 = property.GetValue(obj1);
                    object value2 = property.GetValue(obj2);

                    if (!Equals(value1, value2))
                    {
                        System.Console.WriteLine($"Property mismatch in {property.Name}: {value1} vs {value2}");
                        return false;
                    }
                }
                else
                {
                    object nestedObj1 = property.GetValue(obj1);
                    object nestedObj2 = property.GetValue(obj2);

                    if (!AreObjectsEqual(nestedObj1, nestedObj2))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool CompareEnumerables(IEnumerable enum1, IEnumerable enum2)
        {
            var enum1Enumerator = enum1.GetEnumerator();
            var enum2Enumerator = enum2.GetEnumerator();

            while (enum1Enumerator.MoveNext() && enum2Enumerator.MoveNext())
            {
                if (!AreObjectsEqual(enum1Enumerator.Current, enum2Enumerator.Current))
                {
                    return false;
                }
            }

            if (enum1Enumerator.MoveNext() || enum2Enumerator.MoveNext())
            {
                System.Console.WriteLine("Enumerable length mismatch.");
                return false;
            }

            return true;
        }

        private bool CompareDictionaries(IDictionary dict1, IDictionary dict2)
        {
            if (dict1.Count != dict2.Count)
            {
                System.Console.WriteLine("Dictionary count mismatch.");
                return false;
            }

            foreach (DictionaryEntry entry in dict1)
            {
                if (!dict2.Contains(entry.Key))
                {
                    System.Console.WriteLine($"Dictionary key missing: {entry.Key}");
                    return false;
                }

                var value1 = entry.Value;
                var value2 = dict2[entry.Key];

                if (!AreObjectsEqual(value1, value2))
                {
                    System.Console.WriteLine($"Dictionary value mismatch for key {entry.Key}: {value1} vs {value2}");
                    return false;
                }
            }

            return true;
        }
    }
}
