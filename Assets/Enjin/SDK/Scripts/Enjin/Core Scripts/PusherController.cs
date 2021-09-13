namespace EnjinSDK
{
    using PusherClient;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    public enum ChannelType
    {
        APPLICATION,
        IDENTITY
    }

    public class PusherEventType
    {
        public string PusherEvent { get; set; }
    }

    public class IdentityEventType : PusherEventType
    {
        private IdentityEventType(string pusherEvent) { PusherEvent = pusherEvent; }

        public static IdentityEventType IdentityUpdated { get { return new IdentityEventType("EnjinCoin\\Events\\EnjinEventIdentity"); } }
    }

    public class RequestEventType : PusherEventType
    {
        private RequestEventType(string pusherEvent, string eventType, string requestType)
        { PusherEvent = pusherEvent; EventType = eventType; RequestType = requestType; }

        public string EventType { get; set; }
        public string RequestType { get; set; }

        public static RequestEventType MintPending { get { return new RequestEventType("EnjinCoin\\Events\\EnjinEventTransaction", "txr_pending", "mint"); } }
        public static RequestEventType MintBroadcast { get { return new RequestEventType("EnjinCoin\\Events\\EnjinEventTransaction", "tx_broadcast", "mint"); } }
        public static RequestEventType MintExecuted { get { return new RequestEventType("EnjinCoin\\Events\\EnjinEventTokenEvent", "tx_executed", "mint"); } }

        public static RequestEventType SendPending { get { return new RequestEventType("EnjinCoin\\Events\\EnjinEventTransaction", "txr_pending", "advanced_send"); } }
        public static RequestEventType SendBroadcast { get { return new RequestEventType("EnjinCoin\\Events\\EnjinEventTransaction", "tx_broadcast", "advanced_send"); } }
        public static RequestEventType SendExecuted { get { return new RequestEventType("EnjinCoin\\Events\\EnjinEventTokenEvent", "tx_executed", "send"); } }

        public static RequestEventType CreateTradePending { get { return new RequestEventType("EnjinCoin\\Events\\EnjinEventTransaction", "txr_pending", "create_trade"); } }
        public static RequestEventType CreateTradeBroadcast { get { return new RequestEventType("EnjinCoin\\Events\\EnjinEventTransaction", "tx_broadcast", "create_trade"); } }
        public static RequestEventType CreateTradeExecuted { get { return new RequestEventType("EnjinCoin\\Events\\EnjinEventTokenEvent", "tx_executed", "create_trade"); } }

        public static RequestEventType CompleteTradePending { get { return new RequestEventType("EnjinCoin\\Events\\EnjinEventTransaction", "txr_pending", "complete_trade"); } }
        public static RequestEventType CompleteTradeBroadcast { get { return new RequestEventType("EnjinCoin\\Events\\EnjinEventTransaction", "tx_broadcast", "complete_trade"); } }
        public static RequestEventType CompleteTradeExecuted { get { return new RequestEventType("EnjinCoin\\Events\\EnjinEventTokenEvent", "tx_executed", "complete_trade"); } }

        public static RequestEventType MeltPending { get { return new RequestEventType("EnjinCoin\\Events\\EnjinEventTransaction", "txr_pending", "melt"); } }
        public static RequestEventType MeltBroadcast { get { return new RequestEventType("EnjinCoin\\Events\\EnjinEventTransaction", "tx_broadcast", "melt"); } }
        public static RequestEventType MeltExecuted { get { return new RequestEventType("EnjinCoin\\Events\\EnjinEventTokenEvent", "tx_executed", "melt"); } }
    }

    public class PusherController
    {
        private Notifications _notifications;
        private readonly string _network;
        private readonly string _platformId;
        private readonly int _appId;

        private Pusher _client;
        private Channel _applicationChannel;
        private Dictionary<int, Channel> _identityChannels;

        private static Dictionary<Guid, Dictionary<RequestEventType, Action<EnjinRequestEvent>>> _eventRegistry = new Dictionary<Guid, Dictionary<RequestEventType, Action<EnjinRequestEvent>>>();
        private static Dictionary<int, Guid> _requestHandlerMap = new Dictionary<int, Guid>();

        private class IdentityEventRegistrationRequest
        {
            public ChannelType channelType;
            public IdentityEventType eventType;
            public Action<EnjinIdentityEvent> callback;
            public int identityId;
        }

        private class EventRegistrationRequest
        {
            public ChannelType channelType;
            public Guid guid;
            public RequestEventType eventType;
            public Action<EnjinRequestEvent> callback;
            public int identityId;
        }

        private bool _isConnected;
        private List<IdentityEventRegistrationRequest> _bufferedIdentityEventRequests;
        private List<EventRegistrationRequest> _bufferedEventRequests;

        public PusherController(Notifications notifications, string network, string platformId, int appId)
        {
            _notifications = notifications;
            _network = network;
            _platformId = platformId;
            _appId = appId;

            // Check for platform validity.
            if (_notifications == null)
            {
                throw new Exception("This platform has no specified notifications. It was perhaps requested with no specified access token.");
            }
            _isConnected = false;
            _bufferedIdentityEventRequests = new List<IdentityEventRegistrationRequest>();
            _bufferedEventRequests = new List<EventRegistrationRequest>();
            _applicationChannel = null;
            _identityChannels = new Dictionary<int, Channel>();

            PusherSettings.Verbose = false;
            PusherOptions options = new PusherOptions
            {
                Cluster = _notifications.SDK.Options.Cluster,
                Encrypted = (_notifications.SDK.Options.Encrypted == "true")
            };
            _client = new Pusher(_notifications.SDK.Key, options);
            _client.Connected += EventConnected;
            _client.ConnectionStateChanged += EventStateChange;
            _client.Connect();
        }

        public void LinkHandler(int requestId, Guid handlerId)
        {
            if (_requestHandlerMap.ContainsKey(requestId))
            {
                _requestHandlerMap[requestId] = handlerId;
            }
            else
            {
                _requestHandlerMap.Add(requestId, handlerId);
            }
        }

        private void SetupPusherChannels(int identityId, ChannelType channelType, PusherEventType eventType, Action<string> responseSetup, Action bufferSetup)
        {
            if (_isConnected)
            {
                switch (channelType)
                {
                    case ChannelType.APPLICATION:
                        {
                            string channelString = "enjin.server." + _network + "." + _platformId + "." + _appId;
                            if (_applicationChannel == null)
                            {
                                _applicationChannel = _client.Subscribe(channelString);
                                EnjinLogger.ConsoleReporter(LoggerType.EVENT, "Successfully subscribed to the APPLICATION channel: " + channelString);
                            }

                            _applicationChannel.BindOnce(eventType.PusherEvent, (eventData) =>
                            {
                                string dataString = JsonConvert.SerializeObject(eventData);
                                responseSetup(dataString);
                            });

                            break;
                        }
                    case ChannelType.IDENTITY:
                        {
                            // TODO: store all strings in external configuration files
                            // TODO: refactor configuration files to be something sensible, like JSON
                            Channel identityChannel;
                            string channelString = "enjin.server." + _network + "." + _platformId + ".identity." + identityId;
                            if (!_identityChannels.ContainsKey(identityId))
                            {
                                identityChannel = _client.Subscribe(channelString);
                                _identityChannels.Add(identityId, identityChannel);
                                EnjinLogger.ConsoleReporter(LoggerType.EVENT, "Successfully subscribed to the IDENTITY channel: " + channelString);
                            }
                            else
                            {
                                identityChannel = _identityChannels[identityId];
                            }

                            identityChannel.BindOnce(eventType.PusherEvent, (eventData) =>
                            {
                                string dataString = JsonConvert.SerializeObject(eventData);
                                responseSetup(dataString);
                            });

                            break;
                        }
                }
            }
            else
            {
                bufferSetup();
            }
        }

        /// <summary>
        /// Handle the registration of IdentityEvents onto channels.
        /// </summary>
        /// <param name="channelType">Channel type.</param>
        /// <param name="eventType">Event type.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="identityId">Identity identifier.</param>
        public void RegisterEvent(int identityId, ChannelType channelType, IdentityEventType eventType, Action<EnjinIdentityEvent> callback)
        {
            SetupPusherChannels(identityId, channelType, eventType,
                (responseString) =>
                {
                    EnjinIdentityEvent eventOutput = JsonConvert.DeserializeObject<EnjinIdentityEvent>(responseString, EnjinConnector.DeserializationSettings);
                    callback(eventOutput);
                },
                () =>
                {
                    _bufferedIdentityEventRequests.Add(new IdentityEventRegistrationRequest()
                    {
                        channelType = channelType,
                        eventType = eventType,
                        callback = callback,
                        identityId = identityId
                    });
                });
        }

        /// <summary>
        /// Handle the registration of RequestEvents onto channels.
        /// </summary>
        /// <param name="identityId">Identity identifier.</param>
        /// <param name="channelType">Channel type.</param>
        /// <param name="eventType">Event type.</param>
        /// <param name="callback">Callback.</param>
        public void RegisterEvent(int identityId, ChannelType channelType, Guid guid, RequestEventType eventType, Action<EnjinRequestEvent> callback)
        {
            if (_eventRegistry.ContainsKey(guid))
            {
                Dictionary<RequestEventType, Action<EnjinRequestEvent>> events = _eventRegistry[guid];
                events[eventType] = callback;
                _eventRegistry[guid] = events;
            }
            else
            {
                Dictionary<RequestEventType, Action<EnjinRequestEvent>> events = new Dictionary<RequestEventType, Action<EnjinRequestEvent>>
                {
                    [eventType] = callback
                };
                _eventRegistry[guid] = events;
            }

            SetupPusherChannels(identityId, channelType, eventType,
                (responseString) =>
                {
                    EnjinRequestEvent eventOutput = JsonConvert.DeserializeObject<EnjinRequestEvent>(responseString, EnjinConnector.DeserializationSettings);
                    int requestId = eventOutput.Event_Data.Id;
                    if (_requestHandlerMap.ContainsKey(requestId))
                    {
                        Guid handlerId = _requestHandlerMap[requestId];
                        if (_eventRegistry.ContainsKey(handlerId))
                        {
                            Dictionary<RequestEventType, Action<EnjinRequestEvent>> events = _eventRegistry[handlerId];
                            foreach (RequestEventType requestEventType in events.Keys)
                            {
                                if (requestEventType.PusherEvent.Equals(eventType.PusherEvent)
                                    && requestEventType.EventType.Equals(eventOutput.Event_Type)
                                    && requestEventType.RequestType.Equals(eventOutput.Event_Data.RequestType))
                                {
                                    Action<EnjinRequestEvent> listener = events[requestEventType];
                                    listener(eventOutput);
                                }
                            }
                        }
                    }
                },
                () =>
                {
                    _bufferedEventRequests.Add(new EventRegistrationRequest()
                    {
                        channelType = channelType,
                        guid = guid,
                        eventType = eventType,
                        callback = callback,
                        identityId = identityId
                    });
                });
        }

        public void Shutdown()
        {
            _client.Disconnect();
        }

        /// <summary>
        /// Pusher connected event
        /// </summary>
        /// <param name="sender">Object connector for pusher</param>
        private void EventConnected(object sender)
        {
            EnjinLogger.ConsoleReporter(LoggerType.EVENT, "Client connected.");
            _isConnected = true;
            foreach (IdentityEventRegistrationRequest toRegister in _bufferedIdentityEventRequests)
            {
                RegisterEvent(toRegister.identityId, toRegister.channelType, toRegister.eventType,
                    toRegister.callback);
            }
            foreach (EventRegistrationRequest toRegister in _bufferedEventRequests)
            {
                RegisterEvent(toRegister.identityId, toRegister.channelType, toRegister.guid, toRegister.eventType,
                    toRegister.callback);
            }
        }

        /// <summary>
        /// Pusher state change. Reports any state changes from pusher
        /// </summary>
        /// <param name="sender">Object connector to track</param>
        /// <param name="state">State change of pusher connector</param>
        private void EventStateChange(object sender, ConnectionState state)
        {
            EnjinLogger.ConsoleReporter(LoggerType.EVENT, "Connection state changed to: " + state);
        }
    }
}