namespace Craftsman.Builders.Tests.FunctionalTests
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class GetEntityListTestBuilder
    {
        public static void CreateTests(string solutionDirectory, Entity entity, List<Policy> policies, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.FunctionalTestClassPath(solutionDirectory, $"Get{entity.Name}ListTests.cs", entity.Name, projectBaseName);
            var fileText = WriteTestFileText(solutionDirectory, classPath, entity, policies, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        private static string WriteTestFileText(string solutionDirectory, ClassPath classPath, Entity entity, List<Policy> policies, string projectBaseName)
        {
            var testUtilClassPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(solutionDirectory, projectBaseName, "");
            var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", entity.Name, projectBaseName);

            var hasRestrictedEndpoints = policies.Count > 0;
            var authOnlyTests = hasRestrictedEndpoints ? $@"
            {GetEntityTestUnauthorized(entity)}
            {GetEntityTestForbidden(entity)}" : "";

            return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {testUtilClassPath.ClassNamespace};
using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
{{
    {GetEntityTest(entity, hasRestrictedEndpoints, policies)}{authOnlyTests}
}}";
        }

        private static string GetEntityTest(Entity entity, bool hasRestrictedEndpoints, List<Policy> policies)
        {
            var testName = $"get_{entity.Name.ToLower()}_list_returns_success";
            testName += hasRestrictedEndpoints ? "_using_valid_auth_credentials" : "";
            var scopes = Utilities.BuildTestAuthorizationString(policies, new List<Endpoint>() { Endpoint.GetList }, entity.Name, PolicyType.Scope);
            var clientAuth = hasRestrictedEndpoints ? @$"_client.AddAuth(new[] {scopes});" : null;

            return $@"[Test]
        public async Task {testName}()
        {{
            // Arrange
            {clientAuth ?? "// N/A"}

            // Act
            var result = await _client.GetRequestAsync(ApiRoutes.{entity.Plural}.GetList);

            // Assert
            result.StatusCode.Should().Be(200);
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
            result.StatusCode.Should().Be(401);
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
            result.StatusCode.Should().Be(403);
        }}";
        }
    }
}