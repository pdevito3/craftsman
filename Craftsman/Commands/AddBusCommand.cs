namespace Craftsman.Commands;

using System.IO.Abstractions;
using Builders;
using Builders.Docker;
using Builders.ExtensionBuilders;
using Builders.Tests.Utilities;
using Domain;
using Helpers;
using MediatR;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;

public class AddBusCommand(
    IFileSystem fileSystem,
    IConsoleWriter consoleWriter,
    ICraftsmanUtilities utilities,
    IScaffoldingDirectoryStore scaffoldingDirectoryStore,
    IFileParsingHelper fileParsingHelper,
    IMediator mediator)
    : Command<AddBusCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[Filepath]")]
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
        var contextName = utilities.GetDbContext(scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.ProjectBaseName);

        var template = new Bus();
        template.Environment = new ApiEnvironment();
        if (!string.IsNullOrEmpty(settings.Filepath))
        {
            fileParsingHelper.RunInitialTemplateParsingGuards(settings.Filepath);
            template = fileParsingHelper.GetTemplateFromFile<Bus>(settings.Filepath);
        }
        template.ProjectBaseName = scaffoldingDirectoryStore.ProjectBaseName;

        AddBus(template,
            scaffoldingDirectoryStore.SrcDirectory,
            scaffoldingDirectoryStore.TestDirectory,
            scaffoldingDirectoryStore.ProjectBaseName,
            solutionDirectory
        );

        consoleWriter.WriteHelpHeader($"{Environment.NewLine}Your feature has been successfully added. Keep up the good work! {Emoji.Known.Sparkles}");
        return 0;
    }

    public void AddBus(Bus template, string srcDirectory, string testDirectory, string projectBaseName, string solutionDirectory)
    {
        var massTransitPackages = new Dictionary<string, string>{
            { "MassTransit", "8.1.3" },
            { "MassTransit.RabbitMQ", "8.1.3" }
        };
        var webApiClassPath = ClassPathHelper.WebApiProjectClassPath(srcDirectory, projectBaseName);
        utilities.AddPackages(webApiClassPath, massTransitPackages);

        mediator.Send(new MassTransitExtensionsBuilder.MassTransitExtensionsBuilderCommand(solutionDirectory)).GetAwaiter().GetResult();
        new WebApiLaunchSettingsModifier(fileSystem).UpdateLaunchSettingEnvVar(srcDirectory, "RMQ_HOST", template.Environment.BrokerSettings.Host, projectBaseName);
        new WebApiLaunchSettingsModifier(fileSystem).UpdateLaunchSettingEnvVar(srcDirectory, "RMQ_VIRTUAL_HOST", template.Environment.BrokerSettings.VirtualHost, projectBaseName);
        new WebApiLaunchSettingsModifier(fileSystem).UpdateLaunchSettingEnvVar(srcDirectory, "RMQ_USERNAME", template.Environment.BrokerSettings.Username, projectBaseName);
        new WebApiLaunchSettingsModifier(fileSystem).UpdateLaunchSettingEnvVar(srcDirectory, "RMQ_PASSWORD", template.Environment.BrokerSettings.Password, projectBaseName);
        new ProgramModifier(fileSystem).RegisterMassTransitService(srcDirectory, projectBaseName);

        new IntegrationTestFixtureModifier(fileSystem, consoleWriter).AddMassTransit(testDirectory, projectBaseName);
        new DockerComposeBuilders(utilities, fileSystem).AddRmqToDockerCompose(solutionDirectory, template.Environment.BrokerSettings);
    }
}