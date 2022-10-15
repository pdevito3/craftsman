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
using Resources;
using Sieve.Services;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

public static class {FileNames.WebAppServiceConfiguration()}
{{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {{
        builder.Services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        builder.Services.AddSingleton(Log.Logger);
        // TODO update CORS for your env
        builder.Services.AddCorsService(""{corsName}"", builder.Environment);
        builder.Services.OpenTelemetryRegistration(""{projectBaseName}"");
        builder.Services.AddInfrastructure(builder.Environment);

        builder.Services.AddControllers()
            .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
        builder.Services.AddApiVersioningExtension();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
        builder.Services.AddScoped<SieveProcessor>();

        // registers all services that inherit from your base service interface - {boundaryServiceName}
        builder.Services.AddBoundaryServices(Assembly.GetExecutingAssembly());

        builder.Services
            .AddMvc(options => options.Filters.Add<ErrorHandlerFilterAttribute>())
            .AddJsonOptions(opt => opt.JsonSerializerOptions.AddDateOnlyConverters());

        if(builder.Environment.EnvironmentName != Consts.Testing.FunctionalTestingEnvName)
        {{
            var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
            typeAdapterConfig.Scan(Assembly.GetExecutingAssembly());
            var mapperConfig = new Mapper(typeAdapterConfig);
            builder.Services.AddSingleton<IMapper>(mapperConfig);
        }}

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
}}

// TODO these will be baked into System.Text.Json in .NET 7
public static class DateOnlyConverterExtensions
{{
    public static void AddDateOnlyConverters(this JsonSerializerOptions options)
    {{
        options.Converters.Add(new DateOnlyConverter());
        options.Converters.Add(new DateOnlyNullableConverter());
    }}
}}

public class DateOnlyConverter : JsonConverter<DateOnly>
{{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {{
        if (reader.TryGetDateTime(out var dt))
        {{
            return DateOnly.FromDateTime(dt);
        }};
        var value = reader.GetString();
        if (value == null)
        {{
            return default;
        }}
        var match = new Regex(""^(\\d\\d\\d\\d)-(\\d\\d)-(\\d\\d)(T|\\s|\\z)"").Match(value);
        return match.Success
            ? new DateOnly(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value))
            : default;
    }}

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(""yyyy-MM-dd""));
}}

public class DateOnlyNullableConverter : JsonConverter<DateOnly?>
{{
    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {{
        if (reader.TryGetDateTime(out var dt))
        {{
            return DateOnly.FromDateTime(dt);
        }};
        var value = reader.GetString();
        if (value == null)
        {{
            return default;
        }}
        var match = new Regex(""^(\\d\\d\\d\\d)-(\\d\\d)-(\\d\\d)(T|\\s|\\z)"").Match(value);
        return match.Success
            ? new DateOnly(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value))
            : default;
    }}

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
        => writer.WriteStringValue(value?.ToString(""yyyy-MM-dd""));
}}";
    }
}
