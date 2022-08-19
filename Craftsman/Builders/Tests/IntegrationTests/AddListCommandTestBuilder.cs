namespace Craftsman.Builders.Tests.IntegrationTests;

using Craftsman.Services;
using Domain;
using Domain.Enums;
using Helpers;

public class AddListCommandTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public AddListCommandTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, Entity entity, Feature feature, string projectBaseName)
    {
        var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"AddList{entity.Name}CommandTests.cs", entity.Plural, projectBaseName);
        var fileText = WriteTestFileText(solutionDirectory, testDirectory, srcDirectory, classPath, entity, feature, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string solutionDirectory, string testDirectory, string srcDirectory, ClassPath classPath, Entity entity, Feature feature, string projectBaseName)
    {
        var featureName = FileNames.AddEntityFeatureClassName(entity.Name);
        var testFixtureName = FileNames.GetIntegrationTestFixtureName();
        var commandName = feature.Command;

        var dtoUtilClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(testDirectory, "");
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var parentFakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", feature.ParentEntity, projectBaseName);
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, featureName, entity.Plural, projectBaseName);

        var foreignEntityUsings = CraftsmanUtilities.GetForeignEntityUsings(testDirectory, entity, projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {dtoUtilClassPath.ClassNamespace};
using {fakerClassPath.ClassNamespace};
using {parentFakerClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Threading.Tasks;
using static {testFixtureName};{foreignEntityUsings}

public class {classPath.ClassNameWithoutExt} : TestBase
{{
    {GetAddListCommandTest(entity, feature)}
}}";
    }

    private static string GetAddListCommandTest(Entity entity, Feature feature)
    {
        var createDto = FileNames.GetDtoName(entity.Name, Dto.Creation);
        var fakeCreationDto = $"Fake{createDto}";
        var fakeEntityVariableNameOne = $"fake{entity.Name}One";
        var fakeEntityVariableNameTwo = $"fake{entity.Name}Two";
        var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
        var fakeParentEntity = $"fake{feature.ParentEntity}";
        var fakeParentCreationDto = FileNames.FakerName(FileNames.GetDtoName(feature.ParentEntity, Dto.Creation));

        return $@"[Test]
    public async Task can_add_new_{entity.Name.ToLower()}_list_to_db()
    {{
        // Arrange
        var {fakeParentEntity} = Fake{feature.ParentEntity}.Generate(new {fakeParentCreationDto}().Generate());
        await InsertAsync({fakeParentEntity});
        var {fakeEntityVariableNameOne} = new {fakeCreationDto}().Generate();
        var {fakeEntityVariableNameTwo} = new {fakeCreationDto}().Generate();

        // Act
        var command = new {feature.Name}.{feature.Command}(new List<{createDto}>() {{{fakeEntityVariableNameOne}, {fakeEntityVariableNameTwo}}}, {fakeParentEntity}.Id);
        var {lowercaseEntityName}Returned = await SendAsync(command);
        var {lowercaseEntityName}Db = await ExecuteDbContextAsync(db => db.{entity.Plural}.ToListAsync());

        // Assert
        {lowercaseEntityName}Returned.Should().ContainEquivalentOf({fakeEntityVariableNameOne}, options =>
            options.ExcludingMissingMembers());
        {lowercaseEntityName}Db.Should().ContainEquivalentOf({fakeEntityVariableNameOne}, options =>
            options.ExcludingMissingMembers());

        {lowercaseEntityName}Returned.Should().ContainEquivalentOf({fakeEntityVariableNameTwo}, options =>
            options.ExcludingMissingMembers());
        {lowercaseEntityName}Db.Should().ContainEquivalentOf({fakeEntityVariableNameTwo}, options =>
            options.ExcludingMissingMembers());
    }}";
    }
}
