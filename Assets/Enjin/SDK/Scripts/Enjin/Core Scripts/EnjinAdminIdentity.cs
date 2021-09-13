using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodingHelmet.Optional;
using Newtonsoft.Json;

namespace EnjinSDK
{

    // TODO: refactor this to no longer be an "app-listener" once filtering channels are ready.
    public class EnjinAdminIdentity : IPublicOperations, IAdminOperations, IIdentityEventHandler
    {
        private EnjinIdentity _identity;

        public EnjinAdminIdentity(EnjinIdentity identity)
        {
            _identity = identity;
        }

        public EnjinUser GetUser()
        {
            return _identity.User;
        }

        public void Cleanup()
        {
            _identity.Cleanup();
        }

        public Task<Option<List<EnjinCryptoItem>>> GetItem(string itemId, bool refreshCache = true)
        {
            return ((IPublicOperations)_identity).GetItem(itemId, refreshCache);
        }

        public Task<Option<EnjinCryptoItem>> GetItem(string itemId, string index, bool refreshCache = true)
        {
            return ((IPublicOperations)_identity).GetItem(itemId, index, refreshCache);
        }

        public Task<Dictionary<string, List<EnjinCryptoItem>>> GetAllItems(bool refreshCache = true)
        {
            return ((IPublicOperations)_identity).GetAllItems(refreshCache);
        }

        public Task<Dictionary<string, List<EnjinCryptoItem>>> GetAllItemsForApp(int appId, bool refreshCache = true)
        {
            return ((IPublicOperations)_identity).GetAllItemsForApp(appId, refreshCache);
        }

        public Task<Dictionary<int, Dictionary<string, List<EnjinCryptoItem>>>> GetAllItemsByApp(bool refreshCache = true)
        {
            return ((IPublicOperations)_identity).GetAllItemsByApp(refreshCache);
        }

        public Task<Option<float>> GetBalance(string itemId, bool refreshCache = true)
        {
            return ((IPublicOperations)_identity).GetBalance(itemId, refreshCache);
        }

        async public Task<EnjinRequest> SendItem(EnjinCryptoItem item, EnjinUser user)
        {
            return await _identity.SendItem(item, user);
        }

        async public Task<EnjinRequest> MeltItem(EnjinCryptoItem item, int amount)
        {
            return await _identity.MeltItem(item, amount);
        }

        public IRequestHandler CreateMintItemRequestHandler(string itemId, string[] ethereumAddresses, int[] amounts)
        {
            string addressList = "\"" + string.Join("\", \"", ethereumAddresses) + "\"";
            string amountList = string.Join(",", amounts);
            string formatString = EnjinConnector.GetQuery(GraphQLTemplate.QueryType.MINT_ITEM);
            string queryString = string.Format(formatString, _identity.GetId(), itemId, addressList, amountList);

            return new EnjinRequestHandler(queryString, GetUser().AccessToken, _identity);
        }

        public IRequestHandler CreateMintItemRequestHandler(string itemId, string[] ethereumAddress, int amount)
        {
            throw new NotImplementedException();
        }

        public IRequestHandler CreateMintItemRequestHandler(string itemId, string ethereumAddress, int amount)
        {
            throw new NotImplementedException();
        }

        async public Task<EnjinRequest> UpdateMetaData()
        {
            throw new System.NotImplementedException();
        }

        async public Task<EnjinRequest> CreateUser(string name, string email, string password, string roleName)
        {
            string queryString = string.Format(EnjinConnector.GetQuery(GraphQLTemplate.QueryType.CREATE_USER), name, email, password, roleName);
            string results = await EnjinConnector.PostQuery(queryString, GetUser().AccessToken);
            EnjinRequest request = JsonConvert.DeserializeObject<EnjinRequest>(results, EnjinConnector.DeserializationSettings);
            return request;
        }

        async public Task<EnjinRequest> CreateItem(EnjinCryptoItem item)
        {
            int appId = _identity.GetAppId();
            string query = string.Format(EnjinConnector.GetQuery(GraphQLTemplate.QueryType.CREATE_ITEM), appId, item.Name, item.TotalSupply, item.Reserve, item.SupplyModel, item.MeltValue, item.MeltFeeRatio, item.Transferable, item.TransferFeeSettings.Type, item.TransferFeeSettings.Token_ID, item.TransferFeeSettings.Value, item.NonFungible.ToString().ToLower());
            string results = await EnjinConnector.PostQuery(query, GetUser().AccessToken, appId);
            EnjinRequest request = JsonConvert.DeserializeObject<EnjinRequest>(results, EnjinConnector.DeserializationSettings);
            return request;
        }

        // TODO: track down and replace Mutex with semaphore slim?
        // TODO: implement and test fields
        async public Task<EnjinRequest> CreateIdentity(int userId, string ethereumAddress, List<EnjinIdentityField> fields)
        {
            string fieldsString = "[]";
            string query = string.Format(EnjinConnector.GetQuery(
                GraphQLTemplate.QueryType.CREATE_IDENTITY_BY_USER_ID), userId, ethereumAddress, fieldsString);
            string results = await EnjinConnector.PostQuery(query, GetUser().AccessToken, _identity.GetAppId());
            EnjinRequest request = JsonConvert.DeserializeObject<EnjinRequest>(results, EnjinConnector.DeserializationSettings);
            return request;
        }

        async public Task<EnjinRequest> CreateIdentity(string email, string ethereumAddress, List<EnjinIdentityField> fields)
        {
            string fieldsString = "[]";
            string query = string.Format(EnjinConnector.GetQuery(
                GraphQLTemplate.QueryType.CREATE_IDENTITY_BY_EMAIL), email, ethereumAddress, fieldsString);
            string results = await EnjinConnector.PostQuery(query, GetUser().AccessToken, _identity.GetAppId());
            EnjinRequest request = JsonConvert.DeserializeObject<EnjinRequest>(results, EnjinConnector.DeserializationSettings);
            return request;
        }

        public Task RegisterCallback(IdentityEventType eventType, Action<EnjinIdentityEvent> callback)
        {
            return _identity.RegisterCallback(eventType, callback);
        }

        public Task RefreshIdentity()
        {
            return ((IIdentityEventHandler)_identity).RefreshIdentity();
        }

        public IRequestHandler CreateSendItemRequestHandler(string[] itemIds, string[] ethereumAddresses, int[] amounts)
        {
            return ((IPublicOperations)_identity).CreateSendItemRequestHandler(itemIds, ethereumAddresses, amounts);
        }

        public IRequestHandler CreateSendItemRequestHandler(string itemId, string[] ethereumAddresses, int[] amounts)
        {
            return ((IPublicOperations)_identity).CreateSendItemRequestHandler(itemId, ethereumAddresses, amounts);
        }

        public IRequestHandler CreateSendItemRequestHandler(string itemId, string[] ethereumAddresses, int amount)
        {
            return ((IPublicOperations)_identity).CreateSendItemRequestHandler(itemId, ethereumAddresses, amount);
        }

        public IRequestHandler CreateSendItemRequestHandler(string itemId, string ethereumAddress, int amount)
        {
            return ((IPublicOperations)_identity).CreateSendItemRequestHandler(itemId, ethereumAddress, amount);
        }

        public IRequestHandler CreateTradeItemRequestHandler(EnjinItemParameter[] offeringItems, EnjinItemParameter[] askingItems, string secondPartyAddress)
        {
            return ((IPublicOperations)_identity).CreateTradeItemRequestHandler(offeringItems, askingItems, secondPartyAddress);
        }

        public IRequestHandler CompleteTradeItemRequestHandler(string tradeId)
        {
            return ((IPublicOperations)_identity).CompleteTradeItemRequestHandler(tradeId);
        }

        public IRequestHandler CreateMeltItemRequestHandler(string itemId, int amount)
        {
            return ((IPublicOperations)_identity).CreateMeltItemRequestHandler(itemId, amount);
        }

        public IRequestHandler CreateMeltItemRequestHandler(string itemId, string index)
        {
            return ((IPublicOperations)_identity).CreateMeltItemRequestHandler(itemId, index);
        }
    }
}
