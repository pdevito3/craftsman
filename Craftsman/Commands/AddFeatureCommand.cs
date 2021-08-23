namespace Craftsman.Commands
{
    using Craftsman.Builders;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using static Helpers.ConsoleWriter;
    using Spectre.Console;
    using Craftsman.Validators;
    using System.IO;
    using Craftsman.Builders.Features;

    public static class AddFeatureCommand
    {
        public static void Help()
        {
            WriteHelpHeader(@$"Description:");
            WriteHelpText(@$"   This command will scaffold out a skeleton feature command.{Environment.NewLine}");

            WriteHelpHeader(@$"Usage:");
            WriteHelpText(@$"   craftsman add:feature");
            WriteHelpText(@$"   OR");
            WriteHelpText(@$"   craftsman new:feature");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Options:");
            WriteHelpText(@$"   -h, --help          Display this help message.");

            WriteHelpText(Environment.NewLine);
        }

        public static void Run(string solutionDirectory)
        {
            try
            {
                var srcDirectory = Path.Combine(solutionDirectory, "src");
                var testDirectory = Path.Combine(solutionDirectory, "tests");

                Utilities.IsBoundedContextDirectoryGuard(srcDirectory, testDirectory);
                var projectBaseName = Directory.GetParent(srcDirectory)?.Name;
                var contextName = Utilities.GetDbContext(srcDirectory, projectBaseName);

                var feature = RunPrompt();
                EmptyFeatureBuilder.CreateCommand(srcDirectory, contextName, projectBaseName, feature);

                WriteHelpHeader($"{Environment.NewLine}Your feature has been successfully added. Keep up the good work! {Emoji.Known.Sparkles}");
                AnsiConsole.WriteLine();
            }
            catch (Exception e)
            {
                if (e is SolutionNotFoundException 
                    or DataValidationErrorException)
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

        private static Feature RunPrompt()
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Render(new Rule("[yellow]Add a New Feature[/]").RuleStyle("grey").Centered());

            var feature = AskFeature();
            var command = AskCommand(feature);
            var responseType = AskResponseType();
            var producer = AskIsProducer();
            var entityPluralForDir = AskEntityPluralForDir();

            AnsiConsole.WriteLine();
            AnsiConsole.Render(new Table().AddColumns("[grey]Property[/]", "[grey]Value[/]")
                .RoundedBorder()
                .BorderColor(Color.Grey)
                .AddRow("[grey]Feature Name[/]", feature)
                .AddRow("[grey]Command Name[/]", command)
                .AddRow("[grey]Response Type[/]", responseType)
                .AddRow("[grey]Is Producer[/]", producer.ToString())
                .AddRow("[grey]Entity Plural[/]", entityPluralForDir)
            );

            return new Feature()
            {
                Type = "AdHoc",
                Name = feature,
                Command = command,
                ResponseType = responseType,
                EntityPlural = entityPluralForDir
            };
        }

        private static string AskFeature()
        {
            var feature = AnsiConsole.Ask<string>("What's the name of your [bold]feature[/] (e.g. [green]AddCustomer[/])?");

            return feature.UppercaseFirstLetter();
        }

        private static string AskCommand(string feature)
        {
            var command = AnsiConsole.Prompt(
                new TextPrompt<string>($"What's the name of your [bold]command[/] (Default: [green]{feature}Command[/])?")
                .DefaultValue($"{feature}Command")
                .HideDefaultValue()
            );

            return command;
        }

        private static bool AskIsProducer()
        {
            var command = AnsiConsole.Prompt(
                new TextPrompt<string>("Does this command produce any message bus notifications (Default: [green]n[/])?")
                    .InvalidChoiceMessage("[red]Please respond 'y' or 'n'[/]")
                    .DefaultValue("n")
                    .HideDefaultValue()
                    .AddChoice("y")
                    .AddChoice("n"));

            return command == "y";
        }

        private static string AskResponseType()
        {
            var responseType = AnsiConsole.Prompt(
                new TextPrompt<string>($"What type of response would you like the command to return? This could be any C# property type (case-insensitive) or the string of a custom Class. (Default: [green]bool[/])?")
                .DefaultValue($"bool")
                .HideDefaultValue()
            );

            return responseType;
        }

        private static string AskEntityPluralForDir()
        {
            var command = AnsiConsole.Prompt(
                new TextPrompt<string>($"[grey][[Optional]][/] What is the *plural* name of the entity for this feature? You can also leave this response blank to put the feature will be added directly to the Domain directory. (Default: [green]none[/])?")
                .DefaultValue($"")
                .HideDefaultValue()
            );

            return command;
        }

        private static string AskSport(IAnsiConsole console)
        {
            console.WriteLine();
            console.Write(new Rule("[yellow]Choices[/]").RuleStyle("grey").LeftAligned());

            return console.Prompt(
                new TextPrompt<string>("What's your [green]favorite sport[/]?")
                    .InvalidChoiceMessage("[red]That's not a sport![/]")
                    .DefaultValue("Sport?")
                    .AddChoice("Soccer")
                    .AddChoice("Hockey")
                    .AddChoice("Basketball"));
        }

        private static int AskAge(IAnsiConsole console)
        {
            console.WriteLine();
            console.Write(new Rule("[yellow]Integers[/]").RuleStyle("grey").LeftAligned());

            return console.Prompt(
                new TextPrompt<int>("How [green]old[/] are you?")
                    .PromptStyle("green")
                    .ValidationErrorMessage("[red]That's not a valid age[/]")
                    .Validate(age =>
                    {
                        return age switch
                        {
                            <= 0 => ValidationResult.Error("[red]You must at least be 1 years old[/]"),
                            >= 123 => ValidationResult.Error("[red]You must be younger than the oldest person alive[/]"),
                            _ => ValidationResult.Success(),
                        };
                    }));
        }

        private static string AskPassword(IAnsiConsole console)
        {
            console.WriteLine();
            console.Write(new Rule("[yellow]Secrets[/]").RuleStyle("grey").LeftAligned());

            return console.Prompt(
                new TextPrompt<string>("Enter [green]password[/]?")
                    .PromptStyle("red")
                    .Secret());
        }

        private static string AskColor(IAnsiConsole console)
        {
            console.WriteLine();
            console.Write(new Rule("[yellow]Optional[/]").RuleStyle("grey").LeftAligned());

            return console.Prompt(
                new TextPrompt<string>("[grey][[Optional]][/] What is your [green]favorite color[/]?")
                    .AllowEmpty());
        }
    }
}