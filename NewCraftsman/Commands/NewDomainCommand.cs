namespace NewCraftsman.Commands;

using System.IO.Abstractions;
using Builders;
using Builders.Docker;
using Domain.DomainProjects;
using Domain.DomainProjects.Dtos;
using Helpers;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;
using static Helpers.ConsoleWriter;

public class NewDomainCommand : Command<NewDomainCommand.Settings>
{
    private IAnsiConsole _console;
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

    public NewDomainCommand(IAnsiConsole console, IFileSystem fileSystem, IConsoleWriter consoleWriter, ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore)
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
        var rootDir = _fileSystem.Directory.GetCurrentDirectory();
        var myEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        if (myEnv == "Dev")
            rootDir = _console.Ask<string>("Enter the root directory of your project:");
        
        new FileParsingHelper(_fileSystem).RunInitialTemplateParsingGuards(rootDir);
        var domainProjectDto = FileParsingHelper.GetTemplateFromFile<DomainProjectDto>(settings.Filepath);
        _consoleWriter.WriteLogMessage($"Your template file was parsed successfully");
        
        var domainProject = DomainProject.Create(domainProjectDto);

        _scaffoldingDirectoryStore.SetSolutionDirectory(rootDir, domainProject.DomainName);
        CreateNewDomainProject(domainProject); // TODO create DomainProject.Create?

        _console.MarkupLine($"{Environment.NewLine}[bold yellow1]Your domain project is ready! Build something amazing. [/]");

        _consoleWriter.StarGithubRequest();
        return 0;
    }
    
    public void CreateNewDomainProject(DomainProject domainProject)
    {
        var solutionDirectory = _scaffoldingDirectoryStore.SolutionDirectory;
        _fileSystem.Directory.CreateDirectory(solutionDirectory);
        new SolutionBuilder(_fileSystem, _utilities, _consoleWriter).BuildSolution(solutionDirectory, domainProject.DomainName);
        
        // need this before boundaries to give them something to build against
        new DockerComposeBuilders(_utilities).CreateDockerComposeSkeleton(solutionDirectory);
        DockerComposeBuilders.AddJaegerToDockerCompose(solutionDirectory);
        // DockerBuilders.CreateDockerComposeDbSkeleton(solutionDirectory);
            
        //Parallel.ForEach(domainProject.BoundedContexts, (template) =>
        //    ApiScaffolding.ScaffoldApi(solutionDirectory, template, verbosity));
        foreach (var bc in domainProject.BoundedContexts)
            _consoleWriter.WriteInfo("Scaffolding Bounded Context: " + bc.ProjectName);
            // ApiScaffolding.ScaffoldApi(solutionDirectory, bc);

        // // auth server
        // if (domainProject.AuthServer != null)
        //     AddAuthServerCommand.AddAuthServer(solutionDirectory, domainProject.AuthServer);
        //     
        // // bff
        // if (domainProject.AuthServer != null)
        //     AddBffCommand.AddBff(domainProject.Bff, solutionDirectory);
        //
        // // messages
        // if (domainProject.Messages.Count > 0)
        //     AddMessageCommand.AddMessages(solutionDirectory, domainProject.Messages);
        //
        // // migrations
        // Utilities.RunDbMigrations(domainProject.BoundedContexts, solutionDirectory);
        //
        // //final
        // ReadmeBuilder.CreateReadme(solutionDirectory, domainProject.DomainName);
        //
        // if (domainProject.AddGit)
        //     Utilities.GitSetup(solutionDirectory, domainProject.UseSystemGitUser);
    }
}