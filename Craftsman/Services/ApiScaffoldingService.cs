namespace Craftsman.Services;

using System.IO.Abstractions;
using Builders;
using Builders.Auth;
using Builders.Docker;
using Builders.Tests.FunctionalTests;
using Builders.Tests.UnitTests;
using Builders.Tests.Utilities;
using Commands;
using Domain;
using FluentAssertions.Common;
using Helpers;
using MediatR;
using Spectre.Console;

public class ApiScaffoldingService
{
    private readonly IAnsiConsole _console;
    private readonly IConsoleWriter _consoleWriter;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IFileSystem _fileSystem;
    private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;
    private readonly IMediator _mediator;
    private readonly IFileParsingHelper _fileParsingHelper;

    public ApiScaffoldingService(IAnsiConsole console, IConsoleWriter consoleWriter, ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore, IFileSystem fileSystem, IMediator mediator, IFileParsingHelper fileParsingHelper)
    {
        _console = console;
        _consoleWriter = consoleWriter;
        _utilities = utilities;
        _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        _fileSystem = fileSystem;
        _mediator = mediator;
        _fileParsingHelper = fileParsingHelper;
    }

    public void ScaffoldApi(string buildSolutionDirectory, ApiTemplate template)
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
                _scaffoldingDirectoryStore.SetBoundedContextDirectoryAndProject(projectName);
                _fileSystem.Directory.CreateDirectory(_scaffoldingDirectoryStore.SrcDirectory);
                _fileSystem.Directory.CreateDirectory(_scaffoldingDirectoryStore.TestDirectory);

                ctx.Spinner(Spinner.Known.BouncingBar);
                ctx.Status($"[bold blue]Building {projectName} Projects [/]");
                new SolutionBuilder(_utilities, _fileSystem, _mediator)
                    .AddProjects(buildSolutionDirectory,
                        _scaffoldingDirectoryStore.SrcDirectory,
                        _scaffoldingDirectoryStore.TestDirectory,
                        template.DbContext.ProviderEnum,
                        template.DbContext.DatabaseName,
                        projectName, template.AddJwtAuthentication);

                // add all files based on the given template config
                ctx.Status($"[bold blue]Scaffolding Files for {projectName} [/]");
                RunTemplateBuilders(_scaffoldingDirectoryStore.BoundedContextDirectory, _scaffoldingDirectoryStore.SrcDirectory, _scaffoldingDirectoryStore.TestDirectory, template);
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
        if (template.AddJwtAuthentication)
            template.DockerConfig.AuthServerPort ??= template?.Environment?.AuthSettings?.AuthorizationUrl
                .Replace("localhost", "")
                .Replace("https://", "")
                .Replace("http://", "")
                .Replace(":", ""); // this is fragile and i hate it. also not in domain...
        new DbContextBuilder(_utilities, _fileSystem).CreateDbContext(srcDirectory,
            template.Entities,
            template.DbContext.ContextName,
            template.DbContext.ProviderEnum,
            template.DbContext.DatabaseName,
            template.DockerConfig.DbConnectionString,
            template.DbContext.NamingConventionEnum,
            template.UseSoftDelete,
            projectBaseName
        );
        new ApiRoutesBuilder(_utilities).CreateClass(testDirectory, projectBaseName);

        if (template.AddJwtAuthentication)
        {
            new PermissionsBuilder(_utilities).GetPermissions(srcDirectory, projectBaseName); // <-- needs to run before entity features
            new RolesBuilder(_utilities).GetRoles(solutionDirectory);
            new UserPolicyHandlerBuilder(_utilities).CreatePolicyBuilder(solutionDirectory, srcDirectory, projectBaseName);
            new InfrastructureServiceRegistrationModifier(_fileSystem).InitializeAuthServices(srcDirectory, projectBaseName);
            new EntityScaffoldingService(_utilities, _fileSystem, _mediator).ScaffoldRolePermissions(solutionDirectory,
                srcDirectory,
                testDirectory,
                projectBaseName,
                template.DbContext.ContextName,
                template.SwaggerConfig.AddSwaggerComments,
                template.UseSoftDelete);
        }

        //entities
        new EntityScaffoldingService(_utilities, _fileSystem, _mediator).ScaffoldEntities(solutionDirectory,
            srcDirectory,
            testDirectory,
            projectBaseName,
            template.Entities,
            template.DbContext.ContextName,
            template.SwaggerConfig.AddSwaggerComments,
            template.UseSoftDelete);

        // config
        new AppSettingsBuilder(_utilities).CreateWebApiAppSettings(srcDirectory, template.DbContext.DatabaseName, projectBaseName);
        new WebApiLaunchSettingsModifier(_fileSystem).AddProfile(srcDirectory, template.Environment, template.Port, template.DockerConfig, projectBaseName);
        
        // unit tests, test utils, and one offs
        new PagedListTestBuilder(_utilities).CreateTests(srcDirectory, testDirectory, projectBaseName);
        new IntegrationTestFixtureBuilder(_utilities).CreateFixture(testDirectory,
            srcDirectory,
            projectBaseName,
            template.DbContext.ContextName,
            template.DbContext.ProviderEnum,
            template.AddJwtAuthentication);
        new IntegrationTestBaseBuilder(_utilities).CreateBase(testDirectory, projectBaseName, template.AddJwtAuthentication);
        new WebAppFactoryBuilder(_utilities).CreateWebAppFactory(testDirectory, projectBaseName, template.DbContext.ContextName, template.AddJwtAuthentication);
        new FunctionalTestBaseBuilder(_utilities).CreateBase(testDirectory, projectBaseName, template.DbContext.ContextName);
        new HealthTestBuilder(_utilities).CreateTests(testDirectory, projectBaseName);
        new HttpClientExtensionsBuilder(_utilities).Create(testDirectory, projectBaseName);
        new EntityBuilder(_utilities).CreateBaseEntity(srcDirectory, projectBaseName, template.UseSoftDelete);
        new CurrentUserServiceTestBuilder(_utilities).CreateTests(testDirectory, projectBaseName);
        _mediator.Send(new ValueObjectBuilder.ValueObjectBuilderCommand());
        _mediator.Send(new CommonValueObjectBuilder.CommonValueObjectBuilderCommand());
        _mediator.Send(new ValueObjectDtoBuilder.ValueObjectDtoBuilderCommand());
        _mediator.Send(new DomainEventBuilder.DomainEventBuilderCommand());

        //services
        _mediator.Send(new UnitOfWorkBuilder.UnitOfWorkBuilderCommand(template.DbContext.ContextName));
        _mediator.Send(new IBoundaryServiceInterfaceBuilder.IBoundaryServiceInterfaceBuilderCommand());
        _mediator.Send(new GenericRepositoryBuilder.GenericRepositoryBuilderCommand(template.DbContext.ContextName));

        new CurrentUserServiceBuilder(_utilities).GetCurrentUserService(srcDirectory, projectBaseName);
        new SwaggerBuilder(_utilities, _fileSystem).AddSwagger(srcDirectory, template.SwaggerConfig, template.ProjectName, template.AddJwtAuthentication, template.PolicyName, projectBaseName);

        if (template.Bus.AddBus)
            new AddBusCommand(_fileSystem, _consoleWriter, _utilities, _scaffoldingDirectoryStore, _console, _fileParsingHelper)
                .AddBus(template.Bus, srcDirectory, testDirectory, projectBaseName, solutionDirectory);

        if (template.Consumers.Count > 0)
            new AddConsumerCommand(_fileSystem, _consoleWriter, _utilities, _scaffoldingDirectoryStore, _fileParsingHelper)
                .AddConsumers(template.Consumers, projectBaseName, solutionDirectory, srcDirectory, testDirectory);

        if (template.Producers.Count > 0)
            new AddProducerCommand(_console, _fileSystem, _consoleWriter, _utilities, _scaffoldingDirectoryStore, _fileParsingHelper)
                .AddProducers(template.Producers, projectBaseName, solutionDirectory, srcDirectory, testDirectory);

        new WebApiDockerfileBuilder(_utilities).CreateStandardDotNetDockerfile(srcDirectory, projectBaseName);
        new DockerIgnoreBuilder(_utilities).CreateDockerIgnore(srcDirectory, projectBaseName);
        // DockerBuilders.AddBoundaryToDockerCompose(solutionDirectory,
        //     template.DockerConfig,
        //     template.Environment.AuthSettings.ClientId,
        //     template.Environment.AuthSettings.ClientSecret,
        //     template.Environment.AuthSettings.Audience);
        new DockerComposeBuilders(_utilities, _fileSystem).AddVolumeToDockerComposeDb(solutionDirectory, template.DockerConfig);
    }

    private void AddStartupEnvironmentsWithServices(
        string srcDirectory,
        string dbName,
        ApiEnvironment environment,
        SwaggerConfig swaggerConfig,
        int port,
        string projectBaseName,
        DockerConfig dockerConfig)
    {
        new AppSettingsBuilder(_utilities).CreateWebApiAppSettings(srcDirectory, dbName, projectBaseName);
        new WebApiLaunchSettingsModifier(_fileSystem).AddProfile(srcDirectory, environment, port, dockerConfig, projectBaseName);
    }
}
