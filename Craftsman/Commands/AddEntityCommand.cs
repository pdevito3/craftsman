namespace Craftsman.Commands;

using System.IO.Abstractions;
using Builders;
using Domain;
using Helpers;
using MediatR;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;

public class AddEntityCommand(IFileSystem fileSystem,
        IConsoleWriter consoleWriter,
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore,
        IFileParsingHelper fileParsingHelper, IMediator mediator)
    : Command<AddEntityCommand.Settings>
{
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
        var template = fileParsingHelper.GetTemplateFromFile<AddEntityTemplate>(settings.Filepath);
        consoleWriter.WriteLogMessage($"Your template file was parsed successfully");

        FileParsingHelper.RunPrimaryKeyGuard(template.Entities);

        RunEntityBuilders(solutionDirectory, scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.TestDirectory, template);

        consoleWriter.WriteHelpHeader($"{Environment.NewLine}Your entities have been successfully added. Keep up the good work!");
        consoleWriter.StarGithubRequest();
        return 0;
    }

    private void RunEntityBuilders(string solutionDirectory, string srcDirectory, string testDirectory, AddEntityTemplate template)
    {
        template = GetDbContext(scaffoldingDirectoryStore.SrcDirectory, template, scaffoldingDirectoryStore.ProjectBaseName);
        template.SolutionName = scaffoldingDirectoryStore.ProjectBaseName;
        var useSoftDelete = utilities.ProjectUsesSoftDelete(srcDirectory, scaffoldingDirectoryStore.ProjectBaseName);

        //entities
        new EntityScaffoldingService(utilities, fileSystem, mediator, consoleWriter).ScaffoldEntities(solutionDirectory,
            srcDirectory,
            testDirectory,
            scaffoldingDirectoryStore.ProjectBaseName,
            template.Entities,
            template.DbContextName,
            template.AddSwaggerComments,
            useSoftDelete,
            DbProvider.Unknown);
    }

    private AddEntityTemplate GetDbContext(string srcDirectory, AddEntityTemplate template, string projectBaseName)
    {
        template.DbContextName = utilities.GetDbContext(srcDirectory, projectBaseName);
        return template;
    }
}