using System.IO.Abstractions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NewCraftsman;
using NewCraftsman.Commands;
using NewCraftsman.Exceptions;
using NewCraftsman.Helpers;
using NewCraftsman.Interceptors;
using NewCraftsman.Services;
using Spectre.Console;
using Spectre.Console.Cli;

var serviceCollection = new ServiceCollection();
serviceCollection.AddSingleton<IFileSystem, FileSystem>();
serviceCollection.AddSingleton<IConsoleWriter, ConsoleWriter>();
serviceCollection.AddSingleton<ICraftsmanUtilities, CraftsmanUtilities>();
serviceCollection.AddSingleton<IScaffoldingDirectoryStore, ScaffoldingDirectoryStore>();
serviceCollection.AddSingleton<IDbMigrator, DbMigrator>();
serviceCollection.AddSingleton<IGitService, GitService>();
serviceCollection.AddCraftsmanBuildersAndModifiers(typeof(Program));
serviceCollection.AddAutoMapper(Assembly.GetExecutingAssembly());

var registrar = new TypeRegistrar(serviceCollection);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.SetApplicationName("craftsman");
    config.SetInterceptor(new OperatingSystemInterceptor());
    
    config.AddBranch("new", @new =>
    {
        @new.AddCommand<NewDomainCommand>("domain")
            .WithDescription("Scaffolds a project based on a given template file in a json or yaml format.")
            .WithExample(new [] { "new domain", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.yaml" })
            .WithExample(new [] { "new domain", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.yml" })
            .WithExample(new [] { "new domain", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.json" });

        @new.AddCommand<NewExampleCommand>("example")
            .WithDescription("Scaffolds out an example project via CLI prompts into the current directory.")
            .WithExample(new[] { "new example" })
            .WithExample(new[] { "new example", "MyProjectName" });
        
        // hidden commands for aliases, etc.
        @new.AddCommand<AddEntityCommand>("entity")
            .WithAlias("entities")
            .IsHidden();
        
        @new.AddCommand<AddFeatureCommand>("feature")
            .IsHidden();
    });

    config.AddBranch("add", @new =>
    {
        @new.AddCommand<AddEntityCommand>("entity")
            .WithAlias("entities")
            .WithDescription("Add one or more new entities to your Wrapt project using a formatted yaml or json file.")
            .WithExample(new [] { "add entity", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.yaml" })
            .WithExample(new [] { "add entity", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.yml" })
            .WithExample(new [] { "add entity", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.json" });

        @new.AddCommand<AddFeatureCommand>("feature")
            .WithDescription("Scaffolds out a new feature using CLI prompts.")
            .WithExample(new[] { "add feature" });

        @new.AddCommand<AddBusCommand>("bus")
            .WithDescription(
                "Adds a message bus to your web api and a messages directory to your shared kernel using a formatted yaml or " +
                "json file. A template file is optional.")
            .WithExample(new[] { "add bus" })
            .WithExample(new[]
                { "add bus", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.yaml" });

        @new.AddCommand<AddAuthServerCommand>("authserver")
            .WithDescription(
                "Adds a Duende based auth server project to your solution.")
            .WithExample(new [] { "add authserver", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.yaml" })
            .WithExample(new [] { "add authserver", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.yml" })
            .WithExample(new [] { "add authserver", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.json" });
    });
    
});


try
{
    app.Run(args);
}
catch (Exception e)
{
    if (e is ICraftsmanException)
        AnsiConsole.MarkupLine($"{e.Message}");
    else
    {
        AnsiConsole.WriteException(e, new ExceptionSettings
        {
            Format = ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks,
            Style = new ExceptionStyle
            {
                Exception = new Style().Foreground(Color.Grey),
                Message = new Style().Foreground(Color.White),
                NonEmphasized = new Style().Foreground(Color.Cornsilk1),
                Parenthesis = new Style().Foreground(Color.Cornsilk1),
                Method = new Style().Foreground(Color.Red),
                ParameterName = new Style().Foreground(Color.Cornsilk1),
                ParameterType = new Style().Foreground(Color.Red),
                Path = new Style().Foreground(Color.Red),
                LineNumber = new Style().Foreground(Color.Cornsilk1),
            }
        });
    }
}
finally
{
    VersionChecker.CheckForLatestVersion();
}

