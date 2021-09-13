namespace EnjinSDK
{
    public class EnjinRequest
    {
        public int ID { get; set; }
        public string State { get; set; }
        public string Encoded_Data { get; set; }

        public EnjinRequest()
        {
            ID = 0;
            State = string.Empty;
            Encoded_Data = string.Empty;
        }
    }

    public class EnjinRequestResult
    {
        public EnjinRequest Request { get; set; }
    }
}