using Newtonsoft.Json;
using System.Numerics;

namespace EnjinSDK
{
    public class EnjinCryptoItem
    {
        // TODO: refactor fields into data class?
        public string Token_ID { get; set; }
        public int App_Id { get; set; } // TODO: unify *_Id naming convention
        // TODO: use JSON.NET to refactor to C#-styled names
        public string Name { get; set; }
        public string Creator { get; set; }
        public string MeltValue { get; set; }
        public int MeltFeeRatio { get; set; }
        public int MeltFeeMaxRatio { get; set; }
        public SupplyModelType SupplyModel { get; set; }

        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger TotalSupply { get; set; }

        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger CirculatingSupply { get; set; }

        [JsonConverter(typeof(BigIntegerConverter))]
        public BigInteger Reserve { get; set; }

        public TransferableType Transferable { get; set; }
        public TransferFeeSettings TransferFeeSettings { get; set; }
        public bool NonFungible { get; set; }
        public int FirstBlock { get; set; }
        public int BlockHeight { get; set; }
        public bool MarkedForDelete { get; set; }
        public bool IsCreator { get; set; }
        public string ItemURI { get; set; }
        public string Icon { get; set; }

        // TODO: figure out what to do about retrieval vs. storage responsibilities
        public float Balance { get; set; }
        public string Index { get; set; }
        public string AvailableToMint { get; set; }
        public ItemMetaData MetaData { get; set; }
    }
}