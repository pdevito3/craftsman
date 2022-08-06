namespace Craftsman.Builders.AuthServer;

using Craftsman.Helpers;
using Craftsman.Services;

public class PulumiYamlBuilders
{
    private readonly ICraftsmanUtilities _utilities;

    public PulumiYamlBuilders(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateBaseFile(string solutionDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.AuthServerProjectRootClassPath(solutionDirectory, "Pulumi.yaml", projectBaseName);
        var fileText = GetBaseFileText(projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public void CreateDevConfig(string solutionDirectory, string projectBaseName, int? port, string username, string password)
    {
        var classPath = ClassPathHelper.AuthServerProjectRootClassPath(solutionDirectory, "Pulumi.dev.yaml", projectBaseName);
        var fileText = GetDevConfigText(port, username, password);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetBaseFileText(string projectBaseName)
    {
        return @$"name: {projectBaseName}
runtime: dotnet
description: Local setup for {projectBaseName}
";
    }

    private static string GetDevConfigText(int? port, string username, string password)
    {
        return @$"config:
  keycloak:url: http://localhost:{port}
  keycloak:clientId: admin-cli
  keycloak:username: {username}
  keycloak:password: {password}
";
    }
}
