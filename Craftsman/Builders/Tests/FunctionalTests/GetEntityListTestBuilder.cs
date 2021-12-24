namespace Craftsman.Builders.Tests.FunctionalTests
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class GetEntityListTestBuilder
    {
        public static void CreateTests(string solutionDirectory, Entity entity, bool isProtected, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.FunctionalTestClassPath(solutionDirectory, $"Get{entity.Name}ListTests.cs", entity.Name, projectBaseName);
            var fileText = WriteTestFileText(solutionDirectory, classPath, entity, isProtected, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        private static string WriteTestFileText(string solutionDirectory, ClassPath classPath, Entity entity, bool isProtected, string projectBaseName)
        {
            var testUtilClassPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(solutionDirectory, projectBaseName, "");
            var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", entity.Name, projectBaseName);

            var hasRestrictedEndpoints = isProtected;
            var authOnlyTests = hasRestrictedEndpoints ? $@"
            {GetEntityTestUnauthorized(entity)}
            {GetEntityTestForbidden(entity)}" : "";

            return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {testUtilClassPath.ClassNamespace};
using FluentAssertions;
using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
{{
    {GetEntityTest(entity, hasRestrictedEndpoints)}{authOnlyTests}
}}";
        }

        private static string GetEntityTest(Entity entity, bool hasRestrictedEndpoints)
        {
            var testName = $"get_{entity.Name.ToLower()}_list_returns_success";
            testName += hasRestrictedEndpoints ? "_using_valid_auth_credentials" : "";
            var clientAuth = hasRestrictedEndpoints ? @$"

        _client.AddAuth(new[] {{Permissions.SuperAdmin}});" : "";

            return $@"[Test]
    public async Task {testName}()
    {{
        // Arrange
        {clientAuth ?? "// N/A"}

        // Act
        var result = await _client.GetRequestAsync(ApiRoutes.{entity.Plural}.GetList);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }}";
        }

        private static string GetEntityTestUnauthorized(Entity entity)
        {
            return $@"
    [Test]
    public async Task get_{entity.Name.ToLower()}_list_returns_unauthorized_without_valid_token()
    {{
        // Arrange
        // N/A

        // Act
        var result = await _client.GetRequestAsync(ApiRoutes.{entity.Plural}.GetList);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }}";
        }

        private static string GetEntityTestForbidden(Entity entity)
        {
            return $@"
    [Test]
    public async Task get_{entity.Name.ToLower()}_list_returns_forbidden_without_proper_scope()
    {{
        // Arrange
        _client.AddAuth();

        // Act
        var result = await _client.GetRequestAsync(ApiRoutes.{entity.Plural}.GetList);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }}";
        }
    }
}