using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace EnjinSDK
{
    public class EnjinCompleteTradeRequestHandler : IRequestHandler
    {
        private readonly Guid _guid;
        private readonly string _queryString;
        private readonly string _accessToken;
        private readonly EnjinIdentity _identity;

        public EnjinCompleteTradeRequestHandler(string queryString, string accessToken, EnjinIdentity identity)
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
            EnjinLogger.ConsoleReporter(LoggerType.DEBUG_INFO, "** Trade Completion Register: " + eventType.EventType + " / " + eventType.PusherEvent + " / " + eventType.RequestType);
            if (eventType.EventType.Equals("tx_executed"))
            {
                eventType.RequestType = "CompleteTrade";
                EnjinLogger.ConsoleReporter(LoggerType.DEBUG_INFO, "** Set event type flag: " + eventType.RequestType);
            }
            await EnjinConnector.RegisterRequestCallback(GetGuid(), eventType, callback, GetIdentity());
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
