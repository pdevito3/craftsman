namespace Craftsman.Commands
{
    using Craftsman.Enums;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO.Abstractions;
    using static Helpers.ConsoleWriter;
    using Spectre.Console;
    using Craftsman.Exceptions;
    using System.IO;

    public static class AddBoundedContextCommand
    {
        public static void Help()
        {
            WriteHelpHeader(@$"Description:");
            WriteHelpText(@$"   Scaffolds out new bounded context for a Wrapt domain project based on a given template file in a json or yaml format.{Environment.NewLine}");

            WriteHelpHeader(@$"Usage:");
            WriteHelpText(@$"   craftsman add:bc [options] <filepath>{Environment.NewLine}");
            WriteHelpText(@$"   or");
            WriteHelpText(@$"   craftsman add:boundedcontext [options] <filepath>{Environment.NewLine}");

            WriteHelpHeader(@$"Arguments:");
            WriteHelpText(@$"   filepath         The full filepath for the yaml or json file that describes your web API using a proper Wrapt format.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Options:");
            WriteHelpText(@$"   -h, --help          Display this help message. No filepath is needed to display the help message.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Example:");
            WriteHelpText(@$"   craftsman add:bc C:\fullpath\api.yaml");
            WriteHelpText(@$"   craftsman add:bc C:\fullpath\api.yml");
            WriteHelpText(@$"   craftsman add:bc C:\fullpath\api.json{Environment.NewLine}");
        }

        public static void Run(string filePath, string domainDirectory, IFileSystem fileSystem, Verbosity verbosity)
        {
            try
            {
                FileParsingHelper.RunInitialTemplateParsingGuards(filePath);
                Utilities.SolutionGuard(domainDirectory);

                var boundedContexts = FileParsingHelper.GetTemplateFromFile<BoundedContextsTemplate>(filePath);
                WriteHelpText($"Your template file was parsed successfully.");

                foreach (var template in boundedContexts.BoundedContexts)
                {
                    ApiScaffolding.ScaffoldApi(domainDirectory, template, fileSystem, verbosity);
                }

                WriteHelpHeader($"{Environment.NewLine}Your bounded contexts have been successfully added. Keep up the good work!");
                StarGithubRequest();
            }
            catch (Exception e)
            {
                if (e is FileAlreadyExistsException
                    || e is DirectoryAlreadyExistsException
                    || e is InvalidSolutionNameException
                    || e is FileNotFoundException
                    || e is InvalidDbProviderException
                    || e is InvalidFileTypeException
                    || e is DataValidationErrorException
                    || e is SolutiuonNameEntityMatchException)
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