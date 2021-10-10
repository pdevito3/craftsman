namespace Craftsman.Models
{
    using System;
    using System.Collections.Generic;
    using Enums;

    public class AuthServerTemplate
    {
        public string Name { get; set; }
        
        public List<Policy> Policies { get; set; }
        
        public List<AuthClient> Clients { get; set; }
        
        public int Port { get; set; }
        
        public List<AuthApi> Apis { get; set; }
        
    }

    public class AuthApi
    {
        public string ApiName { get; set; }
        public string ApiDisplayName { get; set; }
        public List<AuthScope> ScopeNames { get; set; }
        public List<string> Secrets { get; set; }
    }

    public class AuthScope
    {
        public string ScopeName { get; set; }
        public string ScopeDisplayName { get; set; }
        public List<string> UserClaims { get; set; }
    }

    /// <summary>
    /// swagger info:<br/>
    ///     RedirectUris = {"https://localhost:apiPort/swagger/oauth2-redirect.html"},<br/>
    ///     PostLogoutRedirectUris = { "http://localhost:apiPort/" },<br/>
    ///     FrontChannelLogoutUri =    "http://localhost:apiPort/signout-oidc",<br/>
    ///     AllowedCorsOrigins = {"https://localhost:apiPort"},<br/>
    /// <br/><br/>
    /// duende info:<br/>
    /// RedirectUris = { "https://localhost:BffPort/signin-oidc" },<br/>
    /// FrontChannelLogoutUri = "https://localhost:BffPort/signout-oidc",<br/>
    /// PostLogoutRedirectUris = { "https://localhost:BffPort/signout-callback-oidc" },<br/>
    /// AllowedCorsOrigins = {"https://localhost:apiPort", "https://localhost:BffPort"},<br/>
    /// 
    /// </summary>
    public class AuthClient
    {
        private int? _port;
        public int? ClientPort
        {
            get => _port;
            set => _port = value ?? throw new Exception($"ClientPort required for AuthClient '{ClientName}'");
        }
        
        public string ClientId { get; set; }
        
        public string ClientName { get; set; }

        public List<string> ClientSecrets { get; set; } = new List<string>(){Guid.NewGuid().ToString()};

        private GrantType _grantType { get; set; } = Enums.GrantType.Code;
        public string GrantType
        {
            get => _grantType.Name;
            set
            {
                if (!Enums.GrantType.TryFromName(value, true, out var parsed))
                {   
                    _grantType = Enums.GrantType.Code;
                }
                _grantType = parsed;
            }
        }

        private List<string> _redirectUris = null;
        public List<string> RedirectUris
        {
            get => _redirectUris;
            set => _redirectUris = value ?? new List<string>(){$"https://localhost:{ClientPort}/swagger/oauth2-redirect.html"};
        }
        
        private List<string> _postLogoutRedirectUris = null;
        public List<string> PostLogoutRedirectUris
        {
            get => _postLogoutRedirectUris;
            set => _postLogoutRedirectUris = value ?? new List<string>(){$"https://localhost:{ClientPort}"};
        }

        private List<string> _allowedCorsOrigins = null;
        public List<string> AllowedCorsOrigins
        {
            get => _allowedCorsOrigins;
            set => _allowedCorsOrigins = value ?? new List<string>(){$"https://localhost:{ClientPort}"};
        }

        private string _frontChannelLogoutUri = null;
        public string FrontChannelLogoutUri 
        {
            get => _frontChannelLogoutUri;
            set => _frontChannelLogoutUri = value ?? $"http://localhost:{ClientPort}/signout-oidc";
        }

        public bool AllowOfflineAccess { get; set; } = true;
        
        public bool RequirePkce { get; set; } = true;
        
        public bool RequireClientSecret { get; set; } = true;

        public bool AllowPlainTextPkce { get; set; } = false;

        public List<string> AllowedScopes { get; set; } = new() {"openid", "profile"}; // only if `Code` flow
    }
}