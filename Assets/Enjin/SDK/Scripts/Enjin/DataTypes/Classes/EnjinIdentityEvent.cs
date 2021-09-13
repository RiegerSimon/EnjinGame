using Newtonsoft.Json;

namespace EnjinSDK
{
    public class EnjinIdentityEventData
    {

        [JsonConverter(typeof(IntegerConverter))]
        public int id;
        public string ethereum_address;
        public string linking_code;
    }

    public class EnjinIdentityEvent : EnjinEvent
    {
        public EnjinIdentityEventData Data { get; set; }
    }
}
