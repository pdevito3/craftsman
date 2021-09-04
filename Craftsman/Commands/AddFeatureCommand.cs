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
    using System.Linq;
    using Craftsman.Builders.Features;
    using Enums;

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

        public static void Run(string solutionDirectory, IFileSystem fileSystem)
        {
            try
            {
                var srcDirectory = Path.Combine(solutionDirectory, "src");
                var testDirectory = Path.Combine(solutionDirectory, "tests");

                Utilities.IsBoundedContextDirectoryGuard(srcDirectory, testDirectory);
                var projectBaseName = Directory.GetParent(srcDirectory)?.Name;
                var contextName = Utilities.GetDbContext(srcDirectory, projectBaseName);

                var feature = RunPrompt();
                // EmptyFeatureBuilder.CreateCommand(srcDirectory, contextName, projectBaseName, feature);
                EntityScaffolding.AddFeatureToProject(
                    srcDirectory,
                    testDirectory,
                    projectBaseName,
                    contextName,
                    true,
                    feature.Policies,
                    feature,
                    new Entity() { Name = feature.EntityName, Plural = feature.EntityPlural},
                    fileSystem);
                
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

            var featureType = AskFeatureType();
            
            if (featureType != FeatureType.AdHoc.Name && featureType != FeatureType.AddListByFk.Name)
            {
                var entityName = AskEntityName();
                var entityPlural = AskEntityPlural(entityName);
                var useSwagger = AskUseSwaggerComments();
                var policies = AskPolicies();

                AnsiConsole.WriteLine();
                AnsiConsole.Render(new Table().AddColumns("[grey]Property[/]", "[grey]Value[/]")
                    .RoundedBorder()
                    .BorderColor(Color.Grey)
                    .AddRow("[grey]Entity Name[/]", entityName)
                    .AddRow("[grey]Entity Plural[/]", entityPlural)
                    .AddRow("[grey]Use Swagger[/]", useSwagger.ToString())
                    .AddRow("[grey]Policies[/]", policies.Count > 0 ? $"{policies.Count} policies" : "-")
                );
                
                return new Feature()
                {
                    Type = featureType,
                    EntityName = entityName,
                    EntityPlural = entityPlural,
                    Policies = policies
                };
            }
            
            if (featureType == FeatureType.AddListByFk.Name)
            {
                var entityName = AskEntityName();
                var entityPlural = AskEntityPlural(entityName);
                var useSwagger = AskUseSwaggerComments();
                var policies = AskPolicies();
                var propName = AskBatchOnPropertyName();
                var propType = AskBatchOnPropertyType();
                var parentEntity = AskParentEntity();
                var dbSet = AskBatchPropertyDbSetName(parentEntity);

                AnsiConsole.WriteLine();
                AnsiConsole.Render(new Table().AddColumns("[grey]Property[/]", "[grey]Value[/]")
                    .RoundedBorder()
                    .BorderColor(Color.Grey)
                    .AddRow("[grey]Entity Name[/]", entityName)
                    .AddRow("[grey]Entity Plural[/]", entityPlural)
                    .AddRow("[grey]Use Swagger[/]", useSwagger.ToString())
                    .AddRow("[grey]Policies[/]", policies.Count > 0 ? $"{policies.Count} policies" : "-")
                    .AddRow("[grey]Batch Prop Name[/]", propName)
                    .AddRow("[grey]Batch Prop Type[/]", propType)
                    .AddRow("[grey]Parent Entity[/]", parentEntity)
                    .AddRow("[grey]Batch DbSet[/]", dbSet)
                );
                
                return new Feature()
                {
                    Type = featureType,
                    EntityName = entityName,
                    EntityPlural = entityPlural,
                    Policies = policies,
                    BatchPropertyName = propName,
                    BatchPropertyType = propType,
                    BatchPropertyDbSetName = dbSet,
                    ParentEntity = parentEntity
                };
            }
            
            
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

        private static string AskFeatureType()
        {
            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What [green]type of feature[/] do you want to add?")
                    .PageSize(50)
                    .AddChoices(FeatureType.AdHoc.Name,
                        FeatureType.AddListByFk.Name,
                        FeatureType.AddRecord.Name,
                        FeatureType.DeleteRecord.Name,
                        FeatureType.GetList.Name,
                        FeatureType.GetRecord.Name,
                        FeatureType.PatchRecord.Name,
                        FeatureType.UpdateRecord.Name
                    )
                );
        }

        private static string AskEntityName()
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>("What's the [green]name of the entity[/] that will use this feature?")
            );
        }

        private static string AskEntityPlural(string entityName)
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>($"What's the [green]plural name[/] of the entity that will use this feature (Default: [green]{entityName}s[/])?")
                    .DefaultValue($"{entityName}s")
                    .HideDefaultValue()
            );
        }

        private static string AskBatchOnPropertyName()
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>("What's the [green]name of the property[/] that you will batch add for in this feature (e.g. `EventId` would add a list of records that all have the same event id)?")
            );
        }

        private static string AskBatchOnPropertyType()
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>(
                        $"What's the [green]data type[/] of the the property you are doing the batch add on (case insensitive)? (Default: [green]Guid[/])")
                    .DefaultValue($"Guid")
                    .HideDefaultValue()
            );
        }

        private static string AskBatchPropertyDbSetName(string entityName)
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>($"What's the [green]name of the DbSet[/] of the the property you are doing the batch add on? Leave [green]null[/] if you're not batching on a FK. (Default: [green]{entityName}s[/])")
                    .DefaultValue($"{entityName}s")
                    .HideDefaultValue()
            );
        }

        private static string AskParentEntity()
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>("What's the [green]name of the parent entity[/] that the FK you using is associated to? For example, if you had a FK of `EventId`, the parent entity might be `Event`. Leave null if you're not batching on a FK.")
            );
        }

        private static bool AskUseSwaggerComments()
        {
            var command = AnsiConsole.Prompt(
                new TextPrompt<string>("Do you want to include swagger comments? (Default: [green]y[/])?")
                    .InvalidChoiceMessage("[red]Please respond 'y' or 'n'[/]")
                    .DefaultValue("y")
                    .HideDefaultValue()
                    .AddChoice("y")
                    .AddChoice("n"));

            return command == "y";
        }

        private static List<Policy> AskPolicies()
        {
            var command = AnsiConsole.Prompt(
                new TextPrompt<string>("Do you want to add a policy to this feature? (Default: [green]n[/])?")
                    .InvalidChoiceMessage("[red]Please respond 'y' or 'n'[/]")
                    .DefaultValue("n")
                    .HideDefaultValue()
                    .AddChoice("y")
                    .AddChoice("n"));

            var wantsToAddPolicies = command == "y";

            if (wantsToAddPolicies)
            {
                var policies = new List<Policy>();
                var firstPolicy = AskPolicy();
                policies.Add(firstPolicy);

                var additional = "y";
                while (additional == "y")
                {
                    additional = AnsiConsole.Prompt(
                        new TextPrompt<string>("Do you want to add any more policies? (Default: [green]n[/])?")
                            .InvalidChoiceMessage("[red]Please respond 'y' or 'n'[/]")
                            .DefaultValue("n")
                            .HideDefaultValue()
                            .AddChoice("y")
                            .AddChoice("n"));

                    var wantsAdditional = additional == "y";
                    if (!wantsAdditional) break;
                    
                    var newPolicy = AskPolicy();
                    policies.Add(newPolicy);
                }

                return policies;
            }

            return new List<Policy>();
        }
        
        
        private static Policy AskPolicy()
        {
            // ask policy type, then
            var name = AskPolicyName();
            return new Policy()
            {
                Name = name
            };
        }

        private static string AskPolicyName()
        {
            return AnsiConsole.Ask<string>("What's the name of the policy you want to apply?");
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
