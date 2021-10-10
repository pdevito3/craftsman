namespace Craftsman.Builders
{
    using System;
    using System.Collections.Generic;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text;

    public class AuthServerConfigBuilder
    {
        public static void CreateConfig(string projectDirectory, AuthServerTemplate authServer, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.AuthServerConfigClassPath(projectDirectory, "Config.cs", authServer.Name);
            var fileText = GetConfigText(classPath.ClassNamespace, authServer);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        
        public static string GetConfigText(string classNamespace, AuthServerTemplate authServer)
        {
            var apiResources = authServer.Apis.Aggregate("", (current, api) => current + ApiResourceTextBuilder(api));
            var apiScopes = authServer.Scopes.Aggregate("", (current, scope) => current + ApiScopeTextBuilder(scope));

            return @$"namespace {classNamespace}
{{
    using Duende.IdentityServer.Models;
    using System.Collections.Generic;

    public static class Config
    {{
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {{
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            }};

        // Api Resource is your Api as a whole. 
        // Api with multiple endpoints has a name that clients can ask for
        // we can have an api with various resources
        // have multiple scopes for the api (e.g. full_access, readr_only
        // when someone asks for a token for one of these scopes, which claims should be included in the scope
        // You can add these `UserClaims` to the Api level so that, regardless of which scope you are asking for, they will be included (e.g. name, email)
        // You can then add specific claims that will be included when requesting that particular scope
        // ***Be aware, that scopes are purely for authorizing clients, not users.**
        public static IEnumerable<ApiResource> ApiResources =>
            new List<ApiResource>
            {{{apiResources}
            }};
        
        // allow access to identity information. client level rules of who can access what (e.g. read:sample, read:order, create:order, read:report)
        // this will be in the audience claim and will be checked by the jwt middleware to grant access or not
        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {{{apiScopes}
            }};

        public static IEnumerable<Client> Clients =>
            new Client[]
            {{
                // m2m client credentials flow client
                new Client
                {{
                    ClientId = ""m2m.client"",
                    ClientName = ""Client Credentials Client"",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = {{ new Secret(""511536EF-F270-4058-80CA-1C89C192F69A"".Sha256()) }},

                    AllowedScopes = {{ ""scope1"" }}
                }},

                // interactive client using code flow + pkce
                new Client
                {{
                    ClientId = ""interactive"",
                    ClientName = ""Interactive Client"",
                    ClientSecrets = {{ new Secret(""secret"".Sha256()) }},
                    
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = {{""https://localhost:5375/swagger/oauth2-redirect.html""}},
                    PostLogoutRedirectUris = {{ ""http://localhost:5375/"" }},
                    FrontChannelLogoutUri =    ""http://localhost:5375/signout-oidc"",
                    AllowedCorsOrigins = {{""https://localhost:5375""}},
                    
                    AllowOfflineAccess = true,
                    RequirePkce = true,
                    RequireClientSecret = true,
                    AllowedScopes = {{ ""openid"", ""profile"",
                        ""recipes.read"",
                        ""recipes.add"",
                        ""recipes.update"",
                        ""recipes.delete"" }}
                }},

                // interactive client using code flow + pkce
                new Client
                {{
                    ClientId = ""interactive.bff"",
                    ClientName = ""Interactive BFF"",
                    ClientSecrets = {{ new Secret(""secret"".Sha256()) }},
                    
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = {{ ""https://localhost:4301/signin-oidc"" }},
                    FrontChannelLogoutUri = ""https://localhost:4301/signout-oidc"",
                    PostLogoutRedirectUris = {{ ""https://localhost:4301/signout-callback-oidc"" }},
                    AllowedCorsOrigins = {{""https://localhost:5375"", ""https://localhost:4301""}},
                    
                    AllowOfflineAccess = true,
                    RequirePkce = true,
                    RequireClientSecret = true,
                    AllowedScopes = {{ ""openid"", ""profile"",
                        ""recipes.read"",
                        ""recipes.add"",
                        ""recipes.update"",
                        ""recipes.delete"" }}
                }},
            }};
    }}
}}";
        }
        
        private static string ApiResourceTextBuilder(AuthApi api)
        {
            return $@"{Environment.NewLine}                new ApiResource(""{api.Name}"", ""{api.DisplayName}"")
                {{
                    Scopes = {{ { api.GetScopeNameString() } }},
                    ApiSecrets = {{ { api.GetSecretsString() } }},
                    UserClaims = {{ { api.GetClaimsString() } }},
                }},";
        }
        
        private static string ApiScopeTextBuilder(AuthScope scope)
        {
            return $@"{Environment.NewLine}                new ApiScope(""{scope.Name}"", ""{scope.DisplayName}"", new[] {{ { scope.GetClaimsString() } }}),";
        }
    }
}