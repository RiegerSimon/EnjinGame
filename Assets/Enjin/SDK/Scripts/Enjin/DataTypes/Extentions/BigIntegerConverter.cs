using System;
using System.Numerics;
using Newtonsoft.Json;

namespace EnjinSDK
{

    public class BigIntegerConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return BigInteger.Parse((string)reader.Value);
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
