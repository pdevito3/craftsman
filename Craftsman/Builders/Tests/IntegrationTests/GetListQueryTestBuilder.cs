namespace Craftsman.Builders.Tests.IntegrationTests;

using System;
using System.IO;
using Craftsman.Services;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class GetListQueryTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public GetListQueryTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string testDirectory, string srcDirectory, Entity entity, string projectBaseName,
        string permission, bool featureIsProtected)
    {
        var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"{entity.Name}ListQueryTests.cs", entity.Plural, projectBaseName);
        var fileText = WriteTestFileText(testDirectory, srcDirectory, classPath, entity, projectBaseName, permission, featureIsProtected);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string testDirectory, string srcDirectory, ClassPath classPath,
        Entity entity, string projectBaseName, string permission, bool featureIsProtected)
    {
        var featureName = FileNames.GetEntityListFeatureClassName(entity.Name);
        var permissionTest = !featureIsProtected ? null : GetPermissionTest(entity.Name, featureName, permission);

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
        var queryName = FileNames.QueryListName();
        var entityParams = FileNames.GetDtoName(entity.Name, Dto.ReadParamaters);
        var fakeEntityVariableNameOne = $"fake{entity.Name}One";
        var fakeEntityVariableNameTwo = $"fake{entity.Name}Two";
        var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();

        
        return @$"
    [Fact]
    public async Task can_get_{entity.Name.ToLower()}_list()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        var {fakeEntityVariableNameOne} = new {FileNames.FakeBuilderName(entity.Name)}().Build();
        var {fakeEntityVariableNameTwo} = new {FileNames.FakeBuilderName(entity.Name)}().Build();
        var queryParameters = new {entityParams}();

        await testingServiceScope.InsertAsync({fakeEntityVariableNameOne}, {fakeEntityVariableNameTwo});

        // Act
        var query = new {FileNames.GetEntityListFeatureClassName(entity.Name)}.{queryName}(queryParameters);
        var {lowercaseEntityPluralName} = await testingServiceScope.SendAsync(query);

        // Assert
        {lowercaseEntityPluralName}.Count.Should().BeGreaterThanOrEqualTo(2);
    }}";
    }
    
    private static string GetPermissionTest(string entityName, string featureName, string permission)
    {
        var queryName = FileNames.QueryListName();
        var entityParams = FileNames.GetDtoName(entityName, Dto.ReadParamaters);
        
        return $@"

    [Fact]
    public async Task must_be_permitted()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        testingServiceScope.SetUserNotPermitted(Permissions.{permission});
        var queryParameters = new {entityParams}();

        // Act
        var command = new {featureName}.{queryName}(queryParameters);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }}";
    }
}
