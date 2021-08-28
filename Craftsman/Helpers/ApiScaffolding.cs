namespace Craftsman.Helpers
{
    using Craftsman.Builders;
    using Craftsman.Builders.Seeders;
    using Craftsman.Builders.Tests.FunctionalTests;
    using Craftsman.Builders.Tests.UnitTests;
    using Craftsman.Builders.Tests.Utilities;
    using Craftsman.Commands;
    using Craftsman.Enums;
    using Craftsman.Models;
    using Spectre.Console;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using static Helpers.ConsoleWriter;

    public class ApiScaffolding
    {
        public static void ScaffoldApi(string buildSolutionDirectory, ApiTemplate template, IFileSystem fileSystem, Verbosity verbosity)
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
                    RunTemplateBuilders(bcDirectory, srcDirectory, testDirectory, template, fileSystem, verbosity);
                    WriteLogMessage($"File scaffolding for {template.ProjectName} was successful");
                });
        }

        private static void RunTemplateBuilders(string boundedContextDirectory, string srcDirectory, string testDirectory, ApiTemplate template, IFileSystem fileSystem, Verbosity verbosity)
        {
            var projectBaseName = template.ProjectName;

            // get solution dir from bcDir
            var solutionDirectory = Directory.GetParent(boundedContextDirectory).FullName;
            Utilities.IsSolutionDirectoryGuard(solutionDirectory);

            // base files needed before below is ran
            DbContextBuilder.CreateDbContext(srcDirectory, template.Entities, template.DbContext.ContextName, template.DbContext.Provider, template.DbContext.DatabaseName, projectBaseName);
            ApiRoutesBuilder.CreateClass(testDirectory, projectBaseName, fileSystem);

            //entities
            EntityScaffolding.ScaffoldEntities(srcDirectory,
                testDirectory,
                projectBaseName,
                template.Entities,
                template.DbContext.ContextName,
                template.SwaggerConfig.AddSwaggerComments,
                template.AuthorizationSettings.Policies,
                fileSystem,
                verbosity);

            // environments
            Utilities.AddStartupEnvironmentsWithServices(
                srcDirectory,
                projectBaseName,
                template.DbContext.DatabaseName,
                template.Environments,
                template.SwaggerConfig,
                template.Port,
                template.AddJwtAuthentication,
                projectBaseName
            );

            // unit tests, test utils, and one offs
            PagedListTestBuilder.CreateTests(testDirectory, projectBaseName);
            IntegrationTestFixtureBuilder.CreateFixture(testDirectory, projectBaseName, template.DbContext.ContextName, template.DbContext.DatabaseName, template.DbContext.Provider, fileSystem);
            IntegrationTestBaseBuilder.CreateBase(testDirectory, projectBaseName, template.DbContext.Provider, fileSystem);
            DockerUtilitiesBuilder.CreateGeneralUtilityClass(testDirectory, projectBaseName, template.DbContext.Provider, fileSystem);
            DockerUtilitiesBuilder.CreateDockerDatabaseUtilityClass(testDirectory, projectBaseName, template.DbContext.Provider, fileSystem);
            WebAppFactoryBuilder.CreateWebAppFactory(testDirectory, projectBaseName, template.DbContext.ContextName, template.AddJwtAuthentication);
            FunctionalTestBaseBuilder.CreateBase(testDirectory, projectBaseName, template.DbContext.ContextName, fileSystem);
            HealthTestBuilder.CreateTests(testDirectory, projectBaseName);
            HttpClientExtensionsBuilder.Create(testDirectory, projectBaseName);

            //seeders
            SeederBuilder.AddSeeders(srcDirectory, template.Entities, template.DbContext.ContextName, projectBaseName);

            //services
            // TODO move the auth stuff to a modifier to make it SOLID so i can add it to an add auth command
            SwaggerBuilder.AddSwagger(srcDirectory, template.SwaggerConfig, projectBaseName, template.AddJwtAuthentication, template.AuthorizationSettings.Policies, projectBaseName, fileSystem);

            if (template.AddJwtAuthentication)
            {
                InfrastructureServiceRegistrationModifier.InitializeAuthServices(srcDirectory, projectBaseName, template.AuthorizationSettings.Policies);
            }

            if (template.Bus.AddBus)
                AddBusCommand.AddBus(template.Bus, srcDirectory, testDirectory, projectBaseName, solutionDirectory, fileSystem);

            if (template.Consumers.Count > 0)
                AddConsumerCommand.AddConsumers(template.Consumers, projectBaseName, srcDirectory, testDirectory);

            if (template.Producers.Count > 0)
                AddProducerCommand.AddProducers(template.Producers, projectBaseName, srcDirectory);
        }
    }
}