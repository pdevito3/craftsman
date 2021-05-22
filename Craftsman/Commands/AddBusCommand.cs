namespace Craftsman.Commands
{
    using Craftsman.Builders;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using static Helpers.ConsoleWriter;
    using Spectre.Console;

    public static class AddBusCommand
    {
        public static void Help()
        {
            WriteHelpHeader(@$"Description:");
            WriteHelpText(@$"   This command will add a message bus to your web api and a messages project to your root directory using a formatted yaml or json file.{Environment.NewLine}");

            WriteHelpHeader(@$"Usage:");
            WriteHelpText(@$"   craftsman add:bus [options] <filepath>");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Arguments:");
            WriteHelpText(@$"   filepath         The full filepath for the yaml or json file that lists the bus information that you want to add to your API.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Options:");
            WriteHelpText(@$"   -h, --help          Display this help message. No filepath is needed to display the help message.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Example:");
            WriteHelpText(@$"   craftsman add:bus C:\fullpath\mybusinfo.yaml");
            WriteHelpText(@$"   craftsman add:bus C:\fullpath\mybusinfo.yml");
            WriteHelpText(@$"   craftsman add:bus C:\fullpath\mybusinfo.json");
            WriteHelpText(Environment.NewLine);
        }

        public static void Run(string filePath, string boundedContextDirectory, IFileSystem fileSystem)
        {
            try
            {
                var template = new Bus();
                template.Environments.Add(new ApiEnvironment { EnvironmentName = "Development" });
                if (!string.IsNullOrEmpty(filePath))
                {
                    FileParsingHelper.RunInitialTemplateParsingGuards(filePath);
                    template = FileParsingHelper.GetTemplateFromFile<Bus>(filePath);
                }

                var srcDirectory = Path.Combine(boundedContextDirectory, "src");
                var testDirectory = Path.Combine(boundedContextDirectory, "tests");

                Utilities.IsBoundedContextDirectoryGuard(srcDirectory, testDirectory);
                var projectBaseName = Directory.GetParent(srcDirectory).Name;
                template.ProjectBaseName = projectBaseName;

                // get solution dir
                var solutionDirectory = Directory.GetParent(boundedContextDirectory).FullName;
                Utilities.IsSolutionDirectoryGuard(solutionDirectory);
                AddBus(template, srcDirectory, projectBaseName, solutionDirectory, fileSystem);

                WriteHelpHeader($"{Environment.NewLine}Your event bus has been successfully added. Keep up the good work!");
            }
            catch (Exception e)
            {
                if (e is InvalidMessageBrokerException
                    || e is IsNotBoundedContextDirectory)
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

        public static void AddBus(Bus template, string srcDirectory, string projectBaseName, string solutionDirectory, IFileSystem fileSystem)
        {
            var messagesDirectory = Path.Combine(solutionDirectory, "Messages");

            var massTransitPackages = new Dictionary<string, string>{
                    { "MassTransit", "7.1.8" },
                    { "MassTransit.AspNetCore", "7.1.8" },
                    { "MassTransit.Extensions.DependencyInjection", "7.1.8" },
                    { "MassTransit.RabbitMQ", "7.1.8" }
                };
            var webApiClassPath = ClassPathHelper.WebApiProjectClassPath(srcDirectory, projectBaseName);
            Utilities.AddPackages(webApiClassPath, massTransitPackages);

            WebApiServiceExtensionsBuilder.CreateMassTransitServiceExtension(srcDirectory, projectBaseName, fileSystem);
            foreach (var env in template.Environments)
            {
                WebApiAppSettingsModifier.AddRmq(srcDirectory, env, projectBaseName, fileSystem);
                StartupModifier.RegisterMassTransitService(srcDirectory, env.EnvironmentName, projectBaseName);
            }

            SolutionBuilder.BuildMessagesProject(solutionDirectory, messagesDirectory);
            Utilities.AddProjectReference(webApiClassPath, @"..\..\..\Messages\Messages.csproj");
        }
    }
}