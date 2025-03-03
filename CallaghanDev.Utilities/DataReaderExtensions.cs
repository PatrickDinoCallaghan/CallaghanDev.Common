
using System;
using System.Data;


namespace CallaghanDev.Utilities
{
    public static class DataReaderExtensions
    {
        /// <summary>
        /// Retrieves a column value from the data reader and converts it to the specified type.
        /// If the column does not exist, is null, or conversion fails, returns default(T).
        /// </summary>
        /// <typeparam name="T">The expected type of the column value.</typeparam>
        /// <param name="reader">The IDataReader instance.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <returns>The converted value if successful; otherwise, default(T).</returns>
        public static T TryGet<T>(this IDataReader reader, string columnName)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException(nameof(columnName));

            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal))
                    return default(T);

                object rawValue = reader.GetValue(ordinal);

                // If the raw value is already of the desired type, cast directly.
                if (rawValue is T variable)
                {
                    return variable;
                }
                else
                {
                    // Attempt to convert the raw value to type T.
                    return (T)Convert.ChangeType(rawValue, typeof(T));
                }
            }
            catch
            {
                // Either the column doesn't exist or conversion failed.
                return default(T);
            }
        }
    }

}
