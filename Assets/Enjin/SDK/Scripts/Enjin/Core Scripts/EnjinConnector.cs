namespace EnjinSDK
{
    using System.Threading.Tasks;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Net.Http.Headers;
    using System.Collections.Generic;

    public static class EnjinConnector
    {
        #region Definitions
        public class Query { public string query; }
        public static string GraphQLEndPoint { get; private set; }          // Endpoint for GraphQL queries
        public static string MeltValueEndPoint { get; private set; }        // Endpoint for item min melt value
        public static string AllowanceEndPoint { get; private set; }        // Endpoint for checking wallet allowance
        public static JsonSerializerSettings DeserializationSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        private static GraphQLTemplate _queryTemplates;                     // Query & Mutation templates
        private static bool _hasInitialized = false;
        #endregion

        #region Methods
        /// <summary>
        /// Initializes Connector
        /// </summary>
        /// <param name="serverURL">URL of platform server</param>
        public static void Initialize(string serverURL)
        {
            _queryTemplates = new GraphQLTemplate("QueryCommands");
            SetAPIURLS(serverURL);
            _hasInitialized = true;
        }

        /// <summary>
        /// 
        /// </summary>
        private static void RequireInitialization()
        {
            if (!_hasInitialized)
            {
                EnjinLogger.ConsoleReporter(LoggerType.ERROR, "This EnjinConnector has not been initialized.");
                throw new Exception("This EnjinConnector has not been initialized.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryType"></param>
        /// <returns></returns>
        public static string GetQuery(GraphQLTemplate.QueryType queryType)
        {
            RequireInitialization();
            return _queryTemplates.Queries[queryType];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryData"></param>
        /// <returns></returns>
        public async static Task<string> PostQuery(string queryData)
        {
            return await PostQuery(queryData, string.Empty, -1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryData"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public async static Task<string> PostQuery(string queryData, string accessToken)
        {
            return await PostQuery(queryData, accessToken, -1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryData"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async static Task<string> PostQuery(string queryData, int appId) // TODO: clean all of this stuff up!
        {
            return await PostQuery(queryData, string.Empty, appId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryData"></param>
        /// <param name="accessToken"></param>
        /// <param name="appID"></param>
        /// <returns></returns>
        public async static Task<string> PostQuery(string queryData, string accessToken, int appID)
        {
            RequireInitialization();

            string jsonData = "";

            queryData = queryData.Trim(new char[] { '\n', '\r' });
            Query query = new Query() { query = queryData };
            jsonData = JsonConvert.SerializeObject(query);
            EnjinLogger.ConsoleReporter(LoggerType.GRAPHQL_REQUEST, jsonData);

            HttpClient c = new HttpClient
            {
                BaseAddress = new Uri(GraphQLEndPoint)
            };

            if (accessToken != string.Empty)
                c.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + accessToken);

            if (appID != -1)
                c.DefaultRequestHeaders.Add("X-App-Id", appID.ToString());

            c.DefaultRequestHeaders.Add("X-Enjin-Platform", "unity");
            c.DefaultRequestHeaders.Add("X-EnjinPlatform-Version", "1.1.0");

            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, GraphQLEndPoint)
            {
                Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
            };

            HttpResponseMessage httpResponseMessage = await c.SendAsync(req);
            httpResponseMessage.EnsureSuccessStatusCode();
            HttpContent httpContent = httpResponseMessage.Content;
            string responseString = await httpContent.ReadAsStringAsync();

            return ValidateData(responseString);
        }

        /// <summary>
        /// Validates the resonse data & returns clean json response
        /// </summary>
        /// <param name="jsonResponse">Raw json response from trusted platform</param>
        /// <returns>Validated json data</returns>
        private static string ValidateData(string jsonResponse)
        {
            string pData = Regex.Replace(jsonResponse, @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+", "$1");

            if (pData.Contains("errors"))
            {
                int index = pData.IndexOf("\"errors\":") + 9;
                pData = pData.Substring(index, pData.Length - 1 - index);

                EnjinLogger.ConsoleReporter(LoggerType.ERROR, pData);
            }
            else if (pData.Contains("result"))
            {
                int index = pData.IndexOf("\"result\":") + 9;
                pData = pData.Substring(index, pData.Length - 2 - index);
                EnjinLogger.ConsoleReporter(LoggerType.GRAPHQL_RESPONSE, pData);
            }
            else
            {
                int index = pData.IndexOf("\"data\":") + 7;
                pData = pData.Substring(index, pData.Length - 1 - index);
                EnjinLogger.ConsoleReporter(LoggerType.GRAPHQL_RESPONSE, pData);
            }

            return pData;
        }

        /// <summary>
        /// Sets all the trusted plaftform connection endpoints
        /// </summary>
        /// <param name="serverURL">Server URL of trusted platform</param>
        private static void SetAPIURLS(string serverURL)
        {
            if (serverURL.EndsWith("/"))
            {
                GraphQLEndPoint = serverURL + "graphql";
                MeltValueEndPoint = serverURL + "api/v1/ethereum/get-min-melt-value/";
                AllowanceEndPoint = serverURL + "api/v1/ethereum/";
            }
            else
            {
                GraphQLEndPoint = serverURL + "/graphql";
                MeltValueEndPoint = serverURL + "/api/v1/ethereum/get-min-melt-value/";
                AllowanceEndPoint = serverURL + "/api/v1/ethereum/";
            }
        }
        #endregion

        #region Event Management
        private static PusherController _pusherController;

        async public static Task RegisterIdentityCallback(EnjinIdentity identity, IdentityEventType eventType, Action<EnjinIdentityEvent> callback)
        {
            if (_pusherController == null)
            {
                string platformDataString = await PostQuery(GetQuery(GraphQLTemplate.QueryType.GET_PLATFORM_INFO), identity.User.AccessToken);
                PlatformInfo platformInfo = JsonConvert.DeserializeObject<PlatformInfo>(platformDataString);
                _pusherController = new PusherController(platformInfo.Notifications, platformInfo.Network, platformInfo.ID, identity.GetAppId());
            }
            _pusherController.RegisterEvent(identity.GetId(), ChannelType.IDENTITY, eventType, callback);
        }

        public static void LinkHandler(int requestId, Guid handlerId)
        {
            _pusherController.LinkHandler(requestId, handlerId);
        }

        async public static Task RegisterRequestCallback(Guid guid, RequestEventType eventType, Action<EnjinRequestEvent> callback, EnjinIdentity identity)
        {
            if (_pusherController == null)
            {
                string platformDataString = await PostQuery(GetQuery(GraphQLTemplate.QueryType.GET_PLATFORM_INFO), identity.User.AccessToken);
                PlatformInfo platformInfo = JsonConvert.DeserializeObject<PlatformInfo>(platformDataString);
                _pusherController = new PusherController(platformInfo.Notifications, platformInfo.Network, platformInfo.ID, identity.GetAppId());
            }
            _pusherController.RegisterEvent(identity.GetId(), ChannelType.APPLICATION, guid, eventType, callback);
        }
        #endregion
    }
}