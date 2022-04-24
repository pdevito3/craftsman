namespace Craftsman.Builders.Tests.FunctionalTests;

using System;
using System.IO;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class CreateEntityTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public CreateEntityTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, string testDirectory, Entity entity, bool isProtected, string projectBaseName)
    {
        var classPath = ClassPathHelper.FunctionalTestClassPath(testDirectory, $"Create{entity.Name}Tests.cs", entity.Plural, projectBaseName);
        var fileText = WriteTestFileText(solutionDirectory, testDirectory, classPath, entity, isProtected, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string solutionDirectory, string testDirectory, ClassPath classPath, Entity entity, bool isProtected, string projectBaseName)
    {
        var testUtilClassPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(testDirectory, projectBaseName, "");
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var permissionsClassPath = ClassPathHelper.PolicyDomainClassPath(testDirectory, "", projectBaseName);
        var rolesClassPath = ClassPathHelper.SharedKernelDomainClassPath(solutionDirectory, "");

        var permissionsUsing = isProtected
            ? $"{Environment.NewLine}using {permissionsClassPath.ClassNamespace};{Environment.NewLine}using {rolesClassPath.ClassNamespace};"
            : string.Empty;

        var authOnlyTests = isProtected ? $@"
            {CreateEntityTestUnauthorized(entity)}
            {CreateEntityTestForbidden(entity)}" : "";

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {testUtilClassPath.ClassNamespace};{permissionsUsing}
using FluentAssertions;
using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
{{
    {CreateEntityTest(entity, isProtected)}{authOnlyTests}
}}";
    }

    private static string CreateEntityTest(Entity entity, bool isProtected)
    {
        var fakeEntityForCreation = $"Fake{FileNames.GetDtoName(entity.Name, Dto.Creation)}";
        var fakeEntityVariableName = $"fake{entity.Name}";
        var pkName = Entity.PrimaryKeyProperty.Name;

        var testName = $"create_{entity.Name.ToLower()}_returns_created_using_valid_dto";
        testName += isProtected ? "_and_valid_auth_credentials" : "";
        var clientAuth = isProtected ? @$"

        _client.AddAuth(new[] {{Roles.SuperAdmin}});" : "";

        return $@"[Test]
    public async Task {testName}()
    {{
        // Arrange
        var {fakeEntityVariableName} = new {fakeEntityForCreation} {{ }}.Generate();{clientAuth}

        // Act
        var route = ApiRoutes.{entity.Plural}.Create;
        var result = await _client.PostJsonRequestAsync(route, {fakeEntityVariableName});

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }}";
    }

    private static string CreateEntityTestUnauthorized(Entity entity)
    {
        var fakeEntity = FileNames.FakerName(entity.Name);
        var fakeEntityVariableName = $"fake{entity.Name}";
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));

        return $@"
    [Test]
    public async Task create_{entity.Name.ToLower()}_returns_unauthorized_without_valid_token()
    {{
        // Arrange
        var {fakeEntityVariableName} = {fakeEntity}.Generate(new {fakeCreationDto}().Generate());

        await InsertAsync({fakeEntityVariableName});

        // Act
        var route = ApiRoutes.{entity.Plural}.Create;
        var result = await _client.PostJsonRequestAsync(route, {fakeEntityVariableName});

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }}";
    }

    private static string CreateEntityTestForbidden(Entity entity)
    {
        var fakeEntity = FileNames.FakerName(entity.Name);
        var fakeEntityVariableName = $"fake{entity.Name}";
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));

        return $@"
    [Test]
    public async Task create_{entity.Name.ToLower()}_returns_forbidden_without_proper_scope()
    {{
        // Arrange
        var {fakeEntityVariableName} = {fakeEntity}.Generate(new {fakeCreationDto}().Generate());
        _client.AddAuth();

        await InsertAsync({fakeEntityVariableName});

        // Act
        var route = ApiRoutes.{entity.Plural}.Create;
        var result = await _client.PostJsonRequestAsync(route, {fakeEntityVariableName});

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }}";
    }
}
