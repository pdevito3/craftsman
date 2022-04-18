namespace NewCraftsman.Commands;

using System.IO.Abstractions;
using Builders;
using Domain.DomainProject;
using Domain.DomainProject.Dtos;
using Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using static Helpers.ConsoleWriter;

public class NewDomainCommand : Command<NewDomainCommand.Settings>
{
    private IAnsiConsole _console;
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;
    private readonly ICraftsmanUtilities _utilities;

    public NewDomainCommand(IAnsiConsole console, IFileSystem fileSystem, IConsoleWriter consoleWriter, ICraftsmanUtilities utilities)
    {
        _console = console;
        _fileSystem = fileSystem;
        _consoleWriter = consoleWriter;
        _utilities = utilities;
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
        var domainProjectDto = FileParsingHelper.GetTemplateFromFile<DomainProjectDto>(settings.Filepath);
        _consoleWriter.WriteLogMessage($"Your template file was parsed successfully");
        
        var domainProject = DomainProject.Create(domainProjectDto);

        var solutionDirectory = $"{rootDir}{Path.DirectorySeparatorChar}{domainProject.DomainName}";
        CreateNewDomainProject(solutionDirectory, domainProject); // TODO create DomainProject.Create?

        AnsiConsole.MarkupLine($"{Environment.NewLine}[bold yellow1]Your domain project is ready! Build something amazing. [/]");

        _consoleWriter.StarGithubRequest();
        return 0;
    }
    
    public void CreateNewDomainProject(string solutionDirectory, DomainProject domainProject)
    {
        _fileSystem.Directory.CreateDirectory(solutionDirectory);
        new SolutionBuilder(_fileSystem, _utilities, _consoleWriter).BuildSolution(solutionDirectory, domainProject.DomainName);
        
        // // need this before boundaries to give them something to build against
        // DockerComposeBuilders.CreateDockerComposeSkeleton(solutionDirectory);
        // DockerComposeBuilders.AddJaegerToDockerCompose(solutionDirectory);
        // // DockerBuilders.CreateDockerComposeDbSkeleton(solutionDirectory);
        //     
        // //Parallel.ForEach(domainProject.BoundedContexts, (template) =>
        // //    ApiScaffolding.ScaffoldApi(solutionDirectory, template, verbosity));
        // foreach (var bc in domainProject.BoundedContexts)
        //     ApiScaffolding.ScaffoldApi(solutionDirectory, bc);
        //
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