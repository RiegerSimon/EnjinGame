namespace EnjinSDK
{
    using System.Collections.Generic;

    public enum PermissionType { none }

    public class EnjinApps
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public EnjinApps()
        {
            ID = 0;
            Name = string.Empty;
            Description = string.Empty;
            Image = string.Empty;
        }
    }

    public class AccessTokens
    {
        public string App_ID;
        public string Access_Token;
        public string Refresh_Token;

        /// <summary>
        /// Constructor
        /// </summary>
        public AccessTokens()
        {
            App_ID = string.Empty;
            Access_Token = string.Empty;
            Refresh_Token = string.Empty;
        }
    }

    public class PlatformOptions
    {
        public string Cluster;
        public string Encrypted;

        /// <summary>
        /// Constructor
        /// </summary>
        public PlatformOptions()
        {
            Cluster = string.Empty;
            Encrypted = string.Empty;
        }
    }

    public class SDKData
    {
        public string Driver;
        public string Key;
        public string Secret;
        public string App_ID;
        public PlatformOptions Options;

        /// <summary>
        /// Constructor
        /// </summary>
        public SDKData()
        {
            Driver = string.Empty;
            Key = string.Empty;
            Secret = string.Empty;
            App_ID = string.Empty;
            Options = new PlatformOptions();
        }
    }

    public class Mobile
    {
        public string Type;

        /// <summary>
        /// Constructor
        /// </summary>
        public Mobile() { Type = string.Empty; }
    }

    public class Notifications
    {
        public SDKData SDK;
        public Mobile Mobile;

        /// <summary>
        /// Constructor
        /// </summary>
        public Notifications()
        {
            SDK = new SDKData();
            Mobile = new Mobile();
        }
    }

    public class PlatformInfo
    {
        public string Name;
        public string ID;
        public string Network;
        public Notifications Notifications;

        /// <summary>
        /// Constructor
        /// </summary>
        public PlatformInfo()
        {
            Name = string.Empty;
            ID = string.Empty;
            Network = string.Empty;
            Notifications = new Notifications();
        }
    }

    public class Role
    {
        /// <summary>
        /// Permissions object contains the permission id and name.
        /// </summary>
        public struct Permission
        {
            public int id;                      // Permission ID
            public string name;                 // Permission name
        }

        public int ID;                          // Role ID
        public int AppID;                       // App ID Role is assigned to
        public string Name;                     // Role name
        public List<Permission> Permissions;    // List of permissions role has

        /// <summary>
        /// Constructor
        /// </summary>
        public Role()
        {
            ID = -1;
            Name = string.Empty;
            Permissions = new List<Permission>();
        }

        /// <summary>
        /// Checks is role has requested permission
        /// </summary>
        /// <param name="permission">Permission to check for</param>
        /// <returns>(true/false) depending if permission is part of role</returns>
        public bool HasPermission(PermissionType permission)
        {
            foreach (Permission perm in Permissions)
            {
                if (perm.name == permission.ToString())
                    return true;
            }

            return false;
        }
    }
}