namespace Craftsman.Builders.Configurations;

using Craftsman.Helpers;
using Craftsman.Services;

public class RabbitMqConfigurationsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public RabbitMqConfigurationsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateConfig(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiConfigurationsClassPath(srcDirectory, $"{FileNames.RabbitMqOptions()}.cs", projectBaseName);
        var fileText = GetConfigText(classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetConfigText(string classNamespace)
    {
        return @$"namespace {classNamespace};

public class {FileNames.RabbitMqOptions()}
{{
    public const string SectionName = ""RabbitMq"";
    public const string HostKey = nameof(Host);
    public const string VirtualHostKey = nameof(VirtualHost);
    public const string UsernameKey = nameof(Username);
    public const string PasswordKey = nameof(Password);
    public const string PortKey = nameof(Port);

    public string Host {{ get; set; }} = String.Empty; // ""localhost"";
    public string VirtualHost {{ get; set; }} = String.Empty; // ""/"";
    public string Username {{ get; set; }} = String.Empty; // ""guest"";
    public string Password {{ get; set; }} = String.Empty; // ""guest"";
    public string Port {{ get; set; }} = String.Empty; // ""57481"";
}}

public static class RabbitMqOptionsExtensions
{{
    public static RabbitMqOptions GetRabbitMqOptions(this IConfiguration configuration)
        => configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>();
}}";
    }
}
