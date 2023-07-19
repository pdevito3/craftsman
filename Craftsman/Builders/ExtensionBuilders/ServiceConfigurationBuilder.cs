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

    public void CreateWebAppServiceConfiguration(string srcDirectory, string projectBaseName, bool useCustomErrorHandler)
    {
        var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"{FileNames.WebAppServiceConfiguration()}.cs", projectBaseName);
        var fileText = GetWebApiServiceExtensionText(classPath.ClassNamespace, srcDirectory, projectBaseName, useCustomErrorHandler);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetWebApiServiceExtensionText(string classNamespace, string srcDirectory, string projectBaseName, bool useCustomErrorHandler)
    {
        var corsName = $"{projectBaseName}CorsPolicy";
        var boundaryServiceName = FileNames.BoundaryServiceInterface(projectBaseName);
        
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        var middlewareClassPath = ClassPathHelper.WebApiMiddlewareClassPath(srcDirectory, $"", projectBaseName);
        
        var hellangErrorUsings = "";
        if (!useCustomErrorHandler)
            hellangErrorUsings = $@"{Environment.NewLine}using Hellang.Middleware.ProblemDetails;
using Hellang.Middleware.ProblemDetails.Mvc;";

        var hellangRegistration = "";
        var customErrorHandlerRegistration = "";
        if(!useCustomErrorHandler)
        {
            hellangRegistration =
                $@"{Environment.NewLine}        builder.Services.AddProblemDetails(ProblemDetailsConfigurationExtension.ConfigureProblemDetails)
            .AddProblemDetailsConventions();";
        }
        else
        {
            customErrorHandlerRegistration = "options => options.Filters.Add<ErrorHandlerFilterAttribute>()";
        }

        return @$"namespace {classNamespace};

using {middlewareClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using Configurations;
using System.Text.Json.Serialization;
using Serilog;
using FluentValidation.AspNetCore;{hellangErrorUsings}
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Resources;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

public static class {FileNames.WebAppServiceConfiguration()}
{{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {{
        builder.Services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        builder.Services.AddSingleton(Log.Logger);{hellangRegistration}

        // TODO update CORS for your env
        builder.Services.AddCorsService(""{corsName}"", builder.Environment);
        builder.OpenTelemetryRegistration(builder.Configuration, ""{projectBaseName}"");
        builder.Services.AddInfrastructure(builder.Environment, builder.Configuration);

        builder.Services.AddControllers()
            .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
        builder.Services.AddApiVersioningExtension();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // registers all services that inherit from your base service interface - {boundaryServiceName}
        builder.Services.AddBoundaryServices(Assembly.GetExecutingAssembly());

        builder.Services.AddMvc({customErrorHandlerRegistration});

        builder.Services.AddHealthChecks();
        builder.Services.AddSwaggerExtension(builder.Configuration);
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
