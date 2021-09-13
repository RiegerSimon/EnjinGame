using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CodingHelmet.Optional;
using CodingHelmet.Optional.Extensions;
using Newtonsoft.Json;

namespace EnjinSDK
{
    public class EnjinIdentity : IPublicOperations, IIdentityEventHandler
    {
        public EnjinUser User { get; set; }

        private SemaphoreSlim _dataLock = new SemaphoreSlim(1, 1);
        private EnjinIdentityData _data { get; set; }

        private SemaphoreSlim _inventoryLock = new SemaphoreSlim(1, 1);
        private Dictionary<string, List<EnjinCryptoItem>> _inventory;

        public EnjinIdentity(EnjinIdentityData identityData)
        {
            _data = identityData;
        }

        public EnjinAdminIdentity AsAdmin()
        {
            return new EnjinAdminIdentity(this);
        }

        public int GetId()
        {
            return _data.ID;
        }

        public int GetAppId()
        {
            return _data.App_ID;
        }

        public string GetLinkingCode()
        {
            return _data.Linking_Code;
        }

        public Option<string> GetEthereumAddress()
        {
            string possibleAddress = _data.Ethereum_Address;
            if (possibleAddress.Equals(string.Empty))
            {
                return None.Value;
            }
            else
            {
                return possibleAddress;
            }
        }

        public void Cleanup()
        {
            // TODO: Call clean up methods to unsubscribe from events etc...
            // _pusherController.Shutdown();
        }

        async private Task ReplaceInventoryData()
        {
            if (_inventory == null)
            {
                _inventory = new Dictionary<string, List<EnjinCryptoItem>>();
            }
            else
            {
                _inventory.Clear();
            }

            string formatString = EnjinConnector.GetQuery(GraphQLTemplate.QueryType.GET_ITEM_BALANCES_FOR_IDENTITY_ID);
            string query = string.Format(formatString, _data.ID);
            string results = await EnjinConnector.PostQuery(query, User.AccessToken);
            IEnumerable<TokenGroup> tokenGroups = JsonConvert.DeserializeObject<IEnumerable<TokenGroup>>(results, EnjinConnector.DeserializationSettings);
            foreach (TokenGroup tokenGroup in tokenGroups)
            {
                List<EnjinCryptoItem> itemList = tokenGroup.Tokens;
                foreach (EnjinCryptoItem item in itemList)
                {
                    if (_inventory.ContainsKey(item.Token_ID))
                    {
                        List<EnjinCryptoItem> itemIndices = _inventory[item.Token_ID];
                        itemIndices.Add(item);
                        _inventory[item.Token_ID] = itemIndices;
                    }
                    else
                    {
                        List<EnjinCryptoItem> itemIndices = new List<EnjinCryptoItem>
                        {
                            item
                        };
                        _inventory.Add(item.Token_ID, itemIndices);
                    }
                }
            }
        }

        // Safely prepare this identity's inventory for use in the given callback.
        async private Task PrepareInventory(Action callback, bool refreshCache = true)
        {
            // Lock the identity data and inventory for thread-safe population.
            await _dataLock.WaitAsync();
            try
            {
                await _inventoryLock.WaitAsync();

                // Force refresh.
                if (refreshCache)
                {
                    await ReplaceInventoryData();
                    callback();
                    _inventoryLock.Release();
                }
                else
                {
                    if (_inventory == null)
                    {
                        await ReplaceInventoryData();
                    }
                    callback();
                    _inventoryLock.Release();
                }
            }
            finally
            {
                _dataLock.Release();
            }
        }

        async public Task<Option<List<EnjinCryptoItem>>> GetItem(string itemId, bool refreshCache = true)
        {
            Option<List<EnjinCryptoItem>> outputItemIndices = None.Value;
            await PrepareInventory(() =>
            {
                outputItemIndices = _inventory.TryGetValue(itemId);
            }, refreshCache);
            return outputItemIndices;
        }

        async public Task<Option<EnjinCryptoItem>> GetItem(string itemId, string index, bool refreshCache = true)
        {
            Option<EnjinCryptoItem> outputItem = None.Value;
            await PrepareInventory(() =>
            {
                List<EnjinCryptoItem> outputItemIndices = _inventory.TryGetValue(itemId).Reduce(new List<EnjinCryptoItem>());
                foreach (EnjinCryptoItem itemCandidate in outputItemIndices)
                {
                    if (itemCandidate.Index.Equals(index))
                    {
                        outputItem = itemCandidate;
                        break;
                    }
                }
            }, refreshCache);
            return outputItem;
        }

        // A helper method to return a cloned copy of the Identity inventory.
        private Dictionary<string, List<EnjinCryptoItem>> CloneInventory()
        {
            Dictionary<string, List<EnjinCryptoItem>> inventoryClone = new Dictionary<string, List<EnjinCryptoItem>>();
            foreach (string itemId in _inventory.Keys)
            {
                List<EnjinCryptoItem> itemIndicesClone = new List<EnjinCryptoItem>();
                foreach (EnjinCryptoItem item in _inventory[itemId])
                {
                    itemIndicesClone.Add(item);
                }
                inventoryClone.Add(itemId, itemIndicesClone);
            }
            return inventoryClone;
        }

        async public Task<Dictionary<string, List<EnjinCryptoItem>>> GetAllItems(bool refreshCache = true)
        {
            Dictionary<string, List<EnjinCryptoItem>> inventoryClone = new Dictionary<string, List<EnjinCryptoItem>>();
            await PrepareInventory(() =>
            {
                inventoryClone = CloneInventory();
            }, refreshCache);
            return inventoryClone;
        }

        async public Task<Dictionary<string, List<EnjinCryptoItem>>> GetAllItemsForApp(int appId, bool refreshCache = true)
        {
            Dictionary<string, List<EnjinCryptoItem>> appInventory = new Dictionary<string, List<EnjinCryptoItem>>();
            await PrepareInventory(() =>
            {
                Dictionary<string, List<EnjinCryptoItem>> inventoryClone = CloneInventory();
                foreach (string itemId in _inventory.Keys)
                {
                    List<EnjinCryptoItem> itemIndices = inventoryClone[itemId];
                    if (itemIndices[0].App_Id == appId)
                    {
                        appInventory.Add(itemId, itemIndices);
                    }
                }
            }, refreshCache);
            return appInventory;
        }

        async public Task<Dictionary<int, Dictionary<string, List<EnjinCryptoItem>>>> GetAllItemsByApp(bool refreshCache = true)
        {
            Dictionary<int, Dictionary<string, List<EnjinCryptoItem>>> perAppInventory = new Dictionary<int, Dictionary<string, List<EnjinCryptoItem>>>();
            await PrepareInventory(() =>
            {
                Dictionary<string, List<EnjinCryptoItem>> inventoryClone = CloneInventory();
                foreach (string itemId in inventoryClone.Keys)
                {
                    List<EnjinCryptoItem> itemIndices = inventoryClone[itemId];
                    int appId = itemIndices[0].App_Id;
                    if (perAppInventory.ContainsKey(appId))
                    {
                        Dictionary<string, List<EnjinCryptoItem>> appInventory = perAppInventory[appId];
                        appInventory.Add(itemId, itemIndices);
                        perAppInventory[appId] = appInventory;
                    }
                    else
                    {
                        Dictionary<string, List<EnjinCryptoItem>> appInventory = new Dictionary<string, List<EnjinCryptoItem>>
                        {
                            { itemId, itemIndices }
                        };
                        perAppInventory[appId] = appInventory;
                    }
                }
            }, refreshCache);
            return perAppInventory;
        }

        async public Task<Option<float>> GetBalance(string itemId, bool refreshCache = true)
        {
            Option<float> balance = None.Value;
            await PrepareInventory(() =>
            {
                balance = _inventory.TryGetValue(itemId).Map((items) =>
                {
                    if (items[0].NonFungible)
                    {
                        return items.Count;
                    }
                    else
                    {
                        return items[0].Balance;
                    }
                });
            }, refreshCache);
            return balance;
        }

        public Task<EnjinRequest> SendItem(EnjinCryptoItem item, EnjinUser user)
        {
            throw new System.NotImplementedException();
        }

        public async Task<EnjinRequest> SendItems(CryptoItemBatch sendItems)
        {
            string result;

            await _dataLock.WaitAsync();
            try
            {
                string temp = string.Empty;

                for (int i = 0; i < sendItems.Items.Count; i++)
                {
                    temp += "{" + sendItems.Items[i] + "}";

                    if (i < sendItems.Items.Count - 1)
                        temp += ",";
                }

                string formatString = EnjinConnector.GetQuery(GraphQLTemplate.QueryType.SEND_ITEMS);
                string query = string.Format(formatString, _data.ID, temp);
                result = await EnjinConnector.PostQuery(query, User.AccessToken);
            }
            finally
            {
                _dataLock.Release();
            }

            return JsonConvert.DeserializeObject<EnjinRequest>(result, EnjinConnector.DeserializationSettings);
        }

        public Task<EnjinRequest> MeltItem(EnjinCryptoItem item, int amount)
        {
            throw new System.NotImplementedException();
        }

        async public Task RefreshIdentity()
        {

            // Make sure no one is relying on the data that we're about to mutate.
            await _dataLock.WaitAsync();
            try
            {

                string formatString = EnjinConnector.GetQuery(GraphQLTemplate.QueryType.REFRESH_IDENTITY);
                string query = string.Format(formatString, _data.ID);
                string result = await EnjinConnector.PostQuery(query, User.AccessToken);
                EnjinIdentityData refreshedData =
                    JsonConvert.DeserializeObject<IEnumerable<EnjinIdentityData>>(result, EnjinConnector.DeserializationSettings)
                    .FirstOrNone<EnjinIdentityData>().Reduce(() =>
                    {
                        throw new Exception("Unable to refresh identity " + _data.ID); // TODO: proper exception classes
                    });
                _data = refreshedData;
            }
            finally
            {
                _dataLock.Release();
            }
        }

        async public Task RegisterCallback(IdentityEventType eventType, Action<EnjinIdentityEvent> callback)
        {
            await EnjinConnector.RegisterIdentityCallback(this, eventType, callback);
        }

        // TODO: expose a version of the interface which deals directly in Transfer objects
        public IRequestHandler CreateSendItemRequestHandler(string[] itemIds, string[] ethereumAddresses, int[] amounts)
        {
            if (itemIds == null)
            {
                throw new ArgumentNullException(nameof(itemIds));
            }

            if (ethereumAddresses == null)
            {
                throw new ArgumentNullException(nameof(ethereumAddresses));
            }

            if (amounts == null)
            {
                throw new ArgumentNullException(nameof(amounts));
            }

            int idCount = itemIds.Length;
            if (idCount == 0)
            {
                throw new ArgumentException("The array of item identifiers must not be empty.", nameof(itemIds));
            }

            int addressCount = ethereumAddresses.Length;
            if (addressCount == 0)
            {
                throw new ArgumentException("The array of Ethereum addresses must not be empty.", nameof(ethereumAddresses));
            }

            int amountCount = amounts.Length;
            if (amountCount == 0)
            {
                throw new ArgumentException("The array of item send amounts must not be empty.", nameof(amountCount));
            }

            if (idCount != addressCount || idCount != amountCount)
            {
                throw new ArgumentException("The arrays of item identifiers, Ethereum addresses, and amounts must all be equal in length.");
            }

            // Construct a list of transfers.
            string transferList = "";
            for (int i = 0; i < idCount; i++)
            {
                EnjinTokenTransferData transferEntry = new EnjinTokenTransferData
                {
                    FromAddress = GetEthereumAddress().Reduce(() => { throw new InvalidOperationException("You cannot send items from an identity which has no linked Ethereum address."); }),
                    ToAddress = ethereumAddresses[i],
                    ItemId = itemIds[i],
                    Amount = amounts[i].ToString()
                };
                transferList += transferEntry.ToString();
                if (i != idCount - 1)
                {
                    transferList += ",";
                }
            }

            string formatString = EnjinConnector.GetQuery(GraphQLTemplate.QueryType.SEND_ITEMS);
            string queryString = string.Format(formatString, GetId(), transferList);

            return new EnjinRequestHandler(queryString, User.AccessToken, this);
        }

        public IRequestHandler CreateSendItemRequestHandler(string itemId, string[] ethereumAddresses, int[] amounts)
        {
            throw new NotImplementedException();
        }

        public IRequestHandler CreateSendItemRequestHandler(string itemId, string[] ethereumAddresses, int amount)
        {
            throw new NotImplementedException();
        }

        public IRequestHandler CreateSendItemRequestHandler(string itemId, string ethereumAddress, int amount)
        {
            return CreateSendItemRequestHandler(new string[] { itemId }, new string[] { ethereumAddress }, new int[] { amount });
        }

        // Item trade creation operations and appropriate overloads.
        public IRequestHandler CreateTradeItemRequestHandler(EnjinItemParameter[] offeringItems, EnjinItemParameter[] askingItems, string secondPartyAddress)
        {
            if (offeringItems == null)
            {
                throw new ArgumentNullException(nameof(offeringItems));
            }

            if (askingItems == null)
            {
                throw new ArgumentNullException(nameof(askingItems));
            }

            if (secondPartyAddress == null)
            {
                throw new ArgumentNullException(nameof(secondPartyAddress));
            }

            int offeringCount = offeringItems.Length;
            if (offeringCount == 0)
            {
                throw new ArgumentException("The array of items being offered must not be empty.", nameof(offeringItems));
            }

            int askingCount = askingItems.Length;
            if (askingCount == 0)
            {
                throw new ArgumentException("The array of items being asked for must not be empty.", nameof(askingItems));
            }

            // Construct a list of item offers.
            string offerList = "";
            for (int i = 0; i < offeringCount; i++)
            {
                EnjinItemParameter offeredItem = offeringItems[i];
                offerList += offeredItem.ToString();
                if (i != offeringCount - 1)
                {
                    offerList += ",";
                }
            }

            // Construct a list of item asks.
            string askingList = "";
            for (int i = 0; i < askingCount; i++)
            {
                EnjinItemParameter askedItem = askingItems[i];
                askingList += askedItem.ToString();
                if (i != askingCount - 1)
                {
                    askingList += ",";
                }
            }

            string formatString = EnjinConnector.GetQuery(GraphQLTemplate.QueryType.CREATE_TRADE);
            string queryString = string.Format(formatString, GetId(), askingList, offerList, secondPartyAddress);

            return new EnjinRequestHandler(queryString, User.AccessToken, this);
        }

        public IRequestHandler CompleteTradeItemRequestHandler(string tradeId)
        {
            if (tradeId == null)
            {
                throw new ArgumentNullException(nameof(tradeId));
            }

            string formatString = EnjinConnector.GetQuery(GraphQLTemplate.QueryType.COMPLETE_TRADE);
            string queryString = string.Format(formatString, GetId(), tradeId);

            return new EnjinRequestHandler(queryString, User.AccessToken, this);
        }

        public IRequestHandler CreateMeltItemRequestHandler(string itemId, int amount)
        {
            if (itemId == null)
            {
                throw new ArgumentNullException(nameof(itemId));
            }

            string formatString = EnjinConnector.GetQuery(GraphQLTemplate.QueryType.MELT_FUNGIBLE_ITEM);
            string queryString = string.Format(formatString, GetId(), itemId, amount);

            return new EnjinRequestHandler(queryString, User.AccessToken, this);
        }

        public IRequestHandler CreateMeltItemRequestHandler(string itemId, string index)
        {
            if (itemId == null)
            {
                throw new ArgumentNullException(nameof(itemId));
            }

            if (index == null)
            {
                throw new ArgumentNullException(nameof(index));
            }


            string formatString = EnjinConnector.GetQuery(GraphQLTemplate.QueryType.MELT_NONFUNGIBLE_ITEM);
            string queryString = string.Format(formatString, GetId(), itemId, index);

            return new EnjinRequestHandler(queryString, User.AccessToken, this);
        }
    }
}