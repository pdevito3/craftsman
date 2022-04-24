namespace Craftsman.Commands;

using System.IO.Abstractions;
using Builders;
using Builders.AuthServer;
using Builders.Docker;
using Domain;
using Helpers;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;

public class AddBffEntityCommand : Command<AddBffEntityCommand.Settings>
{
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;
    private readonly IAnsiConsole _console;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;
    private readonly IFileParsingHelper _fileParsingHelper;

    public AddBffEntityCommand(IFileSystem fileSystem,
        IConsoleWriter consoleWriter,
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore,
        IAnsiConsole console, IFileParsingHelper fileParsingHelper)
    {
        _fileSystem = fileSystem;
        _consoleWriter = consoleWriter;
        _utilities = utilities;
        _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        _console = console;
        _fileParsingHelper = fileParsingHelper;
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
        var template = FileParsingHelper.GetTemplateFromFile<BffEntityTemplate>(settings.Filepath);
        _consoleWriter.WriteHelpText($"Your template file was parsed successfully.");

        new EntityScaffoldingService(_utilities, _fileSystem).ScaffoldBffEntities(template.Entities, _scaffoldingDirectoryStore.SpaDirectory);

        _consoleWriter.WriteHelpHeader($"{Environment.NewLine}Your feature has been successfully added. Keep up the good work! {Emoji.Known.Sparkles}");
        return 0;
    }

    public void AddAuthServer(string solutionDirectory, AuthServerTemplate template)
    {
        new SolutionBuilder(_utilities, _fileSystem).BuildAuthServerProject(solutionDirectory, template.Name);

        new AuthServerLaunchSettingsBuilder(_utilities).CreateLaunchSettings(solutionDirectory, template.Name, template.Port);
        new StartupBuilder(_utilities).CreateAuthServerStartup(solutionDirectory, template.Name);
        new ProgramBuilder(_utilities).CreateAuthServerProgram(solutionDirectory, template.Name);
        new AuthServerConfigBuilder(_utilities).CreateConfig(solutionDirectory, template);
        new AppSettingsBuilder(_utilities).CreateAuthServerAppSettings(solutionDirectory, template.Name);

        new AuthServerPackageJsonBuilder(_utilities).CreatePackageJson(solutionDirectory, template.Name);
        new AuthServerTailwindConfigBuilder(_utilities).CreateTailwindConfig(solutionDirectory, template.Name);
        new AuthServerPostCssBuilder(_utilities).CreatePostCss(solutionDirectory, template.Name);

        // controllers
        new AuthServerAccountControllerBuilder(_utilities).CreateAccountController(solutionDirectory, template.Name);
        new AuthServerExternalControllerBuilder(_utilities).CreateExternalController(solutionDirectory, template.Name);
        // AuthServerHomeControllerBuilder.CreateHomeController(projectDirectory, template.Name);

        // view models + models
        new AuthServerAccountViewModelsBuilder(_utilities).CreateViewModels(solutionDirectory, template.Name);
        new AuthServerSharedViewModelsBuilder(_utilities).CreateViewModels(solutionDirectory, template.Name);
        new AuthServerExternalModelsBuilder(_utilities).CreateModels(solutionDirectory, template.Name);
        new AuthServerAccountModelsBuilder(_utilities).CreateModels(solutionDirectory, template.Name);

        // views
        new AuthServerAccountViewsBuilder(_utilities).CreateLoginView(solutionDirectory, template.Name);
        new AuthServerAccountViewsBuilder(_utilities).CreateLogoutView(solutionDirectory, template.Name);
        new AuthServerAccountViewsBuilder(_utilities).CreateAccessDeniedView(solutionDirectory, template.Name);
        new AuthServerSharedViewsBuilder(_utilities).CreateLayoutView(solutionDirectory, template.Name);
        new AuthServerSharedViewsBuilder(_utilities).CreateStartView(solutionDirectory, template.Name);
        new AuthServerSharedViewsBuilder(_utilities).CreateViewImports(solutionDirectory, template.Name);

        // css files for TW
        new AuthServerCssBuilder(_utilities).CreateOutputCss(solutionDirectory, template.Name);
        new AuthServerCssBuilder(_utilities).CreateSiteCss(solutionDirectory, template.Name);

        // helpers
        new AuthServerTestUsersBuilder(_utilities).CreateTestModels(solutionDirectory, template.Name);
        new AuthServerExtensionsBuilder(_utilities).CreateExtensions(solutionDirectory, template.Name);
        new SecurityHeadersAttributeBuilder(_utilities).CreateAttribute(solutionDirectory, template.Name);
        new AuthServerDockerfileBuilder(_utilities).CreateAuthServerDotNetDockerfile(solutionDirectory, template.Name);
        new DockerIgnoreBuilder(_utilities).CreateDockerIgnore(solutionDirectory, template.Name);
        // DockerComposeBuilders.AddAuthServerToDockerCompose(projectDirectory, template.Name, template.Port);
    }
}