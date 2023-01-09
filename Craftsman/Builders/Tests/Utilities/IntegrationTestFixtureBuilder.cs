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
        var fileText = GetFixtureText(classPath.ClassNamespace, srcDirectory, projectBaseName, dbContextName, provider, isProtected);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetFixtureText(string classNamespace, string srcDirectory, string projectBaseName, string dbContextName, DbProvider provider, bool isProtected)
    {
        var apiClassPath = ClassPathHelper.WebApiProjectClassPath(srcDirectory, projectBaseName);
        var contextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
        var utilsClassPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "", projectBaseName);
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        var configClassPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, "", projectBaseName);
        var envServiceClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        
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
using {apiClassPath.ClassNamespace};
using {envServiceClassPath.ClassNamespace};
using {utilsClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Configurations.Databases;
using DotNet.Testcontainers.Containers.Modules;
using DotNet.Testcontainers.Containers.Modules.Abstractions;
using DotNet.Testcontainers.Containers.Modules.Databases;
using Extensions.Services;
using FluentAssertions;
using FluentAssertions.Extensions;{heimGuardUsing}
using Moq;{sqlServerInteropUsing}
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

[SetUpFixture]
public class TestFixture
{{
    public static IServiceScopeFactory BaseScopeFactory;
    private readonly TestcontainerDatabase _dbContainer = DbSetup();
    private readonly RmqConfig _rmqContainer = RmqSetup();

    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {{
        await _dbContainer.StartAsync();
        {provider.IntegrationTestConnectionStringSetup()}
        await RunMigration();

        await _rmqContainer.Container.StartAsync();
        Environment.SetEnvironmentVariable(EnvironmentService.RmqPortKey, _rmqContainer.Port.ToString());
        Environment.SetEnvironmentVariable(EnvironmentService.RmqHostKey, ""localhost"");
        Environment.SetEnvironmentVariable(EnvironmentService.RmqUsernameKey, ""guest"");
        Environment.SetEnvironmentVariable(EnvironmentService.RmqPasswordKey, ""guest"");
        Environment.SetEnvironmentVariable(EnvironmentService.RmqVirtualHostKey, ""/"");

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {{
            EnvironmentName = Consts.Testing.IntegrationTestingEnvName
        }});
        builder.Configuration.AddEnvironmentVariables();

        builder.ConfigureServices();
        var services = builder.Services;

        // add any mock services here
        services.ReplaceServiceWithSingletonMock<IHttpContextAccessor>();
        services.ReplaceServiceWithSingletonMock<IHeimGuardClient>();

        var provider = services.BuildServiceProvider();
        BaseScopeFactory = provider.GetService<IServiceScopeFactory>();{equivalencyCall}
    }}

    private static async Task RunMigration()
    {{
        var options = new DbContextOptionsBuilder<{dbContextName}>()
            .{provider.DbRegistrationStatement()}(EnvironmentService.DbConnectionString)
            .Options;
        var context = new {dbContextName}(options, null, null, null);
        await context?.Database?.MigrateAsync();
    }}

    {provider.TestingDbSetupMethod(projectBaseName, true)}

    private class RmqConfig
    {{
        public TestcontainersContainer Container {{ get; set; }}
        public int Port {{ get; set; }}
    }}

    private static RmqConfig RmqSetup()
    {{
        // var freePort = DockerUtilities.GetFreePort();
        var freePort = 7741;
        return new RmqConfig
        {{
            Container = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage(""masstransit/rabbitmq"")
                .WithPortBinding(freePort, 4566)
                .WithName($""IntegrationTesting_RMQ_{{Guid.NewGuid()}}"")
                .Build(),
            Port = freePort
        }};
    }}{equivalencyMethod}

    [OneTimeTearDown]
    public async Task RunAfterAnyTests()
    {{
        await _dbContainer.DisposeAsync();
        await _rmqContainer.Container.DisposeAsync();
    }}
}}
";
    }
}
