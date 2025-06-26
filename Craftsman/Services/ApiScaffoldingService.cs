namespace Craftsman.Services;

using System.IO.Abstractions;
using Builders;
using Builders.Auth;
using Builders.Docker;
using Builders.Endpoints;
using Builders.Tests.Fakes;
using Builders.Tests.FunctionalTests;
using Builders.Tests.UnitTests;
using Builders.Tests.Utilities;
using Commands;
using Domain;
using Domain.Enums;
using FluentAssertions.Common;
using Helpers;
using MediatR;
using Spectre.Console;

public class ApiScaffoldingService(
    IAnsiConsole console,
    IConsoleWriter consoleWriter,
    ICraftsmanUtilities utilities,
    IScaffoldingDirectoryStore scaffoldingDirectoryStore,
    IFileSystem fileSystem,
    IMediator mediator,
    IFileParsingHelper fileParsingHelper)
{
    public void ScaffoldApi(string buildSolutionDirectory, ApiTemplate template)
    {
        var projectName = template.ProjectName;
        console.Status()
            .AutoRefresh(true)
            .Spinner(Spinner.Known.Dots2)
            .Start($"[yellow]Creating {template.ProjectName} [/]", async ctx =>
            {
                FileParsingHelper.RunPrimaryKeyGuard(template.Entities);
                FileParsingHelper.RunSolutionNameAssignedGuard(projectName);
                FileParsingHelper.SolutionNameDoesNotEqualEntityGuard(projectName, template.Entities);

                // add an accelerate.config.yaml file to the root?
                scaffoldingDirectoryStore.SetBoundedContextDirectoryAndProject(projectName);
                fileSystem.Directory.CreateDirectory(scaffoldingDirectoryStore.SrcDirectory);
                fileSystem.Directory.CreateDirectory(scaffoldingDirectoryStore.TestDirectory);

                ctx.Spinner(Spinner.Known.BouncingBar);
                ctx.Status($"[bold blue]Building {projectName} Projects [/]");
                await new SolutionBuilder(utilities, fileSystem, mediator)
                    .AddProjects(buildSolutionDirectory,
                        scaffoldingDirectoryStore.SrcDirectory,
                        scaffoldingDirectoryStore.TestDirectory,
                        template.DbContext.ProviderEnum,
                        projectName, 
                        template.AddJwtAuthentication, 
                        template.DockerConfig.OTelAgentPort,
                        template.UseCustomErrorHandler);

                // add all files based on the given template config
                ctx.Status($"[bold blue]Scaffolding Files for {projectName} [/]");
                RunTemplateBuilders(template);
                consoleWriter.WriteLogMessage($"File scaffolding for {template.ProjectName} was successful");
            });
    }

    private void RunTemplateBuilders(ApiTemplate template)
    {
        var projectBaseName = template.ProjectName;
        var srcDirectory = scaffoldingDirectoryStore.SrcDirectory;
        var testDirectory = scaffoldingDirectoryStore.TestDirectory;

        // docker config data transform
        template.DockerConfig.ProjectName = template.ProjectName;
        template.DockerConfig.Provider = template.DbContext.Provider;

        // get solution dir from bcDir
        var solutionDirectory = Directory.GetParent(scaffoldingDirectoryStore.BoundedContextDirectory)?.FullName;
        utilities.IsSolutionDirectoryGuard(solutionDirectory);

        // base files needed before below is ran
        template.DockerConfig.ApiPort ??= template.Port; // set to the launch settings port if needed... really need to refactor to a domain layer and dto layer 😪
        if (template.AddJwtAuthentication)
            template.DockerConfig.AuthServerPort ??= template?.Environment?.AuthSettings?.AuthorizationUrl
                .Replace("localhost", "")
                .Replace("https://", "")
                .Replace("http://", "")
                .Replace(":", ""); // this is fragile and i hate it. also not in domain...
        mediator.Send(new DbContextBuilder.DbContextBuilderCommand(
            srcDirectory,
            template.Entities,
            template.DbContext.ContextName,
            template.DbContext.ProviderEnum,
            template.DbContext.DatabaseName,
            template.DockerConfig.DbConnectionString,
            template.DbContext.NamingConventionEnum,
            template.UseSoftDelete,
            projectBaseName,
            template.AddJwtAuthentication
        )).GetAwaiter().GetResult();
        new ApiRoutesBuilder(utilities).CreateClass(testDirectory, projectBaseName);
        mediator.Send(new DbMigrationsHostedServiceBuilder.Command(template.DbContext.ProviderEnum)).GetAwaiter().GetResult();

        if (template.AddJwtAuthentication)
        {
            new PermissionsBuilder(utilities).GetPermissions(srcDirectory, projectBaseName, template.AddJwtAuthentication); // <-- needs to run before entity features
            new UserPolicyHandlerBuilder(utilities).CreatePolicyBuilder(srcDirectory, projectBaseName, template.DbContext.ContextName);
            new InfrastructureServiceRegistrationModifier(fileSystem).InitializeAuthServices(srcDirectory, projectBaseName);
            new EntityScaffoldingService(utilities, fileSystem, mediator, consoleWriter).ScaffoldRolePermissions(solutionDirectory,
                srcDirectory,
                testDirectory,
                projectBaseName,
                template.DbContext.ContextName,
                template.SwaggerConfig.AddSwaggerComments,
                template.UseSoftDelete);

            new EntityScaffoldingService(utilities, fileSystem, mediator, consoleWriter).ScaffoldUser(solutionDirectory,
                srcDirectory,
                testDirectory,
                projectBaseName,
                template.DbContext.ContextName,
                template.SwaggerConfig.AddSwaggerComments,
                template.UseSoftDelete);
            new RolesControllerBuilder(utilities).CreateController(srcDirectory, projectBaseName);
            new PermissionsControllerBuilder(utilities).CreateController(srcDirectory, projectBaseName);
        }

        //entities
        new EntityScaffoldingService(utilities, fileSystem, mediator, consoleWriter).ScaffoldEntities(solutionDirectory,
            srcDirectory,
            testDirectory,
            projectBaseName,
            template.Entities,
            template.DbContext.ContextName,
            template.SwaggerConfig.AddSwaggerComments,
            template.UseSoftDelete,
            template.DbContext.ProviderEnum);

        // config
        mediator.Send(new EditorConfigBuilder.EditorConfigBuilderCommand()).GetAwaiter().GetResult();
        mediator.Send(new AppSettingsBuilder.AppSettingsBuilderCommand(template.DbContext.DatabaseName)).GetAwaiter().GetResult();
        mediator.Send(new AppSettingsDevelopmentBuilder.AppSettingsDevelopmentBuilderCommand(template.Environment, template.DockerConfig)).GetAwaiter().GetResult();
        new WebApiLaunchSettingsModifier(fileSystem).AddProfile(srcDirectory, template.Environment, template.Port, projectBaseName);
        
        // unit tests, test utils, and one offs
        new PagedListTestBuilder(utilities).CreateTests(srcDirectory, testDirectory, projectBaseName);
        new IntegrationTestFixtureBuilder(utilities).CreateFixture(testDirectory,
            srcDirectory,
            projectBaseName,
            template.DbContext.ContextName,
            template.DbContext.ProviderEnum,
            template.AddJwtAuthentication);
        new IntegrationTestBaseBuilder(utilities).CreateBase(testDirectory, projectBaseName);
        new IntegrationTestServiceScopeBuilder(utilities).CreateBase(testDirectory, projectBaseName, template.DbContext.ContextName, template.AddJwtAuthentication);
        new WebAppFactoryBuilder(utilities).CreateWebAppFactory(testDirectory, projectBaseName, template.DbContext.ProviderEnum, template.AddJwtAuthentication);
        new FunctionalTestBaseBuilder(utilities).CreateBase(srcDirectory, testDirectory, projectBaseName, template.DbContext.ContextName, template.AddJwtAuthentication);
        new HealthTestBuilder(utilities).CreateTests(testDirectory, projectBaseName);
        new HttpClientExtensionsBuilder(utilities).Create(testDirectory, projectBaseName);
        mediator.Send(new EntityBuilder.CreateBaseEntityCommand(template.UseSoftDelete));
        new CurrentUserServiceTestBuilder(utilities).CreateTests(testDirectory, srcDirectory, projectBaseName);
        mediator.Send(new ValueObjectBuilder.ValueObjectBuilderCommand());
        mediator.Send(new CommonValueObjectBuilder.Command(template.AddJwtAuthentication));
        new FakesBuilder(utilities).CreateAddressFakes(srcDirectory, testDirectory, projectBaseName);
        mediator.Send(new ValueObjectDtoBuilder.ValueObjectDtoBuilderCommand());
        mediator.Send(new DomainEventBuilder.DomainEventBuilderCommand());
        new EmailUnitTestBuilder(utilities).CreateTests(testDirectory,
            srcDirectory,
            ValueObjectEnum.Email.Name,
            ValueObjectEnum.Email.Plural(),
            projectBaseName);
        new AllEndpointsProtectedUnitTestBuilder(utilities).CreateTests(testDirectory, projectBaseName);
        
        // if(template.AddJwtAuthentication)
        //     new UserPolicyHandlerUnitTests(_utilities).CreateTests(testDirectory, srcDirectory, projectBaseName);

        //services
        mediator.Send(new SharedTestUtilsBuilder.Command());
        mediator.Send(new IBoundaryServiceInterfaceBuilder.BoundaryServiceInterfaceBuilderCommand());
        mediator.Send(new TestUsingsBuilder.Command(TestUsingsBuilder.TestingTarget.Functional));
        mediator.Send(new TestUsingsBuilder.Command(TestUsingsBuilder.TestingTarget.Integration));
        mediator.Send(new TestUsingsBuilder.Command(TestUsingsBuilder.TestingTarget.Unit));
        mediator.Send(new CurrentUserFilterAttributeBuilder.Command());
        mediator.Send(new HangfireAuthorizationFilterBuilder.Command());
        mediator.Send(new JobWithUserContextBuilder.Command());
        mediator.Send(new ServiceJobActivatorScopeBuilder.Command());
        new OptionsConfigurationsBuilder(utilities).CreateConfig(srcDirectory, projectBaseName);

        mediator.Send(new CurrentUserServiceBuilder.CurrentUserServiceBuilderCommand()).GetAwaiter().GetResult();
        mediator.Send(new SwaggerBuilder.SwaggerBuilderCommand(template.SwaggerConfig, template.ProjectName, template.AddJwtAuthentication, template?.Environment?.AuthSettings?.Audience)).GetAwaiter().GetResult();

        if (template.Bus.AddBus)
            new AddBusCommand(fileSystem, consoleWriter, utilities, scaffoldingDirectoryStore, fileParsingHelper, mediator)
                .AddBus(template.Bus, srcDirectory, testDirectory, projectBaseName, solutionDirectory);

        if (template.Consumers.Count > 0)
            new AddConsumerCommand(fileSystem, consoleWriter, utilities, scaffoldingDirectoryStore, fileParsingHelper, mediator)
                .AddConsumers(template.Consumers, projectBaseName, solutionDirectory, srcDirectory, testDirectory).GetAwaiter().GetResult();

        if (template.Producers.Count > 0)
            new AddProducerCommand(console, fileSystem, consoleWriter, utilities, scaffoldingDirectoryStore, fileParsingHelper, mediator)
                .AddProducers(template.Producers, projectBaseName, solutionDirectory, srcDirectory, testDirectory);

        if (template.IncludeGithubTestActions)
        {
            mediator.Send(new GithubTestActionsBuilder.CreateUnitTestActionCommand(solutionDirectory, projectBaseName)).GetAwaiter().GetResult();
            mediator.Send(new GithubTestActionsBuilder.CreateIntegrationTestActionCommand(solutionDirectory, projectBaseName)).GetAwaiter().GetResult();
            mediator.Send(new GithubTestActionsBuilder.CreateFunctionalTestActionCommand(solutionDirectory, projectBaseName)).GetAwaiter().GetResult();
        }
        
        new WebApiDockerfileBuilder(utilities).CreateStandardDotNetDockerfile(srcDirectory, projectBaseName);
        new DockerIgnoreBuilder(utilities).CreateDockerIgnore(srcDirectory, projectBaseName);
        // new DockerComposeBuilders(_utilities, _fileSystem).AddBoundaryToDockerCompose(solutionDirectory,
        //     template.DockerConfig,
        //     template.Environment.AuthSettings.ClientId,
        //     template.Environment.AuthSettings.ClientSecret,
        //     template.Environment.AuthSettings.Audience);
        new DockerComposeBuilders(utilities, fileSystem).AddVolumeToDockerComposeDb(solutionDirectory, template.DockerConfig);
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
        new WebApiLaunchSettingsModifier(fileSystem).AddProfile(srcDirectory, environment, port, projectBaseName);
    }
}
