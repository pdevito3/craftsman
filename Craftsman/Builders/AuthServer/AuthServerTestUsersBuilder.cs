namespace Craftsman.Builders.AuthServer
{
    using Helpers;
    using Services;
    using static Helpers.ConstMessages;

    public class AuthServerTestUsersBuilder
    {
        private readonly ICraftsmanUtilities _utilities;

        public AuthServerTestUsersBuilder(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }

        public void CreateTestModels(string projectDirectory, string authServerProjectName)
        {
            var classPath = ClassPathHelper.AuthServerSeederClassPath(projectDirectory, "TestUsers.cs", authServerProjectName);
            var fileText = GetTestUserText(classPath.ClassNamespace);
            _utilities.CreateFile(classPath, fileText);
        }
        
        public static string GetTestUserText(string classNamespace)
        {
            return @$"{DuendeDisclosure}// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


namespace {classNamespace};

using IdentityModel;
using System.Security.Claims;
using System.Text.Json;
using Duende.IdentityServer;
using Duende.IdentityServer.Test;

public class TestUsers
{{
    public static List<TestUser> Users
    {{
        get
        {{
            var address = new
            {{
                street_address = ""One Hacker Way"",
                locality = ""Heidelberg"",
                postal_code = 69118,
                country = ""Germany""
            }};
            
            return new List<TestUser>
            {{
                new TestUser
                {{
                    SubjectId = ""818727"",
                    Username = ""alice"",
                    Password = ""alice"",
                    Claims =
                    {{
                        new Claim(JwtClaimTypes.Name, ""Alice Smith""),
                        new Claim(JwtClaimTypes.GivenName, ""Alice""),
                        new Claim(JwtClaimTypes.FamilyName, ""Smith""),
                        new Claim(JwtClaimTypes.Email, ""AliceSmith@email.com""),
                        new Claim(JwtClaimTypes.EmailVerified, ""true"", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.Role, ""SuperAdmin""),
                        new Claim(JwtClaimTypes.WebSite, ""http://alice.com""),
                        new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(address), IdentityServerConstants.ClaimValueTypes.Json)
                    }}
                }},
                new TestUser
                {{
                    SubjectId = ""88421113"",
                    Username = ""bob"",
                    Password = ""bob"",
                    Claims =
                    {{
                        new Claim(JwtClaimTypes.Name, ""Bob Smith""),
                        new Claim(JwtClaimTypes.GivenName, ""Bob""),
                        new Claim(JwtClaimTypes.FamilyName, ""Smith""),
                        new Claim(JwtClaimTypes.Email, ""BobSmith@email.com""),
                        new Claim(JwtClaimTypes.EmailVerified, ""true"", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.Role, ""User""),
                        new Claim(JwtClaimTypes.WebSite, ""http://bob.com""),
                        new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(address), IdentityServerConstants.ClaimValueTypes.Json)
                    }}
                }},
                new TestUser
                {{
                    SubjectId = ""884262290"",
                    Username = ""john"",
                    Password = ""john"",
                    Claims =
                    {{
                        new Claim(JwtClaimTypes.Name, ""John Smith""),
                        new Claim(JwtClaimTypes.GivenName, ""John""),
                        new Claim(JwtClaimTypes.FamilyName, ""Smith""),
                        new Claim(JwtClaimTypes.Email, ""JohnSmith@email.com""),
                        new Claim(JwtClaimTypes.EmailVerified, ""true"", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.WebSite, ""http://john.com""),
                        new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(address), IdentityServerConstants.ClaimValueTypes.Json)
                    }}
                }}
            }};
        }}
    }}
}}";
        }
    }
}