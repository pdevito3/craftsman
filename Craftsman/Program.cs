namespace Craftsman
{
    using CommandLine;
    using Craftsman.Commands;
    using Craftsman.CraftsmanOptions;
    using Craftsman.Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using YamlDotNet.RepresentationModel;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;


    class Program
    {
        static void Main(string[] args)
        {
            var myEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (args.Length == 0)
            {
                ListCommand.Run();
                return;
            }
            
            if(args[0] == "list" || args[0] == "-h" || args[0] == "--help")
            {
                ListCommand.Run();
                return;
            }

            if (args.Length == 2 && (args[0] == "new:api"))
            {
                var filePath = args[1];
                if(filePath == "-h" || filePath == "--help")
                    NewApiCommand.Help();
                else
                {
                    var solutionDir = myEnv == "Dev" ? @"C:\Users\Paul\Documents\testoutput" : Directory.GetCurrentDirectory();
                    NewApiCommand.Run(filePath, solutionDir);
                }
            }

            if (args.Length == 2 && (args[0] == "add:entity"))
            {
                var filePath = args[1];
                if (filePath == "-h" || filePath == "--help")
                    AddEntityCommand.Help();
                else
                {
                    var solutionDir = myEnv == "Dev" ? @"C:\Users\Paul\Documents\testoutput\MyApi.Mine" : Directory.GetCurrentDirectory();
                    AddEntityCommand.Run(filePath, solutionDir);
                }
            }

            if (args.Length > 1 && (args[0] == "add:property"))
            {
                if (args[1] == "-h" || args[1] == "--help")
                    AddEntityCommand.Help();
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

                    var solutionDir = myEnv == "Dev" ? @"C:\Users\Paul\Documents\testoutput\MyApi.Mine" : Directory.GetCurrentDirectory();
                    AddEntityPropertyCommand.Run(solutionDir,entityName,newProperty);
                }
            }
        }
    }
}
