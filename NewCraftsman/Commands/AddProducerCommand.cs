namespace NewCraftsman.Commands;

using System.IO.Abstractions;
using Builders;
using Builders.Docker;
using Builders.Features;
using Builders.Tests.IntegrationTests;
using Builders.Tests.Utilities;
using Domain;
using Exceptions;
using Helpers;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;
using Validators;

public class AddProducerCommand : Command<AddProducerCommand.Settings>
{
    private IAnsiConsole _console;
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

    public AddProducerCommand(IAnsiConsole console,
        IFileSystem fileSystem,
        IConsoleWriter consoleWriter,
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
    {
        _console = console;
        _fileSystem = fileSystem;
        _consoleWriter = consoleWriter;
        _utilities = utilities;
        _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
    }

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<Filepath>")]
        public string Filepath { get; set; }
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

        // TODO make injectable
        new FileParsingHelper(_fileSystem).RunInitialTemplateParsingGuards(potentialBoundaryDirectory);
        var template = FileParsingHelper.ReadYamlString<ProducerTemplate>(settings.Filepath);
        _consoleWriter.WriteLogMessage($"Your template file was parsed successfully");
        
        AddProducers(template.Producers, _scaffoldingDirectoryStore.ProjectBaseName, solutionDirectory, _scaffoldingDirectoryStore.SrcDirectory, _scaffoldingDirectoryStore.TestDirectory);
        
        _consoleWriter.WriteHelpHeader($"{Environment.NewLine}Your consumer has been successfully added. Keep up the good work!");
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

        producers.ForEach(producer =>
        {
            new ProducerBuilder(_utilities).CreateProducerFeature(solutionDirectory, srcDirectory, producer, projectBaseName);
            new ProducerRegistrationBuilder(_utilities).CreateProducerRegistration(solutionDirectory, srcDirectory, producer, projectBaseName);
            new MassTransitModifier(_fileSystem).AddProducerRegistration(srcDirectory, producer.EndpointRegistrationMethodName, projectBaseName);
                
            new ProducerTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, producer, projectBaseName);
        });
    }
}