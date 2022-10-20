namespace Craftsman.Commands;

using System.IO.Abstractions;
using Builders;
using Builders.Docker;
using Builders.ExtensionBuilders;
using Builders.Tests.Utilities;
using Domain;
using Helpers;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;

public class AddBusCommand : Command<AddBusCommand.Settings>
{
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;
    private readonly IAnsiConsole _console;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;
    private readonly IFileParsingHelper _fileParsingHelper;

    public AddBusCommand(IFileSystem fileSystem,
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
        [CommandArgument(0, "[Filepath]")]
        public string Filepath { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var potentialBoundaryDirectory = _utilities.GetRootDir();

        var solutionDirectory = _fileSystem.Directory.GetParent(potentialBoundaryDirectory)?.FullName;
        _utilities.IsSolutionDirectoryGuard(solutionDirectory, true);
        _scaffoldingDirectoryStore.SetSolutionDirectory(solutionDirectory);

        var projectName = new DirectoryInfo(potentialBoundaryDirectory).Name;
        _scaffoldingDirectoryStore.SetBoundedContextDirectoryAndProject(projectName);
        _utilities.IsBoundedContextDirectoryGuard();
        var contextName = _utilities.GetDbContext(_scaffoldingDirectoryStore.SrcDirectory, _scaffoldingDirectoryStore.ProjectBaseName);

        var template = new Bus();
        template.Environment = new ApiEnvironment();
        if (!string.IsNullOrEmpty(settings.Filepath))
        {
            _fileParsingHelper.RunInitialTemplateParsingGuards(settings.Filepath);
            template = _fileParsingHelper.GetTemplateFromFile<Bus>(settings.Filepath);
        }
        template.ProjectBaseName = _scaffoldingDirectoryStore.ProjectBaseName;

        AddBus(template,
            _scaffoldingDirectoryStore.SrcDirectory,
            _scaffoldingDirectoryStore.TestDirectory,
            _scaffoldingDirectoryStore.ProjectBaseName,
            solutionDirectory
        );

        _consoleWriter.WriteHelpHeader($"{Environment.NewLine}Your feature has been successfully added. Keep up the good work! {Emoji.Known.Sparkles}");
        return 0;
    }

    public void AddBus(Bus template, string srcDirectory, string testDirectory, string projectBaseName, string solutionDirectory)
    {
        var massTransitPackages = new Dictionary<string, string>{
            { "MassTransit", "8.0.7" },
            { "MassTransit.RabbitMQ", "8.0.7" }
        };
        var webApiClassPath = ClassPathHelper.WebApiProjectClassPath(srcDirectory, projectBaseName);
        _utilities.AddPackages(webApiClassPath, massTransitPackages);

        new MassTransitExtensionsBuilder(_utilities).CreateMassTransitServiceExtension(solutionDirectory, srcDirectory, projectBaseName);
        new WebApiLaunchSettingsModifier(_fileSystem).UpdateLaunchSettingEnvVar(srcDirectory, "RMQ_HOST", template.Environment.BrokerSettings.Host, projectBaseName);
        new WebApiLaunchSettingsModifier(_fileSystem).UpdateLaunchSettingEnvVar(srcDirectory, "RMQ_VIRTUAL_HOST", template.Environment.BrokerSettings.VirtualHost, projectBaseName);
        new WebApiLaunchSettingsModifier(_fileSystem).UpdateLaunchSettingEnvVar(srcDirectory, "RMQ_USERNAME", template.Environment.BrokerSettings.Username, projectBaseName);
        new WebApiLaunchSettingsModifier(_fileSystem).UpdateLaunchSettingEnvVar(srcDirectory, "RMQ_PASSWORD", template.Environment.BrokerSettings.Password, projectBaseName);
        new ProgramModifier(_fileSystem).RegisterMassTransitService(srcDirectory, projectBaseName);

        new IntegrationTestFixtureModifier(_fileSystem).AddMassTransit(testDirectory, projectBaseName);
        new DockerComposeBuilders(_utilities, _fileSystem).AddRmqToDockerCompose(solutionDirectory, template.Environment.BrokerSettings);
    }
}