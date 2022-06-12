namespace Craftsman.Builders.Tests.FunctionalTests;

using System;
using System.IO;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class GetEntityRecordTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public GetEntityRecordTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, string testDirectory, Entity entity, bool isProtected, string projectBaseName)
    {
        var classPath = ClassPathHelper.FunctionalTestClassPath(testDirectory, $"Get{entity.Name}Tests.cs", entity.Plural, projectBaseName);
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
            {GetEntityTestUnauthorized(entity)}
            {GetEntityTestForbidden(entity)}" : "";

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {testUtilClassPath.ClassNamespace};{permissionsUsing}
using FluentAssertions;
using Xunit;
using System.Net;
using System.Threading.Tasks;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
{{
    {GetEntityTest(entity, isProtected)}{authOnlyTests}
}}";
    }

    private static string GetEntityTest(Entity entity, bool isProtected)
    {
        var fakeEntity = FileNames.FakerName(entity.Name);
        var fakeEntityVariableName = $"fake{entity.Name}";
        var pkName = Entity.PrimaryKeyProperty.Name;
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));

        var testName = $"get_{entity.Name.ToLower()}_returns_success_when_entity_exists";
        testName += isProtected ? "_using_valid_auth_credentials" : "";
        var clientAuth = isProtected ? @$"

        _client.AddAuth(new[] {{Roles.SuperAdmin}});" : "";

        return $@"[Fact]
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
        var fakeEntity = FileNames.FakerName(entity.Name);
        var fakeEntityVariableName = $"fake{entity.Name}";
        var pkName = Entity.PrimaryKeyProperty.Name;
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));

        return $@"
    [Fact]
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
        var fakeEntity = FileNames.FakerName(entity.Name);
        var fakeEntityVariableName = $"fake{entity.Name}";
        var pkName = Entity.PrimaryKeyProperty.Name;
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));

        return $@"
    [Fact]
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
