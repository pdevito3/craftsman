namespace Craftsman.Builders.AuthServer;

using Craftsman.Helpers;
using Craftsman.Services;
using Domain;

public class RealmBuildBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public RealmBuildBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void Create(string solutionDirectory, string projectBaseName, string templateName, List<AuthServerTemplate.AuthClient> clients)
    {
        var classPath = ClassPathHelper.AuthServerExtensionsClassPath(solutionDirectory, "ClientExtensions.cs", projectBaseName);
        var fileText = GetFileText(classPath.ClassNamespace, templateName, clients);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetFileText(string classNamespace, string templateName, List<AuthServerTemplate.AuthClient> clients)
    {
        var realm = @$"var realm = new Realm(""{templateName}-realm"", new RealmArgs
        {{
            RealmName = ""{templateName} Realm"",
            RegistrationAllowed = true,
            ResetPasswordAllowed = true,
            RememberMe = true,
            EditUsernameAllowed = true
        }});";
        var clientsString = "";
        
        foreach (AuthServerTemplate.AuthClient client in clients)
        {
            clientsString += GetNewClientString(client);
        }
        
        
        return @$"namespace {classNamespace};

using Extensions;
using Factories;
using Pulumi;
using Pulumi.Keycloak;

class RealmBuild : Stack
{{
    public RealmBuild()
    {{
        {realm}        
        {clientsString}
    }}
}}";
    }

    private static string GetNewClientString(AuthServerTemplate.AuthClient client)
    {
        var scopeVar = $"{client.Name.Replace(" ", "")}Scope";
        var clientVar = $"{client.Name.Replace(" ", "")}Client";
        
        string redirectUris = GetRedirectUris(client);
        string webOrigins = GetCors(client);

        var clientsString = $@"
        
        var {scopeVar} = ScopeFactory.CreateScope(realm.Id, ""{client.Name}-scopes"");
        var {clientVar} = ClientFactory.CreateCodeFlowClient(realm.Id,
            ""{client.Id}"", 
            ""{client.Secret}"", 
            ""{client.Name}"",
            ""{client.BaseUrl}"",
            {redirectUris},
            {webOrigins}
            );
        {clientVar}.AddScope({scopeVar}.Name);";
        return clientsString;
    }

    private static string GetRedirectUris(AuthServerTemplate.AuthClient client)
    {
        var redirectUrisString = "";
        client.RedirectUris.ForEach(uri =>
        {
            redirectUrisString += $@"
                ""{uri}"",";
        });
        var redirectUris = client.RedirectUris == new List<string>()
            ? "new InputList<string>()"
            : @$"new InputList<string>() 
            {{{redirectUrisString}
            }}";
        return redirectUris;
    }

    private static string GetCors(AuthServerTemplate.AuthClient client)
    {
        var corsString = "";
        client.AllowedCorsOrigins.ForEach(uri =>
        {
            corsString += $@"
                ""{uri}"",";
        });
        var cors = client.RedirectUris == new List<string>()
            ? "new InputList<string>()"
            : @$"new InputList<string>() 
            {{{corsString}
            }}";
        return corsString;
    }
}
