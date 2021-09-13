using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnjinSDK
{
    public class EnjinRequestHandler : IRequestHandler
    {
        private readonly Guid _guid;
        private readonly string _queryString;
        private readonly string _accessToken;
        private readonly EnjinIdentity _identity;

        public EnjinRequestHandler(string queryString, string accessToken, EnjinIdentity identity)
        {
            _guid = Guid.NewGuid();
            _queryString = queryString;
            _accessToken = accessToken;
            _identity = identity;
        }

        public Guid GetGuid()
        {
            return _guid;
        }

        public EnjinIdentity GetIdentity()
        {
            return _identity;
        }

        async public Task RegisterCallback(RequestEventType eventType, Action<EnjinRequestEvent> callback)
        {
            await EnjinConnector.RegisterRequestCallback(_guid, eventType, callback, _identity);
        }

        async public Task<EnjinRequest> Execute()
        {
            string result = await EnjinConnector.PostQuery(_queryString, _accessToken, _identity.GetAppId());
            EnjinLogger.ConsoleReporter(LoggerType.DEBUG_INFO, "Received EnjinRequest string " + result);
            EnjinRequestResult requestResult = JsonConvert.DeserializeObject<EnjinRequestResult>(result, EnjinConnector.DeserializationSettings);
            EnjinRequest request = requestResult.Request;
            EnjinLogger.ConsoleReporter(LoggerType.DEBUG_INFO, "Executed request of ID " + request.ID);
            EnjinConnector.LinkHandler(request.ID, _guid);
            return request;
        }
    }
}
