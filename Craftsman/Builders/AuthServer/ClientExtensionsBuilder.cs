namespace Craftsman.Builders.AuthServer;

using Craftsman.Helpers;
using Craftsman.Services;
using Domain;

public class ClientExtensionsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public ClientExtensionsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void Create(string solutionDirectory, string projectBaseName, List<AuthServerTemplate.AuthClient> clients)
    {
        var classPath = ClassPathHelper.AuthServerExtensionsClassPath(solutionDirectory, "ClientExtensions.cs", projectBaseName);
        var fileText = GetFileText(classPath.ClassNamespace, clients);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetFileText(string classNamespace, List<AuthServerTemplate.AuthClient> clients)
    {
        var scopesString = "";
        var defaultScopes = new List<string>() { "profile", "email", "roles", "openid", "web-origins" };
        var clientScopes = clients
            .SelectMany(x => x.Scopes)
            .Distinct()
            .ToList();

        var scopesToAdd = new List<string>();
        scopesToAdd.AddRange(defaultScopes);
        scopesToAdd.AddRange(clientScopes);
        scopesToAdd = scopesToAdd.Distinct().ToList();
        scopesToAdd.ForEach(scope =>
        {
            scopesString += $@"
                ""{scope}"",";
        });
        
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
                {scopesString}
            }},
        }});
    }}
}}";
    }
}
