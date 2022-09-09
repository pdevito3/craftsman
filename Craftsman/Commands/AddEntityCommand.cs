namespace Craftsman.Commands;

using System.IO.Abstractions;
using Builders;
using Domain;
using Helpers;
using MediatR;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;

public class AddEntityCommand : Command<AddEntityCommand.Settings>
{
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;
    private readonly IFileParsingHelper _fileParsingHelper;
    private readonly IMediator _mediator;

    public AddEntityCommand(IFileSystem fileSystem,
        IConsoleWriter consoleWriter,
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore,
        IFileParsingHelper fileParsingHelper, IMediator mediator)
    {
        _fileSystem = fileSystem;
        _consoleWriter = consoleWriter;
        _utilities = utilities;
        _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        _fileParsingHelper = fileParsingHelper;
        _mediator = mediator;
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
        _utilities.IsSolutionDirectoryGuard(solutionDirectory, true);
        _scaffoldingDirectoryStore.SetSolutionDirectory(solutionDirectory);

        var projectName = new DirectoryInfo(potentialBoundaryDirectory).Name;
        _scaffoldingDirectoryStore.SetBoundedContextDirectoryAndProject(projectName);
        _utilities.IsBoundedContextDirectoryGuard();

        // TODO make injectable
        _fileParsingHelper.RunInitialTemplateParsingGuards(settings.Filepath);
        var template = _fileParsingHelper.GetTemplateFromFile<AddEntityTemplate>(settings.Filepath);
        _consoleWriter.WriteLogMessage($"Your template file was parsed successfully");

        FileParsingHelper.RunPrimaryKeyGuard(template.Entities);

        RunEntityBuilders(solutionDirectory, _scaffoldingDirectoryStore.SrcDirectory, _scaffoldingDirectoryStore.TestDirectory, template);

        _consoleWriter.WriteHelpHeader($"{Environment.NewLine}Your entities have been successfully added. Keep up the good work!");
        return 0;
    }

    private void RunEntityBuilders(string solutionDirectory, string srcDirectory, string testDirectory, AddEntityTemplate template)
    {
        template = GetDbContext(_scaffoldingDirectoryStore.SrcDirectory, template, _scaffoldingDirectoryStore.ProjectBaseName);
        template.SolutionName = _scaffoldingDirectoryStore.ProjectBaseName;
        var useSoftDelete = _utilities.ProjectUsesSoftDelete(srcDirectory, _scaffoldingDirectoryStore.ProjectBaseName);

        //entities
        new EntityScaffoldingService(_utilities, _fileSystem, _mediator).ScaffoldEntities(solutionDirectory,
            srcDirectory,
            testDirectory,
            _scaffoldingDirectoryStore.ProjectBaseName,
            template.Entities,
            template.DbContextName,
            template.AddSwaggerComments,
            useSoftDelete);
    }

    private AddEntityTemplate GetDbContext(string srcDirectory, AddEntityTemplate template, string projectBaseName)
    {
        template.DbContextName = _utilities.GetDbContext(srcDirectory, projectBaseName);
        return template;
    }
}