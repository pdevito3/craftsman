namespace Craftsman.Services;

using Domain;
using Helpers;
using Spectre.Console;

public interface IDbMigrator
{
    void RunDbMigrations(List<ApiTemplate> boundedContexts, string domainDirectory);
}

public class DbMigrator : IDbMigrator
{
    private readonly IConsoleWriter _consoleWriter;
    private readonly IAnsiConsole _console;
    private readonly ICraftsmanUtilities _utilities;

    public DbMigrator(IConsoleWriter consoleWriter, IAnsiConsole console, ICraftsmanUtilities utilities)
    {
        _consoleWriter = consoleWriter;
        _console = console;
        _utilities = utilities;
    }

    private bool RunDbMigration(ApiTemplate template, string srcDirectory)
    {
        var webApiProjectClassPath = ClassPathHelper.WebApiProjectClassPath(srcDirectory, template.ProjectName);

        return _utilities.ExecuteProcess(
            "dotnet",
            @$"ef migrations add ""InitialMigration"" --project ""{webApiProjectClassPath.FullClassPath}""",
            srcDirectory,
            new Dictionary<string, string>()
            {
                { "ASPNETCORE_ENVIRONMENT", "Development" }
            },
            20000,
            $"{Emoji.Known.Warning} {template.ProjectName} Database Migrations timed out and will need to be run manually");
    }

    public void RunDbMigrations(List<ApiTemplate> boundedContexts, string domainDirectory)
    {
        _console.Status()
            .AutoRefresh(true)
            .Spinner(Spinner.Known.Dots2)
            .Start($"[yellow]Running Migrations [/]", ctx =>
            {
                foreach (var bc in boundedContexts)
                {
                    var bcDirectory = $"{domainDirectory}{Path.DirectorySeparatorChar}{bc.ProjectName}";
                    var srcDirectory = Path.Combine(bcDirectory, "src");

                    ctx.Spinner(Spinner.Known.Dots2);
                    ctx.Status($"[bold blue]Running {bc.ProjectName} Database Migrations [/]");
                    if (RunDbMigration(bc, srcDirectory))
                        _consoleWriter.WriteLogMessage($"Database Migrations for {bc.ProjectName} were successful");
                }
            });
    }
    
}