namespace Craftsman.Builders.Tests.Utilities;

using Domain;
using Helpers;
using Services;
using MediatR;

public static class IntegrationTestFixtureBuilder
{
    public sealed record Command(string DbContextName, DbProvider Provider, bool IsProtected) : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.IntegrationTestProjectRootClassPath(scaffoldingDirectoryStore.TestDirectory, "TestFixture.cs", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFixtureText(classPath.ClassNamespace, scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.TestDirectory, scaffoldingDirectoryStore.ProjectBaseName, request.DbContextName, request.Provider, request.IsProtected);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
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

        return $@"namespace {classNamespace};

using {configClassPath.ClassNamespace};
using {contextClassPath.ClassNamespace};
using {utilsClassPath.ClassNamespace};
using {sharedUtilsClassPath.ClassNamespace};
using Resources;
using FluentAssertions;
using FluentAssertions.Extensions;
using Hangfire;{heimGuardUsing}
using NSubstitute;{provider.TestingDbSetupUsings()}
using Testcontainers.RabbitMq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using static Resources.{FileNames.OptionsClassName(projectBaseName)};

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
        services.ReplaceServiceWithSingletonMock<IHttpContextAccessor>();
        services.ReplaceServiceWithSingletonMock<IBackgroundJobClient>();{heimGuardMock}

        var provider = services.BuildServiceProvider();
        BaseScopeFactory = provider.GetService<IServiceScopeFactory>();
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
    }}
}}

public static class ServiceCollectionServiceExtensions
{{
    public static IServiceCollection ReplaceServiceWithSingletonMock<TService>(this IServiceCollection services)
        where TService : class
    {{
        services.RemoveAll(typeof(TService));
        services.AddSingleton(_ => Substitute.For<TService>());
        return services;
    }}
}}";
    }
}