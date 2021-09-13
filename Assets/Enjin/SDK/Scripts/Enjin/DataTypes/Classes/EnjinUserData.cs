namespace EnjinSDK
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class EnjinUserData
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ServerURL { get; set; }
        public List<AccessTokens> Access_Tokens { get; set; }

        [JsonConverter(typeof(EnjinIdentityConverter))]
        public List<EnjinIdentity> Identities { get; set; }
        public EnjinDateData CreatedAt { get; set; }
        public EnjinDateData UpdatedAt { get; set; }
        public List<Role> Roles { get; set; }
        public List<EnjinApps> Apps { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public EnjinUserData()
        {
            ID = -1;
            Name = string.Empty;
            Email = string.Empty;
            ServerURL = string.Empty;
            Access_Tokens = new List<AccessTokens>();
            Identities = new List<EnjinIdentity>();
            UpdatedAt = new EnjinDateData();
            CreatedAt = new EnjinDateData();
            Roles = new List<Role>();
            Apps = new List<EnjinApps>();
        }
    }
}