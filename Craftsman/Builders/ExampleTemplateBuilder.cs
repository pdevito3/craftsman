namespace Craftsman.Builders;

using Domain;
using Helpers;
using RestSharp.Serialization.Json;
using Services;

public class ExampleTemplateBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public ExampleTemplateBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateFile(string solutionDirectory, DomainProject domainProject)
    {
        var classPath = ClassPathHelper.ExampleYamlRootClassPath(solutionDirectory, "exampleTemplate.json");
        var fileText = GetTemplateText(domainProject);
        _utilities.CreateFile(classPath, fileText);
    }

    public void CreateYamlFile(string solutionDirectory, string domainProject)
    {
        var classPath = ClassPathHelper.ExampleYamlRootClassPath(solutionDirectory, "exampleTemplate.yaml");
        _utilities.CreateFile(classPath, domainProject);
    }

    public static string GetTemplateText(DomainProject domainProject)
    {
        var serializer = new JsonSerializer();
        var templateFromDomain = serializer.Serialize(domainProject);

        return templateFromDomain;
    }
}
