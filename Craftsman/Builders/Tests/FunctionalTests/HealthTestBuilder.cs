namespace Craftsman.Builders.Tests.FunctionalTests;

using System.IO;
using Helpers;
using Services;

public class HealthTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public HealthTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.FunctionalTestClassPath(solutionDirectory, $"HealthCheckTests.cs", "HealthChecks", projectBaseName);
        var fileText = WriteTestFileText(solutionDirectory, classPath, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string solutionDirectory, ClassPath classPath, string projectBaseName)
    {
        var testUtilClassPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(solutionDirectory, projectBaseName, "");

        return @$"namespace {classPath.ClassNamespace};

using {testUtilClassPath.ClassNamespace};
using System.Net;
using System.Threading.Tasks;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
{{
    {HealthTest()}
}}";
    }

    private static string HealthTest()
    {
        return $@"[Fact]
    public async Task health_check_returns_ok()
    {{
        // Arrange
        // N/A

        // Act
        var result = await FactoryClient.GetRequestAsync(ApiRoutes.Health);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }}";
    }
}
