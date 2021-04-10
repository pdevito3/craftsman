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
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using YamlDotNet.Serialization;
    using static Helpers.ConsoleWriter;

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
            WriteHelpText(@$"       craftsman add:bc C:\fullpath\api.yaml");
            WriteHelpText(@$"       craftsman add:bc C:\fullpath\api.yml");
            WriteHelpText(@$"       craftsman add:bc C:\fullpath\api.json{Environment.NewLine}");

        }

        public static void Run(string filePath, string domainDirectory, IFileSystem fileSystem, Verbosity verbosity)
        {
            try
            {
                FileParsingHelper.RunInitialTemplateParsingGuards(filePath);
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
                    || e is SolutiuonNameEntityMatchException)
                {
                    WriteError($"{e.Message}");
                }
                else
                    WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
            }
        }
    }
}
