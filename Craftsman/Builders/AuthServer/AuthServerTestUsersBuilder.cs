namespace Craftsman.Builders.AuthServer
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using Enums;
    using Helpers;
    using Models;
    using static Helpers.ConstMessages;

    public class AuthServerTestUsersBuilder
    {
        public static void CreateTestModels(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.AuthServerSeederClassPath(projectDirectory, "TestUsers.cs", authServerProjectName);
            var fileText = GetTestUserText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
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
                }}
            }};
        }}
    }}
}}";
        }
    }
}