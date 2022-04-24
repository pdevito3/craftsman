namespace Craftsman.Commands;

using System.IO.Abstractions;
using Builders;
using Builders.Bff;
using Builders.Bff.Components.Headers;
using Builders.Bff.Components.Layouts;
using Builders.Bff.Components.Navigation;
using Builders.Bff.Components.Notifications;
using Builders.Bff.Features.Auth;
using Builders.Bff.Features.Home;
using Builders.Bff.Src;
using Domain;
using Helpers;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;

public class AddBffCommand : Command<AddBffCommand.Settings>
{
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;
    private readonly IFileParsingHelper _fileParsingHelper;
    private readonly IAnsiConsole _console;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

    public AddBffCommand(IFileSystem fileSystem,
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
        var template = FileParsingHelper.GetTemplateFromFile<BffTemplate>(settings.Filepath);
        _consoleWriter.WriteHelpText($"Your template file was parsed successfully.");

        _console.Status()
            .AutoRefresh(true)
            .Spinner(Spinner.Known.Dots2)
            .Start($"[yellow]Creating {template.ProjectName} [/]", ctx =>
            {
                AddBff(template, potentialSolutionDir);

                _consoleWriter.WriteLogMessage($"File scaffolding for {template.ProjectName} was successful");
            });

        _consoleWriter.WriteHelpHeader($"{Environment.NewLine}Your feature has been successfully added. Keep up the good work! {Emoji.Known.Sparkles}");
        return 0;
    }

    public void AddBff(BffTemplate template, string domainDirectory)
    {
        var projectName = template.ProjectName;
        var projectDirectory = template.GetProjectDirectory(domainDirectory);
        var spaDirectory = template.GetSpaDirectory(domainDirectory);

        new SolutionBuilder(_utilities, _fileSystem).BuildBffProject(domainDirectory, projectName, template.ProxyPort);
        _fileSystem.Directory.CreateDirectory(spaDirectory);

        // .NET Project
        new LaunchSettingsBuilder(_utilities).CreateLaunchSettings(projectDirectory, projectName, template);
        new BffAppSettingsBuilder(_utilities).CreateBffAppSettings(projectDirectory);
        new LoggingConfigurationBuilder(_utilities).CreateBffConfigFile(domainDirectory, projectName);

        new BffProgramBuilder(_utilities).CreateProgram(projectDirectory, domainDirectory, projectName, template);
        new BffReadmeBuilder(_utilities).CreateReadme(projectDirectory, projectName);

        // SPA - root
        new ViteConfigBuilder(_utilities).CreateViteConfig(spaDirectory, template.ProxyPort);
        new TsConfigBuilder(_utilities).CreateTsConfigPaths(spaDirectory);
        new TsConfigBuilder(_utilities).CreateTsConfig(spaDirectory);
        new TailwindConfigBuilder(_utilities).CreateTailwindConfig(spaDirectory);
        new PostCssBuilder(_utilities).CreatePostCss(spaDirectory);
        new PackageJsonBuilder(_utilities).CreatePackageJson(spaDirectory, projectName);
        new IndexHtmlBuilder(_utilities).CreateIndexHtml(spaDirectory, template.HeadTitle);
        new AspnetcoreReactBuilder(_utilities).CreateAspnetcoreReact(spaDirectory);
        new AspnetcoreHttpsBuilder(_utilities).CreateAspnetcoreHttps(spaDirectory);
        new EnvBuilder(_utilities).CreateEnv(_scaffoldingDirectoryStore.SpaDirectory);
        new EnvBuilder(_utilities).CreateDevEnv(spaDirectory, template.ProxyPort);
        new PrettierRcBuilder(_utilities).CreatePrettierRc(spaDirectory);

        // SPA - src
        new AssetsBuilder(_utilities).CreateFavicon(spaDirectory);
        new AssetsBuilder(_utilities).CreateLogo(spaDirectory);
        new LibBuilder(_utilities).CreateAxios(spaDirectory);
        new TypesBuilder(_utilities).CreateApiTypes(spaDirectory);
        new ViteEnvBuilder(_utilities).CreateViteEnv(spaDirectory);
        new MainTsxBuilder(_utilities).CreateMainTsx(spaDirectory);
        new CustomCssBuilder(_utilities).CreateCustomCss(spaDirectory);
        new AppTsxBuilder(_utilities).CreateAppTsx(spaDirectory);

        // SPA - src/components
        new HeadersComponentBuilder(_utilities).CreateHeaderComponentItems(spaDirectory);
        new NotificationsComponentBuilder(_utilities).CreateNotificationComponentItems(spaDirectory);
        new NavigationComponentBuilder(_utilities).CreateNavigationComponentItems(spaDirectory);
        new LayoutComponentBuilder(_utilities).CreateLayoutComponentItems(spaDirectory);

        // SPA - src/features
        new AuthFeatureApiBuilder(_utilities).CreateAuthFeatureApis(spaDirectory);
        new AuthFeatureRoutesBuilder(_utilities).CreateAuthFeatureRoutes(spaDirectory);
        new AuthFeatureBuilder(_utilities).CreateAuthFeatureIndex(spaDirectory);

        new HomeFeatureRoutesBuilder(_utilities).CreateHomeFeatureRoutes(spaDirectory);
        new HomeFeatureBuilder(_utilities).CreateHomeFeatureIndex(spaDirectory);

        new EntityScaffoldingService(_utilities, _fileSystem).ScaffoldBffEntities(template.Entities, spaDirectory);

        // Docker
        // new BffDockerfileBuilder(_utilities).CreateBffDotNetDockerfile(projectDirectory, projectName);
        // DockerComposeBuilders.CreateDockerIgnore(projectDirectory, projectDirectory);
        // TODO docs on ApiAddress and making a resource to abstract out the baseurl and that the `ApiAddress` can be a string that incorporates that
        // TODO AnsiConsole injection for status updates
    }
}