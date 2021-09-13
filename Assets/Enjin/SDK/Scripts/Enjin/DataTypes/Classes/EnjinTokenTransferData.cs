namespace EnjinSDK
{
    public class EnjinTokenTransferData
    {
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string ItemId { get; set; }
        public string ItemIndex { get; set; }
        public string Amount { get; set; }

        public EnjinTokenTransferData()
        {
            FromAddress = string.Empty;
            ToAddress = string.Empty;
            ItemId = string.Empty;
            ItemIndex = string.Empty;
            Amount = string.Empty;
        }

        public override string ToString()
        {
            if (!ItemIndex.Equals(string.Empty))
            {
                string formatString = "{{from:\"{0}\",to:\"{1}\",token_id:\"{2}\",token_index:\"{3}\",value:\"{4}\"}}";
                return string.Format(formatString, FromAddress, ToAddress, ItemId, ItemIndex, Amount);
            }
            else
            {
                string formatString = "{{from:\"{0}\",to:\"{1}\",token_id:\"{2}\",value:\"{3}\"}}";
                return string.Format(formatString, FromAddress, ToAddress, ItemId, Amount);
            }
        }
    }
}