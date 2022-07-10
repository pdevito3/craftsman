namespace Craftsman.Domain;

using System;
using System.Collections.Generic;
using System.Linq;
using Enums;

public class AuthServerTemplate
{
    public string Name { get; set; }

    public string RealmName { get; set; }

    public int Port { get; set; }

    public string Username { get; set; } = "admin";

    public string Password { get; set; } = "admin";

    public List<AuthClient> Clients { get; set; } = new List<AuthClient>();

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

        public string Secret { get; set; } = Guid.NewGuid().ToString();

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

        private string _baseUrl;

        public string BaseUrl
        {
            get => _baseUrl;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException($"Base url is required");

                _baseUrl = value;
                
                if (_baseUrl.EndsWith("/"))
                    _baseUrl = _baseUrl.Substring(0, _baseUrl.Length - 1);
            }
        }

        public List<string> RedirectUris { get; set; } = new List<string>();

        public List<string> AllowedCorsOrigins { get; set; } = new List<string>();

        public List<string> Scopes { get; set; } = new List<string>();

        public string GetRedirectUrisString()
        {
            return RedirectUris is not { Count: > 0 }
                ? null
                : string.Join(", ", RedirectUris.Select(claim => $@"""{claim}"""));
        }

        public string GetAllowedCorsOriginsString()
        {
            return AllowedCorsOrigins is not { Count: > 0 }
                ? null
                : string.Join(", ", AllowedCorsOrigins.Select(claim => $@"""{claim}"""));
        }
    }
}