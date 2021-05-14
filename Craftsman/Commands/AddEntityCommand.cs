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
    using Spectre.Console;

    public static class AddEntityCommand
    {
        public static void Help()
        {
            WriteHelpHeader(@$"Description:");
            WriteHelpText(@$"   This command can add one or more new entities to your Wrapt project using a formatted yaml or json file.{Environment.NewLine}");

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
            WriteHelpText(@$"   craftsman add:entities C:\fullpath\newentity.yaml");
            WriteHelpText(@$"   craftsman add:entities C:\fullpath\newentity.yml");
            WriteHelpText(@$"   craftsman add:entities C:\fullpath\newentity.json");
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

                Utilities.IsBoundedContextDirectoryGuard(srcDirectory, testDirectory);
                var projectBaseName = Directory.GetParent(srcDirectory).Name;
                template = GetDbContext(srcDirectory, template, projectBaseName);
                template.SolutionName = projectBaseName;

                WriteHelpText($"Your template file was parsed successfully.");

                FileParsingHelper.RunPrimaryKeyGuard(template.Entities);

                // add all files based on the given template config
                RunEntityBuilders(srcDirectory, testDirectory, template, fileSystem, verbosity);

                WriteHelpHeader($"{Environment.NewLine}Your entities have been successfully added. Keep up the good work!");
            }
            catch (Exception e)
            {
                if (e is IsNotBoundedContextDirectory)
                {
                    WriteError($"{e.Message}");
                }
                else
                {
                    AnsiConsole.WriteException(e, new ExceptionSettings
                    {
                        Format = ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks,
                        Style = new ExceptionStyle
                        {
                            Exception = new Style().Foreground(Color.Grey),
                            Message = new Style().Foreground(Color.White),
                            NonEmphasized = new Style().Foreground(Color.Cornsilk1),
                            Parenthesis = new Style().Foreground(Color.Cornsilk1),
                            Method = new Style().Foreground(Color.Red),
                            ParameterName = new Style().Foreground(Color.Cornsilk1),
                            ParameterType = new Style().Foreground(Color.Red),
                            Path = new Style().Foreground(Color.Red),
                            LineNumber = new Style().Foreground(Color.Cornsilk1),
                        }
                    });
                }
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

        private static AddEntityTemplate GetDbContext(string solutionDirectory, AddEntityTemplate template, string projectBaseName)
        {
            var classPath = ClassPathHelper.DbContextClassPath(solutionDirectory, $"", projectBaseName);
            var contextClass = Directory.GetFiles(classPath.FullClassPath, "*.cs").FirstOrDefault();

            template.DbContextName = Path.GetFileNameWithoutExtension(contextClass);
            return template;
        }
    }
}