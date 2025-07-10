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

public class AddAuthServerCommand(
    IFileSystem fileSystem,
    IConsoleWriter consoleWriter,
    ICraftsmanUtilities utilities,
    IScaffoldingDirectoryStore scaffoldingDirectoryStore,
    IFileParsingHelper fileParsingHelper,
    IMediator mediator,
    IAnsiConsole console)
    : Command<AddAuthServerCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<Filepath>")]
        public string Filepath { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var potentialSolutionDir = utilities.GetRootDir();

        utilities.IsSolutionDirectoryGuard(potentialSolutionDir);
        scaffoldingDirectoryStore.SetSolutionDirectory(potentialSolutionDir);

        fileParsingHelper.RunInitialTemplateParsingGuards(settings.Filepath);
        var template = fileParsingHelper.GetTemplateFromFile<AuthServerTemplate>(settings.Filepath);
        consoleWriter.WriteHelpText($"Your template file was parsed successfully.");

        AddAuthServer(scaffoldingDirectoryStore.SolutionDirectory, template);

        consoleWriter.WriteHelpHeader($"{Environment.NewLine}Your auth server has been successfully added. Keep up the good work! {Emoji.Known.Sparkles}");
        return 0;
    }

    public void AddAuthServer(string solutionDirectory, AuthServerTemplate template)
    {
        console.Status()
            .AutoRefresh(true)
            .Spinner(Spinner.Known.Dots2)
            .Start($"[yellow]Adding Auth Server [/]", ctx =>
            {
                ctx.Spinner(Spinner.Known.BouncingBar);
                ctx.Status($"[bold blue]Scaffolding files for Auth Server [/]");
                var projectBaseName = template.Name;
        
                mediator.Send(new SolutionBuilder.BuildAuthServerProjectCommand(solutionDirectory, projectBaseName)).GetAwaiter().GetResult();

                mediator.Send(new PulumiYamlBuilders.CreateBaseFileCommand(projectBaseName)).GetAwaiter().GetResult();
                mediator.Send(new PulumiYamlBuilders.CreateDevConfigCommand(projectBaseName, template.Port, template.Username, template.Password)).GetAwaiter().GetResult();

                mediator.Send(new Builders.AuthServer.ProgramBuilder.Command(projectBaseName)).GetAwaiter().GetResult();
        
                mediator.Send(new UserExtensionsBuilder.Command(projectBaseName)).GetAwaiter().GetResult();
                mediator.Send(new ClientExtensionsBuilder.Command(projectBaseName)).GetAwaiter().GetResult();
                mediator.Send(new ClientFactoryBuilder.Command(projectBaseName)).GetAwaiter().GetResult();
                mediator.Send(new ScopeFactoryBuilder.Command(projectBaseName)).GetAwaiter().GetResult();
                mediator.Send(new RealmBuildBuilder.Command(projectBaseName, template.RealmName, template.Clients)).GetAwaiter().GetResult();

                new DockerComposeBuilders(utilities, fileSystem).AddAuthServerToDockerCompose(solutionDirectory, template);
                
                consoleWriter.WriteLogMessage($"Auth server scaffolding was successful");
            });
    }
}