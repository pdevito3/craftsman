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
    using Craftsman.Removers;
    using FluentAssertions.Common;
    using LibGit2Sharp;
    using Newtonsoft.Json;
    using System;
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
                var template = FileParsingHelper.GetApiTemplateFromFile(filePath);
                WriteHelpText($"Your template file was parsed successfully.");

                FileParsingHelper.RunPrimaryKeyGuard(template);
                FileParsingHelper.RunSolutionNameAssignedGuard(template);

                //var rootProjectDirectory = Directory.GetCurrentDirectory().Contains("Debug") ? @"C:\Users\Paul\Documents\testoutput" : Directory.GetCurrentDirectory();
                //var buildSolutionDirectory = @"C:\Users\Paul\Documents\testoutput";

                // scaffold projects
                // should i add an accelerate.config.yaml file to the root?
                CreateNewFoundation(template, buildSolutionDirectory); // todo scaffold this manually instead of using dotnet new foundation
                var solutionDirectory = $"{buildSolutionDirectory}{Path.DirectorySeparatorChar}{template.SolutionName}";


                // remove placeholder valuetoreplace files and directories
                ApiTemplateCleaner.CleanTemplateFilesAndDirectories(solutionDirectory, template);

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
            DbContextBuilder.CreateDbContext(solutionDirectory, template);

            //entities
            foreach (var entity in template.Entities)
            {
                EntityBuilder.CreateEntity(solutionDirectory, entity, fileSystem);
                DtoBuilder.CreateDtos(solutionDirectory, entity);

                RepositoryBuilder.AddRepository(solutionDirectory, entity, template.DbContext);
                ValidatorBuilder.CreateValidators(solutionDirectory, entity);
                ProfileBuilder.CreateProfile(solutionDirectory, entity);

                ControllerBuilder.CreateController(solutionDirectory, entity);

                FakesBuilder.CreateFakes(solutionDirectory, template, entity);
                ReadTestBuilder.CreateEntityReadTests(solutionDirectory, template, entity);
                GetTestBuilder.CreateEntityGetTests(solutionDirectory, template, entity);
                PostTestBuilder.CreateEntityWriteTests(solutionDirectory, template, entity);
                UpdateTestBuilder.CreateEntityUpdateTests(solutionDirectory, template, entity);
                DeleteTestBuilder.DeleteEntityWriteTests(solutionDirectory, template, entity);
                WebAppFactoryBuilder.CreateWebAppFactory(solutionDirectory, template, entity);
            }

            // environments
            AddStartupEnvironmentsWithServices(solutionDirectory, template);

            //seeders
            SeederBuilder.AddSeeders(solutionDirectory, template);

            //services
            SwaggerBuilder.AddSwagger(solutionDirectory, template);
            
            if(template.AuthSetup.AuthMethod == "JWT")
            {
                IdentityServicesModifier.SetIdentityOptions(solutionDirectory, template.AuthSetup);
                IdentitySeederBuilder.AddSeeders(solutionDirectory, template);
                IdentityRoleBuilder.CreateRoles(solutionDirectory, template.AuthSetup.Roles);
                RoleSeedBuilder.SeedRoles(solutionDirectory, template);
            }

            //final
            ReadmeBuilder.CreateReadme(solutionDirectory, template, fileSystem);

            if (template.AddGit)
                GitSetup(solutionDirectory);
        }

        private static void CreateNewFoundation(ApiTemplate template, string directory)
        {
            var newDir = $"{directory}{Path.DirectorySeparatorChar}{template.SolutionName}";
            if (Directory.Exists(newDir))
                throw new DirectoryAlreadyExistsException(newDir);

            //UninstallFoundation();
            //InstallFoundation();
            //UpdateFoundation();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = @$"new foundation -n {template.SolutionName}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = directory
                }
            };

            process.Start();
            process.WaitForExit();
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

        private static void UpdateFoundation()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = @$"new Foundation.Api --update-apply",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                }
            };

            process.Start();
            process.WaitForExit();
        }

        private static void UninstallFoundation()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = @$"new -u Foundation.Api",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                }
            };

            process.Start();
            process.WaitForExit();
        }

        private static void InstallFoundation()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = @$"new -i Foundation.Api",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                }
            };

            process.Start();
            process.WaitForExit();
        }

        private static void AddStartupEnvironmentsWithServices(string solutionDirectory, ApiTemplate template)
        {
            // add a development environment by default for local work if none exists
            if (template.Environments.Where(e => e.EnvironmentName == "Development").Count() == 0)
                template.Environments.Add(new ApiEnvironment { EnvironmentName = "Development", ProfileName = $"{template.SolutionName} (Development)" });

            foreach (var env in template.Environments)
            {
                StartupBuilder.CreateStartup(solutionDirectory, env.EnvironmentName, template);
                AppSettingsBuilder.CreateAppSettings(solutionDirectory, env, template.DbContext.DatabaseName, template);
                LaunchSettingsModifier.AddProfile(solutionDirectory, env);

                //services

                if (!template.SwaggerConfig.IsSameOrEqualTo(new SwaggerConfig()))
                    SwaggerBuilder.RegisterSwaggerInStartup(solutionDirectory, env);
            }
        }
    }
}
