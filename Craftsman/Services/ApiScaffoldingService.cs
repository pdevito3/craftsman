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
                await mediator.Send(new SolutionBuilder.AddProjectsCommand(buildSolutionDirectory,
                    scaffoldingDirectoryStore.SrcDirectory,
                    scaffoldingDirectoryStore.TestDirectory,
                    template.DbContext.ProviderEnum,
                    projectName, 
                    template.AddJwtAuthentication, 
                    template.DockerConfig.OTelAgentPort,
                    template.UseCustomErrorHandler));

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
        mediator.Send(new ApiRoutesBuilder.Command()).GetAwaiter().GetResult();
        mediator.Send(new DbMigrationsHostedServiceBuilder.Command(template.DbContext.ProviderEnum)).GetAwaiter().GetResult();

        if (template.AddJwtAuthentication)
        {
            mediator.Send(new PermissionsBuilder.Command(template.AddJwtAuthentication)).GetAwaiter().GetResult(); // <-- needs to run before entity features
            mediator.Send(new UserPolicyHandlerBuilder.Command(template.DbContext.ContextName)).GetAwaiter().GetResult();
            mediator.Send(new InfrastructureServiceRegistrationModifier.InitializeAuthServicesCommand()).GetAwaiter().GetResult();
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
            mediator.Send(new RolesControllerBuilder.Command()).GetAwaiter().GetResult();
            mediator.Send(new PermissionsControllerBuilder.Command()).GetAwaiter().GetResult();
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
        mediator.Send(new WebApiLaunchSettingsModifier.AddProfileCommand(template.Environment, template.Port)).GetAwaiter().GetResult();
        
        // unit tests, test utils, and one offs
        mediator.Send(new PagedListTestBuilder.Command()).GetAwaiter().GetResult();
        mediator.Send(new IntegrationTestFixtureBuilder.Command(template.DbContext.ContextName, template.DbContext.ProviderEnum, template.AddJwtAuthentication)).GetAwaiter().GetResult();
        mediator.Send(new IntegrationTestBaseBuilder.Command()).GetAwaiter().GetResult();
        mediator.Send(new IntegrationTestServiceScopeBuilder.Command(template.DbContext.ContextName, template.AddJwtAuthentication)).GetAwaiter().GetResult();
        mediator.Send(new WebAppFactoryBuilder.Command(template.DbContext.ProviderEnum, template.AddJwtAuthentication)).GetAwaiter().GetResult();
        mediator.Send(new FunctionalTestBaseBuilder.Command(template.DbContext.ContextName, template.AddJwtAuthentication)).GetAwaiter().GetResult();
        mediator.Send(new HealthTestBuilder.Command()).GetAwaiter().GetResult();
        mediator.Send(new HttpClientExtensionsBuilder.Command(projectBaseName)).GetAwaiter().GetResult();
        mediator.Send(new EntityBuilder.CreateBaseEntityCommand(template.UseSoftDelete));
        mediator.Send(new CurrentUserServiceTestBuilder.Command()).GetAwaiter().GetResult();
        mediator.Send(new ValueObjectBuilder.ValueObjectBuilderCommand());
        mediator.Send(new CommonValueObjectBuilder.Command(template.AddJwtAuthentication));
        mediator.Send(new FakesBuilder.CreateAddressFakesCommand()).GetAwaiter().GetResult();
        mediator.Send(new ValueObjectDtoBuilder.ValueObjectDtoBuilderCommand());
        mediator.Send(new DomainEventBuilder.DomainEventBuilderCommand());
        mediator.Send(new EmailUnitTestBuilder.Command(ValueObjectEnum.Email.Name, ValueObjectEnum.Email.Plural())).GetAwaiter().GetResult();
        mediator.Send(new AllEndpointsProtectedUnitTestBuilder.Command()).GetAwaiter().GetResult();
        
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
        mediator.Send(new OptionsConfigurationsBuilder.Command()).GetAwaiter().GetResult();

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
        
        mediator.Send(new WebApiDockerfileBuilder.Command()).GetAwaiter().GetResult();
        mediator.Send(new DockerIgnoreBuilder.Command()).GetAwaiter().GetResult();
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
        mediator.Send(new WebApiLaunchSettingsModifier.AddProfileCommand(environment, port)).GetAwaiter().GetResult();
    }
}
