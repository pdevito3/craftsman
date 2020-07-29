namespace Craftsman
{
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
            // -a or --api to scaffold an entire API with a required yaml arguement
            // -m or --model to create a model. some kind of dto command? to include or not (not by default)
            // -h or --help to console.writeline all the options
            // no arguments has a list of commands as well?

            // parse model: https://github.com/assyadh/cli/blob/6c9942bae6830a4f5b370d978d56b21807be49ba/src/dotnet/Program.cs
            // commands: https://github.com/assyadh/cli/tree/6c9942bae6830a4f5b370d978d56b21807be49ba/src/dotnet/commands

            if (args.Length == 0)
            {
                Console.WriteLine("TBD options description here");
                return;
            }

            var modelArgs = new string[] { "-m", "--model" };
            if (args.Length == 2 && (modelArgs.Contains(args[0])))
            {
                CreateModel(args[1]);
            }

            var Args = new string[] { "-a", "--api" };
            if (args.Length == 2 && (Args.Contains(args[0])))
            {
                //ReadYaml(args[1]);
                ReadYaml("");
            }

            //ShowBot(string.Join(' ', args));
        }

        public static ApiTemplate ReadYaml(string yamlFile)
        {
            yamlFile = $"C:\\Users\\Paul\\Documents\\repos\\Craftsman\\Craftsman\\Model.yml";
            //make sure file exists

            //make sure it's yaml


            var deserializer = new Deserializer();
            ApiTemplate templatefromYaml = deserializer.Deserialize<ApiTemplate>(File.ReadAllText(yamlFile));

            return templatefromYaml;
            /*var serializer = new SerializerBuilder().JsonCompatible().Build();
            var jsonObject = serializer.Serialize(yamlFile);*/

            //process object
        }

        public static void ReadJson(string jsonFile)
        {
            jsonFile = $"C:\\Users\\Paul\\Documents\\repos\\Craftsman\\Craftsman\\Model.json";
            //make sure file exists

            //make sure it's json



            var deserializedTemplate1 = JsonConvert.DeserializeObject<ApiTemplate>(File.ReadAllText(jsonFile));

            // deserialize JSON directly from a file
            using (StreamReader file = File.OpenText(jsonFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                var deserializedTemplate2 = (ApiTemplate)serializer.Deserialize(file, typeof(ApiTemplate));
            }
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
