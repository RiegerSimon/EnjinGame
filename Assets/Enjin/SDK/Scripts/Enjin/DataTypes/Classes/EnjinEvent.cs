using Newtonsoft.Json;

namespace EnjinSDK
{
    public enum TransactionTypes { NONE, TOKEN, REQUEST, IDENTITY, BALANCE, TRANSACTION }

    public class EnjinEventData
    {

        [JsonConverter(typeof(IntegerConverter))]
        public int Id { get; set; }
        public string TransactionId { get; set; }
        public TransactionTypes TransactionType { get; set; }
        public string RequestType { get; set; }
        public string EventType { get; set; }
        public string Param1 { get; set; }
        public string Param2 { get; set; }
        public string Param3 { get; set; }
        public string Param4 { get; set; }
    }

    public class EnjinEvent
    {
        public string Model { get; set; }
        public string Event_Type { get; set; }
        public EnjinEventData Event_Data { get; set; }
    }
}