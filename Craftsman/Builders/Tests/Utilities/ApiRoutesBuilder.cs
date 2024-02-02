namespace Craftsman.Builders.Tests.Utilities;

using Helpers;
using Services;

public class ApiRoutesBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public ApiRoutesBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateClass(string testDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(testDirectory, projectBaseName, "ApiRoutes.cs");
        var fileText = GetBaseText(classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetBaseText(string classNamespace)
    {
        return @$"namespace {classNamespace};
public class ApiRoutes
{{
    public const string Base = ""api/v1"";
    public const string Health = Base + ""/health"";

    // new api route marker - do not delete
}}";
    }
}
