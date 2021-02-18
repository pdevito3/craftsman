namespace Craftsman.Commands
{
    using Craftsman.Builders;
    using Craftsman.Builders.Dtos;
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
                var solutionDirectory = $"{buildSolutionDirectory}{Path.DirectorySeparatorChar}{template.SolutionName}";
                
                SolutionBuilder.BuildSolution(solutionDirectory, template.SolutionName, fileSystem);
                SolutionBuilder.AddProjects(solutionDirectory, solutionDirectory, template.DbContext.Provider, template.SolutionName, template.AddJwtAuthentication, fileSystem);

                // add all files based on the given template config
                RunTemplateBuilders(solutionDirectory, template, fileSystem);

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

        private static void RunTemplateBuilders(string solutionDirectory, ApiTemplate template, IFileSystem fileSystem)
        {
            // dbcontext
            DbContextBuilder.CreateDbContext(solutionDirectory, template.Entities, template.DbContext.ContextName, template.DbContext.Provider, template.DbContext.DatabaseName);

            //entities
            foreach (var entity in template.Entities)
            {
                EntityBuilder.CreateEntity(solutionDirectory, entity, fileSystem);
                DtoBuilder.CreateDtos(solutionDirectory, entity);

                RepositoryBuilder.AddRepository(solutionDirectory, entity, template.DbContext);
                ValidatorBuilder.CreateValidators(solutionDirectory, entity);
                ProfileBuilder.CreateProfile(solutionDirectory, entity);

                ControllerBuilder.CreateController(solutionDirectory, entity, template.SwaggerConfig.AddSwaggerComments, template.AuthorizationSettings.Policies);

                FakesBuilder.CreateFakes(solutionDirectory, template.SolutionName, entity);
                ReadTestBuilder.CreateEntityReadTests(solutionDirectory, template.SolutionName, entity, template.DbContext.ContextName);
                GetTestBuilder.CreateEntityGetTests(solutionDirectory, template.SolutionName, entity, template.DbContext.ContextName, template.AddJwtAuthentication, template.AuthorizationSettings.Policies);
                PostTestBuilder.CreateEntityWriteTests(solutionDirectory, entity, template.SolutionName, template.AddJwtAuthentication, template.AuthorizationSettings.Policies);
                UpdateTestBuilder.CreateEntityUpdateTests(solutionDirectory, entity, template.SolutionName, template.DbContext.ContextName, template.AddJwtAuthentication, template.AuthorizationSettings.Policies);
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
                template.AddJwtAuthentication
            );

            //seeders
            SeederBuilder.AddSeeders(solutionDirectory, template.Entities, template.DbContext.ContextName);

            //services
            SwaggerBuilder.AddSwagger(solutionDirectory, template.SwaggerConfig, template.SolutionName, template.AddJwtAuthentication, template.AuthorizationSettings.Policies);

            if(template.AddJwtAuthentication)
                InfrastructureIdentityServiceRegistrationBuilder.CreateInfrastructureIdentityServiceExtension(solutionDirectory, template.AuthorizationSettings.Policies, fileSystem);

            //final
            ReadmeBuilder.CreateReadme(solutionDirectory, template.SolutionName, fileSystem);

            if (template.AddGit)
                GitSetup(solutionDirectory);
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
