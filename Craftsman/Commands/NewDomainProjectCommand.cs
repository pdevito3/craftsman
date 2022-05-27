namespace Craftsman.Commands
{
    using Craftsman.Builders;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using Spectre.Console;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading.Tasks;
    using Builders.Docker;
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
            WriteHelpText(@$"   craftsman new:domain C:\fullpath\domain.yaml");
            WriteHelpText(@$"   craftsman new:domain C:\fullpath\domain.yml");
            WriteHelpText(@$"   craftsman new:domain C:\fullpath\domain.json{Environment.NewLine}");
        }

        public static void Run(string filePath, string buildSolutionDirectory, IFileSystem fileSystem, Verbosity verbosity)
        {
            try
            {
                FileParsingHelper.RunInitialTemplateParsingGuards(filePath);
                var domainProject = FileParsingHelper.GetTemplateFromFile<DomainProject>(filePath);
                WriteLogMessage($"Your template file was parsed successfully");

                var domainDirectory = $"{buildSolutionDirectory}{Path.DirectorySeparatorChar}{domainProject.DomainName}";
                CreateNewDomainProject(domainDirectory, fileSystem, domainProject);

                AnsiConsole.MarkupLine($"{Environment.NewLine}[bold yellow1]Your domain project is ready! Build something amazing. [/]");
                //StarGithubRequest();
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

        public static void CreateNewDomainProject(string domainDirectory, IFileSystem fileSystem,
            DomainProject domainProject)
        {
            fileSystem.Directory.CreateDirectory(domainDirectory);
            SolutionBuilder.BuildSolution(domainDirectory, domainProject.DomainName, fileSystem);

            // need this before boundaries to give them something to build against
            DockerBuilders.CreateDockerComposeSkeleton(domainDirectory, fileSystem);

            //Parallel.ForEach(domainProject.BoundedContexts, (template) =>
            //    ApiScaffolding.ScaffoldApi(domainDirectory, template, fileSystem, verbosity));
            foreach (var bc in domainProject.BoundedContexts)
                ApiScaffolding.ScaffoldApi(domainDirectory, bc, fileSystem);

            // auth server
            if (domainProject.AuthServer != null)
                AddAuthServerCommand.AddAuthServer(domainDirectory, fileSystem, domainProject.AuthServer);

            // messages
            if (domainProject.Messages.Count > 0)
                AddMessageCommand.AddMessages(domainDirectory, fileSystem, domainProject.Messages);

            // migrations
            Utilities.RunDbMigrations(domainProject.BoundedContexts, domainDirectory);

            //final
            ReadmeBuilder.CreateReadme(domainDirectory, domainProject.DomainName, fileSystem);

            if (domainProject.AddGit)
                Utilities.GitSetup(domainDirectory, domainProject.UseSystemGitUser);
        }
    }
}