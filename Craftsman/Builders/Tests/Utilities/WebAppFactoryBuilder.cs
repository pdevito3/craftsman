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
        var sharedUtilsClassPath = ClassPathHelper.SharedTestUtilitiesClassPath(testDirectory, "", projectBaseName);
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

using {utilsClassPath.ClassNamespace};
using {sharedUtilsClassPath.ClassNamespace};{authUsing}
using Configurations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;{provider.TestingDbSetupUsings()}
using Testcontainers.RabbitMq;
using Microsoft.Extensions.Logging;
using Xunit;

[CollectionDefinition(nameof(TestBase))]
public class TestingWebApplicationFactoryCollection : ICollectionFixture<TestingWebApplicationFactory> {{ }}

public class TestingWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{{
    {provider.TestingContainerDb()}
    private RabbitMqContainer _rmqContainer;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {{
        builder.UseEnvironment(Consts.Testing.FunctionalTestingEnvName);
        builder.ConfigureLogging(logging =>
        {{
            logging.ClearProviders();
        }});
        
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {{
            var functionalConfig = new ConfigurationBuilder()
                .AddJsonFile(""appsettings.json"")
                .AddEnvironmentVariables()
                .Build();
            functionalConfig.GetSection(""JaegerHost"").Value = ""localhost"";
            configurationBuilder.AddConfiguration(functionalConfig);
        }});

        builder.ConfigureServices(services =>
        {{{authRegistration}
        }});
    }}

    public async Task InitializeAsync()
    {{
        {provider.TestingDbSetupMethod(projectBaseName, false)}
        // migrations applied in MigrationHostedService

        var freePort = DockerUtilities.GetFreePort();
        _rmqContainer = new RabbitMqBuilder()
            .WithPortBinding(freePort, 5672)
            .Build();
        await _rmqContainer.StartAsync();
        Environment.SetEnvironmentVariable($""{{RabbitMqOptions.SectionName}}__{{RabbitMqOptions.HostKey}}"", ""localhost"");
        Environment.SetEnvironmentVariable($""{{RabbitMqOptions.SectionName}}__{{RabbitMqOptions.VirtualHostKey}}"", ""/"");
        Environment.SetEnvironmentVariable($""{{RabbitMqOptions.SectionName}}__{{RabbitMqOptions.UsernameKey}}"", ""guest"");
        Environment.SetEnvironmentVariable($""{{RabbitMqOptions.SectionName}}__{{RabbitMqOptions.PasswordKey}}"", ""guest"");
        Environment.SetEnvironmentVariable($""{{RabbitMqOptions.SectionName}}__{{RabbitMqOptions.PortKey}}"", _rmqContainer.GetConnectionString());
    }}

    public new async Task DisposeAsync() 
    {{
        {provider.DbDisposal()}
        await _rmqContainer.DisposeAsync();
    }}
}}";
    }
}
