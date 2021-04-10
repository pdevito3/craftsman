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
        public static T GetTemplateFromFile<T>(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            if (ext == ".yml" || ext == ".yaml")
                return ReadYaml<T>(filePath);
            else
                return ReadJson<T>(filePath);
        }

        public static T ReadYaml<T>(string yamlFile)
        {
            var deserializer = new Deserializer();
            T templatefromYaml = deserializer.Deserialize<T>(File.ReadAllText(yamlFile));

            return templatefromYaml;
        }

        public static T ReadJson<T>(string jsonFile)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(jsonFile));
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

        public static void RunPrimaryKeyGuard(List<Entity> entities)
        {
            if (entities.Where(e => e.PrimaryKeyProperty == null).ToList().Count > 0)
                throw new MissingPrimaryKeyException("One of your entity properties is missing a primary key designation. " +
                    "Please make sure you have an `IsPrimaryKey: true` option on whichever property you want to be used as your prmary key.");
        }

        public static void RunSolutionNameAssignedGuard(string solutionName)
        {
            if (solutionName == null || solutionName.Length <= 0)
                throw new InvalidSolutionNameException();
        }

        public static void SolutionNameDoesNotEqualEntityGuard(string solutionName, List<Entity> entities)
        {
            if(entities.Where(e => e.Name == solutionName).ToList().Count > 0 
                || entities.Where(e => e.Plural == solutionName).ToList().Count > 0
            )
                throw new SolutiuonNameEntityMatchException();
        }
    }
}
