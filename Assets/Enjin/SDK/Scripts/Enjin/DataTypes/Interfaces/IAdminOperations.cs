using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnjinSDK
{
    public interface IAdminOperations
    {

        // Item mint operations and appropriate overloads.
        IRequestHandler CreateMintItemRequestHandler(string itemId, string[] ethereumAddresses, int[] amounts);
        IRequestHandler CreateMintItemRequestHandler(string itemId, string[] ethereumAddresses, int amount);
        IRequestHandler CreateMintItemRequestHandler(string itemId, string ethereumAddress, int amount);

        Task<EnjinRequest> CreateUser(string name, string email, string password, string roleName);
        Task<EnjinRequest> CreateIdentity(string email, string ethereumAddress, List<EnjinIdentityField> fields);
        Task<EnjinRequest> CreateIdentity(int userId, string ethereumAddress, List<EnjinIdentityField> fields);
        Task<EnjinRequest> CreateItem(EnjinCryptoItem item);
        Task<EnjinRequest> UpdateMetaData();
    }
}