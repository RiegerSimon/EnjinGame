namespace EnjinSDK
{
    using System.Collections.Generic;
    using UnityEngine;

    public enum TransferType { NONE, PER_TRANSFER, PER_CRYPTO_ITEM, RATIO_CUT, RATIO_EXTRA, TYPE_COUNT }
    public enum SupplyModelType { FIXED, SETTABLE, INFINITE, COLLAPSING, ANNUAL_VALUE, ANNUAL_PERCENTAGE }
    public enum TransferableType { PERMANENT, TEMPORARY, BOUND }

    public class TransferFeeSettings
    {
        public TransferType Type { get; set; }
        public string Token_ID { get; set; }
        public string Value { get; set; }

        // TODO: unneeded?
        //public TransferFeeSettings()
        //{
        //    Type = TransferType.NONE;
        //    Token_ID = string.Empty;
        //    Value = string.Empty;
        //}
    }

    public class ItemMetaData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageData { get; set; }
        public string URI { get; set; }
        public Sprite Image { get; set; }

        public ItemMetaData()
        {
            Name = string.Empty;
            Description = string.Empty;
            ImageData = string.Empty;
            URI = string.Empty;
            Image = null;
        }

        public Texture2D GetImageTexture2D() { return Image.texture; }
    }

    /// <summary>
    /// CryptoItem Back data structure
    /// </summary>
    public class CryptoItemBatch
    {
        public List<string> Items { get; private set; }
        public int UserID { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userID"></param>
        public CryptoItemBatch(int userID)
        {
            Items = new List<string>();
            UserID = userID;
        }

        /// <summary>
        /// Adds a fungible item to the transfer list
        /// </summary>
        /// <param name="fromAddress">Address to send item from</param>
        /// <param name="toAddress">Address to send item to</param>
        /// <param name="item">Item to send</param>
        /// <param name="amount">Amount of item to send.</param>
        public void Add(string fromAddress, string toAddress, EnjinCryptoItem item, int amount)
        {
            if (amount <= 0)
                return;

            if (item.NonFungible)
            {
                string tItem = @"from:""{0}"",to:""{1}"",token_id:""{2}"",token_index:""{3}"",value:""1""";
                tItem = string.Format(tItem, fromAddress, toAddress, item.Token_ID, item.Index);
                Items.Add(tItem);
            }
            else
            {
                string tItem = @"from:""{0}"",to:""{1}"",token_id:""{2}"",value:""{3}""";
                tItem = string.Format(tItem, fromAddress, toAddress, item.Token_ID, amount.ToString());
                Items.Add(tItem);
            }
        }

        /// <summary>
        /// Clears the list of items to transfer
        /// </summary>
        public void Clear() { Items.Clear(); }
    }

    /// <summary>
    /// A class for tracking the independent per-app token groupings returned when requesting all items for an address.
    /// </summary>
    public class TokenGroup
    {
        public List<EnjinCryptoItem> Tokens { get; set; }
    }
}