namespace NewCraftsman.Services;

using System.IO.Abstractions;
using Builders;
using Builders.Auth;
using Builders.Docker;
using Builders.Tests.FunctionalTests;
using Builders.Tests.UnitTests;
using Builders.Tests.Utilities;
using Domain;
using Helpers;
using Spectre.Console;

public class ApiScaffoldingService
{
    private IAnsiConsole _console;
    private readonly IConsoleWriter _consoleWriter;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;
    
    public ApiScaffoldingService(IAnsiConsole console, IConsoleWriter consoleWriter, ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore)
    {
        _console = console;
        _consoleWriter = consoleWriter;
        _utilities = utilities;
        _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
    }
    
    public void ScaffoldApi(string buildSolutionDirectory, ApiTemplate template, IFileSystem fileSystem)
    {
        var projectName = template.ProjectName;
        _console.Status()
            .AutoRefresh(true)
            .Spinner(Spinner.Known.Dots2)
            .Start($"[yellow]Creating {template.ProjectName} [/]", ctx =>
            {
                FileParsingHelper.RunPrimaryKeyGuard(template.Entities);
                FileParsingHelper.RunSolutionNameAssignedGuard(projectName);
                FileParsingHelper.SolutionNameDoesNotEqualEntityGuard(projectName, template.Entities);

                // add an accelerate.config.yaml file to the root?
                var bcDirectory = $"{buildSolutionDirectory}{Path.DirectorySeparatorChar}{projectName}";
                var srcDirectory = Path.Combine(bcDirectory, "src");
                var testDirectory = Path.Combine(bcDirectory, "tests");
                fileSystem.Directory.CreateDirectory(srcDirectory);
                fileSystem.Directory.CreateDirectory(testDirectory);

                ctx.Spinner(Spinner.Known.BouncingBar);
                ctx.Status($"[bold blue]Building {projectName} Projects [/]");
                new SolutionBuilder(fileSystem, _utilities, _consoleWriter)
                    .AddProjects(buildSolutionDirectory,
                        srcDirectory,
                        testDirectory,
                        template.DbContext.ProviderEnum,
                        template.DbContext.DatabaseName,
                        projectName, template.AddJwtAuthentication);

                // add all files based on the given template config
                ctx.Status($"[bold blue]Scaffolding Files for {projectName} [/]");
                RunTemplateBuilders(bcDirectory, srcDirectory, testDirectory, template, fileSystem);
                _consoleWriter.WriteLogMessage($"File scaffolding for {template.ProjectName} was successful");
            });
    }

    private void RunTemplateBuilders(string boundedContextDirectory, string srcDirectory, string testDirectory, ApiTemplate template)
    {
        var projectBaseName = template.ProjectName;
        
        // docker config data transform
        template.DockerConfig.ProjectName = template.ProjectName;
        template.DockerConfig.Provider = template.DbContext.Provider;

        // get solution dir from bcDir
        var solutionDirectory = Directory.GetParent(boundedContextDirectory)?.FullName;
        _utilities.IsSolutionDirectoryGuard(solutionDirectory);

        // base files needed before below is ran
        template.DockerConfig.ApiPort ??= template.Port; // set to the launch settings port if needed... really need to refactor to a domain layer and dto layer 😪
        if(template.AddJwtAuthentication)
            template.DockerConfig.AuthServerPort ??= template?.Environment?.AuthSettings?.AuthorizationUrl
                .Replace("localhost", "")
                .Replace("https://", "")
                .Replace("http://", "")
                .Replace(":", ""); // this is fragile and i hate it. also not in domain...
        DbContextBuilder.CreateDbContext(srcDirectory,
            template.Entities,
            template.DbContext.ContextName,
            template.DbContext.Provider,
            template.DbContext.DatabaseName,
            template.DockerConfig.DbConnectionString,
            template.DbContext.NamingConventionEnum,
            template.UseSoftDelete,
            projectBaseName,
            fileSystem
        );
        ApiRoutesBuilder.CreateClass(testDirectory, projectBaseName, fileSystem);
        
        if (template.AddJwtAuthentication)
        {
            PermissionsBuilder.GetPermissions(srcDirectory, projectBaseName, fileSystem); // <-- needs to run before entity features
            RolesBuilder.GetRoles(solutionDirectory, fileSystem);
            UserPolicyHandlerBuilder.CreatePolicyBuilder(solutionDirectory, srcDirectory, projectBaseName, template.DbContext.ContextName, fileSystem);
            InfrastructureServiceRegistrationModifier.InitializeAuthServices(srcDirectory, projectBaseName);
            EntityScaffolding.ScaffoldRolePermissions(solutionDirectory,
                srcDirectory,
                testDirectory,
                projectBaseName,
                template.DbContext.ContextName,
                template.SwaggerConfig.AddSwaggerComments,
                template.UseSoftDelete,
                fileSystem);
        }
        
        //entities
        EntityScaffolding.ScaffoldEntities(solutionDirectory,
            srcDirectory,
            testDirectory,
            projectBaseName,
            template.Entities,
            template.DbContext.ContextName,
            template.SwaggerConfig.AddSwaggerComments,
            template.UseSoftDelete,
            fileSystem);

        // environments
        Utilities.AddStartupEnvironmentsWithServices(
            srcDirectory,
            template.DbContext.DatabaseName,
            template.Environment,
            template.SwaggerConfig,
            template.Port,
            projectBaseName,
            template.DockerConfig,
            fileSystem
        );

        // unit tests, test utils, and one offs∂
        PagedListTestBuilder.CreateTests(srcDirectory, testDirectory, projectBaseName);
        IntegrationTestFixtureBuilder.CreateFixture(testDirectory, projectBaseName, template.DbContext.ContextName, template.DbContext.DatabaseName, template.DbContext.Provider, fileSystem);
        IntegrationTestBaseBuilder.CreateBase(testDirectory, projectBaseName, template.DbContext.Provider, fileSystem);
        DockerUtilitiesBuilder.CreateGeneralUtilityClass(testDirectory, projectBaseName, template.DbContext.Provider, fileSystem);
        DockerUtilitiesBuilder.CreateDockerDatabaseUtilityClass(testDirectory, projectBaseName, template.DbContext.Provider, fileSystem);
        WebAppFactoryBuilder.CreateWebAppFactory(testDirectory, projectBaseName, template.DbContext.ContextName, template.AddJwtAuthentication);
        FunctionalTestBaseBuilder.CreateBase(testDirectory, projectBaseName, template.DbContext.ContextName, fileSystem);
        HealthTestBuilder.CreateTests(testDirectory, projectBaseName, fileSystem);
        HttpClientExtensionsBuilder.Create(testDirectory, projectBaseName);
        EntityBuilder.CreateBaseEntity(srcDirectory, projectBaseName, template.UseSoftDelete, fileSystem);
        CurrentUserServiceTestBuilder.CreateTests(testDirectory, projectBaseName, fileSystem);

        //services
        CurrentUserServiceBuilder.GetCurrentUserService(srcDirectory, projectBaseName, fileSystem);
        SwaggerBuilder.AddSwagger(srcDirectory, template.SwaggerConfig, template.ProjectName, template.AddJwtAuthentication, template.PolicyName, projectBaseName, fileSystem);

        if (template.Bus.AddBus)
            AddBusCommand.AddBus(template.Bus, srcDirectory, testDirectory, projectBaseName, solutionDirectory, fileSystem);

        if (template.Consumers.Count > 0)
            AddConsumerCommand.AddConsumers(template.Consumers, projectBaseName, solutionDirectory, srcDirectory, testDirectory, fileSystem);

        if (template.Producers.Count > 0)
            AddProducerCommand.AddProducers(template.Producers, projectBaseName, solutionDirectory, srcDirectory, testDirectory, fileSystem);
        
        WebApiDockerfileBuilder.CreateStandardDotNetDockerfile(srcDirectory, projectBaseName, fileSystem);
        DockerIgnoreBuilder.CreateDockerIgnore(srcDirectory, projectBaseName, fileSystem);
        // DockerBuilders.AddBoundaryToDockerCompose(solutionDirectory,
        //     template.DockerConfig,
        //     template.Environment.AuthSettings.ClientId,
        //     template.Environment.AuthSettings.ClientSecret,
        //     template.Environment.AuthSettings.Audience);
        DockerComposeBuilders.AddVolumeToDockerComposeDb(solutionDirectory, template.DockerConfig);
    }
}
