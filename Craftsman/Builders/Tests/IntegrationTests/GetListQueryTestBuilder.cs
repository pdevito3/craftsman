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

    public void CreateTests(string testDirectory, string srcDirectory, Entity entity, string projectBaseName, bool overwrite = false)
    {
        var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"{entity.Name}ListQueryTests.cs", entity.Plural, projectBaseName);
        var fileText = WriteTestFileText(testDirectory, srcDirectory, classPath, entity, projectBaseName);
        _utilities.CreateFile(classPath, fileText, overwrite);
    }

    private static string WriteTestFileText(string testDirectory, string srcDirectory, ClassPath classPath, Entity entity, string projectBaseName)
    {
        var featureName = FileNames.GetEntityListFeatureClassName(entity.Name);
        var testFixtureName = FileNames.GetIntegrationTestFixtureName();

        var exceptionClassPath = ClassPathHelper.ExceptionsClassPath(testDirectory, "");
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(testDirectory, featureName, entity.Plural, projectBaseName);

        var foreignEntityUsings = CraftsmanUtilities.GetForeignEntityUsings(testDirectory, entity, projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {dtoClassPath.ClassNamespace};
using {fakerClassPath.ClassNamespace};
using {exceptionClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using static {testFixtureName};{foreignEntityUsings}

public class {classPath.ClassNameWithoutExt} : TestBase
{{
    {GetEntitiesTest(entity)}
}}";
    }

    private static string GetEntitiesTest(Entity entity)
    {
        var queryName = FileNames.QueryListName();
        var fakeEntity = FileNames.FakerName(entity.Name);
        var entityParams = FileNames.GetDtoName(entity.Name, Dto.ReadParamaters);
        var fakeEntityVariableNameOne = $"fake{entity.Name}One";
        var fakeEntityVariableNameTwo = $"fake{entity.Name}Two";
        var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));

        var fakeParent = IntegrationTestServices.FakeParentTestHelpersTwoCount(entity, out var fakeParentIdRuleForOne, out var fakeParentIdRuleForTwo);
        return @$"
    [Test]
    public async Task can_get_{entity.Name.ToLower()}_list()
    {{
        // Arrange
        {fakeParent}var {fakeEntityVariableNameOne} = {fakeEntity}.Generate(new {fakeCreationDto}(){fakeParentIdRuleForOne}.Generate());
        var {fakeEntityVariableNameTwo} = {fakeEntity}.Generate(new {fakeCreationDto}(){fakeParentIdRuleForTwo}.Generate());
        var queryParameters = new {entityParams}();

        await InsertAsync({fakeEntityVariableNameOne}, {fakeEntityVariableNameTwo});

        // Act
        var query = new {FileNames.GetEntityListFeatureClassName(entity.Name)}.{queryName}(queryParameters);
        var {lowercaseEntityPluralName} = await SendAsync(query);

        // Assert
        {lowercaseEntityPluralName}.Count.Should().BeGreaterThanOrEqualTo(2);
    }}";
    }
}
