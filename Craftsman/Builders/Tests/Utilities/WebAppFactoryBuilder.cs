namespace Craftsman.Builders.Tests.Utilities;

using System.IO;
using Domain;
using Helpers;
using Services;

public class WebAppFactoryBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public WebAppFactoryBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateWebAppFactory(string testDirectory, string projectName, DbProvider provider, bool addJwtAuthentication)
    {
        var classPath = ClassPathHelper.FunctionalTestProjectRootClassPath(testDirectory, $"{FileNames.GetWebHostFactoryName()}.cs", projectName);
        var fileText = GetWebAppFactoryFileText(classPath, testDirectory, projectName, provider, addJwtAuthentication);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetWebAppFactoryFileText(ClassPath classPath, string testDirectory, string projectBaseName, DbProvider provider, bool addJwtAuthentication)
    {
        var webApiClassPath = ClassPathHelper.WebApiProjectRootClassPath(testDirectory, "", projectBaseName);
        var utilsClassPath = ClassPathHelper.WebApiResourcesClassPath(testDirectory, "", projectBaseName);

        var authUsing = addJwtAuthentication ? $@"
using WebMotions.Fake.Authentication.JwtBearer;" : "";

        var authRegistration = addJwtAuthentication ? $@"
            // add authentication using a fake jwt bearer
            services.AddAuthentication(options =>
            {{
                options.DefaultAuthenticateScheme = FakeJwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = FakeJwtBearerDefaults.AuthenticationScheme;
            }}).AddFakeJwtBearer();
" : "";

        return @$"namespace {classPath.ClassNamespace};

using {utilsClassPath.ClassNamespace};{authUsing}
using WebMotions.Fake.Authentication.JwtBearer;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Configurations.Databases;
using DotNet.Testcontainers.Containers.Modules;
using DotNet.Testcontainers.Containers.Modules.Abstractions;
using DotNet.Testcontainers.Containers.Modules.Databases;
using Microsoft.Extensions.Logging;
using Services;
using Xunit;

[CollectionDefinition(nameof(TestBase))]
public class TestingWebApplicationFactoryCollection : ICollectionFixture<TestingWebApplicationFactory> {{ }}

public class TestingWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{{
    private readonly TestcontainerDatabase _dbContainer = DbSetup();
    private readonly RmqConfig _rmqContainer = RmqSetup();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {{
        builder.UseEnvironment(Consts.Testing.FunctionalTestingEnvName);
        builder.ConfigureLogging(logging =>
        {{
            logging.ClearProviders();
        }});

        builder.ConfigureServices(services =>
        {{{authRegistration}
            var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
            typeAdapterConfig.Scan(Assembly.GetExecutingAssembly());
            var mapperConfig = new Mapper(typeAdapterConfig);
            services.AddSingleton<IMapper>(mapperConfig);
        }});
    }}

    public async Task InitializeAsync()
    {{
        await _dbContainer.StartAsync();
        Environment.SetEnvironmentVariable(EnvironmentService.DbConnectionStringKey, _dbContainer.ConnectionString);
        // migrations applied in MigrationHostedService

        await _rmqContainer.Container.StartAsync();
        Environment.SetEnvironmentVariable(EnvironmentService.RmqPortKey, _rmqContainer.Port.ToString());
        Environment.SetEnvironmentVariable(EnvironmentService.RmqHostKey, ""localhost"");
        Environment.SetEnvironmentVariable(EnvironmentService.RmqUsernameKey, ""guest"");
        Environment.SetEnvironmentVariable(EnvironmentService.RmqPasswordKey, ""guest"");
        Environment.SetEnvironmentVariable(EnvironmentService.RmqVirtualHostKey, ""/"");
    }}

    public new async Task DisposeAsync() 
    {{
        await _dbContainer.DisposeAsync();
        await _rmqContainer.Container.DisposeAsync();
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
                .WithName($""FunctionalTesting_RMQ_{{Guid.NewGuid()}}"")
                .Build(),
            Port = freePort
        }};
    }}
}}";
    }
}
