namespace Craftsman.Commands
{
    using Craftsman.Builders;
    using Craftsman.Builders.Dtos;
    using Craftsman.Builders.Features;
    using Craftsman.Builders.Seeders;
    using Craftsman.Builders.Tests.Fakes;
    using Craftsman.Builders.Tests.IntegrationTests;
    using Craftsman.Builders.Tests.RepositoryTests;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using FluentAssertions.Common;
    using LibGit2Sharp;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using YamlDotNet.Serialization;
    using static Helpers.ConsoleWriter;

    public static class NewApiCommand
    {
        public static void Help()
        {
            WriteHelpHeader(@$"Description:");
            WriteHelpText(@$"   Scaffolds out API files and projects based on a given template file in a json or yaml format.{Environment.NewLine}");

            WriteHelpHeader(@$"Usage:");
            WriteHelpText(@$"   craftsman new:api [options] <filepath>{Environment.NewLine}");

            WriteHelpHeader(@$"Arguments:");
            WriteHelpText(@$"   filepath         The full filepath for the yaml or json file that describes your web API using a proper Wrapt format.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Options:");
            WriteHelpText(@$"   -h, --help          Display this help message. No filepath is needed to display the help message.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Example:");
            WriteHelpText(@$"       craftsman new:api C:\fullpath\api.yaml");
            WriteHelpText(@$"       craftsman new:api C:\fullpath\api.yml");
            WriteHelpText(@$"       craftsman new:api C:\fullpath\api.json{Environment.NewLine}");

        }

        public static void Run(string filePath, string buildSolutionDirectory, IFileSystem fileSystem)
        {
            try
            {
                GlobalSingleton instance = GlobalSingleton.GetInstance();

                FileParsingHelper.RunInitialTemplateParsingGuards(filePath);
                var template = FileParsingHelper.GetTemplateFromFile<ApiTemplate>(filePath);
                WriteHelpText($"Your template file was parsed successfully.");

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

                // add all files based on the given template config
                RunTemplateBuilders(solutionDirectory, srcDirectory, template, fileSystem);

                WriteFileCreatedUpdatedResponse();
                WriteHelpHeader($"{Environment.NewLine}Your API is ready! Build something amazing.");
                WriteGettingStarted(template.SolutionName);
                StarGithubRequest();
            }
            catch (Exception e)
            {
                if (e is FileAlreadyExistsException
                    || e is DirectoryAlreadyExistsException
                    || e is InvalidSolutionNameException
                    || e is FileNotFoundException
                    || e is InvalidDbProviderException
                    || e is InvalidFileTypeException)
                {
                    WriteError($"{e.Message}");
                }
                else
                    WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
            }
        }

        private static void RunTemplateBuilders(string rootDirectory, string solutionDirectory, ApiTemplate template, IFileSystem fileSystem)
        {
            // dbcontext
            DbContextBuilder.CreateDbContext(solutionDirectory, template.Entities, template.DbContext.ContextName, template.DbContext.Provider, template.DbContext.DatabaseName, template.SolutionName);

            //entities
            foreach (var entity in template.Entities)
            {
                EntityBuilder.CreateEntity(solutionDirectory, entity, template.SolutionName, fileSystem);
                DtoBuilder.CreateDtos(solutionDirectory, entity, template.SolutionName);

                ValidatorBuilder.CreateValidators(solutionDirectory, template.SolutionName, entity);
                ProfileBuilder.CreateProfile(solutionDirectory, entity, template.SolutionName);

                QueryGetRecordBuilder.CreateQuery(solutionDirectory, entity, template.DbContext.ContextName, template.SolutionName);
                QueryGetListBuilder.CreateQuery(solutionDirectory, entity, template.DbContext.ContextName, template.SolutionName);
                CommandAddRecordBuilder.CreateCommand(solutionDirectory, entity, template.DbContext.ContextName, template.SolutionName);
                CommandDeleteRecordBuilder.CreateCommand(solutionDirectory, entity, template.DbContext.ContextName, template.SolutionName);
                CommandUpdateRecordBuilder.CreateCommand(solutionDirectory, entity, template.DbContext.ContextName, template.SolutionName);
                CommandPatchRecordBuilder.CreateCommand(solutionDirectory, entity, template.DbContext.ContextName, template.SolutionName);

                ControllerBuilder.CreateController(solutionDirectory, entity, template.SwaggerConfig.AddSwaggerComments, template.AuthorizationSettings.Policies, template.SolutionName);

                FakesBuilder.CreateFakes(solutionDirectory, template.SolutionName, entity);
                ReadTestBuilder.CreateEntityReadTests(solutionDirectory, template.SolutionName, entity, template.DbContext.ContextName);
                GetTestBuilder.CreateEntityGetTests(solutionDirectory, template.SolutionName, entity, template.DbContext.ContextName, template.AuthorizationSettings.Policies, template.SolutionName);
                PostTestBuilder.CreateEntityWriteTests(solutionDirectory, entity, template.SolutionName, template.AuthorizationSettings.Policies, template.SolutionName);
                UpdateTestBuilder.CreateEntityUpdateTests(solutionDirectory, entity, template.SolutionName, template.DbContext.ContextName, template.AuthorizationSettings.Policies, template.SolutionName);
                DeleteIntegrationTestBuilder.CreateEntityDeleteTests(solutionDirectory, entity, template.SolutionName, template.DbContext.ContextName, template.AuthorizationSettings.Policies, template.SolutionName);
                DeleteTestBuilder.DeleteEntityWriteTests(solutionDirectory, entity, template.SolutionName, template.DbContext.ContextName);
                WebAppFactoryBuilder.CreateWebAppFactory(solutionDirectory, template.SolutionName, template.DbContext.ContextName, template.AddJwtAuthentication);
            }

            // environments
            Utilities.AddStartupEnvironmentsWithServices(
                solutionDirectory,
                template.SolutionName,
                template.DbContext.DatabaseName,
                template.Environments,
                template.SwaggerConfig,
                template.Port,
                template.AddJwtAuthentication, 
                template.SolutionName
            );

            //seeders
            SeederBuilder.AddSeeders(solutionDirectory, template.Entities, template.DbContext.ContextName, template.SolutionName);

            //services
            // TODO move the auth stuff to a modifier to make it SOLID so i can add it to an add auth command
            SwaggerBuilder.AddSwagger(solutionDirectory, template.SwaggerConfig, template.SolutionName, template.AddJwtAuthentication, template.AuthorizationSettings.Policies, template.SolutionName);

            if (template.AddJwtAuthentication)
                InfrastructureServiceRegistrationModifier.InitializeAuthServices(solutionDirectory, template.SolutionName, template.AuthorizationSettings.Policies);

            //final
            ReadmeBuilder.CreateReadme(rootDirectory, template.SolutionName, fileSystem);

            if (template.AddGit)
                GitSetup(rootDirectory);
        }

        private static void GitSetup(string solutionDirectory)
        {
            GitBuilder.CreateGitIgnore(solutionDirectory);

            Repository.Init(solutionDirectory);
            var repo = new Repository(solutionDirectory);

            string[] allFiles = Directory.GetFiles(solutionDirectory, "*.*", SearchOption.AllDirectories);
            Commands.Stage(repo, allFiles);

            var author = new Signature("Craftsman", "craftsman", DateTimeOffset.Now);
            repo.Commit("Initial Commit", author, author);
        }
    }
}
