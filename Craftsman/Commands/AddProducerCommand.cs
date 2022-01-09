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
    using Builders.Features;
    using Builders.Tests.IntegrationTests;
    using static Helpers.ConsoleWriter;
    using Spectre.Console;
    using Craftsman.Validators;

    public static class AddProducerCommand
    {
        public static void Help()
        {
            WriteHelpHeader(@$"Description:");
            WriteHelpText(@$"   This command will add a MassTransit configuration for a message producer using a formatted yaml or json file.{Environment.NewLine}");

            WriteHelpHeader(@$"Usage:");
            WriteHelpText(@$"   craftsman add:producer [options] <filepath>");
            WriteHelpText(@$"   OR");
            WriteHelpText(@$"   craftsman add:producers [options] <filepath>");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Arguments:");
            WriteHelpText(@$"   filepath         The full filepath for the yaml or json file that lists the bus information that you want to add to your API.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Options:");
            WriteHelpText(@$"   -h, --help          Display this help message. No filepath is needed to display the help message.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Example:");
            WriteHelpText(@$"   craftsman add:producer C:\fullpath\producerinfo.yaml");
            WriteHelpText(@$"   craftsman add:producer C:\fullpath\producerinfo.yml");
            WriteHelpText(@$"   craftsman add:producer C:\fullpath\producerinfo.json");
            WriteHelpText(Environment.NewLine);
        }

        public static void Run(string filePath, string boundedContextDirectory, IFileSystem fileSystem)
        {
            try
            {
                FileParsingHelper.RunInitialTemplateParsingGuards(filePath);
                var template = FileParsingHelper.GetTemplateFromFile<ProducerTemplate>(filePath);

                var srcDirectory = Path.Combine(boundedContextDirectory, "src");
                var testDirectory = Path.Combine(boundedContextDirectory, "tests");

                Utilities.IsBoundedContextDirectoryGuard(srcDirectory, testDirectory);
                var projectBaseName = Directory.GetParent(srcDirectory).Name;
                template.SolutionName = projectBaseName;

                // get solution dir
                var solutionDirectory = Directory.GetParent(boundedContextDirectory).FullName;
                Utilities.IsSolutionDirectoryGuard(solutionDirectory);
                AddProducers(template.Producers, projectBaseName, srcDirectory, testDirectory, fileSystem);

                WriteHelpHeader($"{Environment.NewLine}Your producer has been successfully added. Keep up the good work!");
            }
            catch (Exception e)
            {
                if (e is IsNotBoundedContextDirectory
                    || e is DataValidationErrorException)
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

        public static void AddProducers(List<Producer> producers, string projectBaseName, string srcDirectory,
            string testDirectory, IFileSystem fileSystem)
        {
            var validator = new ProducerValidator();
            foreach (var producer in producers)
            {
                var results = validator.Validate(producer);
                if (!results.IsValid)
                    throw new DataValidationErrorException(results.Errors);
            }

            producers.ForEach(producer =>
            {
                ProducerBuilder.CreateProducerFeature(srcDirectory, producer, projectBaseName);
                ProducerRegistrationBuilder.CreateProducerRegistration(srcDirectory, producer, projectBaseName);
                MassTransitModifier.AddProducerRegistation(srcDirectory, producer.EndpointRegistrationMethodName, projectBaseName);
                
                ProducerTestBuilder.CreateTests(testDirectory, producer, projectBaseName, fileSystem);
            });
        }
    }
}