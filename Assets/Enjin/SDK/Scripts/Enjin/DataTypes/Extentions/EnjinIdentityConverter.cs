using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EnjinSDK
{
    public class EnjinIdentityConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else
            {
                JArray identityArray = JArray.Load(reader);
                List<EnjinIdentity> identities = new List<EnjinIdentity>();
                if (identityArray.Count > 0 && ((JObject)identityArray[0])["ethereum_address"] != null)
                {
                    for (int i = 0; i < identityArray.Count; i++)
                    {
                        JObject identityObject = (JObject)identityArray[i];
                        JsonSerializerSettings deserializerSettings = new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        };
                        EnjinIdentityData identityData = JsonConvert.DeserializeObject<EnjinIdentityData>(
                            identityObject.ToString(), deserializerSettings);
                        identities.Add(new EnjinIdentity(identityData));
                    }
                    return identities;
                }
                else
                {
                    return identities;
                }
            }
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
            throw new NotImplementedException("Not implemented yet");
        }
    }
}
