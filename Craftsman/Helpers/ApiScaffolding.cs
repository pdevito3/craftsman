namespace Craftsman.Helpers
{
    using Craftsman.Builders;
    using Craftsman.Builders.Seeders;
    using Craftsman.Builders.Tests.FunctionalTests;
    using Craftsman.Builders.Tests.UnitTests;
    using Craftsman.Builders.Tests.Utilities;
    using Craftsman.Commands;
    using Craftsman.Models;
    using Spectre.Console;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using Builders.Auth;
    using static Helpers.ConsoleWriter;

    public class ApiScaffolding
    {
        public static void ScaffoldApi(string buildSolutionDirectory, ApiTemplate template, IFileSystem fileSystem)
        {
            var projectName = template.ProjectName;
            AnsiConsole.Status()
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
                    SolutionBuilder.AddProjects(buildSolutionDirectory, srcDirectory, testDirectory, template.DbContext.Provider, template.DbContext.DatabaseName, projectName, template.AddJwtAuthentication, fileSystem);

                    // add all files based on the given template config
                    ctx.Status($"[bold blue]Scaffolding Files for {projectName} [/]");
                    RunTemplateBuilders(bcDirectory, srcDirectory, testDirectory, template, fileSystem);
                    WriteLogMessage($"File scaffolding for {template.ProjectName} was successful");
                });
        }

        private static void RunTemplateBuilders(string boundedContextDirectory, string srcDirectory, string testDirectory, ApiTemplate template, IFileSystem fileSystem)
        {
            var projectBaseName = template.ProjectName;

            // get solution dir from bcDir
            var solutionDirectory = Directory.GetParent(boundedContextDirectory)?.FullName;
            Utilities.IsSolutionDirectoryGuard(solutionDirectory);

            // base files needed before below is ran
            DbContextBuilder.CreateDbContext(srcDirectory,
                template.Entities,
                template.DbContext.ContextName,
                template.DbContext.Provider,
                template.DbContext.DatabaseName,
                template.DbContext.NamingConventionEnum,
                projectBaseName,
                fileSystem
            );
            ApiRoutesBuilder.CreateClass(testDirectory, projectBaseName, fileSystem);
            
            if (template.AddJwtAuthentication)
            {
                PermissionsBuilder.GetPermissions(srcDirectory, projectBaseName, fileSystem); // <-- needs to run before entity features
                RolesBuilder.GetRoles(srcDirectory, projectBaseName, fileSystem);
                UserPolicyHandlerBuilder.CreatePolicyBuilder(srcDirectory, projectBaseName, fileSystem);
                InfrastructureServiceRegistrationModifier.InitializeAuthServices(srcDirectory, projectBaseName);
                EntityScaffolding.ScaffoldRolePermissions(srcDirectory,
                    testDirectory,
                    projectBaseName,
                    template.DbContext.ContextName,
                    template.SwaggerConfig.AddSwaggerComments,
                    fileSystem);
            }
            
            //entities
            EntityScaffolding.ScaffoldEntities(srcDirectory,
                testDirectory,
                projectBaseName,
                template.Entities,
                template.DbContext.ContextName,
                template.SwaggerConfig.AddSwaggerComments,
                fileSystem);

            // environments
            Utilities.AddStartupEnvironmentsWithServices(
                srcDirectory,
                projectBaseName,
                template.DbContext.DatabaseName,
                template.Environments,
                template.SwaggerConfig,
                template.Port,
                template.AddJwtAuthentication,
                projectBaseName,
                fileSystem
            );

            // unit tests, test utils, and one offs
            PagedListTestBuilder.CreateTests(testDirectory, projectBaseName);
            IntegrationTestFixtureBuilder.CreateFixture(testDirectory, projectBaseName, template.DbContext.ContextName, template.DbContext.DatabaseName, template.DbContext.Provider, fileSystem);
            IntegrationTestBaseBuilder.CreateBase(testDirectory, projectBaseName, template.DbContext.Provider, fileSystem);
            DockerUtilitiesBuilder.CreateGeneralUtilityClass(testDirectory, projectBaseName, template.DbContext.Provider, fileSystem);
            DockerUtilitiesBuilder.CreateDockerDatabaseUtilityClass(testDirectory, projectBaseName, template.DbContext.Provider, fileSystem);
            WebAppFactoryBuilder.CreateWebAppFactory(testDirectory, projectBaseName, template.DbContext.ContextName, template.AddJwtAuthentication);
            FunctionalTestBaseBuilder.CreateBase(testDirectory, projectBaseName, template.DbContext.ContextName, fileSystem);
            HealthTestBuilder.CreateTests(testDirectory, projectBaseName, fileSystem);
            HttpClientExtensionsBuilder.Create(testDirectory, projectBaseName);
            EntityBuilder.CreateBaseEntity(srcDirectory, projectBaseName, fileSystem);
            CurrentUserServiceTestBuilder.CreateTests(testDirectory, projectBaseName, fileSystem);

            //seeders
            SeederBuilder.AddSeeders(srcDirectory, template.Entities, template.DbContext.ContextName, projectBaseName);

            //services
            CurrentUserServiceBuilder.GetCurrentUserService(srcDirectory, projectBaseName, fileSystem);
            SwaggerBuilder.AddSwagger(srcDirectory, template.SwaggerConfig, template.ProjectName, template.AddJwtAuthentication, template.PolicyName, projectBaseName, fileSystem);

            
            if (template.Bus.AddBus)
                AddBusCommand.AddBus(template.Bus, srcDirectory, testDirectory, projectBaseName, solutionDirectory, fileSystem);

            if (template.Consumers.Count > 0)
                AddConsumerCommand.AddConsumers(template.Consumers, projectBaseName, srcDirectory, testDirectory, fileSystem);

            if (template.Producers.Count > 0)
                AddProducerCommand.AddProducers(template.Producers, projectBaseName, srcDirectory);
        }
    }
}
