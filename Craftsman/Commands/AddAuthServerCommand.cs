namespace Craftsman.Commands;

using System.IO.Abstractions;
using Builders;
using Builders.AuthServer;
using Builders.Docker;
using Domain;
using Helpers;
using MediatR;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;

public class AddAuthServerCommand : Command<AddAuthServerCommand.Settings>
{
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;
    private readonly IFileParsingHelper _fileParsingHelper;
    private readonly IMediator _mediator;
    private readonly IAnsiConsole _console;

    public AddAuthServerCommand(IFileSystem fileSystem,
        IConsoleWriter consoleWriter,
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore, IFileParsingHelper fileParsingHelper, IMediator mediator, IAnsiConsole console)
    {
        _fileSystem = fileSystem;
        _consoleWriter = consoleWriter;
        _utilities = utilities;
        _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        _fileParsingHelper = fileParsingHelper;
        _mediator = mediator;
        _console = console;
    }

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<Filepath>")]
        public string Filepath { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var potentialSolutionDir = _utilities.GetRootDir();

        _utilities.IsSolutionDirectoryGuard(potentialSolutionDir);
        _scaffoldingDirectoryStore.SetSolutionDirectory(potentialSolutionDir);

        _fileParsingHelper.RunInitialTemplateParsingGuards(settings.Filepath);
        var template = _fileParsingHelper.GetTemplateFromFile<AuthServerTemplate>(settings.Filepath);
        _consoleWriter.WriteHelpText($"Your template file was parsed successfully.");

        AddAuthServer(_scaffoldingDirectoryStore.SolutionDirectory, template);

        _consoleWriter.WriteHelpHeader($"{Environment.NewLine}Your auth server has been successfully added. Keep up the good work! {Emoji.Known.Sparkles}");
        return 0;
    }

    public void AddAuthServer(string solutionDirectory, AuthServerTemplate template)
    {
        _console.Status()
            .AutoRefresh(true)
            .Spinner(Spinner.Known.Dots2)
            .Start($"[yellow]Adding Auth Server [/]", ctx =>
            {
                ctx.Spinner(Spinner.Known.BouncingBar);
                ctx.Status($"[bold blue]Scaffolding files for Auth Server [/]");
                var projectBaseName = template.Name;
        
                new SolutionBuilder(_utilities, _fileSystem, _mediator).BuildAuthServerProject(solutionDirectory, projectBaseName);

                var pulumiYamlBuilder = new PulumiYamlBuilders(_utilities);
                pulumiYamlBuilder.CreateBaseFile(solutionDirectory, projectBaseName);
                pulumiYamlBuilder.CreateDevConfig(solutionDirectory, projectBaseName, template.Port, template.Username, template.Password);

                new Builders.AuthServer.ProgramBuilder(_utilities).CreateAuthServerProgram(solutionDirectory, projectBaseName);
        
                new UserExtensionsBuilder(_utilities).Create(solutionDirectory, projectBaseName);
                new ClientExtensionsBuilder(_utilities).Create(solutionDirectory, projectBaseName);
                new ClientFactoryBuilder(_utilities).Create(solutionDirectory, projectBaseName);
                new ScopeFactoryBuilder(_utilities).Create(solutionDirectory, projectBaseName);
                new RealmBuildBuilder(_utilities).Create(solutionDirectory, projectBaseName, template.RealmName, template.Clients);

                new DockerComposeBuilders(_utilities, _fileSystem).AddAuthServerToDockerCompose(solutionDirectory, template);
                
                _consoleWriter.WriteLogMessage($"Auth server scaffolding was successful");
            });
    }
}