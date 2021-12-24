namespace Craftsman.Builders.Tests.FunctionalTests
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;
    using Enums;

    public class GetEntityRecordTestBuilder
    {
        public static void CreateTests(string solutionDirectory, Entity entity, bool isProtected, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.FunctionalTestClassPath(solutionDirectory, $"Get{entity.Name}Tests.cs", entity.Name, projectBaseName);
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
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeEntityVariableName = $"fake{entity.Name}";
            var pkName = Entity.PrimaryKeyProperty.Name;
            var fakeCreationDto = Utilities.FakerName(Utilities.GetDtoName(entity.Name, Dto.Creation));

            var testName = $"get_{entity.Name.ToLower()}_returns_success_when_entity_exists";
            testName += hasRestrictedEndpoints ? "_using_valid_auth_credentials" : "";
            var clientAuth = hasRestrictedEndpoints ? @$"

        _client.AddAuth(new[] {{Permissions.SuperAdmin}});" : "";

            return $@"[Test]
    public async Task {testName}()
    {{
        // Arrange
        var {fakeEntityVariableName} = {fakeEntity}.Generate(new {fakeCreationDto}().Generate());{clientAuth}
        await InsertAsync({fakeEntityVariableName});

        // Act
        var route = ApiRoutes.{entity.Plural}.GetRecord.Replace(ApiRoutes.{entity.Plural}.{pkName}, {fakeEntityVariableName}.{pkName}.ToString());
        var result = await _client.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }}";
        }

        private static string GetEntityTestUnauthorized(Entity entity)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeEntityVariableName = $"fake{entity.Name}";
            var pkName = Entity.PrimaryKeyProperty.Name;
            var fakeCreationDto = Utilities.FakerName(Utilities.GetDtoName(entity.Name, Dto.Creation));

            return $@"
    [Test]
    public async Task get_{entity.Name.ToLower()}_returns_unauthorized_without_valid_token()
    {{
        // Arrange
        var {fakeEntityVariableName} = {fakeEntity}.Generate(new {fakeCreationDto}().Generate());

        await InsertAsync({fakeEntityVariableName});

        // Act
        var route = ApiRoutes.{entity.Plural}.GetRecord.Replace(ApiRoutes.{entity.Plural}.{pkName}, {fakeEntityVariableName}.{pkName}.ToString());
        var result = await _client.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }}";
        }

        private static string GetEntityTestForbidden(Entity entity)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeEntityVariableName = $"fake{entity.Name}";
            var pkName = Entity.PrimaryKeyProperty.Name;
            var fakeCreationDto = Utilities.FakerName(Utilities.GetDtoName(entity.Name, Dto.Creation));

            return $@"
    [Test]
    public async Task get_{entity.Name.ToLower()}_returns_forbidden_without_proper_scope()
    {{
        // Arrange
        var {fakeEntityVariableName} = {fakeEntity}.Generate(new {fakeCreationDto}().Generate());
        _client.AddAuth();

        await InsertAsync({fakeEntityVariableName});

        // Act
        var route = ApiRoutes.{entity.Plural}.GetRecord.Replace(ApiRoutes.{entity.Plural}.{pkName}, {fakeEntityVariableName}.{pkName}.ToString());
        var result = await _client.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }}";
        }
    }
}