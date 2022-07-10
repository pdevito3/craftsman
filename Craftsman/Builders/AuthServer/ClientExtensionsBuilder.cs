namespace Craftsman.Builders.AuthServer;

using Craftsman.Helpers;
using Craftsman.Services;

public class ClientExtensionsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public ClientExtensionsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void Create(string solutionDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.AuthServerExtensionsClassPath(solutionDirectory, "ClientExtensions.cs", projectBaseName);
        var fileText = GetFileText(classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetFileText(string classNamespace)
    {
        return @$"namespace {classNamespace};

using Pulumi;
using Pulumi.Keycloak.OpenId;

public static class ClientExtensions
{{
    public static void ExtendDefaultScopes(this Client client, params Output<string>[] scopeNames)
    {{
        // TODO new guid causes rebuild of this every time with a new name, but it breaks and says their is a name collision 
        // without it though, even only calling client name once
        var defaultScopes = new ClientDefaultScopes($""default-scopes-for-{{client.Name}}-{{Guid.NewGuid()}}"", new ClientDefaultScopesArgs()
        {{
            RealmId = client.RealmId,
            ClientId = client.Id,
            DefaultScopes =
            {{
                ""profile"",
                ""email"",
                ""roles"",
                ""web-origins"",
                scopeNames,
            }},
        }});
    }}
}}";
    }
}
