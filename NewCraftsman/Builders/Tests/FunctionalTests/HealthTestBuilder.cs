namespace NewCraftsman.Builders.Tests.FunctionalTests
{
    using System.IO;
    using System.IO.Abstractions;

    public class HealthTestBuilder
    {
        public static void CreateTests(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
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
using FluentAssertions;
using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
{{
    {HealthTest()}
}}";
        }

        private static string HealthTest()
        {
            return $@"[Test]
    public async Task health_check_returns_ok()
    {{
        // Arrange
        // N/A

        // Act
        var result = await _client.GetRequestAsync(ApiRoutes.Health);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }}";
        }
    }
}