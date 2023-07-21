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
        var contextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
        var utilsClassPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "", projectBaseName);
        var configClassPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, "", projectBaseName);
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
using FluentAssertions;
using FluentAssertions.Extensions;{heimGuardUsing}
using Moq;{provider.TestingDbSetupUsings()}
using Testcontainers.RabbitMq;
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
    public static IServiceScopeFactory BaseScopeFactory;{provider.TestingContainerDb()}
    private RabbitMqContainer _rmqContainer;

    public async Task InitializeAsync()
    {{
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {{
            EnvironmentName = Consts.Testing.IntegrationTestingEnvName
        }});

        {provider.TestingDbSetupMethod(projectBaseName, true)}

        var freePort = DockerUtilities.GetFreePort();
        _rmqContainer = new RabbitMqBuilder()
            .WithPortBinding(freePort, 5672)
            .Build();
        await _rmqContainer.StartAsync();
        builder.Configuration.GetSection(RabbitMqOptions.SectionName)[RabbitMqOptions.HostKey] = ""localhost"";
        builder.Configuration.GetSection(RabbitMqOptions.SectionName)[RabbitMqOptions.VirtualHostKey] = ""/"";
        builder.Configuration.GetSection(RabbitMqOptions.SectionName)[RabbitMqOptions.UsernameKey] = ""guest"";
        builder.Configuration.GetSection(RabbitMqOptions.SectionName)[RabbitMqOptions.PasswordKey] = ""guest"";
        builder.Configuration.GetSection(RabbitMqOptions.SectionName)[RabbitMqOptions.PortKey] = _rmqContainer.GetConnectionString();

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

    public async Task DisposeAsync()
    {{        
        {provider.DbDisposal()}
        await _rmqContainer.DisposeAsync();
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
