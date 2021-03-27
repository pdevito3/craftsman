namespace Craftsman.Commands
{
    using CommandLine;
    using Craftsman.CraftsmanOptions;
    using Craftsman.Enums;
    using Craftsman.Models;
    using Flurl.Http;
    using RestSharp;
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
                return;
            }

            if (args[0] == "version" || args[0] == "--version")
            {
                WriteHelpHeader($"v{typeof(ProcessCommand).Assembly.GetName().Version}");
                return;
            }

            if (args[0] == "list" || args[0] == "-h" || args[0] == "--help")
            {
                ListCommand.Run();
                return;
            }

            if (args.Length == 2 && (args[0] == "new:api"))
            {
                WriteHelpHeader($"This command has been depricated. If you'd like to create a new project, use the `new:domain` command. If you want to add a new bounded context to an existing project, use the `add:bc` command. Run `craftsman list` for a full list of commands.");
            }

            if (args.Length >= 2 && (args[0] == "add:bc" || args[0] == "add:boundedcontext"))
            {
                var filePath = args[1];
                var verbosity = Verbosity.Minimal;
                Parser.Default.ParseArguments<AddBcOptions>(args)
                    .WithParsed(options => verbosity = options.Verbosity ? Verbosity.More : Verbosity.Minimal);

                if (filePath == "-h" || filePath == "--help")
                    AddBoundedContextCommand.Help();
                else
                {
                    var solutionDir = myEnv == "Dev" ? fileSystem.Path.Combine(@"C:", "Users", "Paul", "Documents", "testoutput", "LimsLite") : fileSystem.Directory.GetCurrentDirectory();
                    AddBoundedContextCommand.Run(filePath, solutionDir, fileSystem, verbosity);
                }
            }

            if (args.Length >= 2 && (args[0] == "new:domain"))
            {
                var filePath = args[1];
                var verbosity = Verbosity.Minimal;
                Parser.Default.ParseArguments<NewDomainOptions>(args)
                    .WithParsed(options => verbosity = options.Verbosity ? Verbosity.More : Verbosity.Minimal);

                if (filePath == "-h" || filePath == "--help")
                    AddBoundedContextCommand.Help();
                else
                {
                    var solutionDir = myEnv == "Dev" ? fileSystem.Path.Combine(@"C:", "Users", "Paul", "Documents", "testoutput") : fileSystem.Directory.GetCurrentDirectory();
                    NewDomainProjectCommand.Run(filePath, solutionDir, fileSystem, verbosity);
                }
            }

            if (args.Length == 2 && (args[0] == "add:entity" || args[0] == "add:entities"))
            {
                var filePath = args[1];
                if (filePath == "-h" || filePath == "--help")
                    AddEntityCommand.Help();
                else
                {
                    var solutionDir = myEnv == "Dev" ? fileSystem.Path.Combine(@"C:", "Users", "Paul", "Documents", "testoutput", "Lab.Api") : fileSystem.Directory.GetCurrentDirectory();
                    AddEntityCommand.Run(filePath, solutionDir, fileSystem);
                }
            }

            if (args.Length > 1 && (args[0] == "add:property"))
            {
                if (args[1] == "-h" || args[1] == "--help")
                    AddEntityPropertyCommand.Help();
                else
                {
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

                    var solutionDir = myEnv == "Dev" ? fileSystem.Path.Combine(@"C:", "Users", "Paul", "Documents", "testoutput") : fileSystem.Directory.GetCurrentDirectory();
                    AddEntityPropertyCommand.Run(solutionDir, entityName, newProperty);
                }
            }

            CheckForLatestVersion();
        }

        private static void CheckForLatestVersion()
        {
            try
            {
                var installedVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                installedVersion = installedVersion[0..^2]; // equivalent to installedVersion.Substring(0, installedVersion.Length - 2);

                // not sure if/how to account for prerelease yet -- seems like with other repos, the logic github currently uses will redirect to the latest current
                //var isPrelease = installedVersion.IndexOf("-", StringComparison.Ordinal) > -1;

                //if (!isPrelease)
                //{
                    var client = new RestClient("https://github.com/pdevito3/craftsman/releases/latest");
                    var request = new RestRequest() { Method = Method.GET };
                    request.AddHeader("Accept", "text/html");
                    var todos = client.Execute(request);

                    var latestVersion = todos.ResponseUri.Segments.LastOrDefault().ToString();
                    if(latestVersion.FirstOrDefault() == 'v')
                        latestVersion = latestVersion[1..]; // remove the 'v' prefix. equivalent to `latest.Substring(1, latest.Length - 1)`

                    if (installedVersion != latestVersion)
                        WriteHelpHeader(@$"This Craftsman version '{installedVersion}' is older than that of the runtime '{latestVersion}'. Update the tools for the latest features and bug fixes (`dotnet tool update -g craftsman`).");
                //}
            }
            catch (Exception)
            {
                // fail silently
            }
        }


        private static Version CreateVersion(string semanticVersion)
        {
            var prereleaseIndex = semanticVersion.IndexOf("-", StringComparison.Ordinal);
            if (prereleaseIndex != -1)
            {
                semanticVersion = semanticVersion.Substring(0, prereleaseIndex);
            }

            return new Version(semanticVersion);
        }
    }
}
