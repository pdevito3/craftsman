namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class InfrastructureServiceRegistrationBuilder
{
    public sealed record InfrastructureServiceRegistrationBuilderCommand(string SrcDirectory, string ProjectBaseName) : IRequest;

    public class Handler(ICraftsmanUtilities utilities) : IRequestHandler<InfrastructureServiceRegistrationBuilderCommand>
    {
        public Task Handle(InfrastructureServiceRegistrationBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(request.SrcDirectory, $"{FileNames.GetInfraRegistrationName()}.cs", request.ProjectBaseName);
            var fileText = GetServiceRegistrationText(request.SrcDirectory, request.ProjectBaseName, classPath.ClassNamespace);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
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
using Resources;
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
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
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
            options.ServerName = $""{CraftsmanUtilities.GetCleanProjectName(projectBaseName)}-{{env.EnvironmentName}}"";

            if (Consts.HangfireQueues.List().Length > 0)
            {{
                options.Queues = Consts.HangfireQueues.List();
            }}
        }});

    }}
}}";
        }
    }
}
