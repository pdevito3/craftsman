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
        public static void CreateTests(string solutionDirectory, Entity entity, List<Policy> policies, Feature feature, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.FunctionalTestClassPath(solutionDirectory, $"{feature.Name}Tests.cs", entity.Name, projectBaseName);
            var fileText = WriteTestFileText(solutionDirectory, classPath, entity, policies, feature, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        private static string WriteTestFileText(string solutionDirectory, ClassPath classPath, Entity entity, List<Policy> policies, Feature feature, string projectBaseName)
        {
            var dtoUtilClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var testUtilClassPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(solutionDirectory, projectBaseName, "");
            var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var parentFakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", feature.ParentEntity, projectBaseName);

            var hasRestrictedEndpoints = policies.Count > 0;
            var authOnlyTests = hasRestrictedEndpoints ? $@"
            {CreateEntityTestUnauthorized(entity)}
            {CreateEntityTestForbidden(entity)}" : "";

            return @$"namespace {classPath.ClassNamespace}
{{
    using {dtoUtilClassPath.ClassNamespace};
    using {fakerClassPath.ClassNamespace};
    using {parentFakerClassPath.ClassNamespace};
    using {testUtilClassPath.ClassNamespace};
    using FluentAssertions;
    using NUnit.Framework;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
    {{
        {CreateEntityTest(entity, feature, hasRestrictedEndpoints, policies)}
        {NotFoundCreationTest(entity, feature, hasRestrictedEndpoints, policies)}
        {InvalidCreationTest(entity, feature, hasRestrictedEndpoints, policies)}{authOnlyTests}
    }}
}}";
        }

        private static string CreateEntityTest(Entity entity, Feature feature, bool hasRestrictedEndpoints, List<Policy> policies)
        {
            var createDto = Utilities.GetDtoName(entity.Name, Dto.Creation);
            var fakeEntityForCreation = $"Fake{createDto}";
            var fakeEntityVariableName = $"fake{entity.Name}List";
            var fakeParentEntity = $"fake{feature.ParentEntity}";

            var testName = $"{feature.Name}_Returns_Created_When_Valid";
            testName += hasRestrictedEndpoints ? "_WithAuth" : "";
            var scopes = Utilities.BuildTestAuthorizationString(policies, new List<Endpoint>() { Endpoint.AddRecord }, entity.Name, PolicyType.Scope);
            var clientAuth = hasRestrictedEndpoints ? @$"

            _client.AddAuth(new[] {scopes});" : "";

            return $@"[Test]
        public async Task {testName}()
        {{
            // Arrange
            var {fakeParentEntity} = new Fake{feature.ParentEntity}() {{ }}.Generate();
            await InsertAsync({fakeParentEntity});
            var {fakeEntityVariableName} = new List<{createDto}> {{new {fakeEntityForCreation} {{ }}.Generate()}};{clientAuth}

            // Act
            var route = ApiRoutes.{entity.Plural}.Create;
            var result = await _client.PostJsonRequestAsync($""{{route}}?{feature.ParentEntity.LowercaseFirstLetter()}={{{fakeParentEntity}.Id}}"", {fakeEntityVariableName});

            // Assert
            result.StatusCode.Should().Be(201);
        }}";
        }

        private static string NotFoundCreationTest(Entity entity, Feature feature, bool hasRestrictedEndpoints, List<Policy> policies)
        {
            var createDto = Utilities.GetDtoName(entity.Name, Dto.Creation);
            var fakeEntityForCreation = $"Fake{createDto}";
            var fakeEntityVariableName = $"fake{entity.Name}List";

            var testName = $"{feature.Name}_Returns_NotFound_When_Fk_Doesnt_Exist";
            testName += hasRestrictedEndpoints ? "_WithAuth" : "";
            var scopes = Utilities.BuildTestAuthorizationString(policies, new List<Endpoint>() { Endpoint.AddRecord }, entity.Name, PolicyType.Scope);
            var clientAuth = hasRestrictedEndpoints ? @$"

            _client.AddAuth(new[] {scopes});" : "";

            return $@"[Test]
        public async Task {testName}()
        {{
            // Arrange
            var {fakeEntityVariableName} = new List<{createDto}> {{new {fakeEntityForCreation} {{ }}.Generate()}};{clientAuth}

            // Act
            var route = ApiRoutes.{entity.Plural}.Create;
            var result = await _client.PostJsonRequestAsync($""{{route}}?{feature.ParentEntity.LowercaseFirstLetter()}={{Guid.NewGuid()}}"", {fakeEntityVariableName});

            // Assert
            result.StatusCode.Should().Be(404);
        }}";
        }

        private static string InvalidCreationTest(Entity entity, Feature feature, bool hasRestrictedEndpoints, List<Policy> policies)
        {
            var createDto = Utilities.GetDtoName(entity.Name, Dto.Creation);
            var fakeEntityForCreation = $"Fake{createDto}";
            var fakeEntityVariableName = $"fake{entity.Name}List";

            var testName = $"{feature.Name}_Returns_BadRequest_When_No_Fk_Param";
            testName += hasRestrictedEndpoints ? "_WithAuth" : "";
            var scopes = Utilities.BuildTestAuthorizationString(policies, new List<Endpoint>() { Endpoint.AddRecord }, entity.Name, PolicyType.Scope);
            var clientAuth = hasRestrictedEndpoints ? @$"

            _client.AddAuth(new[] {scopes});" : "";

            return $@"[Test]
        public async Task {testName}()
        {{
            // Arrange
            var {fakeEntityVariableName} = new List<{createDto}> {{new {fakeEntityForCreation} {{ }}.Generate()}};{clientAuth}

            // Act
            var result = await _client.PostJsonRequestAsync(ApiRoutes.{entity.Plural}.Create, {fakeEntityVariableName});

            // Assert
            result.StatusCode.Should().Be(400);
        }}";
        }

        private static string CreateEntityTestUnauthorized(Entity entity)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeEntityVariableName = $"fake{entity.Name}";

            return $@"
        [Test]
        public async Task Create_{entity.Name}_Returns_Unauthorized_Without_Valid_Token()
        {{
            // Arrange
            var {fakeEntityVariableName} = new {fakeEntity} {{ }}.Generate();

            await InsertAsync({fakeEntityVariableName});

            // Act
            var route = ApiRoutes.{entity.Plural}.Create;
            var result = await _client.PostJsonRequestAsync(route, {fakeEntityVariableName});

            // Assert
            result.StatusCode.Should().Be(401);
        }}";
        }

        private static string CreateEntityTestForbidden(Entity entity)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeEntityVariableName = $"fake{entity.Name}";

            return $@"
        [Test]
        public async Task Create_{entity.Name}_Returns_Forbidden_Without_Proper_Scope()
        {{
            // Arrange
            var {fakeEntityVariableName} = new {fakeEntity} {{ }}.Generate();
            _client.AddAuth();

            await InsertAsync({fakeEntityVariableName});

            // Act
            var route = ApiRoutes.{entity.Plural}.Create;
            var result = await _client.PostJsonRequestAsync(route, {fakeEntityVariableName});

            // Assert
            result.StatusCode.Should().Be(403);
        }}";
        }
    }
}