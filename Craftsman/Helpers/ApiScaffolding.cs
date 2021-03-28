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
            SolutionBuilder.AddProjects(solutionDirectory, srcDirectory, testDirectory, template.DbContext.Provider, template.SolutionName, template.AddJwtAuthentication, fileSystem);
            if(verbosity == Verbosity.More)
                WriteHelpText($"{template.SolutionName} projects were scaffolded successfully.");

            // add all files based on the given template config
            RunTemplateBuilders(solutionDirectory, srcDirectory, testDirectory, template, fileSystem, verbosity);
            if(verbosity == Verbosity.More)
                WriteHelpText($"{template.SolutionName} templates were scaffolded successfully.");

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
                });
        }

        private static void RunTemplateBuilders(string rootDirectory, string srcDirectory, string testDirectory, ApiTemplate template, IFileSystem fileSystem, Verbosity verbosity)
        {
            // dbcontext
            DbContextBuilder.CreateDbContext(srcDirectory, template.Entities, template.DbContext.ContextName, template.DbContext.Provider, template.DbContext.DatabaseName, template.SolutionName);
            if(verbosity == Verbosity.More)
                WriteHelpText($"{template.SolutionName} db context was scaffolded successfully.");

            //entities
            foreach (var entity in template.Entities)
            {
                EntityBuilder.CreateEntity(srcDirectory, entity, template.SolutionName, fileSystem);
                DtoBuilder.CreateDtos(srcDirectory, entity, template.SolutionName);

                ValidatorBuilder.CreateValidators(srcDirectory, template.SolutionName, entity);
                ProfileBuilder.CreateProfile(srcDirectory, entity, template.SolutionName);

                QueryGetRecordBuilder.CreateQuery(srcDirectory, entity, template.DbContext.ContextName, template.SolutionName);
                QueryGetListBuilder.CreateQuery(srcDirectory, entity, template.DbContext.ContextName, template.SolutionName);
                CommandAddRecordBuilder.CreateCommand(srcDirectory, entity, template.DbContext.ContextName, template.SolutionName);
                CommandDeleteRecordBuilder.CreateCommand(srcDirectory, entity, template.DbContext.ContextName, template.SolutionName);
                CommandUpdateRecordBuilder.CreateCommand(srcDirectory, entity, template.DbContext.ContextName, template.SolutionName);
                CommandPatchRecordBuilder.CreateCommand(srcDirectory, entity, template.DbContext.ContextName, template.SolutionName);

                ControllerBuilder.CreateController(srcDirectory, entity, template.SwaggerConfig.AddSwaggerComments, template.AuthorizationSettings.Policies, template.SolutionName);

                // Shared Tests
                FakesBuilder.CreateFakes(testDirectory, template.SolutionName, entity);
                
                // Integration Tests
                AddCommandTestBuilder.CreateTests(testDirectory, entity, template.SolutionName);
                DeleteCommandTestBuilder.CreateTests(testDirectory, entity, template.SolutionName);
                PatchCommandTestBuilder.CreateTests(testDirectory, entity, template.SolutionName);
                GetRecordQueryTestBuilder.CreateTests(testDirectory, entity, template.SolutionName);
                GetListQueryTestBuilder.CreateTests(testDirectory, entity, template.SolutionName);
                PutCommandTestBuilder.CreateTests(testDirectory, entity, template.SolutionName);

                // Functional Tests
                CreateEntityTestBuilder.CreateTests(testDirectory, entity, template.AuthorizationSettings.Policies, template.SolutionName);
                DeleteEntityTestBuilder.CreateTests(testDirectory, entity, template.AuthorizationSettings.Policies, template.SolutionName);
                GetEntityRecordTestBuilder.CreateTests(testDirectory, entity, template.AuthorizationSettings.Policies, template.SolutionName);
                GetEntityListTestBuilder.CreateTests(testDirectory, entity, template.AuthorizationSettings.Policies, template.SolutionName);
                PatchEntityTestBuilder.CreateTests(testDirectory, entity, template.AuthorizationSettings.Policies, template.SolutionName);
                PutEntityTestBuilder.CreateTests(testDirectory, entity, template.AuthorizationSettings.Policies, template.SolutionName);

                if(verbosity == Verbosity.More)
                    WriteHelpText($"{template.SolutionName} '{entity.Name}' entity was scaffolded successfully.");
            }

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
            if(verbosity == Verbosity.More)
                WriteHelpText($"{template.SolutionName} startup environments were scaffolded successfully.");

            // unit tests, test utils, and one offs
            PagedListTestBuilder.CreateTests(testDirectory, template.SolutionName);
            IntegrationTestFixtureBuilder.CreateFixture(testDirectory, template.SolutionName, template.DbContext.ContextName, template.DbContext.Provider, fileSystem);
            IntegrationTestBaseBuilder.CreateBase(testDirectory, template.SolutionName, fileSystem);
            DockerDatabaseUtilitiesBuilder.CreateClass(testDirectory, template.SolutionName, template.DbContext.Provider, fileSystem);
            ApiRoutesBuilder.CreateClass(testDirectory, template.SolutionName, template.Entities, fileSystem);
            WebAppFactoryBuilder.CreateWebAppFactory(testDirectory, template.SolutionName, template.DbContext.ContextName, template.AddJwtAuthentication);
            FunctionalTestBaseBuilder.CreateBase(testDirectory, template.SolutionName, template.DbContext.ContextName, fileSystem);
            HealthTestBuilder.CreateTests(testDirectory, template.SolutionName);
            if(verbosity == Verbosity.More)
                WriteHelpText($"{template.SolutionName} unit tests and testing utilities were scaffolded successfully.");

            //seeders
            SeederBuilder.AddSeeders(srcDirectory, template.Entities, template.DbContext.ContextName, template.SolutionName);
            if(verbosity == Verbosity.More)
                WriteHelpText($"{template.SolutionName} seeders were scaffolded successfully.");

            //services
            // TODO move the auth stuff to a modifier to make it SOLID so i can add it to an add auth command
            SwaggerBuilder.AddSwagger(srcDirectory, template.SwaggerConfig, template.SolutionName, template.AddJwtAuthentication, template.AuthorizationSettings.Policies, template.SolutionName);

            if(verbosity == Verbosity.More)
                WriteHelpText($"{template.SolutionName} swagger setup was scaffolded successfully.");

            if (template.AddJwtAuthentication)
            {
                InfrastructureServiceRegistrationModifier.InitializeAuthServices(srcDirectory, template.SolutionName, template.AuthorizationSettings.Policies);
                if(verbosity == Verbosity.More)
                    WriteHelpText($"{template.SolutionName} auth helpers was scaffolded successfully.");
            }

            ReadmeBuilder.CreateBoundedContextReadme(rootDirectory, template.SolutionName, srcDirectory, fileSystem);
            if(verbosity == Verbosity.More)
                WriteHelpText($"{template.SolutionName} readme was scaffolded successfully.");
        }
    }
}
