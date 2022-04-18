namespace NewCraftsman.Commands;

using System.IO.Abstractions;
using Dtos;
using Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using static Helpers.ConsoleWriter;

public class NewDomainCommand : Command<NewDomainCommand.Settings>
{
    private IAnsiConsole _console;
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;
    private readonly INewDomainScaffoldingDirectoryManager

    public NewDomainCommand(IAnsiConsole console, IFileSystem fileSystem, IConsoleWriter consoleWriter)
    {
        _console = console;
        _fileSystem = fileSystem;
        _consoleWriter = consoleWriter;
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
        
        FileParsingHelper.RunInitialTemplateParsingGuards(settings.Filepath);
        var domainProject = FileParsingHelper.GetTemplateFromFile<NewDomainDto>(settings.Filepath);
        _consoleWriter.WriteLogMessage($"Your template file was parsed successfully");
        
        // TODO map to craftsman domain entity
        // TODO ******************************

        var solutionDirectory = $"{rootDir}{Path.DirectorySeparatorChar}{domainProject.DomainName}";
        CreateNewDomainProject(solutionDirectory, domainProject); // TODO create DomainProject.Create?

        AnsiConsole.MarkupLine($"{Environment.NewLine}[bold yellow1]Your domain project is ready! Build something amazing. [/]");

        _consoleWriter.StarGithubRequest();
        return 0;
    }
    
    public void CreateNewDomainProject(string domainDirectory, DomainProject domainProject)
    {
        _fileSystem.Directory.CreateDirectory(domainDirectory);
        SolutionBuilder.BuildSolution(domainDirectory, domainProject.DomainName);

        // need this before boundaries to give them something to build against
        DockerComposeBuilders.CreateDockerComposeSkeleton(domainDirectory);
        DockerComposeBuilders.AddJaegerToDockerCompose(domainDirectory);
        // DockerBuilders.CreateDockerComposeDbSkeleton(domainDirectory);
            
        //Parallel.ForEach(domainProject.BoundedContexts, (template) =>
        //    ApiScaffolding.ScaffoldApi(domainDirectory, template, verbosity));
        foreach (var bc in domainProject.BoundedContexts)
            ApiScaffolding.ScaffoldApi(domainDirectory, bc);

        // auth server
        if (domainProject.AuthServer != null)
            AddAuthServerCommand.AddAuthServer(domainDirectory, domainProject.AuthServer);
            
        // bff
        if (domainProject.AuthServer != null)
            AddBffCommand.AddBff(domainProject.Bff, domainDirectory);

        // messages
        if (domainProject.Messages.Count > 0)
            AddMessageCommand.AddMessages(domainDirectory, domainProject.Messages);

        // migrations
        Utilities.RunDbMigrations(domainProject.BoundedContexts, domainDirectory);

        //final
        ReadmeBuilder.CreateReadme(domainDirectory, domainProject.DomainName);

        if (domainProject.AddGit)
            Utilities.GitSetup(domainDirectory, domainProject.UseSystemGitUser);
    }
}