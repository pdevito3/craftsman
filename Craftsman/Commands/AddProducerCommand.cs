namespace Craftsman.Commands;

using System.IO.Abstractions;
using Builders;
using Builders.Features;
using Builders.Tests.IntegrationTests;
using Domain;
using Exceptions;
using Helpers;
using MediatR;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;
using Validators;

public class AddProducerCommand(
    IAnsiConsole console,
    IFileSystem fileSystem,
    IConsoleWriter consoleWriter,
    ICraftsmanUtilities utilities,
    IScaffoldingDirectoryStore scaffoldingDirectoryStore,
    IFileParsingHelper fileParsingHelper,
    IMediator mediator)
    : Command<AddProducerCommand.Settings>
{
    private readonly IAnsiConsole _console = console;

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<Filepath>")]
        public string Filepath { get; set; }
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

        // TODO make injectable
        fileParsingHelper.RunInitialTemplateParsingGuards(settings.Filepath);
        var template = fileParsingHelper.GetTemplateFromFile<ProducerTemplate>(settings.Filepath);
        consoleWriter.WriteLogMessage($"Your template file was parsed successfully");

        AddProducers(template.Producers, scaffoldingDirectoryStore.ProjectBaseName, solutionDirectory, scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.TestDirectory);

        consoleWriter.WriteHelpHeader($"{Environment.NewLine}Your consumer has been successfully added. Keep up the good work!");
        return 0;
    }

    public void AddProducers(List<Producer> producers, string projectBaseName, string solutionDirectory, string srcDirectory, string testDirectory)
    {
        var validator = new ProducerValidator();
        foreach (var producer in producers)
        {
            var results = validator.Validate(producer);
            if (!results.IsValid)
                throw new DataValidationErrorException(results.Errors);
        }

        foreach (var producer in producers)
        {
            mediator.Send(new ProducerBuilder.Command(producer)).GetAwaiter().GetResult();
            mediator.Send(new ProducerRegistrationBuilder.ProducerRegistrationBuilderCommand(solutionDirectory, srcDirectory, producer, projectBaseName)).GetAwaiter().GetResult();
            new MassTransitModifier(fileSystem).AddProducerRegistration(srcDirectory, producer.EndpointRegistrationMethodName, projectBaseName);
        }
    }
}