namespace Craftsman.Helpers
{
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using Domain;
    using Exceptions;
    using Newtonsoft.Json;
    using Services;
    using YamlDotNet.Serialization;

    public interface IFileParsingHelper : ICraftsmanService
    {
        bool RunInitialTemplateParsingGuards(string filePath);
    }

    public class FileParsingHelper : IFileParsingHelper
    {
        private readonly IFileSystem _fileSystem;

        public FileParsingHelper(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        
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

        public static T ReadYamlString<T>(string yamlString)
        {
            var deserializer = new Deserializer();
            T templatefromYaml = deserializer.Deserialize<T>(yamlString);

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

        public bool RunInitialTemplateParsingGuards(string filePath)
        {
            if (!_fileSystem.File.Exists(filePath))
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
            if (entities.Where(e => Entity.PrimaryKeyProperty == null).ToList().Count > 0)
                throw new MissingPrimaryKeyException("One of your entity properties is missing a primary key designation. " +
                    "Please make sure you have an `IsPrimaryKey: true` option on whichever property you want to be used as your prmary key.");
        }

        public static void RunSolutionNameAssignedGuard(string projectName)
        {
            if (projectName == null || projectName.Length <= 0)
                throw new InvalidSolutionNameException();
        }

        public static void SolutionNameDoesNotEqualEntityGuard(string projectName, List<Entity> entities)
        {
            if(entities.Where(e => e.Name == projectName).ToList().Count > 0 
                || entities.Where(e => e.Plural == projectName).ToList().Count > 0
            )
                throw new SolutiuonNameEntityMatchException();
        }
    }
}
