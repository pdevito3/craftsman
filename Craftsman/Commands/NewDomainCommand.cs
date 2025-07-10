namespace Craftsman.Commands;

using System.IO.Abstractions;
using Builders;
using Builders.Docker;
using Domain;
using Helpers;
using MediatR;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;

public class NewDomainCommand(
    IAnsiConsole console,
    IFileSystem fileSystem,
    IConsoleWriter consoleWriter,
    ICraftsmanUtilities utilities,
    IScaffoldingDirectoryStore scaffoldingDirectoryStore,
    IDbMigrator dbMigrator,
    IGitService gitService,
    IFileParsingHelper fileParsingHelper,
    IMediator mediator)
    : Command<NewDomainCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<Filepath>")]
        public string Filepath { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var rootDir = utilities.GetRootDir();

        // TODO make injectable
        fileParsingHelper.RunInitialTemplateParsingGuards(settings.Filepath);
        var domainProject = fileParsingHelper.GetTemplateFromFile<DomainProject>(settings.Filepath);
        consoleWriter.WriteLogMessage($"Your template file was parsed successfully");

        scaffoldingDirectoryStore.SetSolutionDirectory(rootDir, domainProject.DomainName);
        CreateNewDomainProject(domainProject);

        console.MarkupLine($"{Environment.NewLine}[bold yellow1]Your domain project is ready! Build something amazing. [/]");

        consoleWriter.StarGithubRequest();
        return 0;
    }

    public void CreateNewDomainProject(DomainProject domainProject)
    {
        var solutionDirectory = scaffoldingDirectoryStore.SolutionDirectory;
        fileSystem.Directory.CreateDirectory(solutionDirectory);
        mediator.Send(new SolutionBuilder.BuildSolutionCommand(solutionDirectory, domainProject.DomainName)).GetAwaiter().GetResult();

        // need this before boundaries to give them something to build against
        new DockerComposeBuilders(utilities, fileSystem).CreateDockerComposeSkeleton(solutionDirectory);

        var otelAgentPort = CraftsmanUtilities.GetFreePort();
        new DockerComposeBuilders(utilities, fileSystem).AddJaegerToDockerCompose(solutionDirectory, otelAgentPort);
        // DockerBuilders.CreateDockerComposeDbSkeleton(solutionDirectory);

        //Parallel.ForEach(domainProject.BoundedContexts, (template) =>
        //    ApiScaffolding.ScaffoldApi(solutionDirectory, template, verbosity));
        foreach (var bc in domainProject.BoundedContexts)
        {
            bc.DockerConfig.OTelAgentPort = otelAgentPort;
            new ApiScaffoldingService(console, consoleWriter, utilities, scaffoldingDirectoryStore, fileSystem, mediator, fileParsingHelper)
                .ScaffoldApi(solutionDirectory, bc);
        }

        // auth server
        if (domainProject.AuthServer != null)
            new AddAuthServerCommand(fileSystem, consoleWriter, utilities, scaffoldingDirectoryStore, fileParsingHelper, mediator, console)
                .AddAuthServer(solutionDirectory, domainProject.AuthServer);

        // messages
        if (domainProject.Messages.Count > 0)
            new AddMessageCommand(fileSystem, consoleWriter, utilities, scaffoldingDirectoryStore, console, fileParsingHelper, mediator)
                .AddMessages(solutionDirectory, domainProject.Messages);

        // migrations
        dbMigrator.RunDbMigrations(domainProject.BoundedContexts, solutionDirectory);

        // github
        if (domainProject.IncludeDependabot)
        {
            mediator.Send(new GithubDependabotBuilder.GithubDependabotBuilderCommand(solutionDirectory)).GetAwaiter().GetResult();
        }
        
        //final
        mediator.Send(new ReadmeBuilder.ReadmeBuilderCommand(solutionDirectory, domainProject.DomainName)).GetAwaiter().GetResult();

        if (domainProject.AddGit)
            gitService.GitSetup(solutionDirectory, domainProject.UseSystemGitUser);
    }
}