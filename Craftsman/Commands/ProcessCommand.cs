namespace Craftsman.Commands
{
    using CommandLine;
    using Craftsman.CraftsmanOptions;
    using Craftsman.Enums;
    using Craftsman.Models;
    using RestSharp;
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class ProcessCommand
    {
        private readonly IFileSystem fileSystem = new FileSystem();

        public void Run(string[] args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // this makes emojis come up more reliably. might get built into spectre better in the future, so give a go deleting this at some point
                // they seem to show up fine on osx and actually need this to be off to work there
                Console.OutputEncoding = Encoding.Unicode;
            }
            
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

            if (args.Length >= 2 && (args[0] == "add:authserver"))
            {
                var filePath = args[1];
                if (filePath == "-h" || filePath == "--help")
                    AddAuthServerCommand.Help();
                else
                {
                    CheckForLatestVersion();

                    var rootDir = fileSystem.Directory.GetCurrentDirectory();
                    if (myEnv == "Dev")
                    {
                        Console.WriteLine("Enter the root directory.");
                        rootDir = Console.ReadLine();
                    }
                    AddAuthServerCommand.Run(filePath, rootDir, fileSystem);
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
            
            if ((args[0] == "new:example" || args[0] == "example"))
            {
                if (args.Length > 1 && (args[1] == "-h" || args[1] == "--help"))
                    NewExampleCommand.Help();
                else
                {
                    CheckForLatestVersion();

                    var rootDir = fileSystem.Directory.GetCurrentDirectory();
                    if (myEnv == "Dev")
                    {
                        Console.WriteLine("Enter the root directory.");
                        rootDir = Console.ReadLine();
                    }
                    NewExampleCommand.Run(rootDir, fileSystem);
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

            if ((args[0] == "add:consumer" || args[0] == "add:consumers"))
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

            if ((args[0] == "add:producer" || args[0] == "add:producers"))
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

            if (args[0] == "add:feature" || args[0] == "new:feature")
            {
                if (args.Length > 1)
                {
                    var filePath = args[1];
                    if (filePath is "-h" or "--help")
                        AddFeatureCommand.Help();

                    return;
                }

                CheckForLatestVersion();

                var rootDir = fileSystem.Directory.GetCurrentDirectory();
                if (myEnv == "Dev")
                {
                    Console.WriteLine("Enter the root directory.");
                    rootDir = Console.ReadLine();
                }
                AddFeatureCommand.Run(rootDir, fileSystem);
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