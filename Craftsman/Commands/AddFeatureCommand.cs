namespace Craftsman.Commands;

using System.IO.Abstractions;
using Domain;
using Domain.Enums;
using Helpers;
using MediatR;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;

public class AddFeatureCommand : Command<AddFeatureCommand.Settings>
{
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;
    private readonly IAnsiConsole _console;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;
    private readonly IMediator _mediator;

    public AddFeatureCommand(IFileSystem fileSystem,
        IConsoleWriter consoleWriter,
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore,
        IAnsiConsole console, IMediator mediator)
    {
        _fileSystem = fileSystem;
        _consoleWriter = consoleWriter;
        _utilities = utilities;
        _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        _console = console;
        _mediator = mediator;
    }

    public class Settings : CommandSettings
    {
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var potentialBoundaryDirectory = _utilities.GetRootDir();

        var solutionDirectory = _fileSystem.Directory.GetParent(potentialBoundaryDirectory)?.FullName;
        _utilities.IsSolutionDirectoryGuard(solutionDirectory, true);
        _scaffoldingDirectoryStore.SetSolutionDirectory(solutionDirectory);

        var projectName = new DirectoryInfo(potentialBoundaryDirectory).Name;
        _scaffoldingDirectoryStore.SetBoundedContextDirectoryAndProject(projectName);
        _utilities.IsBoundedContextDirectoryGuard();
        var contextName = _utilities.GetDbContext(_scaffoldingDirectoryStore.SrcDirectory, _scaffoldingDirectoryStore.ProjectBaseName);

        var feature = RunPrompt();

        var useSoftDelete = _utilities.ProjectUsesSoftDelete(_scaffoldingDirectoryStore.SrcDirectory, _scaffoldingDirectoryStore.ProjectBaseName);
        new EntityScaffoldingService(_utilities, _fileSystem, _mediator, _consoleWriter).AddFeatureToProject(
            solutionDirectory,
            _scaffoldingDirectoryStore.SrcDirectory,
            _scaffoldingDirectoryStore.TestDirectory,
            _scaffoldingDirectoryStore.ProjectBaseName,
            contextName,
            true,
            feature,
            new Entity() { Name = feature.EntityName, Plural = feature.EntityPlural },
            useSoftDelete);

        _consoleWriter.WriteHelpHeader($"{Environment.NewLine}Your feature has been successfully added. Keep up the good work! {Emoji.Known.Sparkles}");
        return 0;
    }

    private Feature RunPrompt()
    {
        _console.WriteLine();
        _console.Write(new Rule("[yellow]Add a New Feature[/]").RuleStyle("grey").Centered());

        var featureType = AskFeatureType();

        if (featureType != FeatureType.AdHoc.Name && featureType != FeatureType.AddListByFk.Name)
        {
            var entityName = AskEntityName();
            var entityPlural = AskEntityPlural(entityName);
            var isProtected = AskIsProtected();

            _console.WriteLine();
            _console.Write(new Table().AddColumns("[grey]Property[/]", "[grey]Value[/]")
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

            _console.WriteLine();
            _console.Write(new Table().AddColumns("[grey]Property[/]", "[grey]Value[/]")
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


        var feature = AskFeature();
        var responseType = AskResponseType();
        var producer = AskIsProducer();
        var entityPluralForDir = AskEntityPluralForDir();

        _console.WriteLine();
        _console.Write(new Table().AddColumns("[grey]Property[/]", "[grey]Value[/]")
            .RoundedBorder()
            .BorderColor(Color.Grey)
            .AddRow("[grey]Feature Name[/]", feature)
            .AddRow("[grey]Response Type[/]", responseType)
            .AddRow("[grey]Is Producer[/]", producer.ToString())
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
        var feature = _console.Ask<string>("What's the name of your [bold]feature[/] (e.g. [green]AddCustomer[/])?");

        return feature.UppercaseFirstLetter();
    }

    private string AskCommand(string feature)
    {
        var command = _console.Prompt(
            new TextPrompt<string>($"What's the name of your [bold]command[/] (Default: [green]{feature}Command[/])?")
            .DefaultValue($"{feature}Command")
            .HideDefaultValue()
        );

        return command;
    }

    private bool AskIsProducer()
    {
        var command = _console.Prompt(
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
        var responseType = _console.Prompt(
            new TextPrompt<string>($"What type of response would you like the command to return? This could be any C# property type (case-insensitive) or the string of a custom Class. (Default: [green]bool[/])?")
            .DefaultValue($"bool")
            .HideDefaultValue()
        );

        return responseType;
    }

    private string AskEntityPluralForDir()
    {
        var command = _console.Prompt(
            new TextPrompt<string>($"What is the *plural* name of the entity for this feature?")
        );

        return command;
    }

    private string AskFeatureType()
    {
        var featureTypes = FeatureType.List.Select(e => e.Name);

        return _console.Prompt(
            new SelectionPrompt<string>()
                .Title("What [green]type of feature[/] do you want to add?")
                .PageSize(50)
                .AddChoices(featureTypes)
            );
    }

    private string AskEntityName()
    {
        return _console.Prompt(
            new TextPrompt<string>("What's the [green]name of the entity[/] that will use this feature?")
        );
    }

    private string AskEntityPlural(string entityName)
    {
        return _console.Prompt(
            new TextPrompt<string>($"What's the [green]plural name[/] of the entity that will use this feature (Default: [green]{entityName}s[/])?")
                .DefaultValue($"{entityName}s")
                .HideDefaultValue()
        );
    }

    private string AskBatchOnPropertyName()
    {
        return _console.Prompt(
            new TextPrompt<string>("What's the [green]name of the property[/] that you will batch add for in this feature (e.g. `EventId` would add a list of records that all have the same event id)?")
        );
    }

    private string AskBatchOnPropertyType()
    {
        return _console.Prompt(
            new TextPrompt<string>(
                    $"What's the [green]data type[/] of the the property you are doing the batch add on (case insensitive)? (Default: [green]Guid[/])")
                .DefaultValue($"Guid")
                .HideDefaultValue()
        );
    }

    private string AskParentEntityPlural(string entityName)
    {
        return _console.Prompt(
            new TextPrompt<string>($"What's the [green]plural name[/] of the the parent entity you are using for the batch add? Leave [green]null[/] if you're not batching on a FK. (ex: [green]{entityName}s[/])")
                .AllowEmpty()
                .HideDefaultValue()
        );
    }

    private string AskParentEntity()
    {
        return _console.Prompt(
            new TextPrompt<string>("What's the [green]name of the parent entity[/] that the FK you using is associated to? For example, if you had a FK of `EventId`, the parent entity might be `Event`. Leave [green]null[/] if you're not batching on a FK.")
                .AllowEmpty()
        );
    }

    private bool AskIsProtected()
    {
        var command = _console.Prompt(
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
        return _console.Prompt(
            new TextPrompt<string>(
                    $"What's the name of the permission for this feature? (Default: [green]Can{featureName}[/])?")
                .DefaultValue($"Can{featureName}")
                .HideDefaultValue());
    }
}