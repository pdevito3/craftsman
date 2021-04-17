namespace Craftsman.Commands
{
    using CommandLine;
    using Craftsman.CraftsmanOptions;
    using Craftsman.Enums;
    using Craftsman.Models;
    using RestSharp;
    using Spectre.Console;
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using static Helpers.ConsoleWriter;

    public class ProcessCommand
    {
        private readonly IFileSystem fileSystem = new FileSystem();

        public void Run(string[] args)
        {
            var myEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (args.Length == 0)
            {
                ListCommand.Run();
                CheckForLatestVersion();
                return;
            }

            if (args[0] == "version" || args[0] == "--version")
            {
                AnsiConsole.Markup($"[khaki1]v{GetInstalledCraftsmanVersion()}[/]");
                CheckForLatestVersion();
                return;
            }

            if (args[0] == "list" || args[0] == "-h" || args[0] == "--help")
            {
                ListCommand.Run();
                CheckForLatestVersion();
                return;
            }

            if (args.Length >= 2 && (args[0] == "add:bc" || args[0] == "add:boundedcontext"))
            {
                var filePath = args[1];
                var verbosity = GetVerbosityFromArgs<AddBcOptions>(args);

                if (filePath == "-h" || filePath == "--help")
                    AddBoundedContextCommand.Help();
                else
                {
                    CheckForLatestVersion();

                    var rootDir = fileSystem.Directory.GetCurrentDirectory();
                    if (myEnv == "Dev")
                    {
                        Console.WriteLine("Enter the root directory.");
                        rootDir = Console.ReadLine();
                    }
                    AddBoundedContextCommand.Run(filePath, rootDir, fileSystem, verbosity);
                }
            }

            if (args.Length >= 2 && (args[0] == "new:domain"))
            {
                var filePath = args[1];
                var verbosity = GetVerbosityFromArgs<AddBcOptions>(args);

                if (filePath == "-h" || filePath == "--help")
                    AddBoundedContextCommand.Help();
                else
                {
                    CheckForLatestVersion();

                    var rootDir = fileSystem.Directory.GetCurrentDirectory();
                    if (myEnv == "Dev")
                    {
                        Console.WriteLine("Enter the root directory.");
                        rootDir = Console.ReadLine();
                    }
                    NewDomainProjectCommand.Run(filePath, rootDir, fileSystem, verbosity);
                }
            }

            if (args.Length == 2 && (args[0] == "add:entity" || args[0] == "add:entities"))
            {
                var filePath = args[1];
                var verbosity = GetVerbosityFromArgs<AddEntityOptions>(args);

                if (filePath == "-h" || filePath == "--help")
                    AddEntityCommand.Help();
                else
                {
                    CheckForLatestVersion();

                    var solutionDir = fileSystem.Directory.GetCurrentDirectory();
                    if (myEnv == "Dev")
                    {
                        Console.WriteLine("Enter the solution directory.");
                        solutionDir = Console.ReadLine();
                    }

                    AddEntityCommand.Run(filePath, solutionDir, fileSystem, verbosity);
                }
            }

            if (args.Length > 1 && (args[0] == "add:property" || args[0] == "add:prop"))
            {
                if (args[1] == "-h" || args[1] == "--help")
                    AddEntityPropertyCommand.Help();
                else
                {
                    CheckForLatestVersion();

                    var entityName = "";
                    var newProperty = new EntityProperty();
                    Parser.Default.ParseArguments<AddPropertyOptions>(args)
                        .WithParsed(options =>
                        {
                            entityName = options.Entity.UppercaseFirstLetter();
                            newProperty = new EntityProperty()
                            {
                                Name = options.Name,
                                Type = options.Type,
                                CanFilter = options.CanFilter,
                                CanSort = options.CanSort,
                                ForeignKeyPropName = options.ForeignKeyPropName
                            };
                        });

                    var solutionDir = fileSystem.Directory.GetCurrentDirectory();
                    if (myEnv == "Dev")
                    {
                        Console.WriteLine("Enter the solution directory.");
                        solutionDir = Console.ReadLine();
                    }
                    AddEntityPropertyCommand.Run(solutionDir, entityName, newProperty);
                }
            }
        }

        private static Verbosity GetVerbosityFromArgs<TOptions>(string[] args)
            where TOptions : IVerbosable
        {
            var verbosity = Verbosity.Minimal;
            Parser.Default.ParseArguments<TOptions>(args)
                .WithParsed(options => verbosity = options.Verbosity ? Verbosity.More : Verbosity.Minimal);

            return verbosity;
        }

        private static void CheckForLatestVersion()
        {
            try
            {
                var installedVersion = GetInstalledCraftsmanVersion();

                // not sure if/how to account for prerelease yet -- seems like with other repos, the logic github currently uses will redirect to the latest current
                //var isPrelease = installedVersion.IndexOf("-", StringComparison.Ordinal) > -1;

                //if (!isPrelease)
                //{
                var client = new RestClient("https://github.com/pdevito3/craftsman/releases/latest");
                var request = new RestRequest() { Method = Method.GET };
                request.AddHeader("Accept", "text/html");
                var todos = client.Execute(request);

                var latestVersion = todos.ResponseUri.Segments.LastOrDefault().ToString();
                if (latestVersion.FirstOrDefault() == 'v')
                    latestVersion = latestVersion[1..]; // remove the 'v' prefix. equivalent to `latest.Substring(1, latest.Length - 1)`

                if (installedVersion != latestVersion)
                    WriteHelpHeader(@$"{Environment.NewLine}This Craftsman version '{installedVersion}' is older than that of the runtime '{latestVersion}'. Update the tools for the latest features and bug fixes (`dotnet tool update -g craftsman`).{Environment.NewLine}");
                //}
            }
            catch (Exception)
            {
                // fail silently
            }
        }

        private static string GetInstalledCraftsmanVersion()
        {
            var installedVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            installedVersion = installedVersion[0..^2]; // equivalent to installedVersion.Substring(0, installedVersion.Length - 2);

            return installedVersion;
        }
    }
}