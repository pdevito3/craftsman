namespace Craftsman.Builders;

using Helpers;
using Services;

public class LocalConfigBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public LocalConfigBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateLocalConfig(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "LocalConfig.cs", projectBaseName);
        var fileText = GetConfigText(classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetConfigText(string classNamespace)
    {
        return @$"namespace {classNamespace};

public static class LocalConfig
{{
    public const string IntegrationTestingEnvName = ""LocalIntegrationTesting"";
    public const string FunctionalTestingEnvName = ""LocalFunctionalTesting"";
}}";
    }
}
