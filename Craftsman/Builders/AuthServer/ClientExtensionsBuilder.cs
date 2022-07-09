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

    public void CreateClientExtensions(string solutionDirectory, string projectBaseName)
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
    public static void AddScope(this Client client, params Output<string>[] scopeName)
    {{
        var defaultScopes = new ClientDefaultScopes($""default-scopes-for-{{client.Name}}"", new ClientDefaultScopesArgs()
        {{
            RealmId = client.RealmId,
            ClientId = client.Id,
            DefaultScopes =
            {{
                ""profile"",
                ""email"",
                ""roles"",
                ""web-origins"",
                scopeName,
            }},
        }});
    }}
}}";
    }
}
