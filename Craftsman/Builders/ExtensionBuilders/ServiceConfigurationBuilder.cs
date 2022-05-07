namespace Craftsman.Builders.ExtensionBuilders;

using Helpers;
using Services;

public class ServiceConfigurationBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public ServiceConfigurationBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateWebAppServiceConfiguration(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"{FileNames.WebAppServiceConfiguration()}.cs", projectBaseName);
        var fileText = GetWebApiServiceExtensionText(classPath.ClassNamespace, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetWebApiServiceExtensionText(string classNamespace, string srcDirectory, string projectBaseName)
    {
        var corsName = $"{projectBaseName}CorsPolicy";

        return @$"namespace {classNamespace};

using System.Text.Json.Serialization;
using Serilog;
using Services;

public static class {FileNames.WebAppServiceConfiguration()}
{{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {{
        builder.Services.AddSingleton(Log.Logger);
        // TODO update CORS for your env
        builder.Services.AddCorsService(""{corsName}"", builder.Environment);
        builder.Services.OpenTelemetryRegistration(""{projectBaseName}"");
        builder.Services.AddInfrastructure(builder.Environment);
        builder.Services.AddMassTransitServices(builder.Environment);
        builder.Services.AddControllers()
            .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
        builder.Services.AddApiVersioningExtension();
        builder.Services.AddWebApiServices();
        builder.Services.AddHealthChecks();
        builder.Services.AddSwaggerExtension();
    }}
}}";
    }
}
