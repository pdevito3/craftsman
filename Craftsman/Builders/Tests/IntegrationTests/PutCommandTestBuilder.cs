namespace Craftsman.Builders.Tests.IntegrationTests;

using Craftsman.Services;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class PutCommandTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public PutCommandTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, Entity entity, string projectBaseName)
    {
        var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"Update{entity.Name}CommandTests.cs", entity.Plural, projectBaseName);
        var fileText = WriteTestFileText(solutionDirectory, testDirectory, srcDirectory, classPath, entity, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string solutionDirectory, string testDirectory, string srcDirectory, ClassPath classPath, Entity entity, string projectBaseName)
    {
        var featureName = FileNames.UpdateEntityFeatureClassName(entity.Name);
        var testFixtureName = FileNames.GetIntegrationTestFixtureName();
        var commandName = FileNames.CommandUpdateName(entity.Name);
        var fakeEntity = FileNames.FakerName(entity.Name);
        var fakeUpdateDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Update));
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));
        var fakeEntityVariableName = $"fake{entity.Name}One";
        var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
        var pkName = Entity.PrimaryKeyProperty.Name;
        var lowercaseEntityPk = pkName.LowercaseFirstLetter();

        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, featureName, entity.Plural, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, projectBaseName);

        var fakeParent = IntegrationTestServices.FakeParentTestHelpers(entity, out var fakeParentIdRuleFor);
        var foreignEntityUsings = CraftsmanUtilities.GetForeignEntityUsings(testDirectory, entity, projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using static {testFixtureName};{foreignEntityUsings}

public class {commandName}Tests : TestBase
{{
    [Fact]
    public async Task can_update_existing_{entity.Name.ToLower()}_in_db()
    {{
        // Arrange
        {fakeParent}var {fakeEntityVariableName} = {fakeEntity}.Generate(new {fakeCreationDto}(){fakeParentIdRuleFor}.Generate());
        var updated{entity.Name}Dto = new {fakeUpdateDto}(){fakeParentIdRuleFor}.Generate();
        await InsertAsync({fakeEntityVariableName});

        var {lowercaseEntityName} = await ExecuteDbContextAsync(db => db.{entity.Plural}
            .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.Id == {fakeEntityVariableName}.Id));
        var {lowercaseEntityPk} = {lowercaseEntityName}.{pkName};

        // Act
        var command = new {featureName}.{commandName}({lowercaseEntityPk}, updated{entity.Name}Dto);
        await SendAsync(command);
        var updated{entity.Name} = await ExecuteDbContextAsync(db => db.{entity.Plural}.FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.{pkName} == {lowercaseEntityPk}));

        // Assert
        updated{entity.Name}.Should().BeEquivalentTo(updated{entity.Name}Dto, options =>
            options.ExcludingMissingMembers());
    }}
}}";
    }
}
