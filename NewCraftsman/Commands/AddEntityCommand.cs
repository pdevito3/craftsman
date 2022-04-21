namespace NewCraftsman.Commands;

using System.IO.Abstractions;
using Builders;
using Builders.Docker;
using Domain;
using Helpers;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;

public class AddEntityCommand : Command<AddEntityCommand.Settings>
{
    private IAnsiConsole _console;
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;
    private readonly IDbMigrator _dbMigrator;
    private readonly IGitService _gitService;

    public AddEntityCommand(IAnsiConsole console,
        IFileSystem fileSystem,
        IConsoleWriter consoleWriter,
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore, 
        IDbMigrator dbMigrator, 
        IGitService gitService)
    {
        _console = console;
        _fileSystem = fileSystem;
        _consoleWriter = consoleWriter;
        _utilities = utilities;
        _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        _dbMigrator = dbMigrator;
        _gitService = gitService;
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
        
        _scaffoldingDirectoryStore.SetBoundedContextDirectoryAndProject(potentialBoundaryDirectory);
        _utilities.IsBoundedContextDirectoryGuard();

        // TODO make injectable
        new FileParsingHelper(_fileSystem).RunInitialTemplateParsingGuards(potentialBoundaryDirectory);
        var template = FileParsingHelper.ReadYamlString<AddEntityTemplate>(settings.Filepath);
        _consoleWriter.WriteLogMessage($"Your template file was parsed successfully");

        FileParsingHelper.RunPrimaryKeyGuard(template.Entities);
        
        RunEntityBuilders(solutionDirectory, _scaffoldingDirectoryStore.SrcDirectory, _scaffoldingDirectoryStore.TestDirectory, template);

        _console.MarkupLine($"{Environment.NewLine}[bold yellow1]Your domain project is ready! Build something amazing. [/]");

        _consoleWriter.StarGithubRequest();
        return 0;
    }

    private void RunEntityBuilders(string solutionDirectory, string srcDirectory, string testDirectory, AddEntityTemplate template)
    {
        template = GetDbContext(_scaffoldingDirectoryStore.SrcDirectory, template, _scaffoldingDirectoryStore.ProjectBaseName);
        template.SolutionName = _scaffoldingDirectoryStore.ProjectBaseName;
        var useSoftDelete = _utilities.ProjectUsesSoftDelete(srcDirectory, _scaffoldingDirectoryStore.ProjectBaseName);
            
        //entities
        new EntityScaffoldingService(_utilities, _fileSystem).ScaffoldEntities(solutionDirectory,
            srcDirectory,
            testDirectory,
            _scaffoldingDirectoryStore.ProjectBaseName,
            template.Entities,
            template.DbContextName,
            template.AddSwaggerComments,
            useSoftDelete);

        new DbContextModifier(_fileSystem).AddDbSet(srcDirectory, template.Entities, template.DbContextName, _scaffoldingDirectoryStore.ProjectBaseName);
    }

    private AddEntityTemplate GetDbContext(string srcDirectory, AddEntityTemplate template, string projectBaseName)
    {
        template.DbContextName = _utilities.GetDbContext(srcDirectory, projectBaseName);
        return template;
    }
}