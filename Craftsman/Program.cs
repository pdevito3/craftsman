namespace Craftsman
{
    using Craftsman.Commands;
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
            
            if(args[0] == "list")
            {
                ListCommand.Run();
                return;
            }

            if (args.Length == 2 && (args[0] == "new:api"))
            {
                var filePath = args[1];
                if(filePath == "-h" || filePath == "--help")
                    ApiCommand.Help();
                else
                {
                    var solutionDir = myEnv == "Dev" ? @"C:\Users\Paul\Documents\testoutput" : Directory.GetCurrentDirectory();
                    ApiCommand.Run(filePath, solutionDir);
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

        }
    }
}
