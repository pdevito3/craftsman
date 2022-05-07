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
        var fileText = GetWebApiServiceExtensionText(classPath.ClassNamespace, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetWebApiServiceExtensionText(string classNamespace, string projectBaseName)
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

        // using Newtonsoft.Json to support PATCH docs since System.Text.Json does not support them https://github.com/dotnet/aspnetcore/issues/24333
        // if you are not using PatchDocs and would prefer to use System.Text.Json, you can remove The `AddNewtonSoftJson()` line
        builder.Services.AddControllers()
            .AddNewtonsoftJson()
            .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
        builder.Services.AddApiVersioningExtension();
        builder.Services.AddWebApiServices();
        builder.Services.AddHealthChecks();
        builder.Services.AddSwaggerExtension();
    }}
}}";
    }
}
