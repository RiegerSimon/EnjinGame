using System;
using System.Threading.Tasks;

namespace EnjinSDK
{
    public interface IRequestHandler
    {
        Task RegisterCallback(RequestEventType eventType, Action<EnjinRequestEvent> callback);
        Task<EnjinRequest> Execute();
    }
}
