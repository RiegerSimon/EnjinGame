using System.Collections.Generic;
using System.Threading.Tasks;
using CodingHelmet.Optional;

namespace EnjinSDK
{
    public interface IPublicOperations
    {
        void Cleanup();

        // Retrieves a list containing all indices of the specified item that the user has in their inventory.
        // Fungible items will return a single-element list.
        Task<Option<List<EnjinCryptoItem>>> GetItem(string itemId, bool refreshCache = true);

        // Retrieves a specific index of the specified item. Fungible items will ignore the index field.
        Task<Option<EnjinCryptoItem>> GetItem(string itemId, string index, bool refreshCache = true);

        // Returns the full inventory of items that this Identity's address has.
        Task<Dictionary<string, List<EnjinCryptoItem>>> GetAllItems(bool refreshCache = true);

        // Returns the full inventory of items that this Identity's address has on a given app ID.
        Task<Dictionary<string, List<EnjinCryptoItem>>> GetAllItemsForApp(int appId, bool refreshCache = true);

        // Returns the full inventory of items that this Identity's address has, sorted by app ID.
        Task<Dictionary<int, Dictionary<string, List<EnjinCryptoItem>>>> GetAllItemsByApp(bool refreshCache = true);

        // Returns the balance of the specified item. For nonfungible items, this returns the number of unique instances.
        Task<Option<float>> GetBalance(string itemId, bool refreshCache = true);

        Task<EnjinRequest> SendItem(EnjinCryptoItem item, EnjinUser user); // TODO: primitives as parameters
        Task<EnjinRequest> MeltItem(EnjinCryptoItem item, int amount); // TODO: primitives as parameters

        // Item send operations and appropriate overloads.
        IRequestHandler CreateSendItemRequestHandler(string[] itemIds, string[] ethereumAddresses, int[] amounts);
        IRequestHandler CreateSendItemRequestHandler(string itemId, string[] ethereumAddresses, int[] amounts);
        IRequestHandler CreateSendItemRequestHandler(string itemId, string[] ethereumAddresses, int amount);
        IRequestHandler CreateSendItemRequestHandler(string itemId, string ethereumAddress, int amount);

        // Item trade creation operations and appropriate overloads.
        IRequestHandler CreateTradeItemRequestHandler(EnjinItemParameter[] offeringItems, EnjinItemParameter[] askingItems, string secondPartyAddress);

        // Item trade completion operations and appropriate overloads.
        IRequestHandler CompleteTradeItemRequestHandler(string tradeId);

        // Item melt operations and appropriate overloads.
        IRequestHandler CreateMeltItemRequestHandler(string itemId, int amount);
        IRequestHandler CreateMeltItemRequestHandler(string itemId, string index);
    }
}