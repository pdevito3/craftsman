namespace Craftsman.Commands
{
    using Craftsman.Builders;
    using Craftsman.Builders.Tests.Utilities;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using static Helpers.ConsoleWriter;
    using Spectre.Console;

    public static class AddBffEntityCommand
    {
        public static void Help()
        {
            WriteHelpHeader(@$"Description:");
            WriteHelpText(@$"   This command can add one or more new entities to your BFF client using a formatted yaml or json file.{Environment.NewLine}");

            WriteHelpHeader(@$"Usage:");
            WriteHelpText(@$"   craftsman add:bffentity [options] <filepath>");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Arguments:");
            WriteHelpText(@$"   filepath         The filepath for the yaml or json file that lists the new entities that you want to add to your API.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Options:");
            WriteHelpText(@$"   -h, --help          Display this help message. No filepath is needed to display the help message.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Example:");
            WriteHelpText(@$"   craftsman add:bffentity C:\fullpath\newentity.yaml");
            WriteHelpText(@$"   craftsman add:bffentity C:\fullpath\newentity.yml");
            WriteHelpText(@$"   craftsman add:bffentity C:\fullpath\newentity.json");
            WriteHelpText(Environment.NewLine);
        }

        public static void Run(string filePath, string domainDirectory, IFileSystem fileSystem)
        {
            try
            {
                FileParsingHelper.RunInitialTemplateParsingGuards(filePath);
                Utilities.IsSolutionDirectoryGuard(domainDirectory);

                var template = FileParsingHelper.GetTemplateFromFile<BffEntityTemplate>(filePath);
                WriteHelpText($"Your template file was parsed successfully.");
                
                var spaDirectory = template.GetSpaDirectory(domainDirectory);
                EntityScaffolding.ScaffoldBffEntities(template.Entities, fileSystem, spaDirectory);

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
    }
}
