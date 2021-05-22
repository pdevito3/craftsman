namespace Craftsman.Commands
{
    using CommandLine;
    using Craftsman.CraftsmanOptions;
    using Craftsman.Enums;
    using Craftsman.Models;
    using RestSharp;
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Reflection;
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
                WriteInfo($"v{GetInstalledCraftsmanVersion()}");
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
                if (filePath == "-h" || filePath == "--help")
                    AddBoundedContextCommand.Help();
                else
                {
                    CheckForLatestVersion();
                    var verbosity = GetVerbosityFromArgs<AddBcOptions>(args);

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
                if (filePath == "-h" || filePath == "--help")
                    NewDomainProjectCommand.Help();
                else
                {
                    CheckForLatestVersion();
                    var verbosity = GetVerbosityFromArgs<NewDomainOptions>(args);

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
                if (filePath == "-h" || filePath == "--help")
                    AddEntityCommand.Help();
                else
                {
                    CheckForLatestVersion();
                    var verbosity = GetVerbosityFromArgs<AddEntityOptions>(args);

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

            if ((args[0] == "add:bus"))
            {
                var filePath = "";
                if (args.Length >= 2)
                    filePath = args[1];

                if (filePath == "-h" || filePath == "--help")
                    AddBusCommand.Help();
                else
                {
                    CheckForLatestVersion();

                    var rootDir = fileSystem.Directory.GetCurrentDirectory();
                    if (myEnv == "Dev")
                    {
                        Console.WriteLine("Enter the root directory.");
                        rootDir = Console.ReadLine();
                    }

                    AddBusCommand.Run(filePath, rootDir, fileSystem);
                }
            }

            if ((args[0] == "add:message"))
            {
                var filePath = args[1];
                if (filePath == "-h" || filePath == "--help")
                    AddMessageCommand.Help();
                else
                {
                    CheckForLatestVersion();

                    var rootDir = fileSystem.Directory.GetCurrentDirectory();
                    if (myEnv == "Dev")
                    {
                        Console.WriteLine("Enter the root directory.");
                        rootDir = Console.ReadLine();
                    }

                    AddMessageCommand.Run(filePath, rootDir, fileSystem);
                }
            }

            if ((args[0] == "register:consumer"))
            {
                var filePath = args[1];
                if (filePath == "-h" || filePath == "--help")
                    AddConsumerCommand.Help();
                else
                {
                    CheckForLatestVersion();

                    var rootDir = fileSystem.Directory.GetCurrentDirectory();
                    if (myEnv == "Dev")
                    {
                        Console.WriteLine("Enter the root directory.");
                        rootDir = Console.ReadLine();
                    }

                    AddConsumerCommand.Run(filePath, rootDir, fileSystem);
                }
            }

            if ((args[0] == "register:producer"))
            {
                var filePath = args[1];
                if (filePath == "-h" || filePath == "--help")
                    AddProducerCommand.Help();
                else
                {
                    CheckForLatestVersion();

                    var rootDir = fileSystem.Directory.GetCurrentDirectory();
                    if (myEnv == "Dev")
                    {
                        Console.WriteLine("Enter the root directory.");
                        rootDir = Console.ReadLine();
                    }

                    AddProducerCommand.Run(filePath, rootDir, fileSystem);
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