using System.IO.Abstractions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
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

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    // this makes emojis come up more reliably. might get built into spectre better in the future, so give a go deleting this at some point
    // they seem to show up fine on osx and actually need this to be off to work there
    Console.OutputEncoding = Encoding.Unicode;
}

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
        
        @new.AddCommand<AddBoundedContextCommand>("bc")
            .WithAlias("boundedcontext")
            .WithDescription("Scaffolds a new bounded context for a Wrapt domain project based on a given template file in a json or yaml format.")
            .WithExample(new [] { "add bc", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.yaml" })
            .WithExample(new [] { "add bc", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.yml" })
            .WithExample(new [] { "add bc", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.json" });

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
                "Adds a Duende based auth server project to your solution using a formatted yaml or json file.")
            .WithExample(new [] { "add authserver", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.yaml" })
            .WithExample(new [] { "add authserver", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.yml" })
            .WithExample(new [] { "add authserver", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.json" });

        @new.AddCommand<AddBffCommand>("bff")
            .WithDescription(
                "Add a bff to your solution along with a React client using a formatted yaml or json file that describes the bff you want to add.")
            .WithExample(new [] { "add bff", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.yaml" })
            .WithExample(new [] { "add bff", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.yml" })
            .WithExample(new [] { "add bff", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.json" });

        @new.AddCommand<AddBffEntityCommand>("bffentity")
            .WithDescription(
                "Adds one or more new entities to your BFF client using a formatted yaml or json file.")
            .WithExample(new [] { "add bffentity", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.yaml" })
            .WithExample(new [] { "add bffentity", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.yml" })
            .WithExample(new [] { "add bffentity", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.json" });
    });
    
    config.AddBranch("register", @new =>
    {
        @new.AddCommand<RegisterProducerCommand>("producer")
            .WithDescription("Register a producer with MassTransit using CLI prompts. This is especially useful for adding a new publish action to an existing feature (e.g. EntityCreated).")
            .WithExample(new [] { "register producer", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.yaml" })
            .WithExample(new [] { "register producer", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.yml" })
            .WithExample(new [] { "register producer", $"my{Path.DirectorySeparatorChar}file{Path.DirectorySeparatorChar}path.json" });
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

