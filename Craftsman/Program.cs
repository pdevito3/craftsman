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
            // un comment these and run `dotnet run` to cause an error and maybe fund a debug 
            /*Console.WriteLine(args[0]);
            Console.WriteLine(args[1]);*/

            // create a factory for handling arguements
            // -l, --list : for a list of all options and commands
            // -a, --api : to scaffold an entire API with a required yaml arguement
            // -e, --entity : to create an enity. 
            // -ed, --entitydto create an entity with dtos

            // -h, help after any command display help for that command
            // no arguments has a list of commands as well?

            // parse model: https://github.com/assyadh/cli/blob/6c9942bae6830a4f5b370d978d56b21807be49ba/src/dotnet/Program.cs
            // commands: https://github.com/assyadh/cli/tree/6c9942bae6830a4f5b370d978d56b21807be49ba/src/dotnet/commands

            if (args.Length == 0)
            {
                Console.WriteLine("TBD options description here");
                return;
            }

            /*var modelArgs = new string[] { "-e", "--entity" };
            if (args.Length == 2 && (modelArgs.Contains(args[0])))
            {
                CreateModel(args[1]);
            }*/

            var Args = new string[] { "-a", "--api" };
            if (args.Length == 2 && (Args.Contains(args[0])))
            {
                var filePath = args[1];
                if(filePath == "-h" || filePath == "--help")
                    ApiCommand.Help();
                else
                    ApiCommand.Run(filePath);
            }

            //ShowBot(string.Join(' ', args));
        }

        static void CreateModel(string modelName)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            string fileName = $"{modelName.UppercaseFirstLetter()}.cs";

            Console.WriteLine($"Current dir is {currentDirectory}");
            Console.WriteLine($"New model file will be {fileName}");
            var pathString = Path.Combine(currentDirectory, fileName);

            Console.WriteLine($"new file will be here: {pathString}");

            if (!File.Exists(pathString))
            {
                using (FileStream fs = File.Create(pathString))
                {
                    var data = GetEntityFile();
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }
            }
            else
            {
                Console.WriteLine("File \"{0}\" already exists.", fileName);
                return;
            }


        }

        private static string GetEntityFile()
        {
            var fields = "test";
            return @$"namespace Craftsman.FileTemplates
{{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Entity
    {{
        {fields}
    }}
}}";
        }

        private static string PropBuilder(string type, string name)
        {
            return $"public {type} name {{ get; set; }}";
        }

        static void ShowBot(string message)
        {
            string bot = $"\n        {message}";
            bot += @"
    __________________
                      \
                       \
                          ....
                          ....'
                           ....
                        ..........
                    .............'..'..
                 ................'..'.....
               .......'..........'..'..'....
              ........'..........'..'..'.....
             .'....'..'..........'..'.......'.
             .'..................'...   ......
             .  ......'.........         .....
             .    _            __        ......
            ..    #            ##        ......
           ....       .                 .......
           ......  .......          ............
            ................  ......................
            ........................'................
           ......................'..'......    .......
        .........................'..'.....       .......
     ........    ..'.............'..'....      ..........
   ..'..'...      ...............'.......      ..........
  ...'......     ...... ..........  ......         .......
 ...........   .......              ........        ......
.......        '...'.'.              '.'.'.'         ....
.......       .....'..               ..'.....
   ..       ..........               ..'........
          ............               ..............
         .............               '..............
        ...........'..              .'.'............
       ...............              .'.'.............
      .............'..               ..'..'...........
      ...............                 .'..............
       .........                        ..............
        .....
";
            Console.WriteLine(bot);
        }

    }
}
