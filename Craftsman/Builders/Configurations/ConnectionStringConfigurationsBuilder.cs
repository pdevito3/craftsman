namespace Craftsman.Builders.Configurations;

using Craftsman.Helpers;
using Craftsman.Services;
using Domain;

public class ConnectionStringConfigurationsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public ConnectionStringConfigurationsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateConfig(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiConfigurationsClassPath(srcDirectory, $"{FileNames.ConnectionStringOptions()}.cs", projectBaseName);
        var fileText = GetConfigText(classPath.ClassNamespace, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetConfigText(string classNamespace, string projectBaseName)
    {
        return @$"namespace {classNamespace};

public class {FileNames.ConnectionStringOptions()}
{{
    public const string SectionName = ""ConnectionStrings"";
    public const string {FileNames.ConnectionStringOptionKey(projectBaseName)} = ""{CraftsmanUtilities.GetCleanProjectName(projectBaseName)}"";

    public string {CraftsmanUtilities.GetCleanProjectName(projectBaseName)} {{ get; set; }} = String.Empty;
}}

public static class ConnectionStringOptionsExtensions
{{
    public static ConnectionStringOptions GetConnectionStringOptions(this IConfiguration configuration)
        => configuration.GetSection(ConnectionStringOptions.SectionName).Get<ConnectionStringOptions>();
}}";
    }
}
