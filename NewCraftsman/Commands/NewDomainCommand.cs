namespace NewCraftsman.Commands;

using System.IO.Abstractions;
using Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using static Helpers.ConsoleWriter;

public class NewDomainCommand : Command<NewDomainCommand.Settings>
{
    private IAnsiConsole _console;
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;

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
        var domainProject = FileParsingHelper.GetTemplateFromFile<DomainProject>(settings.Filepath);
        _consoleWriter.WriteLogMessage($"Your template file was parsed successfully");

        var domainDirectory = $"{rootDir}{Path.DirectorySeparatorChar}{domainProject.DomainName}";
        CreateNewDomainProject(domainDirectory, domainProject);

        AnsiConsole.MarkupLine($"{Environment.NewLine}[bold yellow1]Your domain project is ready! Build something amazing. [/]");

        _consoleWriter.StarGithubRequest();
        return 0;
    }
}

