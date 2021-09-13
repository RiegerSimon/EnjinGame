using Newtonsoft.Json;

namespace EnjinSDK
{
    public class EnjinRequestEventData
    {

        [JsonConverter(typeof(IntegerConverter))]
        public int id;
        public string Event;
        public string type;
        public string title;
        public string state;
        public string param1;
        public string param2;
        public string param3;
        public string param4;
    }

    public class EnjinRequestEvent : EnjinEvent
    {
        public EnjinRequestEventData Data { get; set; }
    }
}
