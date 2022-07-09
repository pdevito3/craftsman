namespace Craftsman.Builders.AuthServer;

using Craftsman.Helpers;
using Craftsman.Services;

public class ScopeFactoryBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public ScopeFactoryBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateClientFactory(string solutionDirectory, string projectBaseName)
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

public class ScopeFactory
{{
    public static ClientScope CreateScope(Output<string> realmId, string scopeName)
    {{
        return new ClientScope($""{{scopeName}}-scope"", new ClientScopeArgs()
        {{
            Name = scopeName,
            RealmId = realmId,
        }});
    }}
}}";
    }
}
