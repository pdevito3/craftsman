namespace Craftsman.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Enums;

    public class AuthServerTemplate
    {
        public string Name { get; set; }
        
        public List<AuthClient> Clients { get; set; }
        
        public List<AuthScope> Scopes { get; set; }
        
        public int Port { get; set; }
        
        public List<AuthApi> Apis { get; set; }
        
    }

    public class AuthApi
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public List<string> ScopeNames { get; set; }
        public List<string> Secrets { get; set; } = new List<string>(){Guid.NewGuid().ToString()};
        public List<string> UserClaims { get; set; } = new List<string>() {"openid", "profile"};

        public string GetScopeNameString()
        {
            return ScopeNames is not {Count: > 0} 
                ? null 
                : string.Join(", ", ScopeNames.Select(claim => $@"""{claim}"""));
        }

        public string GetSecretsString()
        {
            return Secrets is not {Count: > 0} 
                ? null 
                : string.Join(", ", Secrets.Select(secret => $@"new Secret(""{secret}"".Sha256())"));
        }

        public string GetClaimsString()
        {
            return UserClaims is not {Count: > 0} 
                ? null 
                : string.Join(", ", UserClaims.Select(claim => $@"""{claim}"""));
        }
    }

    public class AuthScope
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public List<string> UserClaims { get; set; }
        
        public string GetClaimsString()
        {
            return UserClaims is not {Count: > 0} 
                ? null 
                : string.Join(", ", UserClaims.Select(claim => $@"""{claim}"""));
        }
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

        public string Id { get; set; }
        
        public string Name { get; set; }

        public List<string> Secrets { get; set; } = new List<string>(){Guid.NewGuid().ToString()};

        internal GrantType GrantTypeEnum { get; private set; } = Enums.GrantType.Code;
        public string GrantType
        {
            get => GrantTypeEnum.Name;
            set
            {
                if (!Enums.GrantType.TryFromName(value, true, out var parsed))
                {   
                    GrantTypeEnum = Enums.GrantType.Code;
                }
                GrantTypeEnum = parsed;
            }
        }

        public List<string> RedirectUris { get; set; }
        
        public List<string> PostLogoutRedirectUris { get; set; }

        public List<string> AllowedCorsOrigins { get; set; }

        public string FrontChannelLogoutUri { get; set; }

        public bool AllowOfflineAccess { get; set; } = true;
        
        public bool RequirePkce { get; set; } = true;
        
        public bool RequireClientSecret { get; set; } = true;

        public bool AllowPlainTextPkce { get; set; } = false;

        private List<string> _allowedScopes = new List<string>();
        public List<string> AllowedScopes 
        {
            get => _allowedScopes;
            set => _allowedScopes = GrantTypeEnum == Enums.GrantType.Code 
            ? value ?? new List<string>() {"openid", "profile"}
            : value;
        }
        
        public string GetSecretsString()
        {
            return Secrets is not {Count: > 0} 
                ? null 
                : string.Join(", ", Secrets.Select(secret => $@"new Secret(""{secret}"".Sha256())"));
        }
        
        public string GetScopeNameString()
        {
            return AllowedScopes is not {Count: > 0} 
                ? null 
                : string.Join(", ", AllowedScopes.Select(claim => $@"""{claim}"""));
        }
        
        public string GetRedirectUrisString()
        {
            return RedirectUris is not {Count: > 0} 
                ? null 
                : string.Join(", ", RedirectUris.Select(claim => $@"""{claim}"""));
        }
        
        public string GetPostLogoutRedirectUrisString()
        {
            return PostLogoutRedirectUris is not {Count: > 0} 
                ? null 
                : string.Join(", ", PostLogoutRedirectUris.Select(claim => $@"""{claim}"""));
        }
        
        public string GetAllowedCorsOriginsString()
        {
            return AllowedCorsOrigins is not {Count: > 0} 
                ? null 
                : string.Join(", ", AllowedCorsOrigins.Select(claim => $@"""{claim}"""));
        }
    }
}