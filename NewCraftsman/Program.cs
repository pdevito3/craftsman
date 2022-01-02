using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using SpectreFail;

var serviceCollection = new ServiceCollection();
serviceCollection.AddSingleton<IFileSystem, FileSystem>();

var registrar = new TypeRegistrar(serviceCollection);
var app = new CommandApp(registrar); // works without registrar

app.Configure(config =>
{
    config.AddCommand<HelloCommand>("hello")
        .WithAlias("hola")
        .WithDescription("Say hello")
        .WithExample(new []{"hello", "Phil"})
        .WithExample(new []{"hello", "Phil", "--count", "4"});
});

return app.Run(args);

public class HelloCommand : Command<HelloCommand.Settings>
{
    private IAnsiConsole _console;

    public HelloCommand(IAnsiConsole console)
    {
        _console = console;
    }

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[Name]")]
        public string Name { get; set; }
    }


    public override int Execute(CommandContext context, Settings settings)
    {
        // AnsiConsole.MarkupLine($"Hello, [blue]{settings.Name}[/]");
        _console.MarkupLine($"Hello, [blue]{settings.Name}[/]");
        return 0;
    }
}