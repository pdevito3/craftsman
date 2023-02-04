namespace Craftsman.Builders.Tests.IntegrationTests.RolePermissions;

using System;
using Craftsman.Domain;
using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;

public class AddCommandTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public AddCommandTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string testDirectory, string srcDirectory, Entity entity, string projectBaseName)
    {
        var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"Add{entity.Name}CommandTests.cs", entity.Plural, projectBaseName);
        var fileText = WriteTestFileText(testDirectory, srcDirectory, classPath, entity, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string testDirectory, string srcDirectory, ClassPath classPath, Entity entity, string projectBaseName)
    {
        var featureName = FileNames.AddEntityFeatureClassName(entity.Name);
        var testFixtureName = FileNames.GetIntegrationTestFixtureName();
        var commandName = FileNames.CommandAddName();

        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(testDirectory, "");
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, featureName, entity.Plural, projectBaseName);

        var foreignEntityUsings = CraftsmanUtilities.GetForeignEntityUsings(testDirectory, entity, projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Threading.Tasks;
using {featuresClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};{foreignEntityUsings}

public class {classPath.ClassNameWithoutExt} : TestBase
{{
    {GetAddCommandTest(commandName, entity, featureName)}
}}";
    }

    private static string GetAddCommandTest(string commandName, Entity entity, string featureName)
    {
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));
        var fakeEntityVariableName = $"fake{entity.Name}One";
        var lowercaseEntityName = entity.Name.LowercaseFirstLetter();

        var fakeParent = "";
        var fakeParentIdRuleFor = "";
        foreach (var entityProperty in entity.Properties)
        {
            if (entityProperty.IsForeignKey && !entityProperty.IsMany && entityProperty.IsPrimitiveType)
            {
                var fakeParentClass = FileNames.FakerName(entityProperty.ForeignEntityName);
                var fakeParentCreationDto = FileNames.FakerName(FileNames.GetDtoName(entityProperty.ForeignEntityName, Dto.Creation));
                fakeParent += @$"var fake{entityProperty.ForeignEntityName}One = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
        await testingServiceScope.InsertAsync(fake{entityProperty.ForeignEntityName}One);{Environment.NewLine}{Environment.NewLine}        ";
                fakeParentIdRuleFor +=
                    $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{entityProperty.ForeignEntityName}One.Id){Environment.NewLine}            ";
            }
        }

        return $@"[Fact]
    public async Task can_add_new_{entity.Name.ToLower()}_to_db()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        {fakeParent}var {fakeEntityVariableName} = new {fakeCreationDto}(){fakeParentIdRuleFor}.Generate();

        // Act
        var command = new {featureName}.{commandName}({fakeEntityVariableName});
        var {lowercaseEntityName}Returned = await testingServiceScope.SendAsync(command);
        var {lowercaseEntityName}Created = await testingServiceScope.ExecuteDbContextAsync(db => db.{entity.Plural}
            .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.Id == {lowercaseEntityName}Returned.Id));

        // Assert
        {lowercaseEntityName}Returned.Permission.Should().Be({fakeEntityVariableName}.Permission);
        {lowercaseEntityName}Returned.Role.Should().Be({fakeEntityVariableName}.Role);

        {lowercaseEntityName}Created?.Permission.Should().Be({fakeEntityVariableName}.Permission);
        {lowercaseEntityName}Created?.Role.Value.Should().Be({fakeEntityVariableName}.Role);
    }}";
    }
}
