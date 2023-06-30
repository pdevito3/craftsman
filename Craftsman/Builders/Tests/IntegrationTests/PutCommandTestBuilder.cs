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

    public void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, Entity entity,
        string projectBaseName, bool featureIsProtected, string permission)
    {
        var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"Update{entity.Name}CommandTests.cs", entity.Plural, projectBaseName);
        var fileText = WriteTestFileText(solutionDirectory, testDirectory, srcDirectory, classPath, entity, projectBaseName, featureIsProtected, permission);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string solutionDirectory, string testDirectory, string srcDirectory,
        ClassPath classPath, Entity entity, string projectBaseName, bool featureIsProtected, string permission)
    {
        var featureName = FileNames.UpdateEntityFeatureClassName(entity.Name);
        var commandName = FileNames.CommandUpdateName();
        var fakeEntity = FileNames.FakerName(entity.Name);
        var fakeUpdateDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Update));
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));
        var fakeEntityVariableName = $"fake{entity.Name}One";
        var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
        var pkName = Entity.PrimaryKeyProperty.Name;

        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, featureName, entity.Plural, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, projectBaseName);

        var fakeParent = IntegrationTestServices.FakeParentTestHelpersForBuilders(entity, out var fakeParentIdRuleFor);
        if (fakeParentIdRuleFor != "")
            fakeParentIdRuleFor += $"{Environment.NewLine}            ";
        IntegrationTestServices.FakeParentTestHelpersForUpdateDto(entity, out var fakeParentForUpdateDtoIdRuleFor);
        if (fakeParentForUpdateDtoIdRuleFor != "")
            fakeParentForUpdateDtoIdRuleFor += $"{Environment.NewLine}            ";

        var foreignEntityUsings = CraftsmanUtilities.GetForeignEntityUsings(testDirectory, entity, projectBaseName);
        var permissionTest = !featureIsProtected ? null : GetPermissionTest(commandName, entity, featureName, permission);

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using Domain;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Threading.Tasks;{foreignEntityUsings}

public class {classPath.ClassNameWithoutExt} : TestBase
{{
    [Fact]
    public async Task can_update_existing_{entity.Name.ToLower()}_in_db()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        {fakeParent}var {fakeEntityVariableName} = new {FileNames.FakeBuilderName(entity.Name)}(){fakeParentIdRuleFor}.Build();
        var updated{entity.Name}Dto = new {fakeUpdateDto}(){fakeParentForUpdateDtoIdRuleFor}.Generate();
        await testingServiceScope.InsertAsync({fakeEntityVariableName});

        var {lowercaseEntityName} = await testingServiceScope.ExecuteDbContextAsync(db => db.{entity.Plural}
            .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.Id == {fakeEntityVariableName}.Id));

        // Act
        var command = new {featureName}.{commandName}({lowercaseEntityName}.{pkName}, updated{entity.Name}Dto);
        await testingServiceScope.SendAsync(command);
        var updated{entity.Name} = await testingServiceScope.ExecuteDbContextAsync(db => db.{entity.Plural}.FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.{pkName} == {lowercaseEntityName}.{pkName}));

        // Assert{GetAssertions(entity.Properties, entity.Name)}
    }}{permissionTest}
}}";
    }

    private static string GetPermissionTest(string commandName, Entity entity, string featureName, string permission)
    {
        var fakeUpdateDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Update));
        var fakeEntityVariableName = $"fake{entity.Name}One";

        return $@"

    [Fact]
    public async Task must_be_permitted()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        testingServiceScope.SetUserNotPermitted(Permissions.{permission});
        var {fakeEntityVariableName} = new {fakeUpdateDto}();

        // Act
        var command = new {featureName}.{commandName}(Guid.NewGuid(), {fakeEntityVariableName});
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }}";
    }

    private static string GetAssertions(List<EntityProperty> properties, string entityName)
    {
        var entityAssertions = "";
        foreach (var entityProperty in properties.Where(x => x.IsPrimitiveType))
        {
            entityAssertions += entityProperty.Type switch
            {
                "DateTime" or "DateTimeOffset" or "TimeOnly" =>
                    $@"{Environment.NewLine}        updated{entityName}.{entityProperty.Name}.Should().BeCloseTo(updated{entityName}Dto.{entityProperty.Name}, 1.Seconds());",
                "DateTime?" =>
                    $@"{Environment.NewLine}        updated{entityName}.{entityProperty.Name}.Should().BeCloseTo((DateTime)updated{entityName}Dto.{entityProperty.Name}, 1.Seconds());",
                "DateTimeOffset?" =>
                    $@"{Environment.NewLine}        updated{entityName}.{entityProperty.Name}.Should().BeCloseTo((DateTimeOffset)updated{entityName}Dto.{entityProperty.Name}, 1.Seconds());",
                "TimeOnly?" =>
                    $@"{Environment.NewLine}        updated{entityName}.{entityProperty.Name}.Should().BeCloseTo((TimeOnly)updated{entityName}Dto.{entityProperty.Name}, 1.Seconds());",
                _ =>
                    $@"{Environment.NewLine}        updated{entityName}.{entityProperty.Name}.Should().Be(updated{entityName}Dto.{entityProperty.Name});"
            };
        }

        return entityAssertions;
    }
}
