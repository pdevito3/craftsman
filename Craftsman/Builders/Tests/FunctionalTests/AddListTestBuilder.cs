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

    public class AddListTestBuilder
    {
        public static void CreateTests(string srcDirectory, Entity entity, Feature feature, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.FunctionalTestClassPath(srcDirectory, $"{feature.Name}Tests.cs", entity.Name, projectBaseName);
            var fileText = WriteTestFileText(srcDirectory, classPath, entity, feature.IsProtected, feature, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        private static string WriteTestFileText(string srcDirectory, ClassPath classPath, Entity entity, bool hasRestrictedEndpoints, Feature feature, string projectBaseName)
        {
            var dtoUtilClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Name, projectBaseName);
            var testUtilClassPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(srcDirectory, projectBaseName, "");
            var fakerClassPath = ClassPathHelper.TestFakesClassPath(srcDirectory, "", entity.Name, projectBaseName);
            var parentFakerClassPath = ClassPathHelper.TestFakesClassPath(srcDirectory, "", feature.ParentEntity, projectBaseName);
            var permissionsClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);

            var authOnlyTests = hasRestrictedEndpoints ? $@"
            {CreateEntityTestUnauthorized(entity)}
            {CreateEntityTestForbidden(entity)}" : "";

            return @$"namespace {classPath.ClassNamespace};

using {dtoUtilClassPath.ClassNamespace};
using {fakerClassPath.ClassNamespace};
using {parentFakerClassPath.ClassNamespace};
using {testUtilClassPath.ClassNamespace};
using {permissionsClassPath.ClassNamespace};
using FluentAssertions;
using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
{{
    {CreateEntityTest(entity, feature, hasRestrictedEndpoints)}
    {NotFoundCreationTest(entity, feature, hasRestrictedEndpoints)}
    {InvalidCreationTest(entity, feature, hasRestrictedEndpoints)}{authOnlyTests}
}}";
        }

        private static string CreateEntityTest(Entity entity, Feature feature, bool hasRestrictedEndpoints)
        {
            var createDto = Utilities.GetDtoName(entity.Name, Dto.Creation);
            var fakeEntityForCreation = $"Fake{createDto}";
            var fakeEntityVariableName = $"fake{entity.Name}List";
            var fakeParentEntity = $"fake{feature.ParentEntity}";
            var fakeParentCreationDto = Utilities.FakerName(Utilities.GetDtoName(feature.ParentEntity, Dto.Creation));

            var testName = $"create_{entity.Name.ToLower()}_list_returns_created_using_valid_dto";
            testName += hasRestrictedEndpoints ? "_and_valid_auth_credentials" : "";
            var clientAuth = hasRestrictedEndpoints ? @$"

        _client.AddAuth(new[] {{Permissions.SuperAdmin}});" : "";

            return $@"[Test]
    public async Task {testName}()
    {{
        // Arrange
        var {fakeParentEntity} = Fake{feature.ParentEntity}.Generate(new {fakeParentCreationDto}().Generate());
        await InsertAsync({fakeParentEntity});
        var {fakeEntityVariableName} = new List<{createDto}> {{new {fakeEntityForCreation} {{ }}.Generate()}};{clientAuth}

        // Act
        var route = ApiRoutes.{entity.Plural}.CreateBatch;
        var result = await _client.PostJsonRequestAsync($""{{route}}?{feature.BatchPropertyName.ToLower()}={{{fakeParentEntity}.Id}}"", {fakeEntityVariableName});

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }}";
        }

        private static string NotFoundCreationTest(Entity entity, Feature feature, bool hasRestrictedEndpoints)
        {
            var createDto = Utilities.GetDtoName(entity.Name, Dto.Creation);
            var fakeEntityForCreation = $"Fake{createDto}";
            var fakeEntityVariableName = $"fake{entity.Name}List";

            var testName = $"create_{entity.Name.ToLower()}_list_returns_notfound_when_fk_doesnt_exist";
            testName += hasRestrictedEndpoints ? "_and_valid_auth_credentials" : "";
            var clientAuth = hasRestrictedEndpoints ? @$"

        _client.AddAuth(new[] {{Permissions.SuperAdmin}});" : "";

            return $@"[Test]
    public async Task {testName}()
    {{
        // Arrange
        var {fakeEntityVariableName} = new List<{createDto}> {{new {fakeEntityForCreation} {{ }}.Generate()}};{clientAuth}

        // Act
        var route = ApiRoutes.{entity.Plural}.CreateBatch;
        var result = await _client.PostJsonRequestAsync($""{{route}}?{feature.BatchPropertyName.ToLower()}={{Guid.NewGuid()}}"", {fakeEntityVariableName});

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }}";
        }

        private static string InvalidCreationTest(Entity entity, Feature feature, bool hasRestrictedEndpoints)
        {
            var createDto = Utilities.GetDtoName(entity.Name, Dto.Creation);
            var fakeEntityForCreation = $"Fake{createDto}";
            var fakeEntityVariableName = $"fake{entity.Name}List";

            var testName = $"create_{entity.Name.ToLower()}_list_returns_badrequest_when_no_fk_param";
            testName += hasRestrictedEndpoints ? "_and_valid_auth_credentials" : "";
            var clientAuth = hasRestrictedEndpoints ? @$"

        _client.AddAuth(new[] {{Permissions.SuperAdmin}});" : "";

            return $@"[Test]
    public async Task {testName}()
    {{
        // Arrange
        var {fakeEntityVariableName} = new List<{createDto}> {{new {fakeEntityForCreation} {{ }}.Generate()}};{clientAuth}

        // Act
        var result = await _client.PostJsonRequestAsync(ApiRoutes.{entity.Plural}.CreateBatch, {fakeEntityVariableName});

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }}";
        }

        private static string CreateEntityTestUnauthorized(Entity entity)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeEntityVariableName = $"fake{entity.Name}";

            return $@"
    [Test]
    public async Task create_{entity.Name.ToLower()}_list_returns_unauthorized_without_valid_token()
    {{
        // Arrange
        var {fakeEntityVariableName} = new {fakeEntity} {{ }}.Generate();

        await InsertAsync({fakeEntityVariableName});

        // Act
        var route = ApiRoutes.{entity.Plural}.CreateBatch;
        var result = await _client.PostJsonRequestAsync(route, {fakeEntityVariableName});

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }}";
        }

        private static string CreateEntityTestForbidden(Entity entity)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeEntityVariableName = $"fake{entity.Name}";

            return $@"
    [Test]
    public async Task create_{entity.Name.ToLower()}_list_returns_forbidden_without_proper_scope()
    {{
        // Arrange
        var {fakeEntityVariableName} = new {fakeEntity} {{ }}.Generate();
        _client.AddAuth();

        await InsertAsync({fakeEntityVariableName});

        // Act
        var route = ApiRoutes.{entity.Plural}.CreateBatch;
        var result = await _client.PostJsonRequestAsync(route, {fakeEntityVariableName});

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }}";
        }
    }
}