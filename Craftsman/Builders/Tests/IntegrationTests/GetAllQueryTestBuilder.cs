namespace Craftsman.Builders.Tests.IntegrationTests;

using System;
using System.IO;
using Craftsman.Services;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class GetAllQueryTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public GetAllQueryTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string testDirectory, string srcDirectory, Entity entity, string projectBaseName,
        string permission, bool featureIsProtected)
    {
        var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"GetAll{entity.Plural}QueryTests.cs", entity.Plural, projectBaseName);
        var fileText = WriteTestFileText(testDirectory, srcDirectory, classPath, entity, projectBaseName, permission, featureIsProtected);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string testDirectory, string srcDirectory, ClassPath classPath,
        Entity entity, string projectBaseName, string permission, bool featureIsProtected)
    {
        var featureName = FileNames.GetEntityListFeatureClassName(entity.Name);
        var permissionTest = !featureIsProtected ? null : GetPermissionTest(entity.Plural, featureName, permission);

        var exceptionClassPath = ClassPathHelper.ExceptionsClassPath(testDirectory, "");
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(testDirectory, featureName, entity.Plural, projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {dtoClassPath.ClassNamespace};
using {fakerClassPath.ClassNamespace};
using {exceptionClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using FluentAssertions;
using Domain;
using Xunit;
using System.Threading.Tasks;

public class {classPath.ClassNameWithoutExt} : TestBase
{{
    {GetEntitiesTest(entity)}{permissionTest}
}}";
    }

    private static string GetEntitiesTest(Entity entity)
    {
        var queryName = FileNames.QueryAllName();
        var fakeEntityVariableNameOne = $"fake{entity.Name}One";
        var fakeEntityVariableNameTwo = $"fake{entity.Name}Two";
        var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();

        
        return @$"
    [Fact]
    public async Task can_get_all_{entity.Plural.ToLower()}()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        var {fakeEntityVariableNameOne} = new {FileNames.FakeBuilderName(entity.Name)}().Build();
        var {fakeEntityVariableNameTwo} = new {FileNames.FakeBuilderName(entity.Name)}().Build();

        await testingServiceScope.InsertAsync({fakeEntityVariableNameOne}, {fakeEntityVariableNameTwo});

        // Act
        var query = new {FileNames.GetAllEntitiesFeatureClassName(entity.Plural)}.{queryName}();
        var {lowercaseEntityPluralName} = await testingServiceScope.SendAsync(query);

        // Assert
        {lowercaseEntityPluralName}.Count.Should().BeGreaterThanOrEqualTo(2);
        {lowercaseEntityPluralName}.FirstOrDefault(x => x.Id == {fakeEntityVariableNameOne}.Id).Should().NotBeNull();
        {lowercaseEntityPluralName}.FirstOrDefault(x => x.Id == {fakeEntityVariableNameTwo}.Id).Should().NotBeNull();
    }}";
    }
    
    private static string GetPermissionTest(string entityPlural, string featureName, string permission)
    {
        var queryName = FileNames.QueryListName();
        return $@"

    [Fact]
    public async Task must_be_permitted()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        testingServiceScope.SetUserNotPermitted(Permissions.{permission});

        // Act
        var query = new {FileNames.GetAllEntitiesFeatureClassName(entityPlural)}.{queryName}();
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }}";
    }
}
