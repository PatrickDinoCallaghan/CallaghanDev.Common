using CallaghanDev.Utilities.MathTools;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
            reader.Read(); // StartArray
            while (reader.TokenType != JsonToken.EndArray)
            {
                var matrix = _matrixConverter.ReadJson(reader, typeof(Matrix<T>), null, serializer);
                list.Add((Matrix<T>)matrix);
                reader.Read(); // Next token
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

            var dataField = typeof(Matrix<T>).GetField("Data", BindingFlags.NonPublic | BindingFlags.Instance);
            var data = (ConcurrentDictionary<MatrixKey, T>)dataField.GetValue(matrix);


            var tempDict = new Dictionary<string, T>();
            foreach (var kvp in data)
            {
                tempDict[$"{kvp.Key.Row},{kvp.Key.Column}"] = kvp.Value;
            }

            serializer.Serialize(writer, tempDict);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var tempDict = serializer.Deserialize<Dictionary<string, T>>(reader);
            var matrix = new Matrix<T>();

            foreach (var kvp in tempDict)
            {
                var keys = kvp.Key.Split(',');
                var row = int.Parse(keys[0]);
                var column = int.Parse(keys[1]);
                matrix[row, column] = kvp.Value;
            }

            return matrix;
        }
    }
}
