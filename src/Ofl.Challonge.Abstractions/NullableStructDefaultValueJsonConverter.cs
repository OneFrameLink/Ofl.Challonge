using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Ofl.Challonge
{
    internal class NullableStructDefaultValueJsonConverter<T> : JsonConverter
        where T : struct
    {
        #region Constructor

        public NullableStructDefaultValueJsonConverter() : this(default(T))
        { }

        public NullableStructDefaultValueJsonConverter(T defaultValue)
        {
            // Assign values.
            _defaultValue = defaultValue;
        }

        #endregion

        #region Read-only state.

        private readonly T _defaultValue;

        #endregion

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // If null, return default.
            // Otherwise, get the converter for the type, convert.
            return reader.Value == null ? _defaultValue : TypeDescriptor.GetConverter(reader.ValueType).ConvertTo(reader.Value, typeof(T));

        }

        public override bool CanConvert(Type objectType)
        {
            // Validate parameters.
            if (objectType == null) throw new ArgumentNullException(nameof(objectType));

            // If a number, or nullable number, then return.
            return objectType == typeof(T) || objectType == typeof(T?);
        }
    }
}
