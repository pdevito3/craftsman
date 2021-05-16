namespace Craftsman.Helpers
{
    using Craftsman.Builders;
    using Craftsman.Builders.Dtos;
    using Craftsman.Builders.Features;
    using Craftsman.Builders.Seeders;
    using Craftsman.Builders.Tests.Fakes;
    using Craftsman.Builders.Tests.FunctionalTests;
    using Craftsman.Builders.Tests.IntegrationTests;
    using Craftsman.Builders.Tests.UnitTests;
    using Craftsman.Builders.Tests.Utilities;
    using Craftsman.Commands;
    using Craftsman.Enums;
    using Craftsman.Models;
    using LibGit2Sharp;
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
            var solutionName = template.SolutionName;
            AnsiConsole.Status()
                .AutoRefresh(true)
                .Spinner(Spinner.Known.Dots2)
                .Start($"[yellow]Creating {template.SolutionName} [/]", ctx =>
                {
                    FileParsingHelper.RunPrimaryKeyGuard(template.Entities);
                    FileParsingHelper.RunSolutionNameAssignedGuard(solutionName);
                    FileParsingHelper.SolutionNameDoesNotEqualEntityGuard(solutionName, template.Entities);

                    // add an accelerate.config.yaml file to the root?
                    var bcDirectory = $"{buildSolutionDirectory}{Path.DirectorySeparatorChar}{solutionName}";
                    var srcDirectory = Path.Combine(bcDirectory, "src");
                    var testDirectory = Path.Combine(bcDirectory, "tests");
                    fileSystem.Directory.CreateDirectory(srcDirectory);
                    fileSystem.Directory.CreateDirectory(testDirectory);

                    ctx.Spinner(Spinner.Known.BouncingBar);
                    ctx.Status($"[bold blue]Building {solutionName} Projects [/]");
                    SolutionBuilder.AddProjects(buildSolutionDirectory, srcDirectory, testDirectory, template.DbContext.Provider, template.DbContext.DatabaseName, solutionName, template.AddJwtAuthentication, fileSystem);

                    // add all files based on the given template config
                    ctx.Status($"[bold blue]Scaffolding Files for {solutionName} [/]");
                    RunTemplateBuilders(bcDirectory, srcDirectory, testDirectory, template, fileSystem, verbosity);
                    WriteLogMessage($"File scaffolding for {template.SolutionName} was successful");

                    ctx.Spinner(Spinner.Known.Moon);
                    ctx.Status($"[bold blue]Running {solutionName} Database Migrations [/]");
                    if (RunDbMigration(template, srcDirectory))
                        WriteLogMessage($"Database Migrations for {template.SolutionName} were successful");
                });
        }

        private static bool RunDbMigration(ApiTemplate template, string srcDirectory)
        {
            var webApiProjectClassPath = ClassPathHelper.WebApiProjectClassPath(srcDirectory, template.SolutionName);
            var infraProjectClassPath = ClassPathHelper.InfrastructureProjectClassPath(srcDirectory, template.SolutionName);

            return Utilities.ExecuteProcess(
                "dotnet",
                @$"ef migrations add ""InitialMigration"" --project ""{infraProjectClassPath.FullClassPath}"" --startup-project ""{webApiProjectClassPath.FullClassPath}"" --output-dir Migrations",
                srcDirectory,
                new Dictionary<string, string>()
                {
                    { "ASPNETCORE_ENVIRONMENT", Guid.NewGuid().ToString() } // guid to not conflict with any given envs
                },
                20000,
                $"{Emoji.Known.Warning} {template.SolutionName} Database Migrations timed out and will need to be run manually");
        }

        private static void RunTemplateBuilders(string boundedContextDirectory, string srcDirectory, string testDirectory, ApiTemplate template, IFileSystem fileSystem, Verbosity verbosity)
        {
            var projectBaseName = template.SolutionName;

            // get solution dir from bcDir
            var solutionDirectory = Directory.GetParent(boundedContextDirectory).FullName;
            Utilities.IsSolutionDirectoryGuard(solutionDirectory);

            // dbcontext
            DbContextBuilder.CreateDbContext(srcDirectory, template.Entities, template.DbContext.ContextName, template.DbContext.Provider, template.DbContext.DatabaseName, projectBaseName);

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
            DockerDatabaseUtilitiesBuilder.CreateClass(testDirectory, projectBaseName, template.DbContext.Provider, fileSystem);
            ApiRoutesBuilder.CreateClass(testDirectory, projectBaseName, template.Entities, fileSystem);
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
                AddBusCommand.AddBus(template.Bus, srcDirectory, projectBaseName, solutionDirectory, fileSystem);

            if (template.Consumers.Count > 0)
                RegisterConsumerCommand.AddConsumers(template.Consumers, projectBaseName, srcDirectory);

            if (template.Messages.Count > 0)
                AddMessageCommand.AddMessages(solutionDirectory, fileSystem, template.Messages);

            ReadmeBuilder.CreateBoundedContextReadme(boundedContextDirectory, template.SolutionName, srcDirectory, fileSystem);
        }
    }
}