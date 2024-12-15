using CallaghanDev.Common.Math;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Reflection;

namespace CallaghanDev.Utilities.Math
{
    public class MatrixArrayJsonConverter<T> : JsonConverter
    {
        private readonly JsonConverter _matrixConverter;

        public MatrixArrayJsonConverter()
        {
            _matrixConverter = new MatrixJsonConverter<T>();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Matrix<T>[]);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var array = (Matrix<T>[])value;
            writer.WriteStartArray();
            foreach (var matrix in array)
            {
                _matrixConverter.WriteJson(writer, matrix, serializer);
            }
            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var list = new List<Matrix<T>>();

            if (reader.TokenType != JsonToken.StartArray)
                throw new JsonSerializationException("Expected StartArray token.");

            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
            {
                var matrix = _matrixConverter.ReadJson(reader, typeof(Matrix<T>), null, serializer);
                list.Add((Matrix<T>)matrix);
            }

            return list.ToArray();
        }
    }

    public class MatrixJsonConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Matrix<T>);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var matrix = (Matrix<T>)value;

            // Access the private 'Data' field
            var dataField = typeof(Matrix<T>).GetField("Data", BindingFlags.NonPublic | BindingFlags.Instance);
            var data = (ConcurrentDictionary<MatrixKey, T>)dataField.GetValue(matrix);

            // Convert matrix data into a dictionary
            var tempDict = new Dictionary<string, object>(); // Use object to store type metadata
            foreach (var kvp in data)
            {
                tempDict[$"{kvp.Key.Row},{kvp.Key.Column}"] = kvp.Value;
            }

            // Serialize with type metadata
            var floatHandlingSettings = new JsonSerializerSettings
            {
                FloatFormatHandling = FloatFormatHandling.String, // Preserve precision
                TypeNameHandling = TypeNameHandling.Objects // Include type information
            };

            var customSerializer = JsonSerializer.Create(floatHandlingSettings);
            customSerializer.Serialize(writer, tempDict);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException("Expected StartObject token.");

            // Deserialize the dictionary with object values
            var tempDict = serializer.Deserialize<Dictionary<string, object>>(reader);

            // Initialize the matrix
            var matrix = new Matrix<T>();

            foreach (var kvp in tempDict)
            {
                var keys = kvp.Key.Split(',');
                if (keys.Length != 2)
                    throw new JsonSerializationException($"Invalid matrix key: {kvp.Key}");

                var row = int.Parse(keys[0]);
                var column = int.Parse(keys[1]);

                // Resolve type metadata if T is an interface or abstract class
                if (kvp.Value is JObject jObject)
                {
                    var concreteValue = jObject.ToObject<T>(serializer);
                    matrix[row, column] = concreteValue;
                }
                else if (typeof(T) == typeof(double))
                {
                    // Handle floating-point precision
                    matrix[row, column] = (T)(object)Convert.ToDouble(kvp.Value);
                }
                else if (typeof(T) == typeof(float))
                {
                    matrix[row, column] = (T)(object)Convert.ToSingle(kvp.Value);
                }
                else
                {
                    // Directly assign other types
                    matrix[row, column] = (T)kvp.Value;
                }
            }

            return matrix;
        }


    }

}
