namespace Craftsman.Commands
{
    using CommandLine;
    using Craftsman.CraftsmanOptions;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
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

            if (args[0] == "list" || args[0] == "-h" || args[0] == "--help")
            {
                ListCommand.Run();
                return;
            }

            if (args.Length == 2 && (args[0] == "new:api"))
            {
                WriteHelpHeader($"This command has been depricated. If you'd like to create a new project, use the `new:domain` command. If you want to add a new bounded context to an existing project, use the `add:bc` command. Run `craftsman list` for a full list of commands.");
            }

            if (args.Length == 2 && (args[0] == "add:bc" || args[0] == "add:boundedcontext"))
            {
                var filePath = args[1];
                if (filePath == "-h" || filePath == "--help")
                    AddBoundedContextCommand.Help();
                else
                {
                    var solutionDir = myEnv == "Dev" ? fileSystem.Path.Combine(@"C:","Users","Paul","Documents","testoutput","LimsLite") : fileSystem.Directory.GetCurrentDirectory();
                    AddBoundedContextCommand.Run(filePath, solutionDir, fileSystem);
                }
            }

            if (args.Length == 2 && (args[0] == "new:domain"))
            {
                var filePath = args[1];
                if (filePath == "-h" || filePath == "--help")
                    AddBoundedContextCommand.Help();
                else
                {
                    var solutionDir = myEnv == "Dev" ? fileSystem.Path.Combine(@"C:", "Users", "Paul", "Documents", "testoutput") : fileSystem.Directory.GetCurrentDirectory();
                    NewDomainProjectCommand.Run(filePath, solutionDir, fileSystem);
                }
            }

            if (args.Length == 2 && (args[0] == "add:entity" || args[0] == "add:entities"))
            {
                var filePath = args[1];
                if (filePath == "-h" || filePath == "--help")
                    AddEntityCommand.Help();
                else
                {
                    var solutionDir = myEnv == "Dev" ? fileSystem.Path.Combine(@"C:","Users","Paul","Documents","testoutput","Lab.Api") : fileSystem.Directory.GetCurrentDirectory();
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

                    var solutionDir = myEnv == "Dev" ? fileSystem.Path.Combine(@"C:","Users","Paul","Documents","testoutput") : fileSystem.Directory.GetCurrentDirectory();
                    AddEntityPropertyCommand.Run(solutionDir, entityName, newProperty);
                }
            }
        }
    }
}
