using System;
using Newtonsoft.Json;

namespace EnjinSDK
{

    public class IntegerConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return (int)Convert.ToInt64((double)reader.Value);
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return false;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException("This method is not supported.");
        }
    }
}
