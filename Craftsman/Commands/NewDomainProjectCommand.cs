namespace Craftsman.Commands
{
    using Craftsman.Builders;
    using Craftsman.Builders.Dtos;
    using Craftsman.Builders.Features;
    using Craftsman.Builders.Seeders;
    using Craftsman.Builders.Tests.Fakes;
    using Craftsman.Builders.Tests.Utilities;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using FluentAssertions.Common;
    using LibGit2Sharp;
    using Newtonsoft.Json;
    using Spectre.Console;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading.Tasks;
    using YamlDotNet.Serialization;
    using static Helpers.ConsoleWriter;

    public static class NewDomainProjectCommand
    {
        public static void Help()
        {
            WriteHelpHeader(@$"Description:");
            WriteHelpText(@$"   Scaffolds out a DDD project based on a given template file in a json or yaml format.{Environment.NewLine}");

            WriteHelpHeader(@$"Usage:");
            WriteHelpText(@$"   craftsman new:domain [options] <filepath>{Environment.NewLine}");

            WriteHelpHeader(@$"Arguments:");
            WriteHelpText(@$"   filepath         The full filepath for the yaml or json file that describes your domain using a proper Wrapt format.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Options:");
            WriteHelpText(@$"   -h, --help          Display this help message. No filepath is needed to display the help message.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Example:");
            WriteHelpText(@$"       craftsman new:domain C:\fullpath\domain.yaml");
            WriteHelpText(@$"       craftsman new:domain C:\fullpath\domain.yml");
            WriteHelpText(@$"       craftsman new:domain C:\fullpath\domain.json{Environment.NewLine}");
        }

        public static void Run(string filePath, string buildSolutionDirectory, IFileSystem fileSystem, Verbosity verbosity)
        {
            try
            {
                FileParsingHelper.RunInitialTemplateParsingGuards(filePath);
                var domainProject = FileParsingHelper.GetTemplateFromFile<DomainProject>(filePath);
                WriteHelpText($"Your template file was parsed successfully.");

                var domainDirectory = $"{buildSolutionDirectory}{Path.DirectorySeparatorChar}{domainProject.DomainName}";
                fileSystem.Directory.CreateDirectory(domainDirectory);

                Parallel.ForEach(domainProject.BoundedContexts, (template) =>
                {
                    ApiScaffolding.ScaffoldApi(domainDirectory, template, fileSystem, verbosity);
                });

                //final
                ReadmeBuilder.CreateReadme(domainDirectory, domainProject.DomainName, fileSystem);

                if (domainProject.AddGit)
                    Utilities.GitSetup(domainDirectory);

                WriteHelpHeader($"{Environment.NewLine}Your domain project is ready! Build something amazing.");
                StarGithubRequest();
            }
            catch (Exception e)
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