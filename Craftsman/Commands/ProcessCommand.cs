namespace Craftsman.Commands
{
    using CommandLine;
    using Craftsman.CraftsmanOptions;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;

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

            if (args.Length == 2 && (args[0] == "new:api" || args[0] == "new:webapi"))
            {
                var filePath = args[1];
                if (filePath == "-h" || filePath == "--help")
                    NewApiCommand.Help();
                else
                {
                    var solutionDir = myEnv == "Dev" ? fileSystem.Path.Combine(@"C:","Users","Paul","Documents","testoutput") : fileSystem.Directory.GetCurrentDirectory();
                    NewApiCommand.Run(filePath, solutionDir, fileSystem);
                }
            }

            if (args.Length == 2 && (args[0] == "new:micro" || args[0] == "new:microservice"))
            {
                var filePath = args[1];
                if (filePath == "-h" || filePath == "--help")
                    NewMicroCommand.Help();
                else
                {
                    var solutionDir = myEnv == "Dev" ? fileSystem.Path.Combine(@"C:", "Users", "Paul", "Documents", "testoutput") : fileSystem.Directory.GetCurrentDirectory();
                    NewMicroCommand.Run(filePath, solutionDir, fileSystem);
                }
            }

            if (args.Length == 2 && (args[0] == "add:entity" || args[0] == "add:entities"))
            {
                var filePath = args[1];
                if (filePath == "-h" || filePath == "--help")
                    AddEntityCommand.Help();
                else
                {
                    var solutionDir = myEnv == "Dev" ? fileSystem.Path.Combine(@"C:","Users","Paul","Documents","testoutput") : fileSystem.Directory.GetCurrentDirectory();
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
