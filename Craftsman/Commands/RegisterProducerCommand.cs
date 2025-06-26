namespace Craftsman.Commands;

using System.IO.Abstractions;
using Builders;
using Domain;
using Domain.Enums;
using Helpers;
using MediatR;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;

public class RegisterProducerCommand(
    IFileSystem fileSystem,
    IConsoleWriter consoleWriter,
    ICraftsmanUtilities utilities,
    IScaffoldingDirectoryStore scaffoldingDirectoryStore,
    IAnsiConsole console,
    IMediator mediator)
    : Command<RegisterProducerCommand.Settings>
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

        var producer = RunPrompt();
        mediator.Send(new ProducerRegistrationBuilder.ProducerRegistrationBuilderCommand(potentialBoundaryDirectory, scaffoldingDirectoryStore.SrcDirectory, producer, scaffoldingDirectoryStore.ProjectBaseName)).GetAwaiter().GetResult();
        new MassTransitModifier(fileSystem).AddProducerRegistration(scaffoldingDirectoryStore.SrcDirectory, producer.EndpointRegistrationMethodName, scaffoldingDirectoryStore.ProjectBaseName);

        consoleWriter.WriteHelpHeader($"{Environment.NewLine}Your producer has been successfully registered. Keep up the good work!");

        var panel = new Panel(@$"[bold yellow4](await IsPublished<{producer.MessageName}>()).Should().BeTrue();[/]");
        panel.Border = BoxBorder.Rounded;
        panel.Padding = new Padding(1);
        console.MarkupLine(@$"{Environment.NewLine}[bold yellow4]Don't forget to add assertions for your producer tests! Adding something like this to your test should do the trick:{Environment.NewLine}[/]");
        console.Write(panel);

        consoleWriter.StarGithubRequest();
        return 0;
    }

    private Producer RunPrompt()
    {
        console.WriteLine();
        console.Write(new Rule("[yellow]Register a Producer[/]").RuleStyle("grey").Centered());

        var producer = new Producer();

        producer.MessageName = AskMessageName();
        producer.EndpointRegistrationMethodName = AskEndpointRegistrationMethodName();
        producer.DomainDirectory = AskDomainDirectory();
        producer.ExchangeType = AskExchangeType();

        return producer;
    }

    private string AskMessageName()
    {
        return console.Ask<string>("What is the name of the message being produced (e.g. [green]IRecipeAdded[/])?");
    }

    private string AskDomainDirectory()
    {
        return console
            .Ask<string>("What domain directory is the producer in? This is generally the plural of the entity publishing the message. (e.g. [green]Recipes[/])? Leave it null if the producer is directly in the Domain directory.");
    }

    private string AskExchangeName()
    {
        return console.Ask<string>("What do you want to name the RMQ exchange (e.g. [green]recipe-added[/])?");
    }

    private string AskEndpointRegistrationMethodName()
    {
        return console.Ask<string>("What do you want to name the service registration for this producer (e.g. [green]RecipeAddedEndpoint[/])?");
    }

    private string AskExchangeType()
    {
        var exampleTypes = ExchangeTypeEnum.List.Select(e => e.Name);

        return console.Prompt(
            new SelectionPrompt<string>()
                .Title("What [green]type of exchange[/] do you want to use?")
                .AddChoices(exampleTypes)
        );
    }
}