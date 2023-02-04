namespace Craftsman.Builders.Tests.Utilities;

using Domain;
using Helpers;
using Services;

public class IntegrationTestFixtureBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public IntegrationTestFixtureBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateFixture(string testDirectory, string srcDirectory, string projectBaseName, string dbContextName, DbProvider provider, bool isProtected)
    {
        var classPath = ClassPathHelper.IntegrationTestProjectRootClassPath(testDirectory, "TestFixture.cs", projectBaseName);
        var fileText = GetFixtureText(classPath.ClassNamespace, srcDirectory, testDirectory, projectBaseName, dbContextName, provider, isProtected);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetFixtureText(string classNamespace, string srcDirectory, string testDirectory, string projectBaseName, string dbContextName, DbProvider provider, bool isProtected)
    {
        var apiClassPath = ClassPathHelper.WebApiProjectClassPath(srcDirectory, projectBaseName);
        var contextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
        var utilsClassPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "", projectBaseName);
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        var configClassPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, "", projectBaseName);
        var envServiceClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        var sharedUtilsClassPath = ClassPathHelper.SharedTestUtilitiesClassPath(testDirectory, "", projectBaseName);
        
        var heimGuardMock = isProtected 
            ? $@"{Environment.NewLine}        services.ReplaceServiceWithSingletonMock<IHeimGuardClient>();" 
            : null;
        var heimGuardUsing = isProtected 
            ? $@"{Environment.NewLine}using HeimGuard;" 
            : null;

        var equivalencyCall = provider == DbProvider.Postgres 
            ? $@"
        SetupDateAssertions();" 
            : null;
        var sqlServerInteropUsing = provider == DbProvider.SqlServer
            ? $"{Environment.NewLine}using System.Runtime.InteropServices;"
            : null;
        var equivalencyMethod = provider == DbProvider.Postgres
            ? $@"

    private static void SetupDateAssertions()
    {{
        // close to equivalency required to reconcile precision differences between EF and Postgres
        AssertionOptions.AssertEquivalencyUsing(options =>
        {{
            options.Using<DateTime>(ctx => ctx.Subject
                .Should()
                .BeCloseTo(ctx.Expectation, 1.Seconds())).WhenTypeIs<DateTime>();
            options.Using<DateTimeOffset>(ctx => ctx.Subject
                .Should()
                .BeCloseTo(ctx.Expectation, 1.Seconds())).WhenTypeIs<DateTimeOffset>();

            return options;
        }});
    }}"
            : null;

        return @$"namespace {classNamespace};

using {configClassPath.ClassNamespace};
using {contextClassPath.ClassNamespace};
using {utilsClassPath.ClassNamespace};
using {sharedUtilsClassPath.ClassNamespace};
using Configurations;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using FluentAssertions.Extensions;{heimGuardUsing}
using Moq;{sqlServerInteropUsing}
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

[CollectionDefinition(nameof(TestFixture))]
public class TestFixtureCollection : ICollectionFixture<TestFixture> {{}}

public class TestFixture : IAsyncLifetime
{{
    public static IServiceScopeFactory BaseScopeFactory;
    private readonly TestcontainerDatabase _dbContainer = DbSetup();
    private readonly RmqConfig _rmqContainer = RmqSetup();

    public async Task InitializeAsync()
    {{
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {{
            EnvironmentName = Consts.Testing.IntegrationTestingEnvName
        }});

        await _dbContainer.StartAsync();
        {provider.IntegrationTestConnectionStringSetup(FileNames.ConnectionStringOptionKey(projectBaseName))}
        await RunMigration(_dbContainer.ConnectionString);

        await _rmqContainer.Container.StartAsync();
        builder.Configuration.GetSection(RabbitMqOptions.SectionName)[RabbitMqOptions.HostKey] = ""localhost"";
        builder.Configuration.GetSection(RabbitMqOptions.SectionName)[RabbitMqOptions.VirtualHostKey] = ""/"";
        builder.Configuration.GetSection(RabbitMqOptions.SectionName)[RabbitMqOptions.UsernameKey] = ""guest"";
        builder.Configuration.GetSection(RabbitMqOptions.SectionName)[RabbitMqOptions.PasswordKey] = ""guest"";
        builder.Configuration.GetSection(RabbitMqOptions.SectionName)[RabbitMqOptions.PortKey] = _rmqContainer.Port.ToString();

        builder.ConfigureServices();
        var services = builder.Services;

        // add any mock services here
        services.ReplaceServiceWithSingletonMock<IHttpContextAccessor>();{heimGuardMock}

        var provider = services.BuildServiceProvider();
        BaseScopeFactory = provider.GetService<IServiceScopeFactory>();{equivalencyCall}
    }}

    private static async Task RunMigration(string connectionString)
    {{
        var options = new DbContextOptionsBuilder<{dbContextName}>()
            .{provider.DbRegistrationStatement()}(connectionString)
            .Options;
        var context = new {dbContextName}(options, null, null, null);
        await context?.Database?.MigrateAsync();
    }}

    {provider.TestingDbSetupMethod(projectBaseName, true)}

    private class RmqConfig
    {{
        public IContainer Container {{ get; set; }}
        public int Port {{ get; set; }}
    }}

    private static RmqConfig RmqSetup()
    {{
        var freePort = DockerUtilities.GetFreePort();
        return new RmqConfig
        {{
            Container = new ContainerBuilder()
                .WithImage(""masstransit/rabbitmq"")
                .WithPortBinding(freePort, 5672)
                .WithName($""IntegrationTesting_RMQ_{{Guid.NewGuid()}}"")
                .Build(),
            Port = freePort
        }};
    }}

    public async Task DisposeAsync()
    {{
        await _dbContainer.DisposeAsync();
        await _rmqContainer.Container.DisposeAsync();
    }}{equivalencyMethod}
}}

public static class ServiceCollectionServiceExtensions
{{
    public static IServiceCollection ReplaceServiceWithSingletonMock<TService>(this IServiceCollection services)
        where TService : class
    {{
        services.RemoveAll(typeof(TService));
        services.AddSingleton(_ => Mock.Of<TService>());
        return services;
    }}
}}
";
    }
}
