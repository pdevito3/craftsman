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
    using System.Linq;
    using Builders.Features;
    using Builders.Tests.IntegrationTests;
    using static Helpers.ConsoleWriter;
    using Spectre.Console;
    using Craftsman.Validators;
    using Enums;

    public static class RegisterProducerCommand
    {
        public static void Help()
        {
            WriteHelpHeader(@$"Description:");
            WriteHelpText(@$"   This command will register a producer with MassTransit using CLI prompts. This is especially useful for adding a new publish action to an existing feature (e.g. EntityCreated).{Environment.NewLine}");

            WriteHelpHeader(@$"Usage:");
            WriteHelpText(@$"   craftsman register:producer [options]");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Options:");
            WriteHelpText(@$"   -h, --help          Display this help message. No filepath is needed to display the help message.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Example:");
            WriteHelpText(@$"   craftsman register:producer");
            WriteHelpText(Environment.NewLine);
        }

        public static void Run(string boundedContextDirectory, IFileSystem fileSystem)
        {
            try
            {
                var srcDirectory = Path.Combine(boundedContextDirectory, "src");
                var testDirectory = Path.Combine(boundedContextDirectory, "tests");

                Utilities.IsBoundedContextDirectoryGuard(srcDirectory, testDirectory);
                var projectBaseName = Directory.GetParent(srcDirectory).Name;

                var producer = RunPrompt();
                ProducerRegistrationBuilder.CreateProducerRegistration(srcDirectory, producer, projectBaseName);
                MassTransitModifier.AddProducerRegistation(srcDirectory, producer.EndpointRegistrationMethodName, projectBaseName);
                
                WriteHelpHeader($"{Environment.NewLine}Your producer has been successfully registered. Keep up the good work!");
                
                var panel = new Panel(@$"[bold yellow4](await IsFaultyPublished<{producer.MessageName}>()).Should().BeFalse();
(await IsPublished<{producer.MessageName}>()).Should().BeTrue();[/]");
                panel.Border = BoxBorder.Rounded;
                panel.Padding = new Padding(1);
                AnsiConsole.MarkupLine(@$"{Environment.NewLine}[bold yellow4]Don't forget to add assertions for your producer tests! Adding something like this to your test should do the trick:{Environment.NewLine}[/]");
                AnsiConsole.Render(panel);
                
                StarGithubRequest();
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
        
        private static Producer RunPrompt()
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Render(new Rule("[yellow]Register a Producer[/]").RuleStyle("grey").Centered());

            var producer = new Producer();
            
            producer.MessageName = AskMessageName();
            producer.EndpointRegistrationMethodName = AskEndpointRegistrationMethodName();
            producer.DomainDirectory = AskDomainDirectory();
            producer.ExchangeType = AskExchangeType();

            return producer;
        }

        private static string AskMessageName()
        {
            return AnsiConsole.Ask<string>("What is the name of the message being produced (e.g. [green]IRecipeAdded[/])?");
        }

        private static string AskDomainDirectory()
        {
            return AnsiConsole
                .Ask<string>("What domain directory is the producer in? This is generally the plural of the entity publishing the message. (e.g. [green]Recipes[/])? Leave it null if the producer is directly in the Domain directory.");
        }

        private static string AskExchangeName()
        {
            return AnsiConsole.Ask<string>("What do you want to name the RMQ exchange (e.g. [green]recipe-added[/])?");
        }

        private static string AskEndpointRegistrationMethodName()
        {
            return AnsiConsole.Ask<string>("What do you want to name the service registration for this producer (e.g. [green]RecipeAddedEndpoint[/])?");
        }
        
        private static string AskExchangeType()
        {
            var exampleTypes = ExchangeTypeEnum.List.Select(e => e.Name);
            
            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What [green]type of exchange[/] do you want to use?")
                    .AddChoices(exampleTypes)
            );
        }
    }
}