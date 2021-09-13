using System;
using System.Threading.Tasks;

namespace EnjinSDK
{

    public interface IIdentityEventHandler
    {
        Task RefreshIdentity();
        Task RegisterCallback(IdentityEventType eventType, Action<EnjinIdentityEvent> callback);
    }
}
