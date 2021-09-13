namespace EnjinSDK
{
    public class EnjinIdentityField {
        public int ID { get; set; }
        public string Key { get; set; }
        public int Searchable { get; set; }
        public int Displayable { get; set; }
        public int Unique { get; set; }
        public string Value { get; set; }
        public EnjinDateData Created_At { get; set; }
        public EnjinDateData Updated_At { get; set; }
    }

    public class EnjinIdentityData
    {
        // Properties
        public int ID { get; set; }
        public int App_ID { get; set; }
        public string Ethereum_Address { get; set; }
        public string Linking_Code { get; set; }
        public double Eth_Balance { get; set; }
        public double Enj_Balance { get; set; }
        public double Enj_Allowance { get; set; }
        public EnjinDateData Created_At { get; set; }
        public EnjinDateData Updated_At { get; set; }
        public EnjinIdentityField[] Fields { get; set; } // TODO: make this sync during refresh

        /// <summary>
        /// Constructor
        /// </summary>
        public EnjinIdentityData()
        {
            ID = 0;
            App_ID = 0;
            Ethereum_Address = string.Empty;
            Linking_Code = string.Empty;
            Eth_Balance = 0;
            Enj_Balance = 0;
            Enj_Allowance = 0;
            Created_At = new EnjinDateData();
            Updated_At = new EnjinDateData();
        }
    }
}