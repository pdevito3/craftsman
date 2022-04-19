namespace NewCraftsman.Builders
{
    using System.IO.Abstractions;
    using RestSharp.Serialization.Json;

    public class ExampleTemplateBuilder
    {
        public static void CreateFile(string solutionDirectory, DomainProject domainProject, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.ExampleYamlRootClassPath(solutionDirectory, "exampleTemplate.json");
            var fileText = GetTemplateText(domainProject);
            _utilities.CreateFile(classPath, fileText);
        }
        
        public static void CreateYamlFile(string solutionDirectory, string domainProject, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.ExampleYamlRootClassPath(solutionDirectory, "exampleTemplate.yaml");
            Utilities.CreateFile(classPath, domainProject, fileSystem);
        }

        public static string GetTemplateText(DomainProject domainProject)
        {
            var serializer = new JsonSerializer();
            var templateFromDomain = serializer.Serialize(domainProject);

            return templateFromDomain;
        }
    }
}