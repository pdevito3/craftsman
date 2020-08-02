namespace Craftsman.Commands
{
    using Craftsman.Exceptions;
    using Craftsman.Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using YamlDotNet.Serialization;
    using static ConsoleWriter;

    public static class ApiCommand
    {
        public static void Help()
        {
            WriteHelpHeader(@$"Description:");
            WriteHelpText(@$"   Scaffolds out API files and projects based on a given template file in a json or yaml format.{Environment.NewLine}");

            WriteHelpHeader(@$"Usage:");
            WriteHelpText(@$"   craftsman new:api [options] <filepath>{Environment.NewLine}");

            WriteHelpText(@$"   For example:");
            WriteHelpText(@$"       craftsman new:api C:\fullpath\api.yaml");
            WriteHelpText(@$"       craftsman new:api C:\fullpath\api.yml");
            WriteHelpText(@$"       craftsman new:api C:\fullpath\api.json{Environment.NewLine}");

            WriteHelpHeader(@$"Options:");
            WriteHelpText(@$"   -h, --help          Display this help message. No filepath is needed to display the help message.");
        }

        public static void Run(string filePath)
        {
            try
            {
                RunInitialGuards(filePath);
                var template = GetApiTemplateFromFile(filePath);
                WriteInfo($"The template file was parsed successfully.");

                // create projects

                var rootProjectDirectory = Directory.GetCurrentDirectory();
                foreach (var entity in template.Entities)
                {
                    CreateEntityCommand.CreateEntity(rootProjectDirectory, entity);
                }

                WriteInfo($"The API command was successfully completed.");
            }
            catch (FileNotFoundException)
            {
                WriteError($"The file `{filePath}` does not exist. Please enter a valid file path.");
            }
            catch (InvalidFileTypeException)
            {
                WriteError($"Invalid file type. You need to use a json or yml file.");
            }
            catch(Exception e)
            {
                ConsoleWriter.WriteError($"An unhandled exception occured when running the API command.\nThe error details are: \n{e.Message}");
            }
        }

        private static ApiTemplate GetApiTemplateFromFile(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            if (ext == ".yml" || ext == ".yaml")
                return ReadYaml(filePath);
            else
                return ReadJson(filePath);
        }

        public static ApiTemplate ReadYaml(string yamlFile)
        {
            var deserializer = new Deserializer();
            ApiTemplate templatefromYaml = deserializer.Deserialize<ApiTemplate>(File.ReadAllText(yamlFile));

            return templatefromYaml;
        }
        public static ApiTemplate ReadJson(string jsonFile)
        {
            return JsonConvert.DeserializeObject<ApiTemplate>(File.ReadAllText(jsonFile));

            // deserialize JSON directly from a file
            /*using (StreamReader file = File.OpenText(jsonFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                var deserializedTemplate2 = (ApiTemplate)serializer.Deserialize(file, typeof(ApiTemplate));
            }*/
        }

        public static bool RunInitialGuards(string filePath)
        {

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            if (!IsJsonOrYaml(filePath))
            {
                //TODO: add link to docs for examples
                throw new InvalidFileTypeException();
            }

            return true;
        }

        public static bool IsJsonOrYaml(string filePath)
        {
            var validExtensions = new string[] { ".json", ".yaml", ".yml" };
            return validExtensions.Contains(Path.GetExtension(filePath));
        }
    }
}
