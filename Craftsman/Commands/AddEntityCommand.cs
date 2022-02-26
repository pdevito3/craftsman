namespace Craftsman.Commands
{
    using Craftsman.Builders;
    using Craftsman.Builders.Seeders;
    using Craftsman.Builders.Tests.Utilities;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
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

        public static void Run(string filePath, string boundaryDirectory, IFileSystem fileSystem, Verbosity verbosity)
        {
            try
            {
                FileParsingHelper.RunInitialTemplateParsingGuards(filePath);
                var template = FileParsingHelper.GetTemplateFromFile<AddEntityTemplate>(filePath);

                var srcDirectory = Path.Combine(boundaryDirectory, "src");
                var testDirectory = Path.Combine(boundaryDirectory, "tests");

                Utilities.IsBoundedContextDirectoryGuard(srcDirectory, testDirectory);
                
                var solutionDirectory = Directory.GetParent(boundaryDirectory)?.FullName;
                Utilities.IsSolutionDirectoryGuard(solutionDirectory);
                
                var projectBaseName = Directory.GetParent(srcDirectory).Name;
                template = GetDbContext(srcDirectory, template, projectBaseName);
                template.SolutionName = projectBaseName;

                WriteHelpText($"Your template file was parsed successfully.");

                FileParsingHelper.RunPrimaryKeyGuard(template.Entities);

                // add all files based on the given template config
                RunEntityBuilders(solutionDirectory, srcDirectory, testDirectory, template, fileSystem);

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

        private static void RunEntityBuilders(string solutionDirectory, string srcDirectory, string testDirectory, AddEntityTemplate template, IFileSystem fileSystem)
        {
            var projectBaseName = template.SolutionName;
            var useSoftDelete = Utilities.ProjectUsesSoftDelete(srcDirectory, projectBaseName);
            
            //entities
            EntityScaffolding.ScaffoldEntities(solutionDirectory,
                srcDirectory,
                testDirectory,
                projectBaseName,
                template.Entities,
                template.DbContextName,
                template.AddSwaggerComments,
                useSoftDelete,
                fileSystem);

            SeederModifier.AddSeeders(srcDirectory, template.Entities, template.DbContextName, projectBaseName);
            DbContextModifier.AddDbSet(srcDirectory, template.Entities, template.DbContextName, projectBaseName);
        }

        private static AddEntityTemplate GetDbContext(string srcDirectory, AddEntityTemplate template, string projectBaseName)
        {
            template.DbContextName = Utilities.GetDbContext(srcDirectory, projectBaseName);
            return template;
        }
    }
}
