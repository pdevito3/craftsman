namespace Craftsman.Helpers
{
    using Craftsman.Exceptions;
    using Craftsman.Models;
    using FluentAssertions.Common;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text;
    using YamlDotNet.Serialization;

    public class FileParsingHelper
    {
        public static ApiTemplate GetApiTemplateFromFile(string filePath)
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
        }

        public static bool IsJsonOrYaml(string filePath)
        {
            var validExtensions = new string[] { ".json", ".yaml", ".yml" };
            return validExtensions.Contains(Path.GetExtension(filePath));
        }

        public static bool RunInitialTemplateParsingGuards(string filePath)
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

        public static void RunPrimaryKeyGuard(ApiTemplate template)
        {
            if (template.Entities.Where(e => e.PrimaryKeyProperty == null).ToList().Count > 0)
                throw new MissingPrimaryKeyException("One of your entity properties is missing a primary key designation. " +
                    "Please make sure you have an `IsPrimaryKey: true` option on whichever property you want to be used as your prmary key.");
        }

        public static void RunSolutionNameAssignedGuard(ApiTemplate template)
        {
            if (template.SolutionName == null || template.SolutionName.Length <= 0)
                throw new InvalidSolutionNameException();
        }
    }
}
