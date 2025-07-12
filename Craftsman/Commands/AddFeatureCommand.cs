namespace Craftsman.Commands;

using System.IO.Abstractions;
using Domain;
using Domain.Enums;
using Helpers;
using MediatR;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;

public class AddFeatureCommand(
    IFileSystem fileSystem,
    IConsoleWriter consoleWriter,
    ICraftsmanUtilities utilities,
    IScaffoldingDirectoryStore scaffoldingDirectoryStore,
    IAnsiConsole console,
    IMediator mediator)
    : Command<AddFeatureCommand.Settings>
{
    public class Settings : CommandSettings
    {
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var potentialBoundaryDirectory = utilities.GetRootDir();

        var solutionDirectory = fileSystem.Directory.GetParent(potentialBoundaryDirectory)?.FullName;
        utilities.IsSolutionDirectoryGuard(solutionDirectory, true);
        scaffoldingDirectoryStore.SetSolutionDirectory(solutionDirectory);

        var projectName = new DirectoryInfo(potentialBoundaryDirectory).Name;
        scaffoldingDirectoryStore.SetBoundedContextDirectoryAndProject(projectName);
        utilities.IsBoundedContextDirectoryGuard();
        var contextName = utilities.GetDbContext(scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.ProjectBaseName);

        var feature = RunPrompt();

        var useSoftDelete = utilities.ProjectUsesSoftDelete(scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.ProjectBaseName);
        new EntityScaffoldingService(utilities, fileSystem, mediator).AddFeatureToProject(
            solutionDirectory,
            scaffoldingDirectoryStore.SrcDirectory,
            scaffoldingDirectoryStore.TestDirectory,
            scaffoldingDirectoryStore.ProjectBaseName,
            contextName,
            true,
            feature,
            new Entity() { Name = feature.EntityName, Plural = feature.EntityPlural },
            useSoftDelete);

        consoleWriter.WriteHelpHeader($"{Environment.NewLine}Your feature has been successfully added. Keep up the good work! {Emoji.Known.Sparkles}");
        return 0;
    }

    private Feature RunPrompt()
    {
        console.WriteLine();
        console.Write(new Rule("[yellow]Add a New Feature[/]").RuleStyle("grey").Centered());

        var featureType = AskFeatureType();

        if (featureType != FeatureType.AdHoc.Name && featureType != FeatureType.AddListByFk.Name && featureType != FeatureType.Job.Name)
        {
            var entityName = AskEntityName();
            var entityPlural = AskEntityPlural(entityName);
            var isProtected = AskIsProtected();

            console.WriteLine();
            console.Write(new Table().AddColumns("[grey]Property[/]", "[grey]Value[/]")
                .RoundedBorder()
                .BorderColor(Color.Grey)
                .AddRow("[grey]Entity Name[/]", entityName)
                .AddRow("[grey]Entity Plural[/]", entityPlural)
                .AddRow("[grey]Is Protected[/]", isProtected.ToString())
            );

            return new Feature()
            {
                Type = featureType,
                EntityName = entityName,
                EntityPlural = entityPlural,
                IsProtected = isProtected
            };
        }

        if (featureType == FeatureType.AddListByFk.Name)
        {
            var entityName = AskEntityName();
            var entityPlural = AskEntityPlural(entityName);
            var isProtected = AskIsProtected();
            var propName = AskBatchOnPropertyName();
            var propType = AskBatchOnPropertyType();
            var parentEntity = AskParentEntity();
            var parentEntityPlural = AskParentEntityPlural(parentEntity);

            console.WriteLine();
            console.Write(new Table().AddColumns("[grey]Property[/]", "[grey]Value[/]")
                .RoundedBorder()
                .BorderColor(Color.Grey)
                .AddRow("[grey]Entity Name[/]", entityName)
                .AddRow("[grey]Entity Plural[/]", entityPlural)
                .AddRow("[grey]Is Protected[/]", isProtected.ToString())
                .AddRow("[grey]Batch Prop Name[/]", propName)
                .AddRow("[grey]Batch Prop Type[/]", propType)
                .AddRow("[grey]Parent Entity[/]", parentEntity)
                .AddRow("[grey]Parent Entity Plural[/]", parentEntityPlural)
            );

            return new Feature()
            {
                Type = featureType,
                EntityName = entityName,
                EntityPlural = entityPlural,
                IsProtected = isProtected,
                BatchPropertyName = propName,
                BatchPropertyType = propType,
                ParentEntityPlural = parentEntityPlural,
                ParentEntity = parentEntity,
            };
        }

        if (featureType == FeatureType.Job.Name)
        {
            var jobName = AskFeature();
            var entityPluralForJobFeatureDir = AskEntityPluralForDir();

            console.WriteLine();
            console.Write(new Table().AddColumns("[grey]Property[/]", "[grey]Value[/]")
                .RoundedBorder()
                .BorderColor(Color.Grey)
                .AddRow("[grey]Feature Name[/]", jobName)
                .AddRow("[grey]Entity Plural[/]", entityPluralForJobFeatureDir)
            );

            return new Feature()
            {
                Type = "Job",
                Name = jobName,
                EntityPlural = entityPluralForJobFeatureDir
            };
        }

        var feature = AskFeature();
        var responseType = AskResponseType();
        var producer = AskIsProducer();
        var entityPluralForDir = AskEntityPluralForDir();

        console.WriteLine();
        console.Write(new Table().AddColumns("[grey]Property[/]", "[grey]Value[/]")
            .RoundedBorder()
            .BorderColor(Color.Grey)
            .AddRow("[grey]Feature Name[/]", feature)
            .AddRow("[grey]Response Type[/]", responseType)
            .AddRow("[grey]Is Producer[/]", producer.ToString())
            .AddRow("[grey]Entity Plural[/]", entityPluralForDir)
        );

        return new Feature()
        {
            Type = "AdHoc",
            Name = feature,
            ResponseType = responseType,
            EntityPlural = entityPluralForDir
        };
    }

    private string AskFeature()
    {
        var feature = console.Ask<string>("What's the name of your [bold]feature[/] (e.g. [green]AddCustomer[/])?");

        return feature.UppercaseFirstLetter();
    }

    private string AskCommand(string feature)
    {
        var command = console.Prompt(
            new TextPrompt<string>($"What's the name of your [bold]command[/] (Default: [green]{feature}Command[/])?")
            .DefaultValue($"{feature}Command")
            .HideDefaultValue()
        );

        return command;
    }

    private bool AskIsProducer()
    {
        var command = console.Prompt(
            new TextPrompt<string>("Does this command produce any message bus notifications (Default: [green]n[/])?")
                .InvalidChoiceMessage("[red]Please respond 'y' or 'n'[/]")
                .DefaultValue("n")
                .HideDefaultValue()
                .AddChoice("y")
                .AddChoice("n"));

        return command == "y";
    }

    private string AskResponseType()
    {
        var responseType = console.Prompt(
            new TextPrompt<string>($"What type of response would you like the command to return? This could be any C# property type (case-insensitive) or the string of a custom Class. (Default: [green]bool[/])?")
            .DefaultValue($"bool")
            .HideDefaultValue()
        );

        return responseType;
    }

    private string AskEntityPluralForDir()
    {
        var command = console.Prompt(
            new TextPrompt<string>($"What is the *plural* name of the entity for this feature?")
        );

        return command;
    }

    private string AskFeatureType()
    {
        var featureTypes = FeatureType.List.Select(e => e.Name);

        return console.Prompt(
            new SelectionPrompt<string>()
                .Title("What [green]type of feature[/] do you want to add?")
                .PageSize(50)
                .AddChoices(featureTypes)
            );
    }

    private string AskEntityName()
    {
        return console.Prompt(
            new TextPrompt<string>("What's the [green]name of the entity[/] that will use this feature?")
        );
    }

    private string AskEntityPlural(string entityName)
    {
        return console.Prompt(
            new TextPrompt<string>($"What's the [green]plural name[/] of the entity that will use this feature (Default: [green]{entityName}s[/])?")
                .DefaultValue($"{entityName}s")
                .HideDefaultValue()
        );
    }

    private string AskBatchOnPropertyName()
    {
        return console.Prompt(
            new TextPrompt<string>("What's the [green]name of the property[/] that you will batch add for in this feature (e.g. `EventId` would add a list of records that all have the same event id)?")
        );
    }

    private string AskBatchOnPropertyType()
    {
        return console.Prompt(
            new TextPrompt<string>(
                    $"What's the [green]data type[/] of the the property you are doing the batch add on (case insensitive)? (Default: [green]Guid[/])")
                .DefaultValue($"Guid")
                .HideDefaultValue()
        );
    }

    private string AskParentEntityPlural(string entityName)
    {
        return console.Prompt(
            new TextPrompt<string>($"What's the [green]plural name[/] of the the parent entity you are using for the batch add? Leave [green]null[/] if you're not batching on a FK. (ex: [green]{entityName}s[/])")
                .AllowEmpty()
                .HideDefaultValue()
        );
    }

    private string AskParentEntity()
    {
        return console.Prompt(
            new TextPrompt<string>("What's the [green]name of the parent entity[/] that the FK you using is associated to? For example, if you had a FK of `EventId`, the parent entity might be `Event`. Leave [green]null[/] if you're not batching on a FK.")
                .AllowEmpty()
        );
    }

    private bool AskIsProtected()
    {
        var command = console.Prompt(
            new TextPrompt<string>("Is this a protected feature? (Default: [green]n[/])?")
                .InvalidChoiceMessage("[red]Please respond 'y' or 'n'[/]")
                .DefaultValue("n")
                .HideDefaultValue()
                .AddChoice("y")
                .AddChoice("n"));

        return command == "y";
    }

    private string AskPermissionName(string featureName)
    {
        return console.Prompt(
            new TextPrompt<string>(
                    $"What's the name of the permission for this feature? (Default: [green]Can{featureName}[/])?")
                .DefaultValue($"Can{featureName}")
                .HideDefaultValue());
    }
}