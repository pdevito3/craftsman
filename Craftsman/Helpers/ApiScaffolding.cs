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
    using Craftsman.Enums;
    using Craftsman.Models;
    using LibGit2Sharp;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using static Helpers.ConsoleWriter;

    public class ApiScaffolding
    {
        public static void ScaffoldApi(string buildSolutionDirectory, ApiTemplate template, IFileSystem fileSystem, Verbosity verbosity)
        {
            if(verbosity == Verbosity.More)
                WriteHelpText($"{Environment.NewLine}Creating {template.SolutionName}...");

            FileParsingHelper.RunPrimaryKeyGuard(template.Entities);
            FileParsingHelper.RunSolutionNameAssignedGuard(template.SolutionName);

            // scaffold projects
            // add an accelerate.config.yaml file to the root?
            // solution level stuff

            // ***********will add a root directory read from template here for the entire overarching whatever (Koi Katan Operations, LimsLite, ProfiseePlatform)
            var solutionDirectory = $"{buildSolutionDirectory}{Path.DirectorySeparatorChar}{template.SolutionName}";
            var srcDirectory = Path.Combine(solutionDirectory, "src");
            var testDirectory = Path.Combine(solutionDirectory, "tests");
            fileSystem.Directory.CreateDirectory(srcDirectory);
            fileSystem.Directory.CreateDirectory(testDirectory);

            SolutionBuilder.BuildSolution(solutionDirectory, template.SolutionName, fileSystem);
            SolutionBuilder.AddProjects(solutionDirectory, srcDirectory, testDirectory, template.DbContext.Provider, template.DbContext.ContextName, template.SolutionName, template.AddJwtAuthentication, fileSystem);
            if(verbosity == Verbosity.More)
                WriteHelpText($"{template.SolutionName} projects were scaffolded successfully.");

            // add all files based on the given template config
            RunTemplateBuilders(solutionDirectory, srcDirectory, testDirectory, template, fileSystem, verbosity);
            if(verbosity == Verbosity.More)
                WriteHelpText($"{template.SolutionName} templates were scaffolded successfully.");

            if (verbosity == Verbosity.More)
                WriteHelpText($"Starting {template.SolutionName} database migrations.");
            RunDbMigration(template, srcDirectory);
            if(verbosity == Verbosity.More)
                WriteHelpText($"{template.SolutionName} migrations are complete.");

            if(verbosity == Verbosity.More)
                WriteHelpText($"Completed {template.SolutionName}.");
        }

        private static void RunDbMigration(ApiTemplate template, string srcDirectory)
        {
            var webApiProjectClassPath = ClassPathHelper.WebApiProjectClassPath(srcDirectory, template.SolutionName);
            var infraProjectClassPath = ClassPathHelper.InfrastructureProjectClassPath(srcDirectory, template.SolutionName);

            Utilities.ExecuteProcess(
                "dotnet",
                @$"ef migrations add ""InitialMigration"" --project ""{infraProjectClassPath.FullClassPath}"" --startup-project ""{webApiProjectClassPath.FullClassPath}"" --output-dir Migrations",
                srcDirectory,
                new Dictionary<string, string>()
                {
                    { "ASPNETCORE_ENVIRONMENT", Guid.NewGuid().ToString() } // guid to not conflict with any given envs
                },
                20000,
                "Db Migrations timed out and will need to be run manually.");
        }

        private static void RunTemplateBuilders(string rootDirectory, string srcDirectory, string testDirectory, ApiTemplate template, IFileSystem fileSystem, Verbosity verbosity)
        {
            // dbcontext
            DbContextBuilder.CreateDbContext(srcDirectory, template.Entities, template.DbContext.ContextName, template.DbContext.Provider, template.DbContext.DatabaseName, template.SolutionName);
            if (verbosity == Verbosity.More)
                WriteHelpText($"{template.SolutionName} db context was scaffolded successfully.");

            //entities
            EntityScaffolding.ScaffoldEntities(srcDirectory, 
                testDirectory,
                template.SolutionName,
                template.Entities,
                template.DbContext.DatabaseName,
                template.SwaggerConfig.AddSwaggerComments,
                template.AuthorizationSettings.Policies,
                fileSystem, 
                verbosity);

            // environments
            Utilities.AddStartupEnvironmentsWithServices(
                srcDirectory,
                template.SolutionName,
                template.DbContext.DatabaseName,
                template.Environments,
                template.SwaggerConfig,
                template.Port,
                template.AddJwtAuthentication,
                template.SolutionName
            );
            if (verbosity == Verbosity.More)
                WriteHelpText($"{template.SolutionName} startup environments were scaffolded successfully.");

            // unit tests, test utils, and one offs
            PagedListTestBuilder.CreateTests(testDirectory, template.SolutionName);
            IntegrationTestFixtureBuilder.CreateFixture(testDirectory, template.SolutionName, template.DbContext.ContextName, template.DbContext.Provider, fileSystem);
            IntegrationTestBaseBuilder.CreateBase(testDirectory, template.SolutionName, template.DbContext.Provider, fileSystem);
            DockerDatabaseUtilitiesBuilder.CreateClass(testDirectory, template.SolutionName, template.DbContext.Provider, fileSystem);
            ApiRoutesBuilder.CreateClass(testDirectory, template.SolutionName, template.Entities, fileSystem);
            WebAppFactoryBuilder.CreateWebAppFactory(testDirectory, template.SolutionName, template.DbContext.ContextName, template.AddJwtAuthentication);
            FunctionalTestBaseBuilder.CreateBase(testDirectory, template.SolutionName, template.DbContext.ContextName, fileSystem);
            HealthTestBuilder.CreateTests(testDirectory, template.SolutionName);
            if (verbosity == Verbosity.More)
                WriteHelpText($"{template.SolutionName} unit tests and testing utilities were scaffolded successfully.");

            //seeders
            SeederBuilder.AddSeeders(srcDirectory, template.Entities, template.DbContext.ContextName, template.SolutionName);
            if (verbosity == Verbosity.More)
                WriteHelpText($"{template.SolutionName} seeders were scaffolded successfully.");

            //services
            // TODO move the auth stuff to a modifier to make it SOLID so i can add it to an add auth command
            SwaggerBuilder.AddSwagger(srcDirectory, template.SwaggerConfig, template.SolutionName, template.AddJwtAuthentication, template.AuthorizationSettings.Policies, template.SolutionName);

            if (verbosity == Verbosity.More)
                WriteHelpText($"{template.SolutionName} swagger setup was scaffolded successfully.");

            if (template.AddJwtAuthentication)
            {
                InfrastructureServiceRegistrationModifier.InitializeAuthServices(srcDirectory, template.SolutionName, template.AuthorizationSettings.Policies);
                if (verbosity == Verbosity.More)
                    WriteHelpText($"{template.SolutionName} auth helpers were scaffolded successfully.");
            }

            ReadmeBuilder.CreateBoundedContextReadme(rootDirectory, template.SolutionName, srcDirectory, fileSystem);
            if (verbosity == Verbosity.More)
                WriteHelpText($"{template.SolutionName} readme was scaffolded successfully.");
        }
    }
}
