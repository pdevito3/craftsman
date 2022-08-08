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
        var boundaryServiceName = FileNames.BoundaryServiceInterface(projectBaseName);

        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        var middlewareClassPath = ClassPathHelper.WebApiMiddlewareClassPath(srcDirectory, $"", projectBaseName);

        return @$"namespace {classNamespace};

using {middlewareClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using System.Text.Json.Serialization;
using Serilog;
using FluentValidation.AspNetCore;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Sieve.Services;
using System.Reflection;

public static class {FileNames.WebAppServiceConfiguration()}
{{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {{
        builder.Services.AddSingleton(Log.Logger);
        // TODO update CORS for your env
        builder.Services.AddCorsService(""{corsName}"", builder.Environment);
        builder.Services.OpenTelemetryRegistration(""{projectBaseName}"");
        builder.Services.AddInfrastructure(builder.Configuration,builder.Environment);

        // using Newtonsoft.Json to support PATCH docs since System.Text.Json does not support them https://github.com/dotnet/aspnetcore/issues/24333
        // if you are not using PatchDocs and would prefer to use System.Text.Json, you can remove The `AddNewtonSoftJson()` line
        builder.Services.AddControllers(options => options.UseDateOnlyTimeOnlyStringConverters())
            .AddJsonOptions(options => options.UseDateOnlyTimeOnlyStringConverters())
            .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles)
            .AddNewtonsoftJson();
        builder.Services.AddApiVersioningExtension();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
        builder.Services.AddScoped<SieveProcessor>();

        // registers all services that inherit from your base service interface - {boundaryServiceName}
        builder.Services.AddBoundaryServices(Assembly.GetExecutingAssembly());

        builder.Services.AddMvc(options => options.Filters.Add<ErrorHandlerFilterAttribute>());

        var config = TypeAdapterConfig.GlobalSettings;
        builder.Services.AddSingleton(config);
        builder.Services.AddScoped<IMapper, ServiceMapper>();

        builder.Services.AddHealthChecks();
        builder.Services.AddSwaggerExtension();
    }}

    /// <summary>
    /// Registers all services in the assembly of the given interface.
    /// </summary>
    private static void AddBoundaryServices(this IServiceCollection services, params Assembly[] assemblies)
    {{
        if (!assemblies.Any())
            throw new ArgumentException(""No assemblies found to scan. Supply at least one assembly to scan for handlers."");

        foreach (var assembly in assemblies)
        {{
            var rules = assembly.GetTypes()
                .Where(x => !x.IsAbstract && x.IsClass && x.GetInterface(nameof({boundaryServiceName})) == typeof({boundaryServiceName}));

            foreach (var rule in rules)
            {{
                foreach (var @interface in rule.GetInterfaces())
                {{
                    services.Add(new ServiceDescriptor(@interface, rule, ServiceLifetime.Scoped));
                }}
            }}
        }}
    }}
}}";
    }
}
