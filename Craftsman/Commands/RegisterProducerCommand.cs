namespace Craftsman.Commands;

using System.IO.Abstractions;
using Builders;
using Domain;
using Domain.Enums;
using Helpers;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;

public class RegisterProducerCommand : Command<RegisterProducerCommand.Settings>
{
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;
    private readonly IAnsiConsole _console;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

    public RegisterProducerCommand(IFileSystem fileSystem,
        IConsoleWriter consoleWriter,
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore,
        IAnsiConsole console)
    {
        _fileSystem = fileSystem;
        _consoleWriter = consoleWriter;
        _utilities = utilities;
        _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        _console = console;
    }

    public class Settings : CommandSettings
    {
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var potentialBoundaryDirectory = _utilities.GetRootDir();

        var solutionDirectory = _fileSystem.Directory.GetParent(potentialBoundaryDirectory)?.FullName;
        _utilities.IsSolutionDirectoryGuard(solutionDirectory);
        _scaffoldingDirectoryStore.SetSolutionDirectory(solutionDirectory);

        var projectName = new DirectoryInfo(potentialBoundaryDirectory).Name;
        _scaffoldingDirectoryStore.SetBoundedContextDirectoryAndProject(projectName);
        _utilities.IsBoundedContextDirectoryGuard();

        var producer = RunPrompt();
        new ProducerRegistrationBuilder(_utilities).CreateProducerRegistration(potentialBoundaryDirectory, _scaffoldingDirectoryStore.SrcDirectory, producer, _scaffoldingDirectoryStore.ProjectBaseName);
        new MassTransitModifier(_fileSystem).AddProducerRegistration(_scaffoldingDirectoryStore.SrcDirectory, producer.EndpointRegistrationMethodName, _scaffoldingDirectoryStore.ProjectBaseName);

        _consoleWriter.WriteHelpHeader($"{Environment.NewLine}Your producer has been successfully registered. Keep up the good work!");

        var panel = new Panel(@$"[bold yellow4](await IsFaultyPublished<{producer.MessageName}>()).Should().BeFalse();
(await IsPublished<{producer.MessageName}>()).Should().BeTrue();[/]");
        panel.Border = BoxBorder.Rounded;
        panel.Padding = new Padding(1);
        _console.MarkupLine(@$"{Environment.NewLine}[bold yellow4]Don't forget to add assertions for your producer tests! Adding something like this to your test should do the trick:{Environment.NewLine}[/]");
        _console.Write(panel);

        _consoleWriter.StarGithubRequest();
        return 0;
    }

    private Producer RunPrompt()
    {
        _console.WriteLine();
        _console.Write(new Rule("[yellow]Register a Producer[/]").RuleStyle("grey").Centered());

        var producer = new Producer();

        producer.MessageName = AskMessageName();
        producer.EndpointRegistrationMethodName = AskEndpointRegistrationMethodName();
        producer.DomainDirectory = AskDomainDirectory();
        producer.ExchangeType = AskExchangeType();

        return producer;
    }

    private string AskMessageName()
    {
        return _console.Ask<string>("What is the name of the message being produced (e.g. [green]IRecipeAdded[/])?");
    }

    private string AskDomainDirectory()
    {
        return _console
            .Ask<string>("What domain directory is the producer in? This is generally the plural of the entity publishing the message. (e.g. [green]Recipes[/])? Leave it null if the producer is directly in the Domain directory.");
    }

    private string AskExchangeName()
    {
        return _console.Ask<string>("What do you want to name the RMQ exchange (e.g. [green]recipe-added[/])?");
    }

    private string AskEndpointRegistrationMethodName()
    {
        return _console.Ask<string>("What do you want to name the service registration for this producer (e.g. [green]RecipeAddedEndpoint[/])?");
    }

    private string AskExchangeType()
    {
        var exampleTypes = ExchangeTypeEnum.List.Select(e => e.Name);

        return _console.Prompt(
            new SelectionPrompt<string>()
                .Title("What [green]type of exchange[/] do you want to use?")
                .AddChoices(exampleTypes)
        );
    }
}