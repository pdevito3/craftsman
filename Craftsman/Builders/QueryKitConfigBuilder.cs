namespace Craftsman.Builders;

using Helpers;
using Services;

public class QueryKitConfigBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public QueryKitConfigBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateConfig(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "CustomQueryKitConfiguration.cs", projectBaseName);
        var fileText = GetConfigText(classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetConfigText(string classNamespace)
    {
        return @$"namespace {classNamespace};

using QueryKit.Configuration;

public class CustomQueryKitConfiguration : QueryKitConfiguration
{{
    public CustomQueryKitConfiguration(Action<QueryKitSettings>? configureSettings = null)
        : base(settings => 
        {{
            // configure custom global settings here
            // settings.EqualsOperator = ""eq"";

            configureSettings?.Invoke(settings);
        }})
    {{
    }}
}}";
    }
}
