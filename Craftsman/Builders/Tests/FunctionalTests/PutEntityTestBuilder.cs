namespace Craftsman.Builders.Tests.FunctionalTests
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class PutEntityTestBuilder
    {
        public static void CreateTests(string solutionDirectory, Entity entity, List<Policy> policies, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.FunctionalTestClassPath(solutionDirectory, $"Update{entity.Name}RecordTests.cs", entity.Name, projectBaseName);
            var fileText = WriteTestFileText(solutionDirectory, classPath, entity, policies, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        private static string WriteTestFileText(string solutionDirectory, ClassPath classPath, Entity entity, List<Policy> policies, string projectBaseName)
        {
            var testUtilClassPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(solutionDirectory, projectBaseName, "");
            var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", entity.Name, projectBaseName);

            var hasRestrictedEndpoints = policies.Count > 0;
            var authOnlyTests = hasRestrictedEndpoints ? $@"
            {EntityTestUnauthorized(entity)}
            {EntityTestForbidden(entity)}" : "";

            return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {testUtilClassPath.ClassNamespace};
using FluentAssertions;
using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
{{
    {PutEntityTest(entity, hasRestrictedEndpoints, policies)}{authOnlyTests}
}}";
        }

        private static string PutEntityTest(Entity entity, bool hasRestrictedEndpoints, List<Policy> policies)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeUpdateDto = Utilities.FakerName(Utilities.GetDtoName(entity.Name, Dto.Update));
            var fakeEntityVariableName = $"fake{entity.Name}";
            var fakeDtoVariableName = $"updated{entity.Name}Dto";
            var pkName = Entity.PrimaryKeyProperty.Name;

            var testName = $"put_{entity.Name.ToLower()}_returns_nocontent_when_entity_exists";
            testName += hasRestrictedEndpoints ? "_and_auth_credentials_are_valid" : "";
            var scopes = Utilities.BuildTestAuthorizationString(policies, new List<Endpoint>() { Endpoint.UpdateRecord }, entity.Name, PolicyType.Scope);
            var clientAuth = hasRestrictedEndpoints ? @$"

            _client.AddAuth(new[] {scopes});
            " : "";

            return $@"[Test]
    public async Task {testName}()
    {{
        // Arrange
        var {fakeEntityVariableName} = new {fakeEntity} {{ }}.Generate();
        var {fakeDtoVariableName} = new {fakeUpdateDto} {{ }}.Generate();{clientAuth}
        await InsertAsync({fakeEntityVariableName});

        // Act
        var route = ApiRoutes.{Utilities.GetApiRouteClass(entity.Plural)}.Put.Replace(ApiRoutes.{Utilities.GetApiRouteClass(entity.Plural)}.{pkName}, {fakeEntityVariableName}.{pkName}.ToString());
        var result = await _client.PutJsonRequestAsync(route, {fakeDtoVariableName});

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }}";
        }

        private static string EntityTestUnauthorized(Entity entity)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeUpdateDto = Utilities.FakerName(Utilities.GetDtoName(entity.Name, Dto.Update));
            var fakeEntityVariableName = $"fake{entity.Name}";
            var fakeDtoVariableName = $"updated{entity.Name}Dto";
            var pkName = Entity.PrimaryKeyProperty.Name;

            return $@"
    [Test]
    public async Task put_{entity.Name.ToLower()}_returns_unauthorized_without_valid_token()
    {{
        // Arrange
        var {fakeEntityVariableName} = new {fakeEntity} {{ }}.Generate();
        var {fakeDtoVariableName} = new {fakeUpdateDto} {{ }}.Generate();

        await InsertAsync({fakeEntityVariableName});

        // Act
        var route = ApiRoutes.{Utilities.GetApiRouteClass(entity.Plural)}.Put.Replace(ApiRoutes.{Utilities.GetApiRouteClass(entity.Plural)}.{pkName}, {fakeEntityVariableName}.{pkName}.ToString());
        var result = await _client.PutJsonRequestAsync(route, {fakeDtoVariableName});

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }}";
        }

        private static string EntityTestForbidden(Entity entity)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeUpdateDto = Utilities.FakerName(Utilities.GetDtoName(entity.Name, Dto.Update));
            var fakeEntityVariableName = $"fake{entity.Name}";
            var fakeDtoVariableName = $"updated{entity.Name}Dto";
            var pkName = Entity.PrimaryKeyProperty.Name;

            return $@"
    [Test]
    public async Task put_{entity.Name.ToLower()}_returns_forbidden_without_proper_scope()
    {{
        // Arrange
        var {fakeEntityVariableName} = new {fakeEntity} {{ }}.Generate();
        var {fakeDtoVariableName} = new {fakeUpdateDto} {{ }}.Generate();
        _client.AddAuth();

        await InsertAsync({fakeEntityVariableName});

        // Act
        var route = ApiRoutes.{Utilities.GetApiRouteClass(entity.Plural)}.Put.Replace(ApiRoutes.{Utilities.GetApiRouteClass(entity.Plural)}.{pkName}, {fakeEntityVariableName}.{pkName}.ToString());
        var result = await _client.PutJsonRequestAsync(route, {fakeDtoVariableName});

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }}";
        }
    }
}