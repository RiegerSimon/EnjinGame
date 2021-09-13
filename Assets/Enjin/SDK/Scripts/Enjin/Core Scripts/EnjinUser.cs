namespace EnjinSDK
{
    using System;
    using System.Threading.Tasks;
    using CodingHelmet.Optional;
    using Newtonsoft.Json;

    // A User of Enjin's services.
    public class EnjinUser
    {
        public EnjinUserData Data { get; private set; }
        public string AccessToken { get { return Data.Access_Tokens[0].Access_Token; } }

        public async static Task<EnjinUser> Login(string email, string password)
        {
            string userDataString = string.Empty;

            try
            {
                string queryString = string.Format(
                    EnjinConnector.GetQuery(GraphQLTemplate.QueryType.LOGIN), email, password);
                userDataString = await EnjinConnector.PostQuery(queryString);
                EnjinUserData userData = JsonConvert.DeserializeObject<EnjinUserData>(userDataString, EnjinConnector.DeserializationSettings);
                return new EnjinUser(userData);
            }
            catch (Exception exception)
            {
                EnjinLogger.ConsoleReporter(LoggerType.ERROR, exception.Message);
                throw exception;
            }
        }

        protected EnjinUser(EnjinUserData userData)
        {
            Data = userData;
            foreach (EnjinIdentity childIdentity in userData.Identities)
            {
                childIdentity.User = this;
            }
        }

        /// <summary>
        /// Logs the player out and peforms clean up
        /// </summary>
        public void Logout()
        {
            foreach (EnjinIdentity identity in Data.Identities)
            {
                identity.Cleanup();
            }
        }

        /// <summary>
        /// Checks if user has a specified role
        /// </summary>
        /// <param name="role">Role to check</param>
        /// <returns>(true/false) depending if user has role</returns>
        public bool HasRole(string role)
        {
            foreach (Role userRole in Data.Roles)
            {
                if (userRole.Name == role)
                    return true;
            }

            return false;
        }

        public async Task<EnjinRequest> CreateRole(string name, string[] permissions)
        {
            string query = string.Format(EnjinConnector.GetQuery(GraphQLTemplate.QueryType.CREATE_ROLE), name, permissions);
            string results = await EnjinConnector.PostQuery(query, AccessToken);

            return JsonConvert.DeserializeObject<EnjinRequest>(results, EnjinConnector.DeserializationSettings);
        }

        public async Task<EnjinRequest> UpdateRole(string name, string[] permissions)
        { return await UpdateRole(name, string.Empty, permissions); }

        public async Task<EnjinRequest> UpdateRole(string name, string newName, string[] permissions)
        {
            string query;

            if (newName == string.Empty)
                query = string.Format(EnjinConnector.GetQuery(GraphQLTemplate.QueryType.UPDATE_ROLE), name, permissions);
            else
                query = string.Format(EnjinConnector.GetQuery(GraphQLTemplate.QueryType.UPDATE_ROLE_NAME), name, permissions);

            string results = await EnjinConnector.PostQuery(query, AccessToken);

            return JsonConvert.DeserializeObject<EnjinRequest>(results, EnjinConnector.DeserializationSettings);
        }

        public async Task<EnjinRequest> DeleteRole(string roleName)
        {
            string query = string.Format(EnjinConnector.GetQuery(GraphQLTemplate.QueryType.DELETE_ROLE), roleName);
            string results = await EnjinConnector.PostQuery(query, AccessToken);

            return JsonConvert.DeserializeObject<EnjinRequest>(results, EnjinConnector.DeserializationSettings);
        }

        /// <summary>
        /// Invite a new User to this app on this platform with the given name
        /// </summary>
        /// <param name="email">Email address to send invitation to</param>
        /// <param name="username">The invited user's username</param>
        public async Task<EnjinRequest> InviteUserToApp(string email, string username = "")
        {
            string query;

            if (username != string.Empty)
                query = string.Format(EnjinConnector.GetQuery(GraphQLTemplate.QueryType.INVITE_USER_WITH_NAME), email, username);
            else
                query = string.Format(EnjinConnector.GetQuery(GraphQLTemplate.QueryType.INVITE_USER), email);

            string results = await EnjinConnector.PostQuery(query, AccessToken);

            return JsonConvert.DeserializeObject<EnjinRequest>(results, EnjinConnector.DeserializationSettings);
        }

        /// <summary>
        /// Creates a new application
        /// </summary>
        /// <param name="app">Application to create</param>
        /// <returns>New applicaiton</returns>
        public async Task<EnjinRequest> CreateApp(string name, string imageURL, string description)
        {
            string query = string.Format(EnjinConnector.GetQuery(GraphQLTemplate.QueryType.CREATE_APP), name, description, imageURL);
            string results = await EnjinConnector.PostQuery(query, AccessToken);

            return JsonConvert.DeserializeObject<EnjinRequest>(results, EnjinConnector.DeserializationSettings);
        }

        /// <summary>
        /// Updates App information
        /// </summary>
        /// <param name="app">App to update information for</param>
        /// <returns>Updated App</returns>
        public async Task<EnjinRequest> UpdateApp(string name, string imageURL, string description)
        {
            string query = string.Format(EnjinConnector.GetQuery(GraphQLTemplate.QueryType.UPDATE_APP), name, description, imageURL);
            string results = await EnjinConnector.PostQuery(query, AccessToken);

            return JsonConvert.DeserializeObject<EnjinRequest>(results, EnjinConnector.DeserializationSettings);
        }

        public async Task<EnjinRequest> SetMaxAllowance()
        {
            //string query = string.Format(EnjinConnector.GetQuery(GraphQLTemplate.QueryType.SET_MAX_ALLOWANCE), ); <-- NEEDS ACTIVE IDENTITY ID
            //string results = await EnjinConnector.PostQuery(query, AccessToken);

            //return JsonConvert.DeserializeObject<EnjinRequest>(results, DeserializationSettings);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the Identity corresponding to a given application identifier.
        /// </summary>
        /// <returns>The Identity associated with the given appId.</returns>
        /// <param name="appId">Application identifier.</param>
        /// <param name="refreshCache">Whether or not to refresh the identity retrieved.</param>
        async public Task<Option<EnjinIdentity>> GetIdentityForAppId(int appId, bool refreshCache = true)
        {
            foreach (EnjinIdentity identity in Data.Identities)
            {
                if (identity.GetAppId() == appId)
                {
                    if (refreshCache)
                    {
                        await identity.RefreshIdentity();
                    }
                    return identity;
                }
            }
            return None.Value;
        }
    }
}