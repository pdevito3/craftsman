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

    public static class RegisterConsumerCommand
    {
        public static void Help()
        {
            WriteHelpHeader(@$"Description:");
            WriteHelpText(@$"   This command will add a receive endpoint and regsiter it with MassTransit using a formatted yaml or json file.{Environment.NewLine}");

            WriteHelpHeader(@$"Usage:");
            WriteHelpText(@$"   craftsman register:consumer [options] <filepath>");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Arguments:");
            WriteHelpText(@$"   filepath         The full filepath for the yaml or json file that lists the bus information that you want to add to your API.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Options:");
            WriteHelpText(@$"   -h, --help          Display this help message. No filepath is needed to display the help message.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Example:");
            WriteHelpText(@$"   craftsman register:consumer C:\fullpath\consumerinfo.yaml");
            WriteHelpText(@$"   craftsman register:consumer C:\fullpath\consumerinfo.yml");
            WriteHelpText(@$"   craftsman register:consumer C:\fullpath\consumerinfo.json");
            WriteHelpText(Environment.NewLine);
        }

        public static void Run(string filePath, string boundedContextDirectory, IFileSystem fileSystem)
        {
            try
            {
                FileParsingHelper.RunInitialTemplateParsingGuards(filePath);
                var template = FileParsingHelper.GetTemplateFromFile<ConsumerTemplate>(filePath);

                var srcDirectory = Path.Combine(boundedContextDirectory, "src");
                var testDirectory = Path.Combine(boundedContextDirectory, "tests");

                Utilities.IsBoundedContextDirectoryGuard(srcDirectory, testDirectory);
                var projectBaseName = Directory.GetParent(srcDirectory).Name;
                template.SolutionName = projectBaseName;

                // get solution dir
                var solutionDirectory = Directory.GetParent(boundedContextDirectory).FullName;
                Utilities.IsSolutionDirectoryGuard(solutionDirectory);
                AddConsumers(template.Consumers, projectBaseName, srcDirectory);

                WriteHelpHeader($"{Environment.NewLine}Your event bus has been successfully added. Keep up the good work!");
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

        public static void AddConsumers(List<Consumer> consumers, string projectBaseName, string srcDirectory)
        {
            consumers.ForEach(consumer =>
            {
                // create consumer registration
                ConsumerRegistrationBuilder.CreateConsumerRegistration(srcDirectory, consumer, projectBaseName);

                // add to MR registration
                MassTransitModifier.AddConsumerRegistation(srcDirectory, consumer.EndpointRegistrationMethodName, projectBaseName);
            });
        }
    }
}