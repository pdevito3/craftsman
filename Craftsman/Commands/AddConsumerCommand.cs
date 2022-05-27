namespace Craftsman.Commands;

using System.IO.Abstractions;
using Builders;
using Builders.Features;
using Builders.Tests.IntegrationTests;
using Builders.Tests.Utilities;
using Domain;
using Exceptions;
using Helpers;
using Services;
using Spectre.Console.Cli;
using Validators;

public class AddConsumerCommand : Command<AddConsumerCommand.Settings>
{
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;
    private readonly IFileParsingHelper _fileParsingHelper;

    public AddConsumerCommand(IFileSystem fileSystem,
        IConsoleWriter consoleWriter,
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore, IFileParsingHelper fileParsingHelper)
    {
        _fileSystem = fileSystem;
        _consoleWriter = consoleWriter;
        _utilities = utilities;
        _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        _fileParsingHelper = fileParsingHelper;
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

        _fileParsingHelper.RunInitialTemplateParsingGuards(potentialBoundaryDirectory);
        var template = _fileParsingHelper.GetTemplateFromFile<ConsumerTemplate>(settings.Filepath);
        _consoleWriter.WriteLogMessage($"Your template file was parsed successfully");

        AddConsumers(template.Consumers, _scaffoldingDirectoryStore.ProjectBaseName, solutionDirectory, _scaffoldingDirectoryStore.SrcDirectory, _scaffoldingDirectoryStore.TestDirectory);

        _consoleWriter.WriteHelpHeader($"{Environment.NewLine}Your consumer has been successfully added. Keep up the good work!");
        return 0;
    }

    public void AddConsumers(List<Consumer> consumers, string projectBaseName, string solutionDirectory, string srcDirectory, string testDirectory)
    {
        var validator = new ConsumerValidator();
        foreach (var consumer in consumers)
        {
            var results = validator.Validate(consumer);
            if (!results.IsValid)
                throw new DataValidationErrorException(results.Errors);
        }

        consumers.ForEach(consumer =>
        {
            new ConsumerBuilder(_utilities).CreateConsumerFeature(solutionDirectory, srcDirectory, consumer, projectBaseName);
            new ConsumerRegistrationBuilder(_utilities).CreateConsumerRegistration(srcDirectory, consumer, projectBaseName);
            new MassTransitModifier(_fileSystem).AddConsumerRegistation(srcDirectory, consumer.EndpointRegistrationMethodName, projectBaseName);

            new IntegrationTestFixtureModifier(_fileSystem).AddMTConsumer(testDirectory, consumer.ConsumerName, consumer.DomainDirectory, projectBaseName, srcDirectory);
            new ConsumerTestBuilder(_utilities).CreateTests(solutionDirectory, testDirectory, srcDirectory, consumer, projectBaseName);
        });
    }
}