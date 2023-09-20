namespace Craftsman.Builders;

using Helpers;
using Services;

public class InfrastructureServiceRegistrationBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public InfrastructureServiceRegistrationBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }
    public void CreateInfrastructureServiceExtension(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"{FileNames.GetInfraRegistrationName()}.cs", projectBaseName);
        var fileText = GetServiceRegistrationText(srcDirectory, projectBaseName, classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetServiceRegistrationText(string srcDirectory, string projectBaseName, string classNamespace)
    {
        var dbContextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
        var utilsClassPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "", projectBaseName);
        var envServiceClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        var hangfireResourceClassPath = ClassPathHelper.HangfireResourcesClassPath(srcDirectory, "", projectBaseName);
        
        return @$"namespace {classNamespace};

using {dbContextClassPath.ClassNamespace};
using {utilsClassPath.ClassNamespace};
using {envServiceClassPath.ClassNamespace};
using {hangfireResourceClassPath.ClassNamespace};
using Configurations;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;

public static class ServiceRegistration
{{
    public static void AddInfrastructure(this IServiceCollection services, IWebHostEnvironment env, IConfiguration configuration)
    {{
        // DbContext -- Do Not Delete

        services.SetupHangfire(env);

        // Auth -- Do Not Delete
    }}
}}
    
public static class HangfireConfig
{{
    public static void SetupHangfire(this IServiceCollection services, IWebHostEnvironment env)
    {{
        services.AddScoped<IJobContextAccessor, JobContextAccessor>();
        services.AddScoped<IJobWithUserContext, JobWithUserContext>();
        // if you want tags with sql server
        // var tagOptions = new TagsOptions() {{ TagsListStyle = TagsListStyle.Dropdown }};
        
        // var hangfireConfig = new MemoryStorageOptions() {{ }};
        services.AddHangfire(config =>
        {{
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseMemoryStorage()
                .UseColouredConsoleLogProvider()
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                // if you want tags with sql server
                // .UseTagsWithSql(tagOptions, hangfireConfig)
                .UseActivator(new JobWithUserContextActivator(services.BuildServiceProvider()
                    .GetRequiredService<IServiceScopeFactory>()));
        }});
        services.AddHangfireServer(options =>
        {{
            options.WorkerCount = 10;
            options.ServerName = $""PeakLims-{{env.EnvironmentName}}"";

            if (Consts.HangfireQueues.List().Length > 0)
            {{
                options.Queues = Consts.HangfireQueues.List();
            }}
        }});

    }}
}}";
    }
}
