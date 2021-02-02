namespace Craftsman.Commands
{
    using Craftsman.Builders;
    using Craftsman.Builders.Dtos;
    using Craftsman.Builders.Seeders;
    using Craftsman.Builders.Tests.Fakes;
    using Craftsman.Builders.Tests.IntegrationTests;
    using Craftsman.Builders.Tests.RepositoryTests;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using static Helpers.ConsoleWriter;

    public static class AddEntityCommand
    {
        public static void Help()
        {
            WriteHelpHeader(@$"Description:");
            WriteHelpText(@$"   This command can add one or more new entities to your Wrapt project using a formatted 
   yaml or json file. The input file uses a simplified format from the `new:api` command that only 
   requires a list of one or more entities.{Environment.NewLine}");

            WriteHelpHeader(@$"Usage:");
            WriteHelpText(@$"   craftsman add:entity [options] <filepath>");
            WriteHelpText(@$"   or");
            WriteHelpText(@$"   craftsman add:entities [options] <filepath>");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Arguments:");
            WriteHelpText(@$"   filepath         The full filepath for the yaml or json file that lists the new entities that you want to add to your API.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Options:");
            WriteHelpText(@$"   -h, --help          Display this help message. No filepath is needed to display the help message.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Example:");
            WriteHelpText(@$"       craftsman add:entities C:\fullpath\newentity.yaml");
            WriteHelpText(@$"       craftsman add:entities C:\fullpath\newentity.yml");
            WriteHelpText(@$"       craftsman add:entities C:\fullpath\newentity.json");
            WriteHelpText(Environment.NewLine);
        }

        public static void Run(string filePath, string solutionDirectory, IFileSystem fileSystem)
        {
            try
            {
                GlobalSingleton instance = GlobalSingleton.GetInstance();

                FileParsingHelper.RunInitialTemplateParsingGuards(filePath);
                var template = FileParsingHelper.GetTemplateFromFile<ApiTemplate>(filePath);

                //var solutionDirectory = Directory.GetCurrentDirectory();
                //var solutionDirectory = @"C:\Users\Paul\Documents\testoutput\MyApi.Mine";
                template = SolutionGuard(solutionDirectory, template);
                template = GetDbContext(solutionDirectory, template);

                WriteHelpText($"Your template file was parsed successfully.");

                FileParsingHelper.RunPrimaryKeyGuard(template.Entities);

                // add all files based on the given template config
                RunEntityBuilders(solutionDirectory, template, fileSystem);

                WriteFileCreatedUpdatedResponse();
                WriteHelpHeader($"{Environment.NewLine}Your entities have been successfully added. Keep up the good work!");
            }
            catch (Exception e)
            {
                if (e is FileAlreadyExistsException
                    || e is DirectoryAlreadyExistsException
                    || e is InvalidSolutionNameException
                    || e is FileNotFoundException
                    || e is InvalidDbProviderException
                    || e is InvalidFileTypeException
                    || e is SolutionNotFoundException)
                {
                    WriteError($"{e.Message}");
                }
                else
                    WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
            }
        }

        private static void RunEntityBuilders(string solutionDirectory, ApiTemplate template, IFileSystem fileSystem)
        {
            //entities
            foreach (var entity in template.Entities)
            {
                EntityBuilder.CreateEntity(solutionDirectory, entity, fileSystem);
                DtoBuilder.CreateDtos(solutionDirectory, entity);

                RepositoryBuilder.AddRepository(solutionDirectory, entity, template.DbContext);
                ValidatorBuilder.CreateValidators(solutionDirectory, entity);
                ProfileBuilder.CreateProfile(solutionDirectory, entity);

                ControllerBuilder.CreateController(solutionDirectory, entity, template.SwaggerConfig.AddSwaggerComments, template.AuthorizationSettings.Policies);
                InfrastructureIdentityServiceRegistrationModifier.AddPolicies(solutionDirectory, template.AuthorizationSettings.Policies);

                FakesBuilder.CreateFakes(solutionDirectory, template.SolutionName, entity);
                ReadTestBuilder.CreateEntityReadTests(solutionDirectory, template.SolutionName, entity, template.DbContext.ContextName);
                GetTestBuilder.CreateEntityGetTests(solutionDirectory, template.SolutionName, entity, template.DbContext.ContextName);
                PostTestBuilder.CreateEntityWriteTests(solutionDirectory, entity, template.SolutionName);
                UpdateTestBuilder.CreateEntityUpdateTests(solutionDirectory, entity, template.SolutionName, template.DbContext.ContextName);
                DeleteTestBuilder.DeleteEntityWriteTests(solutionDirectory, entity, template.SolutionName, template.DbContext.ContextName);
            }

            //seeders & dbsets
            SeederModifier.AddSeeders(solutionDirectory, template.Entities, template.DbContext.ContextName);
            DbContextModifier.AddDbSet(solutionDirectory, template.Entities, template.DbContext.ContextName);
        }

        private static string GetSlnFile(string filePath)
        {
            // make sure i'm in the sln directory -- should i add an accelerate.config.yaml file to the root?
            return Directory.GetFiles(filePath, "*.sln").FirstOrDefault();
        }

        private static ApiTemplate SolutionGuard(string solutionDirectory, ApiTemplate template)
        {
            var slnName = GetSlnFile(solutionDirectory);
            template.SolutionName = Path.GetFileNameWithoutExtension(slnName) ?? throw new SolutionNotFoundException();

            return template;
        }

        private static ApiTemplate GetDbContext(string solutionDirectory, ApiTemplate template)
        {
            var classPath = ClassPathHelper.DbContextClassPath(solutionDirectory, $"");
            var contextClass = Directory.GetFiles(classPath.FullClassPath, "*.cs").FirstOrDefault();

            template.DbContext.ContextName = Path.GetFileNameWithoutExtension(contextClass);
            return template;
        }
    }
}
