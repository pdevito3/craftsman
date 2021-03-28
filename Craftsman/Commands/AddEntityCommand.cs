
namespace Craftsman.Commands
{
    using Craftsman.Builders;
    using Craftsman.Builders.Dtos;
    using Craftsman.Builders.Seeders;
    using Craftsman.Builders.Tests.Fakes;
    using Craftsman.Builders.Tests.Utilities;
    using Craftsman.Enums;
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

        public static void Run(string filePath, string solutionDirectory, IFileSystem fileSystem, Verbosity verbosity)
        {
            try
            {
                FileParsingHelper.RunInitialTemplateParsingGuards(filePath);
                var template = FileParsingHelper.GetTemplateFromFile<AddEntityTemplate>(filePath);

                var srcDirectory = Path.Combine(solutionDirectory, "src");
                var testDirectory = Path.Combine(solutionDirectory, "tests");

                template.SolutionName = Utilities.SolutionGuard(solutionDirectory); // this needs to happen before the projectbasename assignment
                ProperDirectoryGuard(srcDirectory, testDirectory);
                template = GetDbContext(srcDirectory, template, template.SolutionName);

                WriteHelpText($"Your template file was parsed successfully.");

                FileParsingHelper.RunPrimaryKeyGuard(template.Entities);

                // add all files based on the given template config
                RunEntityBuilders(srcDirectory, testDirectory, template, fileSystem, verbosity);

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
                    || e is SolutionNotFoundException
                    || e is InvalidBaseDirectory)
                {
                    WriteError($"{e.Message}");
                }
                else
                    WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
            }
        }

        private static void RunEntityBuilders(string srcDirectory, string testDirectory, AddEntityTemplate template, IFileSystem fileSystem, Verbosity verbosity)
        {
            //entities
            EntityScaffolding.ScaffoldEntities(srcDirectory,
                testDirectory,
                template.SolutionName,
                template.Entities,
                template.DbContextName,
                template.AddSwaggerComments,
                template.AuthorizationSettings.Policies,
                fileSystem,
                verbosity);

            //seeders & dbsets
            InfrastructureServiceRegistrationModifier.AddPolicies(srcDirectory, template.AuthorizationSettings.Policies, template.SolutionName);
            SeederModifier.AddSeeders(srcDirectory, template.Entities, template.DbContextName, template.SolutionName);
            DbContextModifier.AddDbSet(srcDirectory, template.Entities, template.DbContextName, template.SolutionName);
            ApiRouteModifier.AddRoutes(testDirectory, template.Entities, template.SolutionName);
        }

        private static void ProperDirectoryGuard(string srcDirectory, string testDirectory)
        {
            if(!Directory.Exists(srcDirectory) || !Directory.Exists(testDirectory))
                throw new InvalidBaseDirectory();
        }

        private static AddEntityTemplate GetDbContext(string solutionDirectory, AddEntityTemplate template, string projectBaseName)
        {
            var classPath = ClassPathHelper.DbContextClassPath(solutionDirectory, $"", projectBaseName);
            var contextClass = Directory.GetFiles(classPath.FullClassPath, "*.cs").FirstOrDefault();

            template.DbContextName = Path.GetFileNameWithoutExtension(contextClass);
            return template;
        }
    }
}
